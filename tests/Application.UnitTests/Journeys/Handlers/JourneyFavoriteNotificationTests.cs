using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Domain.Journeys.Events;
using Xunit;

namespace Application.UnitTests.Journeys.Handlers;

public class JourneyFavoriteNotificationTests
{
    [Fact]
    public void JourneyUpdated_Event_ShouldContainCorrectProperties()
    {
        // Arrange
        var journeyId = 123;
        var ownerUserId = "owner123";
        var occurredOn = DateTimeOffset.UtcNow;

        // Act
        var journeyUpdated = new JourneyUpdated(journeyId, ownerUserId, occurredOn);

        // Assert
        Assert.Equal(journeyId, journeyUpdated.JourneyId);
        Assert.Equal(ownerUserId, journeyUpdated.OwnerUserId);
        Assert.Equal(occurredOn, journeyUpdated.OccurredOn);
    }

    [Fact]
    public void JourneyDeleted_Event_ShouldContainCorrectProperties()
    {
        // Arrange
        var journeyId = 123;
        var ownerUserId = "owner123";
        var occurredOn = DateTimeOffset.UtcNow;

        // Act
        var journeyDeleted = new JourneyDeleted(journeyId, ownerUserId, occurredOn);

        // Assert
        Assert.Equal(journeyId, journeyDeleted.JourneyId);
        Assert.Equal(ownerUserId, journeyDeleted.OwnerUserId);
        Assert.Equal(occurredOn, journeyDeleted.OccurredOn);
    }

    [Fact]
    public void JourneyFavorite_Create_ShouldSetCorrectProperties()
    {
        // Arrange
        var journeyId = 123;
        var userId = "user123";
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var favorite = JourneyFavorite.Create(journeyId, userId, createdAt);

        // Assert
        Assert.Equal(journeyId, favorite.JourneyId);
        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(createdAt, favorite.CreatedAt);
    }

    [Fact]
    public void JourneyHub_GroupFor_ShouldReturnCorrectGroupName()
    {
        // Arrange
        var journeyId = 123;

        // Act
        var groupName = Journey.Api.Realtime.JourneyHub.GroupFor(journeyId);

        // Assert
        Assert.Equal("fav-123", groupName);
    }

    [Fact]
    public void JourneyHub_GroupFor_ShouldHandleDifferentJourneyIds()
    {
        // Arrange & Act & Assert
        Assert.Equal("fav-1", Journey.Api.Realtime.JourneyHub.GroupFor(1));
        Assert.Equal("fav-999", Journey.Api.Realtime.JourneyHub.GroupFor(999));
        Assert.Equal("fav-0", Journey.Api.Realtime.JourneyHub.GroupFor(0));
    }
}

