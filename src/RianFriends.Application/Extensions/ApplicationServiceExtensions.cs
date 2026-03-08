using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RianFriends.Application.Behaviours;

namespace RianFriends.Application.Extensions;

/// <summary>Application 레이어 DI 등록 확장 메서드</summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Application 레이어 서비스를 DI 컨테이너에 등록합니다.
    /// MediatR, FluentValidation, 파이프라인 Behaviour를 등록합니다.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(AssemblyReference).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        // 파이프라인 순서: Logging → Validation → Handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
