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

    [Fact]
    public void UserTask_ShouldGenerate_UniqueIds()
    {
        var task1 = new UserTask();
        var task2 = new UserTask();

        Assert.NotEqual(Guid.Empty, task1.Id);
        Assert.NotEqual(task1.Id, task2.Id);
    }

    [Theory]
    [InlineData("Терміново", 1, TaskPriority.High)]
    [InlineData("Колись", 0, TaskPriority.Low)]
    [InlineData("Звичайна", 7, TaskPriority.Medium)]
    public void TaskFactory_Create_UsesProvidedParameters(string title, int days, TaskPriority priority)
    {
        var task = TaskFactory.Create(title, days, priority);

        Assert.Equal(title, task.Title);
        Assert.Equal(priority, task.Priority);
    }

    [Fact]
    public void Project_Initialization_CreatesEmptyTasksList()
    {
        var project = new Project();
        
        Assert.NotNull(project.Tasks);
        Assert.Empty(project.Tasks);
    }

    [Fact]
    public void UserTask_CanChangeStatus()
    {
        var task = new UserTask { Status = TaskStatus.Todo };
        
        task.Status = TaskStatus.InProgress;
        
        Assert.Equal(TaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void TaskFactory_Create_NegativeDays_SetsNullDueDate()
    {
        var task = TaskFactory.Create("Тест від'ємних днів", -5, TaskPriority.Low);
        Assert.True(task.DueDate < DateTime.Now);
    }

    [Fact]
    public void TaskFactory_Create_EmptyTitle_CreatesTaskAnyway()
    {
        var task = TaskFactory.Create("", 1, TaskPriority.Medium);
        Assert.Equal("Без назви", task.Title);
    }

    [Fact]
    public void Project_RemoveTask_TaskExists_RemovesTask()
    {
        var project = new Project();
        var task = new UserTask { Title = "Видалити мене" };
        project.AddTask(task);
        
        project.Tasks.Remove(task);

        Assert.Empty(project.Tasks);
    }

    [Fact]
    public void Project_RemoveTask_TaskDoesNotExist_DoesNothing()
    {
        var project = new Project();
        project.AddTask(new UserTask { Title = "Залишити" });
        
        project.Tasks.Remove(new UserTask { Title = "Фантом" });

        Assert.Single(project.Tasks);
    }

    [Theory]
    [InlineData(TaskStatus.Todo, TaskStatus.InProgress)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Done)]
    [InlineData(TaskStatus.Done, TaskStatus.Todo)]
    public void UserTask_Status_CanTransitionToAnyState(TaskStatus initial, TaskStatus target)
    {
        var task = new UserTask { Status = initial };
        task.Status = target;
        Assert.Equal(target, task.Status);
    }

    [Fact]
    public void UserTask_Id_IsGenerated_AsGuidV4()
    {
        var task = new UserTask();
        Assert.NotEqual(Guid.Empty.ToString(), task.Id.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UserTask_Title_AcceptsEmptyOrWhitespace_MaintainsState(string emptyTitle)
    {
        var task = new UserTask { Title = emptyTitle };
        Assert.Equal(emptyTitle, task.Title);
    }

    [Fact]
    public void Project_TasksList_CanStoreLargeAmountOfTasks()
    {
        var project = new Project();
        for (int i = 0; i < 1000; i++)
        {
            project.AddTask(new UserTask { Title = $"Task {i}" });
        }

        Assert.Equal(1000, project.Tasks.Count);
    }
        
    [Fact]
    public void UserTask_DueDate_CanBeModifiedAfterCreation()
    {
        var task = new UserTask { DueDate = DateTime.Now };
        var newDate = DateTime.Now.AddDays(14);
            
        task.DueDate = newDate;
            
        Assert.Equal(newDate, task.DueDate);
    }
}