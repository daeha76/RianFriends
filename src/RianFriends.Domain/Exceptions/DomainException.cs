namespace RianFriends.Domain.Exceptions;

/// <summary>
/// 도메인 규칙 위반을 나타내는 예외.
/// 비즈니스 로직 오류에는 Result 패턴을 사용하고,
/// 이 예외는 프로그래밍 오류(잘못된 상태 전이 등)에만 사용합니다.
/// </summary>
public class DomainException : Exception
{
    /// <summary>도메인 예외를 생성합니다.</summary>
    /// <param name="message">도메인 규칙 위반 설명</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>도메인 예외를 내부 예외와 함께 생성합니다.</summary>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
