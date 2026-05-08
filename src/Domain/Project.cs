namespace Domain;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<UserTask> Tasks { get; set; } = new();

    public void AddTask(UserTask task) => Tasks.Add(task);

    public UserTask this[int index]
    {
        get => Tasks[index];
        set => Tasks[index] = value;
    }
}