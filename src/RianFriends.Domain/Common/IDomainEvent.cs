using MediatR;

namespace RianFriends.Domain.Common;

/// <summary>도메인 이벤트 마커 인터페이스. MediatR INotification을 통해 핸들러로 전파됩니다.</summary>
public interface IDomainEvent : INotification;
