using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
