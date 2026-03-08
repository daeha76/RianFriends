# Shared: Redis 캐싱 전략

## 역할 정의

Redis는 DB 보조 캐시 + 실시간 대화 컨텍스트 저장소로 사용.
DB에 없는 데이터를 Redis에만 저장 금지 — 캐시는 항상 DB의 사본이어야 함.

## 연결 설정

```csharp
// Infrastructure/DependencyInjection.cs
// Nuget: StackExchange.Redis, Microsoft.Extensions.Caching.StackExchangeRedis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "rianfriends:";   // 모든 키에 자동 prefix
});
```

```bash
# 개발 환경
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379"

# Azure Key Vault (운영)
# Redis--ConnectionString → "rianfriends.redis.cache.windows.net:6380,password=...,ssl=True"
```

## 키 네이밍 규칙

```
{도메인}:{엔티티}:{id}:{용도}

예시:
conversation:{conversationId}:context       # 대화 컨텍스트 (메시지 + 메모리)
avatar:{friendId}:state                     # 아바타 상태 (HungerLevel, Mood)
alarm:{userId}:schedule                     # 알람 스케줄 목록
friend:{friendId}:personality               # 친구 성격 설정 (자주 조회)
user:{userId}:session                       # 인증 세션 정보
```

## 도메인별 TTL 기준

| 키 패턴 | TTL | 이유 |
|:--------|:----|:-----|
| `conversation:*:context` | 30분 | 대화 세션 유지 |
| `avatar:*:state` | 1시간 | 배고픔 계산 주기 |
| `alarm:*:schedule` | 24시간 | 일일 알람 스케줄 |
| `friend:*:personality` | 6시간 | 자주 바뀌지 않음 |
| `user:*:session` | 1시간 | JWT 만료와 동기화 |

## 사용 패턴

### 대화 컨텍스트 캐싱 (핵심)

LLM 호출 시 매번 DB에서 최근 메시지 + 메모리 요약을 조회하는 비용을 줄임:

```csharp
// Application/Interfaces/IConversationContextCache.cs
public interface IConversationContextCache
{
    Task<ConversationContext?> GetAsync(Guid conversationId, CancellationToken ct = default);
    Task SetAsync(Guid conversationId, ConversationContext context, CancellationToken ct = default);
    Task InvalidateAsync(Guid conversationId, CancellationToken ct = default);
}

public record ConversationContext(
    MessageDto[] RecentMessages,     // ShortTerm: 최근 50건
    string MidTermSummary,           // MidTerm 메모리 요약
    string FriendPersonality);       // 친구 성격 프롬프트
```

```csharp
// Infrastructure/Caching/ConversationContextCache.cs
public class ConversationContextCache : IConversationContextCache
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public async Task<ConversationContext?> GetAsync(Guid conversationId, CancellationToken ct)
    {
        var key = $"conversation:{conversationId}:context";
        var json = await _cache.GetStringAsync(key, ct);
        return json is null ? null : JsonSerializer.Deserialize<ConversationContext>(json);
    }

    public async Task SetAsync(Guid conversationId, ConversationContext context, CancellationToken ct)
    {
        var key = $"conversation:{conversationId}:context";
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(context),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl }, ct);
    }
}
```

### 아바타 상태 캐싱

```csharp
// 아바타 상태는 1시간마다 배고픔 증가 잡이 갱신 → 잦은 조회 최적화
public async Task<AvatarStateDto?> GetAvatarStateAsync(Guid friendId)
{
    var key = $"avatar:{friendId}:state";
    var cached = await _cache.GetStringAsync(key);
    if (cached is not null)
        return JsonSerializer.Deserialize<AvatarStateDto>(cached);

    // Cache Miss → DB 조회 후 캐시 저장
    var state = await _db.Avatars
        .Where(a => a.FriendId == friendId)
        .Select(a => new AvatarStateDto(a.HungerLevel, a.Mood, a.LastFedAt))
        .FirstOrDefaultAsync();

    if (state is not null)
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(state),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });

    return state;
}
```

## 캐시 무효화 규칙

캐시 무효화는 **도메인 이벤트 핸들러**에서 처리 — 비즈니스 로직과 분리:

```csharp
// Application/Avatar/AvatarFedEventHandler.cs
public class AvatarFedEventHandler : INotificationHandler<AvatarFedEvent>
{
    public async Task Handle(AvatarFedEvent notification, CancellationToken ct)
    {
        // 아바타 상태 캐시 무효화
        await _cache.RemoveAsync($"avatar:{notification.FriendId}:state", ct);
    }
}
```

## Redis 장애 대응 (Graceful Degradation)

Redis 장애 시 앱이 중단되면 안 됨. DB fallback 필수:

```csharp
// Infrastructure/Caching/ResilientCache.cs
public async Task<T?> GetOrFallbackAsync<T>(
    string key,
    Func<Task<T?>> dbFallback,
    CancellationToken ct = default)
{
    try
    {
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<T>(cached);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Redis 조회 실패. DB fallback 실행. Key: {Key}", key);
        // 캐시 오류는 무시하고 DB 조회
    }
    return await dbFallback();
}
```

## 금지 사항

- 인증 토큰을 Redis에 평문 저장 금지 — `SecureStorage` (모바일) 또는 HttpOnly Cookie 사용
- Redis를 Primary 저장소로 사용 금지 — DB가 항상 Source of Truth
- TTL 없는 키 생성 금지 — 모든 캐시 키에 반드시 TTL 설정
- 대용량 객체(1MB 이상) 캐싱 금지 — 대화 전체 이력 캐싱 대신 요약만 캐싱
