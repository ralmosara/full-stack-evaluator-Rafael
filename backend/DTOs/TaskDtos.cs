using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs
{
    public class CreateTaskRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; } = 3;
    }

    public class UpdateTaskRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsDone { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; } = 3;
    }

    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDone { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
