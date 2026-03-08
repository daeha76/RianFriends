using RianFriends.Application.Abstractions;

namespace RianFriends.Application.Identity.Commands.DeleteAccount;

/// <summary>사용자 계정 탈퇴 Command. 대화/메모리를 즉시 삭제하고 계정을 익명화합니다.</summary>
public record DeleteUserAccountCommand(Guid UserId) : ICommand;
