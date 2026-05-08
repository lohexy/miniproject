using System.Text.Json;
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
        if (!File.Exists(_filePath))
        {
            Console.WriteLine("[System] Файл збереження не знайдено. Буде створено нову базу.");
            return _tasks;
        }

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            _tasks = JsonSerializer.Deserialize<List<UserTask>>(json) ?? new List<UserTask>();
            Console.WriteLine($"[System] Успішно завантажено {_tasks.Count} задач із файлу.");
        }
        catch (JsonException)
        {
            Console.WriteLine("[Error] Файл збереження пошкоджено! Створюємо чистий список.");
            _tasks = new List<UserTask>();
        }
        
        return _tasks;
    }

    public async Task SaveAsync(List<UserTask> items)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(items, options);
        
        await File.WriteAllTextAsync(_filePath, json);
        Console.WriteLine("[System] Дані успішно збережено у файл tasks.json.");
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