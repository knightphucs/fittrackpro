namespace FitTrackPro.Domain.Events;

using FitTrackPro.Domain.Common;

public record UserRegisteredEvent(Guid UserId, string Email) : DomainEvent;