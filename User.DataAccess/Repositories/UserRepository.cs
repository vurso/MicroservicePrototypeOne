using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using User.DataAccess.Contracts;
using User.DataAccess.DbContexts;
using User.DataAccess.Entities;

namespace User.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Preference> GetUserPreferenceAsync(Guid userId, bool deleted = false)
        {
            return await _context.Preferences.FirstOrDefaultAsync(j => j.UserId == userId && j.Deleted == deleted);
        }

        public async Task PostUserPreferenceAsync(Preference preferences)
        {
            if (preferences == null)
                throw new ArgumentNullException(nameof(preferences));

            await _context.Preferences.AddAsync(preferences);
        }

        public async Task CompleteTransactionAsync()
        {
            await _context.SaveChangesAsync();
        }

        #region IDisposable

        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}