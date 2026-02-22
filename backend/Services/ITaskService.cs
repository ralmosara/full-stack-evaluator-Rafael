using TaskManager.DTOs;

namespace TaskManager.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponse>> GetAllTasksAsync(int userId);
        Task<TaskResponse?> GetTaskByIdAsync(int id, int userId);
        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int userId);
        Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId);
        Task<bool> DeleteTaskAsync(int id, int userId);
    }
}
