using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Notification.Commands.SetWakeUpAlarm;
using RianFriends.Domain.Notification;

namespace RianFriends.Application.Tests.Notification.Commands;

[Trait("Category", "Unit")]
public class SetWakeUpAlarmCommandHandlerTests
{
    private readonly Mock<IAlarmRepository> _alarmRepositoryMock = new();
    private readonly SetWakeUpAlarmCommandHandler _sut;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidFriendId = Guid.NewGuid();

    public SetWakeUpAlarmCommandHandlerTests()
    {
        _sut = new SetWakeUpAlarmCommandHandler(
            _alarmRepositoryMock.Object,
            NullLogger<SetWakeUpAlarmCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldCreateAndReturnAlarmId()
    {
        // Arrange
        _alarmRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new SetWakeUpAlarmCommand(
            ValidUserId,
            ValidFriendId,
            new TimeOnly(7, 30),
            RepeatDays: 0b0111110); // 월~금 반복

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        _alarmRepositoryMock.Verify(r => r.Add(It.IsAny<WakeUpAlarm>()), Times.Once);
        _alarmRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new SetWakeUpAlarmCommand(
            Guid.Empty,
            ValidFriendId,
            new TimeOnly(7, 30));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _alarmRepositoryMock.Verify(r => r.Add(It.IsAny<WakeUpAlarm>()), Times.Never);
        _alarmRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyFriendId_ShouldReturnFailure()
    {
        // Arrange
        var command = new SetWakeUpAlarmCommand(
            ValidUserId,
            Guid.Empty,
            new TimeOnly(7, 30));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _alarmRepositoryMock.Verify(r => r.Add(It.IsAny<WakeUpAlarm>()), Times.Never);
    }
}
