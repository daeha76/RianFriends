namespace RianFriends.Domain.Common;

/// <summary>
/// 비즈니스 결과 패턴. 비즈니스 오류를 Exception 대신 명시적 타입으로 표현합니다.
/// 비유: 식당 주문서 — 성공이면 음식이 담겨 오고, 실패면 사유가 적힌 쪽지가 옵니다.
/// </summary>
public class Result
{
    /// <summary>성공 여부</summary>
    public bool IsSuccess { get; }

    /// <summary>실패 시 오류 메시지</summary>
    public string Error { get; }

    /// <summary>실패 여부 (IsSuccess의 반대)</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>결과 인스턴스를 생성합니다.</summary>
    /// <param name="isSuccess">성공 여부</param>
    /// <param name="error">실패 시 오류 메시지</param>
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
        {
            throw new InvalidOperationException("성공 결과에 오류 메시지를 포함할 수 없습니다.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException("실패 결과에는 오류 메시지가 필요합니다.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>성공 결과를 생성합니다.</summary>
    public static Result Success() => new(true, string.Empty);

    /// <summary>실패 결과를 생성합니다.</summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>값을 포함하는 성공 결과를 생성합니다.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, string.Empty);

    /// <summary>값 타입의 실패 결과를 생성합니다.</summary>
    public static Result<TValue> Failure<TValue>(string error) => new(default!, false, error);
}

/// <summary>값을 포함하는 비즈니스 결과 패턴.</summary>
/// <typeparam name="TValue">성공 시 반환할 값의 타입</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue _value;

    /// <summary>성공 시 반환 값. 실패 상태에서 접근하면 InvalidOperationException이 발생합니다.</summary>
    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("실패 결과에서 값에 접근할 수 없습니다.");

    internal Result(TValue value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }
}
