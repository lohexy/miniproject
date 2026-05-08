using Domain;

namespace Infrastructure;

public class InMemoryTaskRepository : IDisposable
{
    private readonly List<UserTask> _tasks = new();

    public event Action<UserTask>? OnTaskAdded;

    public void Add(UserTask task)
    {
        _tasks.Add(task);
        OnTaskAdded?.Invoke(task);
    }

    public List<UserTask> GetAll() => _tasks;

    public void Dispose()
    {
        _tasks.Clear();
        Console.WriteLine("\n[Система] Ресурси репозиторію успішно очищено (IDisposable).");
    }
}