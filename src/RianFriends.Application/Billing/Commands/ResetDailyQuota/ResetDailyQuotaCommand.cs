using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Billing.Commands.ResetDailyQuota;

/// <summary>전체 사용자의 일일 토큰 쿼터를 초기화하는 커맨드 (배치 잡용)</summary>
public sealed record ResetDailyQuotaCommand : ICommand;
