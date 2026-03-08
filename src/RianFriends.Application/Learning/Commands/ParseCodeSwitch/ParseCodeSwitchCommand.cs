using RianFriends.Application.Abstractions;
using RianFriends.Domain.Learning;

namespace RianFriends.Application.Learning.Commands.ParseCodeSwitch;

/// <summary>
/// Code-Switching 파싱 명령 (배치/비동기 처리).
/// 메시지 저장 후 응답 지연 없이 비동기로 호출됩니다.
/// 공감 모드(Empathy)에서는 이 명령을 호출해서는 안 됩니다.
/// </summary>
public record ParseCodeSwitchCommand(
    Guid MessageId,
    string MessageText,
    string UserNativeLanguage = "ko") : ICommand<CodeSwitchSegment[]>;
