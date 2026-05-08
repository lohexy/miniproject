using Xunit;
using Domain;
using TaskStatus = Domain.TaskStatus;
using TaskFactory = Domain.TaskFactory;

namespace tests;

public class DomainTests
{
    [Fact]
    public void NewTask_ShouldHave_TodoStatusByDefault()
    {
        var task = new UserTask();

        Assert.Equal(TaskStatus.Todo, task.Status);
    }

    [Fact]
    public void NewTask_ShouldHave_MediumPriorityByDefault()
    {
        var task = new UserTask();

        Assert.Equal(TaskPriority.Medium, task.Priority);
    }

    [Fact]
    public void Task_ShouldSet_CorrectTitle()
    {
        var expectedTitle = "Написати тести";
        
        var task = new UserTask { Title = expectedTitle };

        Assert.Equal(expectedTitle, task.Title);
    }

    [Fact]
    public void Project_Should_AddTaskToList()
    {
        var project = new Project { Name = "Лабораторні" };
        var task = new UserTask { Title = "Здати Лаб 34" };

        project.AddTask(task);

        Assert.Single(project.Tasks);
        Assert.Equal("Здати Лаб 34", project.Tasks[0].Title);
    }

    [Fact]
    public void User_ShouldSet_CorrectData()
    {
        var user = new User { Name = "Іван", Email = "ivan@test.com" };

        Assert.Equal("Іван", user.Name);
        Assert.Equal("ivan@test.com", user.Email);
    }

    [Fact]
    public void TaskFactory_Should_CreateTaskWithCorrectData()
    {
        var task = TaskFactory.Create("Тест Фабрики", 5, TaskPriority.High);

        Assert.Equal("Тест Фабрики", task.Title);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal(TaskStatus.Todo, task.Status);
        Assert.True(task.DueDate.HasValue);
    }

    [Fact]
    public void TaskFactory_Should_SetNullDueDate_WhenZeroDaysPassed()
    {
        var task = TaskFactory.Create("Без дедлайну", 0, TaskPriority.Low);

        Assert.Null(task.DueDate);
    }
}