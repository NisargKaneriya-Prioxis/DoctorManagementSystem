namespace DMS.Services.RepositoryFactory;

public interface IRepositoryFactory
{
    IRepository<T> GetRepository<T>() where T : class;
}