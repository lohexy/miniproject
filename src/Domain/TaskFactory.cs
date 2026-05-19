namespace Domain;

/// <summary>
/// Фабрика для інкапсуляції логіки створення нових задач.
/// Забезпечує правильну ініціалізацію значень за замовчуванням та розрахунок дедлайнів.
/// </summary>
public static class TaskFactory
{
    /// <summary>
    /// Створює новий екземпляр задачі.
    /// </summary>
    /// <param name="title">Назва задачі (якщо порожня - буде автоматично встановлено "Без назви").</param>
    /// <param name="daysToDue">Кількість днів до дедлайну (якщо 0 - дедлайн відсутній).</param>
    /// <param name="priority">Пріоритет задачі.</param>
    /// <returns>Новий об'єкт <see cref="UserTask"/> готовий до збереження.</returns>
    public static UserTask Create(string title, int daysToDue, TaskPriority priority)
    {
        return new UserTask
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Без назви" : title,
            DueDate = daysToDue == 0 ? null : DateTime.Now.AddDays(daysToDue),
            Priority = priority,
            Status = TaskStatus.Todo
        };
    }
}