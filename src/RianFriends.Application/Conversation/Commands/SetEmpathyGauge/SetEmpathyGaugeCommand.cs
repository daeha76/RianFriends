using MediatR;
using RianFriends.Domain.Common;
using RianFriends.Domain.Conversation;

namespace RianFriends.Application.Conversation.Commands.SetEmpathyGauge;

/// <summary>공감 게이지를 설정합니다. 사용자가 슬라이더로 직접 조작할 때 호출됩니다.</summary>
public record SetEmpathyGaugeCommand(
    Guid SessionId,
    Guid UserId,
    int Gauge,
    GaugeControlMode ControlMode) : IRequest<Result>;
