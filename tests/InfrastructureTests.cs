using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Infrastructure;

namespace tests;

public class InfrastructureTests : IDisposable
{
    private readonly string _testFilePath = "test_tasks.json";

    public void Dispose()
    {
        if (File.Exists(_testFilePath)) File.Delete(_testFilePath);
    }

    [Fact]
    public async Task JsonRepository_LoadAsync_NoFile_ReturnsEmptyList()
    {
        using var repo = new JsonTaskRepository(_testFilePath);
        
        var tasks = await repo.LoadAsync();

        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task JsonRepository_SaveAndLoad_MaintainsData()
    {
        using var repo = new JsonTaskRepository(_testFilePath);
        var tasksToSave = new List<UserTask> { new UserTask { Title = "Тест JSON" } };

        await repo.SaveAsync(tasksToSave);
        var loadedTasks = await repo.LoadAsync();

        Assert.Single(loadedTasks);
        Assert.Equal("Тест JSON", loadedTasks[0].Title);
    }

    [Fact]
    public void JsonRepository_Add_AddsToInternalList()
    {
        using var repo = new JsonTaskRepository(_testFilePath);
        var task = new UserTask { Title = "Внутрішній тест" };

        repo.Add(task);
        var allTasks = repo.GetAll();

        Assert.Single(allTasks);
        Assert.Equal(task.Title, allTasks[0].Title);
    }

    [Fact]
    public async Task JsonRepository_SaveAsync_CreatesFileOnDisk()
    {
        using var repo = new JsonTaskRepository(_testFilePath);
        var tasksToSave = new List<UserTask> { new UserTask { Title = "Перевірка файлу" } };

        await repo.SaveAsync(tasksToSave);

        Assert.True(File.Exists(_testFilePath));
    }
}