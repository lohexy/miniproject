namespace Domain;

public static class TaskFactory
{
    public static UserTask Create(string title, int daysToDue, TaskPriority priority)
    {
        return new UserTask
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Без назви" : title,
            DueDate = DateTime.Now.AddDays(daysToDue),
            Priority = priority,
            Status = TaskStatus.Todo
        };
    }
}