using DMS.Services.RepositoryFactory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DMS.Services.UnitOfWork;

public interface IUnitOfWork : IDisposable
{

    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    int Commit(bool autoHistory = false);
    Task<int> CommitAsync(bool autoHistory = false);
    Task<int> CommitAsyncWithTransaction();
    void ClearContext();
    IDbContextTransaction dbContextTransaction { get; set; }
}

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    TContext Context { get; }
}