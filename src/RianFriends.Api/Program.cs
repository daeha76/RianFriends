using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using RianFriends.Application.Extensions;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Api.Services;
using RianFriends.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Azure Key Vault (비개발 환경) ──────────────────────────
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["Azure:KeyVaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new Azure.Identity.DefaultAzureCredential());
    }
}

// ── Serilog ──────────────────────────────────────────────
builder.Host.UseSerilog((ctx, services, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .ReadFrom.Services(services)
      .Enrich.FromLogContext();

    var webhookUrl = ctx.Configuration["Serilog:N8nWebhookUrl"];
    if (!string.IsNullOrEmpty(webhookUrl))
    {
        lc.WriteTo.Http(
            requestUri: webhookUrl,
            queueLimitBytes: null,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);
    }
});

// ── Application & Infrastructure ─────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Identity ─────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── JWT Bearer 인증 ───────────────────────────────────────
var supabaseProjectRef = builder.Configuration["Supabase:ProjectRef"]
    ?? throw new InvalidOperationException("Supabase:ProjectRef 설정이 필요합니다.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{supabaseProjectRef}.supabase.co/auth/v1";
        options.Audience = "authenticated";
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var authHeader = ctx.Request.Headers.Authorization.ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Token = authHeader["Bearer ".Length..].Trim();
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting ─────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // 인증 엔드포인트: IP 기반 파티셔닝 (미인증 요청)
    options.AddPolicy("AuthPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // 대화 엔드포인트: 사용자별 파티셔닝
    options.AddPolicy("ConversationPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            context.User.FindFirst("sub")?.Value
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── API & Swagger ─────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "RianFriends API", Version = "v1" });

    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "MAUI SecureStorage에서 가져온 AccessToken을 입력하세요."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RianFriends.Infrastructure.Persistence.AppDbContext>();

// ── Build ─────────────────────────────────────────────────
var app = builder.Build();

// ── Exception Handler (Problem Details) ──────────────────
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exFeature?.Error;

        var (status, title, errors) = exception switch
        {
            FluentValidation.ValidationException ve => (400, "Validation Failed",
                (object?)ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            _ => (500, "Internal Server Error", null)
        };

        context.Response.StatusCode = status;
        await Results.Problem(
            title: title,
            statusCode: status,
            extensions: errors is not null
                ? new Dictionary<string, object?> { ["errors"] = errors }
                : null
        ).ExecuteAsync(context);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();

/// <summary>WebApplicationFactory에서 사용하기 위한 진입점 마커</summary>
public partial class Program;
