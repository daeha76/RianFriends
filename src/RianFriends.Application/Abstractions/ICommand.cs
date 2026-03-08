using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Abstractions;

/// <summary>상태를 변경하는 Command 마커 인터페이스 (반환값 없음)</summary>
public interface ICommand : IRequest<Result>;

/// <summary>상태를 변경하고 값을 반환하는 Command 마커 인터페이스</summary>
/// <typeparam name="TResponse">반환할 값의 타입</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
