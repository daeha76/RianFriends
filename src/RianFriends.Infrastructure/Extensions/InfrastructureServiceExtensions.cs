using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Infrastructure.Avatar;
using RianFriends.Infrastructure.BackgroundJobs;
using RianFriends.Infrastructure.Billing;
using RianFriends.Infrastructure.Conversation;
using RianFriends.Infrastructure.Friend;
using RianFriends.Infrastructure.Identity;
using RianFriends.Infrastructure.Llm;
using RianFriends.Infrastructure.Memory;
using RianFriends.Infrastructure.Notification;
using RianFriends.Infrastructure.Persistence;
using RianFriends.Infrastructure.Redis;

namespace RianFriends.Infrastructure.Extensions;

/// <summary>Infrastructure 레이어 DI 등록 확장 메서드</summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Infrastructure 레이어 서비스를 DI 컨테이너에 등록합니다.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL (Supabase) ─────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection 설정이 필요합니다. " +
                "User Secrets 또는 Azure Key Vault에서 설정하세요.");

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        // ── Redis 캐시 ────────────────────────────────────────
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "rianfriends:";
            });
        }
        else
        {
            // Redis 미설정 시 메모리 캐시로 대체 (개발 환경)
            services.AddDistributedMemoryCache();
        }

        // ── Identity ─────────────────────────────────────────
        RegisterSupabaseClient(services, configuration);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, SupabaseAuthService>();

        // ── Phase 2: Friend ───────────────────────────────────
        services.AddScoped<IFriendRepository, FriendRepository>();

        // ── Phase 2: Conversation ─────────────────────────────
        services.AddScoped<IConversationRepository, ConversationRepository>();

        // ── Phase 2: Memory ───────────────────────────────────
        services.AddScoped<IMemoryRepository, MemoryRepository>();

        // ── Phase 2: Redis Context ────────────────────────────
        services.AddScoped<IRedisContextService, RedisContextService>();
        services.AddScoped<ICacheRemovalService, CacheRemovalService>();

        // ── Phase 2: LLM (Claude) ─────────────────────────────
        services.AddHttpClient<ILlmService, ClaudeLlmService>();

        // ── Phase 2: Background Jobs ──────────────────────────
        services.AddHostedService<MemorySummaryJob>();

        // ── Phase 3: Avatar ───────────────────────────────────
        services.AddScoped<IAvatarRepository, AvatarRepository>();

        // ── Phase 3: Notification ─────────────────────────────
        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<INotificationService, FcmNotificationService>();
        services.AddHostedService<HungerIncreaseJob>();

        // ── Phase 4: Billing ───────────────────────────────────
        services.AddScoped<IUserQuotaRepository, UserQuotaRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IBillingService, RevenueCatWebhookService>();
        services.AddHostedService<DailyQuotaResetJob>();

        return services;
    }

    private static void RegisterSupabaseClient(IServiceCollection services, IConfiguration configuration)
    {
        var supabaseUrl = configuration["Supabase:Url"]
            ?? throw new InvalidOperationException(
                "Supabase:Url 설정이 필요합니다. User Secrets 또는 Azure Key Vault에서 설정하세요.");

        var supabaseKey = configuration["Supabase:ServiceKey"]
            ?? throw new InvalidOperationException(
                "Supabase:ServiceKey 설정이 필요합니다. User Secrets 또는 Azure Key Vault에서 설정하세요.");

        services.AddSingleton(_ => new Supabase.Client(
            supabaseUrl,
            supabaseKey,
            new Supabase.SupabaseOptions { AutoConnectRealtime = false }));
    }
}

