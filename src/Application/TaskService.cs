using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using TaskStatus = Domain.TaskStatus;
using TaskFactory = Domain.TaskFactory;

namespace Application;

/// <summary>
/// Сервіс для управління задачами проєкту. 
/// Містить основну бізнес-логіку: валідацію, зміну статусів, фільтрацію та аналітику.
/// </summary>
public class TaskService
{
    private readonly Project _project;

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="TaskService"/> для заданого проєкту.
    /// </summary>
    /// <param name="project">Об'єкт проєкту, задачами якого керує сервіс.</param>
    public TaskService(Project project)
    {
        _project = project;
    }

    /// <summary>
    /// Додає нову задачу до проєкту після проходження валідації назви.
    /// </summary>
    /// <param name="title">Назва задачі.</param>
    /// <param name="daysToDue">Кількість днів від поточного моменту до дедлайну.</param>
    /// <param name="priority">Пріоритет задачі (Low, Medium, High).</param>
    /// <returns>Створений об'єкт задачі <see cref="UserTask"/>.</returns>
    /// <exception cref="DuplicateTaskTitleException">Викидається, якщо задача з такою назвою вже існує в проєкті (без урахування регістру).</exception>
    public UserTask AddTask(string title, int daysToDue, TaskPriority priority)
    {
        if (_project.Tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateTaskTitleException(title); 
        }

        var createdTask = Domain.TaskFactory.Create(title, daysToDue, priority);
        _project.AddTask(createdTask);
        
        return createdTask;
    }

    /// <summary>
    /// Перевіряє унікальність назви задачі в межах поточного проєкту.
    /// </summary>
    /// <param name="title">Назва задачі для перевірки.</param>
    /// <exception cref="DuplicateTaskTitleException">Викидається, якщо назва вже зайнята іншою задачею.</exception>
    public void ValidateTitle(string title)
    {
        if (_project.Tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateTaskTitleException(title);
        }
    }

    /// <summary>
    /// Спробувати змінити статус задачі за її індексом у списку.
    /// Містить бізнес-правило, яке забороняє завершувати прострочені задачі без подовження дедлайну.
    /// </summary>
    /// <param name="taskIndex">Порядковий номер (индекс) задачі у списку проєкту.</param>
    /// <param name="newStatus">Новий статус, який потрібно встановити.</param>
    /// <param name="errorMessage">Вихідний параметр, що містить текст помилки у разі невдачі.</param>
    /// <returns>Повертає <c>true</c>, якщо статус успішно змінено; інакше — <c>false</c>.</returns>
    public bool TryChangeStatus(int taskIndex, TaskStatus newStatus, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (taskIndex < 0 || taskIndex >= _project.Tasks.Count)
        {
            errorMessage = "Неправильний номер задачі.";
            return false;
        }

        var task = _project.Tasks[taskIndex];

        if (task.DueDate.HasValue && task.DueDate.Value < DateTime.Now && task.Status != TaskStatus.Done && newStatus == TaskStatus.Done)
        {
            errorMessage = "Не можна завершити прострочену задачу! Спочатку подовжте дедлайн.";
            return false;
        }

        task.Status = newStatus;
        return true;
    }

    /// <summary>
    /// Фільтрує та сортує задачі проєкту за текстовим запитом, вибраними статусами та дедлайнами.
    /// </summary>
    /// <param name="searchQuery">Текст для пошуку в назвах задач (регістронезалежний).</param>
    /// <param name="selectedStatuses">Список статусів, задачі з якими мають потрапити в результат.</param>
    /// <param name="onlyOverdue">Якщо <c>true</c>, повертає лише прострочені невиконані задачі.</param>
    /// <param name="sortField">Поле для сортування ("Date" або будь-яке інше для сортування за статусом).</param>
    /// <param name="sortDesc">Якщо <c>true</c>, сортування виконується за спаданням, інакше — за зростанням.</param>
    /// <returns>Відфільтрований та відсортований список задач <see cref="List{UserTask}"/>.</returns>
    public List<UserTask> GetFilteredTasks(string searchQuery, List<TaskStatus> selectedStatuses, bool onlyOverdue, string sortField, bool sortDesc)
    {
        var query = _project.Tasks
            .Where(t => t.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .Where(t => selectedStatuses.Contains(t.Status))
            .Where(t => !onlyOverdue || (t.DueDate.HasValue && t.DueDate.Value < DateTime.Now && t.Status != TaskStatus.Done));

        if (sortField == "Date")
            return sortDesc ? query.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue).ToList() 
                            : query.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ToList();
        
        return sortDesc ? query.OrderByDescending(t => t.Status).ToList() 
                        : query.OrderBy(t => t.Status).ToList();
    }

    /// <summary>
    /// Розраховує аналітичні показники по задачах проєкту (загальна кількість, прогрес, групування, прострочені дедлайни).
    /// </summary>
    /// <returns>Кортеж із розрахованими метриками проєкту, групами задач та списками найближчих дедлайнів.</returns>
    public (
        int Total, 
        int Done, 
        double Progress, 
        IEnumerable<IGrouping<TaskPriority, UserTask>> PriorityGroup, 
        IEnumerable<IGrouping<TaskStatus, UserTask>> StatusGroup,
        List<UserTask> OverdueTasks,
        List<UserTask> ClosestDeadlines
    ) GetAnalytics()
    {
        int total = _project.Tasks.Count;
        int done = _project.Tasks.Count(t => t.Status == TaskStatus.Done);
        double progress = total > 0 ? (double)done / total * 100 : 0;
        
        var priorityGroup = _project.Tasks.GroupBy(t => t.Priority);
        var statusGroup = _project.Tasks.GroupBy(t => t.Status);
        
        var overdue = _project.Tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.Now && t.Status != TaskStatus.Done)
            .ToList();
            
        var closest = _project.Tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value >= DateTime.Now && t.Status != TaskStatus.Done)
            .OrderBy(t => t.DueDate)
            .Take(3)
            .ToList();

        return (total, done, progress, priorityGroup, statusGroup, overdue, closest);
    }
}