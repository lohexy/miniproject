using System.Text.Json;
using System.Text;
using Domain;

namespace Infrastructure;

public class JsonTaskRepository : IDataStore<UserTask>, IDisposable
{
    private readonly string _filePath;
    private List<UserTask> _tasks = new();
    
    public event Action<UserTask>? OnTaskAdded;

    public JsonTaskRepository(string filePath = "tasks.json")
    {
        _filePath = filePath;
    }

public async Task<List<UserTask>> LoadAsync()
{
    try
    {
        if (File.Exists("tasks.json"))
        {
            string json = await File.ReadAllTextAsync("tasks.json");
            var tasks = JsonSerializer.Deserialize<List<UserTask>>(json);
            if (tasks != null) return tasks;
        }
    }
    catch (JsonException)
    {
        Console.WriteLine("\n[УВАГА] Файл tasks.json пошкоджено! Спроба відновлення даних з tasks.csv...");
        return await ImportFromCsvAsync();
    }

    return new List<UserTask>();
}

public async Task SaveAsync(List<UserTask> tasks)
{
    var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync("tasks.json", json);

    var csv = new StringBuilder();
    csv.AppendLine("Title,Status,Priority,DueDate");
    foreach (var task in tasks)
    {
        csv.AppendLine($"{task.Title},{task.Status},{task.Priority},{task.DueDate}");
    }
    await File.WriteAllTextAsync("tasks.csv", csv.ToString());
}

private async Task<List<UserTask>> ImportFromCsvAsync()
{
    var tasks = new List<UserTask>();
    if (!File.Exists("tasks.csv")) return tasks;

    var lines = await File.ReadAllLinesAsync("tasks.csv");
    for (int i = 1; i < lines.Length; i++)
    {
        var parts = lines[i].Split(',');
        if (parts.Length >= 4)
    {
    var task = new UserTask 
    { 
        Title = parts[0],
        Status = Enum.Parse<Domain.TaskStatus>(parts[1]),
        Priority = Enum.Parse<TaskPriority>(parts[2]),
        DueDate = string.IsNullOrEmpty(parts[3]) ? null : DateTime.Parse(parts[3])
    };
    tasks.Add(task);
    }
    }
    Console.WriteLine("[Успіх] Дані частково відновлено з CSV-бекапу!");
    return tasks;
}

    public void Add(UserTask task)
    {
        _tasks.Add(task);
        OnTaskAdded?.Invoke(task);
    }

    public List<UserTask> GetAll() => _tasks;

    public void Dispose()
    {
        _tasks.Clear();
    }
}