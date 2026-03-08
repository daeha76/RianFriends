using NetArchTest.Rules;
using FluentAssertions;

namespace RianFriends.Architecture.Tests;

/// <summary>
/// Clean Architecture 레이어 의존성 규칙 자동 검증 테스트.
/// 의존성 방향: Domain ← Application ← Infrastructure ← Api
/// </summary>
[Trait("Category", "Architecture")]
public class LayerDependencyTests
{
    private const string DomainNamespace = "RianFriends.Domain";
    private const string ApplicationNamespace = "RianFriends.Application";
    private const string InfrastructureNamespace = "RianFriends.Infrastructure";
    private const string ApiNamespace = "RianFriends.Api";

    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(Domain.AssemblyReference).Assembly;

    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(Application.AssemblyReference).Assembly;

    private static readonly System.Reflection.Assembly InfrastructureAssembly =
        typeof(Infrastructure.AssemblyReference).Assembly;

    private static readonly System.Reflection.Assembly ApiAssembly =
        typeof(Api.AssemblyReference).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain 레이어는 Application을 참조할 수 없습니다. 위반 타입: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain 레이어는 Infrastructure를 참조할 수 없습니다. 위반 타입: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Api()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain 레이어는 Api를 참조할 수 없습니다. 위반 타입: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application 레이어는 Infrastructure를 참조할 수 없습니다. 위반 타입: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application 레이어는 Api를 참조할 수 없습니다. 위반 타입: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure 레이어는 Api를 참조할 수 없습니다. 위반 타입: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
