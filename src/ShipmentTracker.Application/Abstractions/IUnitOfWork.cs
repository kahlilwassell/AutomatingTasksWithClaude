namespace ShipmentTracker.Application.Abstractions;

/// <summary>Commits all pending changes made through repositories in one transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
