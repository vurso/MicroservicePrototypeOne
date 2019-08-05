using System;
using System.ComponentModel.DataAnnotations;
using User.DataAccess.Entities;

namespace User.Domain.Models
{
    public class PreferencesModel
    {
        public static explicit operator PreferencesModel(Preference item)
        {
            if (item == null)
                return null;

            return new PreferencesModel
            {
                UserId = item.UserId,
                Language = item.PreferredLanguage,
            };
        }

        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Language { get; set; }
    }
}
