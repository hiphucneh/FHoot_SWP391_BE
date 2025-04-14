using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Repository.Repositories;
using System;
using System.Threading.Tasks;

namespace Kahoot.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly KahootContext _context;
        private IUserRepository _userRepository;
        private IPackageRepository _packageRepository;
        private IUserPackageRepository _userPackageRepository;

        public UnitOfWork(KahootContext context)
        {
            _context = context;
        }

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
        public IPackageRepository PackageRepository => _packageRepository ??= new PackageRepository(_context);
        public IUserPackageRepository UserPackageRepository => _userPackageRepository ??= new UserPackageRepository(_context);

        public async Task BeginTransaction()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
