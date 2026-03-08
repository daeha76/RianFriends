using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Abstractions;

/// <summary>
/// 읽기 전용 Query 마커 인터페이스.
/// Query Handler는 절대 상태를 변경할 수 없습니다.
/// </summary>
/// <typeparam name="TResponse">조회할 값의 타입</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
