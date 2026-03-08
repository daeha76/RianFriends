using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Commands.Login;

/// <summary>
/// 소셜/이메일 로그인 Command. Supabase Auth를 통해 JWT를 발급합니다.
/// Provider: "google" | "kakao" | "apple" | "email"
/// Credential: 소셜 OAuth AccessToken 또는 이메일 비밀번호
/// Email: email 프로바이더 시 필수. IsEmailHidden: Apple Sign In 이메일 숨기기 여부
/// </summary>
public record LoginCommand(
    string Provider,
    string Credential,
    string? Email = null,
    bool IsEmailHidden = false) : ICommand<AuthResultDto>;
