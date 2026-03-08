using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Billing.Queries.CheckTokenQuota;

/// <summary>토큰 쿼터 확인 쿼리</summary>
/// <param name="UserId">사용자 ID</param>
/// <param name="RequiredTokens">요청 예정 토큰 수 (소비 없이 확인만)</param>
public sealed record CheckTokenQuotaQuery(Guid UserId, int RequiredTokens)
    : IQuery<bool>;
