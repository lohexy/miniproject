namespace Domain;

public enum TaskPriority { Low, Medium, High }
public enum TaskStatus { Todo, InProgress, Done }

public class UserTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
}