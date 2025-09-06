//using CleanArchitecture.Domain.Journeys;
//using CleanArchitecture.Domain.Journeys.Events;
//using Xunit;

//namespace Application.UnitTests.Journeys.Handlers;

//public class DomainEventFlowTests
//{
//    [Fact]
//    public void JourneyEntity_ShouldImplementIHasDomainEvents()
//    {
//        // Arrange & Act
//        var journey = JourneyEntity.Create(
//            "user123", "Start", DateTimeOffset.UtcNow,
//            "End", DateTimeOffset.UtcNow.AddHours(1),
//            TransportType.Car, DistanceKm.From(10), DateTimeOffset.UtcNow, false);

//        // Assert
//        Assert.NotNull(journey.DomainEvents);
//        Assert.IsAssignableFrom<CleanArchitecture.Domain.Common.IHasDomainEvents>(journey);
//    }

//    [Fact]
//    public void JourneyEntity_Update_ShouldRaiseJourneyUpdatedEvent()
//    {
//        // Arrange
//        var journey = JourneyEntity.Create(
//            "user123", "Start", DateTimeOffset.UtcNow,
//            "End", DateTimeOffset.UtcNow.AddHours(1),
//            TransportType.Car, DistanceKm.From(10), DateTimeOffset.UtcNow, false);

//        // Clear the initial event from creation
//        journey.ClearDomainEvents();

//        // Act
//        journey.Update("New Start", DateTimeOffset.UtcNow, "New End", 
//            DateTimeOffset.UtcNow.AddHours(2), TransportType.Bike, DistanceKm.From(15), DateTimeOffset.UtcNow);

//        // Assert
//        Assert.Single(journey.DomainEvents);
//        var domainEvent = journey.DomainEvents.First();
//        Assert.IsType<JourneyUpdated>(domainEvent);
        
//        var journeyUpdated = (JourneyUpdated)domainEvent;
//        Assert.Equal(journey.Id, journeyUpdated.JourneyId);
//        Assert.Equal("user123", journeyUpdated.OwnerUserId);
//    }

//    [Fact]
//    public void JourneyEntity_Delete_ShouldRaiseJourneyDeletedEvent()
//    {
//        // Arrange
//        var journey = JourneyEntity.Create(
//            "user123", "Start", DateTimeOffset.UtcNow,
//            "End", DateTimeOffset.UtcNow.AddHours(1),
//            TransportType.Car, DistanceKm.From(10), DateTimeOffset.UtcNow, false);

//        // Clear the initial event from creation
//        journey.ClearDomainEvents();

//        // Act
//        journey.Delete(DateTimeOffset.UtcNow);

//        // Assert
//        Assert.Single(journey.DomainEvents);
//        var domainEvent = journey.DomainEvents.First();
//        Assert.IsType<JourneyDeleted>(domainEvent);
        
//        var journeyDeleted = (JourneyDeleted)domainEvent;
//        Assert.Equal(journey.Id, journeyDeleted.JourneyId);
//        Assert.Equal("user123", journeyDeleted.OwnerUserId);
//    }

//    [Fact]
//    public void JourneyEntity_ClearDomainEvents_ShouldRemoveAllEvents()
//    {
//        // Arrange
//        var journey = JourneyEntity.Create(
//            "user123", "Start", DateTimeOffset.UtcNow,
//            "End", DateTimeOffset.UtcNow.AddHours(1),
//            TransportType.Car, DistanceKm.From(10), DateTimeOffset.UtcNow, false);

//        // Should have one event from creation
//        Assert.Single(journey.DomainEvents);

//        // Act
//        journey.ClearDomainEvents();

//        // Assert
//        Assert.Empty(journey.DomainEvents);
//    }
//}
