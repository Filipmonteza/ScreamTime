using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Common;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the operations every repository shares. Derived
/// repositories override <see cref="Query"/> to add their own Includes and
/// filters, and get GetAllAsync and GetByIdAsync on top of it.
/// </summary>
public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly ScreenTimeDbContext Context;

    protected Repository(ScreenTimeDbContext context)
    {
        Context = context;
    }

    // The table this repository owns. Set<TEntity>() resolves it from the model,
    // so no repository has to name its own DbSet property.
    protected DbSet<TEntity> Entities => Context.Set<TEntity>();

    // The starting point for every read. Override to add Includes, or a filter
    // that should apply to everything the repository returns.
    protected virtual IQueryable<TEntity> Query(bool track = false) =>
        track ? Entities : Entities.AsNoTracking();

    public virtual async Task<List<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Query().ToListAsync(cancellationToken);

    // The BaseEntity constraint is what makes e.Id available here. Without it
    // there is no Id to compare against and this method could not be generic.
    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id,
        bool track = false,
        CancellationToken cancellationToken = default) =>
        await Query(track)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default) =>
        await Entities.AddAsync(entity, cancellationToken);

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) =>
        await Entities.AddRangeAsync(entities, cancellationToken);

    public virtual void Update(TEntity entity) =>
        Entities.Update(entity);

    public virtual async Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        await Context.SaveChangesAsync(cancellationToken) > 0;
}
