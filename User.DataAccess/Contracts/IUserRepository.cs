using System;
using System.Threading.Tasks;
using User.DataAccess.Entities;

namespace User.DataAccess.Contracts
{
    public interface IUserRepository : IDisposable
    {
        Task<Preference> GetUserPreferenceAsync(Guid userId, bool deleted = false);
        Task PostUserPreferenceAsync(Preference preferences);
        Task CompleteTransactionAsync();
    }
}