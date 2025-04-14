using System.Threading.Tasks;

namespace Kahoot.Repository.Interface
{
    public interface IUnitOfWork
    {
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        Task SaveChangesAsync();

        IUserRepository UserRepository { get; }
        IPackageRepository PackageRepository { get; }
        IUserPackageRepository UserPackageRepository { get; }
    }
}