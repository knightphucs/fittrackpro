using MediatR;

namespace FitTrackPro.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
