namespace Domain;

/// <summary>
/// Рівень пріоритету задачі.
/// </summary>
public enum TaskPriority { Low, Medium, High }

/// <summary>
/// Поточний стан виконання задачі.
/// </summary>
public enum TaskStatus { Todo, InProgress, Done }

/// <summary>
/// Представляє сутність задачі користувача в системі.
/// </summary>
public class UserTask
{
    /// <summary>
    /// Унікальний ідентифікатор задачі.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Назва або короткий опис задачі.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Дата та час дедлайну (якщо встановлено).
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Пріоритет виконання задачі.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// Поточний статус виконання задачі.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
}