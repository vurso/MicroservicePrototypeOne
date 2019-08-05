using System;
using System.Threading.Tasks;
using User.DataAccess.Contracts;
using User.DataAccess.Entities;
using User.Domain.Contracts;
using User.Domain.Models;

namespace User.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Preference> GetUserPreferenceAsync(Guid userId)
        {
            return await _userRepository.GetUserPreferenceAsync(userId);
        }
        
        public async Task PostUserPreferenceAsync(PreferencesModel preferenceModel, Guid authorizedUserId)
        {
            await _userRepository.PostUserPreferenceAsync(
                new Preference
                {
                    UserId = preferenceModel.UserId,
                    PreferredLanguage = preferenceModel.Language,
                    Deleted = false,
                    EditedBy = authorizedUserId,
                    EditedOn = DateTimeOffset.Now,
                    CreatedBy = authorizedUserId,
                    CreatedOn = DateTimeOffset.Now
                });
        }

        public void PutUserPreference(Preference preference, PutPreferencesModel preferenceModel, Guid authorizedUserId)
        {
            preference.PreferredLanguage = preferenceModel.Language;
            preference.EditedBy = authorizedUserId;
            preference.EditedOn = DateTimeOffset.Now;
        }

        public void DeleteUserPreference(Preference preference, Guid authorizedUserId)
        {
            preference.EditedBy = authorizedUserId;
            preference.EditedOn = DateTimeOffset.Now;
            preference.Deleted = true;
        }

        public async Task CompleteTransactionAsync()
        {
            await _userRepository.CompleteTransactionAsync();
        }
    }
}
