using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.DataAccess.Entities
{
    [Table("Preferences", Schema = "UserPreference")]
    public class Preference
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string PreferredLanguage { get; set; }
        [Required]
        public bool Deleted { get; set; }
        [Required]
        public Guid CreatedBy { get; set; }
        [Required]
        public DateTimeOffset CreatedOn { get; set; }
        [Required]
        public Guid EditedBy { get; set; }
        [Required]
        public DateTimeOffset EditedOn { get; set; }
    }
}