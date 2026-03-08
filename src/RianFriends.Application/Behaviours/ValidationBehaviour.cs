using FluentValidation;
using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Behaviours;

/// <summary>
/// MediatR 파이프라인 — 모든 Command/Query 진입 전 FluentValidation 자동 실행.
/// 비유: 식당 주방 입구의 검수대. 재료(요청)가 기준에 맞지 않으면 주방 안으로 들어갈 수 없습니다.
/// </summary>
internal sealed class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>생성자</summary>
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
