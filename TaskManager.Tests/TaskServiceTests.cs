using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOrderedByPriorityThenDate()
    {
        var context = CreateContext();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, Title = "Low", Priority = 1, UserId = 1, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = 2, Title = "High", Priority = 5, UserId = 1, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = 3, Title = "Medium", Priority = 3, UserId = 1, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var tasks = (await service.GetAllTasksAsync(userId: 1)).ToList();

        Assert.Equal(3, tasks.Count);
        Assert.Equal("High", tasks[0].Title);
        Assert.Equal("Medium", tasks[1].Title);
        Assert.Equal("Low", tasks[2].Title);
    }

    [Fact]
    public async Task GetAllTasks_FiltersTasksByUserId()
    {
        var context = CreateContext();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, Title = "User1Task", Priority = 3, UserId = 1, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = 2, Title = "User2Task", Priority = 3, UserId = 2, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var tasks = (await service.GetAllTasksAsync(userId: 1)).ToList();

        Assert.Single(tasks);
        Assert.Equal("User1Task", tasks[0].Title);
    }

    [Fact]
    public async Task CreateTask_MapsFieldsCorrectly()
    {
        var context = CreateContext();
        var service = new TaskService(context);

        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "A description",
            Priority = 4
        };

        var result = await service.CreateTaskAsync(request, userId: 1);

        Assert.Equal("New Task", result.Title);
        Assert.Equal("A description", result.Description);
        Assert.Equal(4, result.Priority);
        Assert.False(result.IsDone);
        Assert.True(result.Id > 0);

        var saved = await context.Tasks.FirstAsync();
        Assert.Equal(1, saved.UserId);
        Assert.Equal("New Task", saved.Title);
    }

    [Fact]
    public async Task CreateTask_DefaultsIsDoneToFalse()
    {
        var context = CreateContext();
        var service = new TaskService(context);

        var result = await service.CreateTaskAsync(
            new CreateTaskRequest { Title = "Test" }, userId: 1);

        Assert.False(result.IsDone);
    }

    [Fact]
    public async Task GetTaskById_ReturnsTask_WhenExists()
    {
        var context = CreateContext();
        context.Tasks.Add(new TaskItem
        {
            Id = 1, Title = "Found", Priority = 3, UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var result = await service.GetTaskByIdAsync(1, userId: 1);

        Assert.NotNull(result);
        Assert.Equal("Found", result!.Title);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNull_WhenWrongUser()
    {
        var context = CreateContext();
        context.Tasks.Add(new TaskItem
        {
            Id = 1, Title = "OtherUser", Priority = 3, UserId = 2,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var result = await service.GetTaskByIdAsync(1, userId: 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNull_WhenNotFound()
    {
        var context = CreateContext();
        var service = new TaskService(context);

        var result = await service.GetTaskByIdAsync(999, userId: 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTask_SetsUpdatedAtAndFields()
    {
        var context = CreateContext();
        context.Tasks.Add(new TaskItem
        {
            Id = 1, Title = "Original", Priority = 2, UserId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var request = new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "New desc",
            IsDone = true,
            Priority = 5
        };

        var result = await service.UpdateTaskAsync(1, request, userId: 1);

        Assert.NotNull(result);
        Assert.Equal("Updated", result!.Title);
        Assert.Equal("New desc", result.Description);
        Assert.True(result.IsDone);
        Assert.Equal(5, result.Priority);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNull_WhenNotFound()
    {
        var context = CreateContext();
        var service = new TaskService(context);

        var result = await service.UpdateTaskAsync(999, new UpdateTaskRequest
        {
            Title = "X", Priority = 1
        }, userId: 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTask_RemovesFromDatabase()
    {
        var context = CreateContext();
        context.Tasks.Add(new TaskItem
        {
            Id = 1, Title = "ToDelete", Priority = 3, UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var result = await service.DeleteTaskAsync(1, userId: 1);

        Assert.True(result);
        Assert.Empty(await context.Tasks.ToListAsync());
    }

    [Fact]
    public async Task DeleteTask_ReturnsFalse_WhenNotFound()
    {
        var context = CreateContext();
        var service = new TaskService(context);

        var result = await service.DeleteTaskAsync(999, userId: 1);

        Assert.False(result);
    }

    [Fact]
    public async Task TaskResponse_DoesNotExposeUserId()
    {
        var context = CreateContext();
        var service = new TaskService(context);

        var result = await service.CreateTaskAsync(
            new CreateTaskRequest { Title = "Secret", Priority = 3 }, userId: 42);

        // TaskResponse has no UserId property - the type itself prevents leaking
        var responseType = typeof(TaskResponse);
        Assert.Null(responseType.GetProperty("UserId"));
        Assert.Null(responseType.GetProperty("User"));
    }
}
