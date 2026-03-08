using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EFCore.NamingConventions;

namespace RianFriends.Infrastructure.Persistence;

/// <summary>
/// EF Core CLI 도구(dotnet ef migrations)를 위한 Design-Time 팩토리.
/// 실제 DB 연결 없이 마이그레이션 생성에 사용됩니다.
/// </summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Design-time 전용 더미 연결 문자열 (마이그레이션 생성에만 사용)
        optionsBuilder
            .UseNpgsql("Host=localhost;Database=rianfriends_design;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options, new NoOpPublisher());
    }

    /// <summary>Design-time 전용 IPublisher 스텁. 마이그레이션 생성 시에만 사용됩니다.</summary>
    private sealed class NoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
            where TNotification : INotification => Task.CompletedTask;
    }
}
