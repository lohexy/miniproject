using Xunit;
using Application;
using Domain;
using TaskStatus = Domain.TaskStatus;

namespace tests;

public class ApplicationTests
{
    [Fact]
    public void TaskService_TryAddTask_UniqueTitle_ReturnsTrue()
    {
        var project = new Project();
        var service = new TaskService(project);

        bool result = service.TryAddTask("Нова задача", 3, TaskPriority.High, out string error, out var task);

        Assert.True(result);
        Assert.Empty(error);
        Assert.NotNull(task);
        Assert.Single(project.Tasks);
    }

    [Fact]
    public void TaskService_TryAddTask_DuplicateTitle_ReturnsFalse()
    {
        var project = new Project();
        project.AddTask(new UserTask { Title = "Дублікат" });
        var service = new TaskService(project);

        bool result = service.TryAddTask("Дублікат", 2, TaskPriority.Medium, out string error, out var task);

        Assert.False(result);
        Assert.Null(task);
        Assert.Contains("вже існує", error);
    }

    [Fact]
    public void TaskService_TryChangeStatus_OverdueTaskToDone_ReturnsFalse()
    {
        var project = new Project();
        var task = new UserTask { Title = "Прострочена", DueDate = DateTime.Now.AddDays(-1) }; 
        project.AddTask(task);
        var service = new TaskService(project);

        bool result = service.TryChangeStatus(0, TaskStatus.Done, out string error);

        Assert.False(result);
        Assert.NotEqual(TaskStatus.Done, project.Tasks[0].Status);
        Assert.Contains("Не можна завершити прострочену", error);
    }

    [Fact]
    public void TaskService_GetAnalytics_CalculatesCorrectly()
    {
        var project = new Project();
        var t1 = new UserTask { Status = TaskStatus.Done };
        var t2 = new UserTask { Status = TaskStatus.Todo };
        project.AddTask(t1);
        project.AddTask(t2);
        var service = new TaskService(project);

        var stats = service.GetAnalytics();

        Assert.Equal(2, stats.Total);
        Assert.Equal(1, stats.Done);
        Assert.Equal(50.0, stats.Progress); 
    }

    [Fact]
    public void TaskService_TryChangeStatus_ValidTask_ChangesStatusSuccessfully()
    {
        var project = new Project();
        project.AddTask(new UserTask { Title = "Нормальна", Status = TaskStatus.Todo });
        var service = new TaskService(project);

        bool result = service.TryChangeStatus(0, TaskStatus.InProgress, out string error);

        Assert.True(result);
        Assert.Empty(error);
        Assert.Equal(TaskStatus.InProgress, project.Tasks[0].Status);
    }

    [Fact]
    public void TaskService_GetFilteredTasks_ReturnsOnlyMatching()
    {
        var project = new Project();
        project.AddTask(new UserTask { Title = "Купити молоко" });
        project.AddTask(new UserTask { Title = "Зробити лабу" });
        var service = new TaskService(project);

        var statuses = new System.Collections.Generic.List<TaskStatus> { TaskStatus.Todo };
        var result = service.GetFilteredTasks("молоко", statuses, false, "Date", false);

        Assert.Single(result);
        Assert.Equal("Купити молоко", result[0].Title);
    }
}