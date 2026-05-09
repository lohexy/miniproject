using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Infrastructure;

namespace tests;

public class InfrastructureTests : IDisposable
{
    private readonly string _jsonPath = "tasks.json";
    private readonly string _csvPath = "tasks.csv";

    public InfrastructureTests()
    {
        CleanUpFiles();
    }

    public void Dispose()
    {
        CleanUpFiles();
    }

    private void CleanUpFiles()
    {
        if (File.Exists(_jsonPath)) File.Delete(_jsonPath);
        if (File.Exists(_csvPath)) File.Delete(_csvPath);
        if (File.Exists("test_tasks.json")) File.Delete("test_tasks.json");
    }

    [Fact]
    public async Task JsonRepository_LoadAsync_NoFile_ReturnsEmptyList()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        
        var tasks = await repo.LoadAsync();

        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task JsonRepository_SaveAndLoad_MaintainsData()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        var tasksToSave = new List<UserTask> { new UserTask { Title = "Тест JSON" } };

        await repo.SaveAsync(tasksToSave);
        var loadedTasks = await repo.LoadAsync();

        Assert.Single(loadedTasks);
        Assert.Equal("Тест JSON", loadedTasks[0].Title);
    }

    [Fact]
    public void JsonRepository_Add_AddsToInternalList()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        var task = new UserTask { Title = "Внутрішній тест" };

        repo.Add(task);
        var allTasks = repo.GetAll();

        Assert.Single(allTasks);
        Assert.Equal(task.Title, allTasks[0].Title);
    }

    [Fact]
    public async Task JsonRepository_SaveAsync_CreatesFileOnDisk()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        var tasksToSave = new List<UserTask> { new UserTask { Title = "Перевірка файлу" } };

        await repo.SaveAsync(tasksToSave);

        Assert.True(File.Exists(_jsonPath) || File.Exists(_csvPath));
    }
}