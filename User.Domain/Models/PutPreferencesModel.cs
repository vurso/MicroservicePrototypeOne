using System.ComponentModel.DataAnnotations;

namespace User.Domain.Models
{
    public class PutPreferencesModel
    {
        [Required]
        public string Language { get; set; }
    }
}