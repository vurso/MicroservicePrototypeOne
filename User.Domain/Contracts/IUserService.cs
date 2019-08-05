using System;
using System.Threading.Tasks;
using User.DataAccess.Entities;
using User.Domain.Models;

namespace User.Domain.Contracts
{
    public interface IUserService
    {
        Task<Preference> GetUserPreferenceAsync(Guid userId);
        Task PostUserPreferenceAsync(PreferencesModel preferenceModel, Guid authorizedUserId);
        void PutUserPreference(Preference preference, PutPreferencesModel preferenceModel, Guid authorizedUserId);
        void DeleteUserPreference(Preference preference, Guid authorizedUserId);
        Task CompleteTransactionAsync();
    }
}