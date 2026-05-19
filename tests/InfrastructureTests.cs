using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Infrastructure;
using System.Text;
using TaskStatus = Domain.TaskStatus;

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

    [Fact]
    public async Task JsonRepository_LoadAsync_CorruptedJson_RecoversFromCsvFallback()
    {
        await File.WriteAllTextAsync(_jsonPath, "{ broken_json: [ ");
        
        var csvContent = new StringBuilder();
        csvContent.AppendLine("Title,Status,Priority,DueDate");
        csvContent.AppendLine("Відновлена задача,Todo,High,");
        await File.WriteAllTextAsync(_csvPath, csvContent.ToString());

        using var repo = new JsonTaskRepository(_jsonPath);
        
        var recoveredTasks = await repo.LoadAsync();

        Assert.Single(recoveredTasks);
        Assert.Equal("Відновлена задача", recoveredTasks[0].Title);
    }

    [Fact]
    public async Task JsonRepository_LoadAsync_EmptyJsonArray_ReturnsEmptyList()
    {
        await File.WriteAllTextAsync(_jsonPath, "[]");

        using var repo = new JsonTaskRepository(_jsonPath);
        var tasks = await repo.LoadAsync();

        Assert.Empty(tasks);
    }

    [Fact]
    public async Task JsonRepository_FullCycle_AddMultiple_SaveAndLoad()
    {
        using var saveRepo = new JsonTaskRepository(_jsonPath);
        saveRepo.Add(new UserTask { Title = "Перша" });
        saveRepo.Add(new UserTask { Title = "Друга" });
        saveRepo.Add(new UserTask { Title = "Третя" });
        
        await saveRepo.SaveAsync(saveRepo.GetAll());

        using var loadRepo = new JsonTaskRepository(_jsonPath);
        var loaded = await loadRepo.LoadAsync();

        Assert.Equal(3, loaded.Count);
        Assert.Equal("Перша", loaded[0].Title);
        Assert.Equal("Третя", loaded[2].Title);
    }

    [Fact]
    public async Task JsonRepository_SaveAsync_CreatesBothJsonAndCsv()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        var tasksToSave = new List<UserTask> { new UserTask { Title = "Подвійне збереження" } };

        await repo.SaveAsync(tasksToSave);

        Assert.True(File.Exists(_jsonPath), "JSON має бути створений");
        Assert.True(File.Exists(_csvPath), "CSV бекап має бути створений");
    }

    [Fact]
    public async Task JsonRepository_SaveAsync_EmptyList_OverwritesExistingData()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        await repo.SaveAsync(new List<UserTask> { new UserTask(), new UserTask() });
        
        await repo.SaveAsync(new List<UserTask>());
        
        var loaded = await repo.LoadAsync();
        Assert.Empty(loaded);
    }

    [Fact]
    public async Task JsonRepository_LoadAsync_ZeroByteFile_ReturnsEmptyList()
    {
        await File.WriteAllTextAsync(_jsonPath, "");

        using var repo = new JsonTaskRepository(_jsonPath);
        var tasks = await repo.LoadAsync();

        Assert.Empty(tasks);
    }

    [Fact]
    public async Task JsonRepository_FullCycle_StatusUpdateIsPersisted()
    {
        using var saveRepo = new JsonTaskRepository(_jsonPath);
        var task = new UserTask { Title = "Змінити статус", Status = TaskStatus.Todo };
        saveRepo.Add(task);
        
        saveRepo.GetAll()[0].Status = TaskStatus.Done;
        await saveRepo.SaveAsync(saveRepo.GetAll());

        using var loadRepo = new JsonTaskRepository(_jsonPath);
        var loaded = await loadRepo.LoadAsync();

        Assert.Single(loaded);
        Assert.Equal(TaskStatus.Done, loaded[0].Status);
    }

    [Fact]
    public async Task JsonRepository_SaveAsync_SpecialCharacters_MaintainsEncoding()
    {
        using var repo = new JsonTaskRepository(_jsonPath);
        string weirdTitle = "Завдання \n з \t символами 🇺🇦 !@#$%^&*()";
        await repo.SaveAsync(new List<UserTask> { new UserTask { Title = weirdTitle } });

        using var loadRepo = new JsonTaskRepository(_jsonPath);
        var loaded = await loadRepo.LoadAsync();

        Assert.Single(loaded);
        Assert.Equal(weirdTitle, loaded[0].Title);
    }

    [Fact]
    public async Task JsonRepository_LoadAsync_BothFilesCorrupted_ThrowsException()
    {
        await File.WriteAllTextAsync(_jsonPath, "{ broken JSON...");
        await File.WriteAllTextAsync(_csvPath, "Title,Status\nBroken,CSV,Data,That,Fails");

        using var repo = new JsonTaskRepository(_jsonPath);
        
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await repo.LoadAsync());
    }
}