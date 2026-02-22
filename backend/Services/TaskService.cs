using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskResponse>> GetAllTasksAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => MapToResponse(t))
                .ToListAsync();
        }

        public async Task<TaskResponse?> GetTaskByIdAsync(int id, int userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId) return null;
            return MapToResponse(task);
        }

        public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int userId)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return MapToResponse(task);
        }

        public async Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId) return null;

            task.Title = request.Title;
            task.Description = request.Description;
            task.IsDone = request.IsDone;
            task.Priority = request.Priority;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(task);
        }

        public async Task<bool> DeleteTaskAsync(int id, int userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TaskResponse MapToResponse(TaskItem task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsDone = task.IsDone,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
