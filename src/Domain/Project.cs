namespace Domain;

/// <summary>
/// Представляє проєкт, який містить колекцію задач.
/// </summary>
public class Project
{
    /// <summary>
    /// Унікальний ідентифікатор проєкту.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Назва проєкту.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Список задач, що належать цьому проєкту.
    /// </summary>
    public List<UserTask> Tasks { get; set; } = new();

    /// <summary>
    /// Додає нову задачу до списку проєкту.
    /// </summary>
    /// <param name="task">Об'єкт задачі для додавання.</param>
    public void AddTask(UserTask task) => Tasks.Add(task);

    /// <summary>
    /// Індексатор для швидкого доступу до задач за їхнім порядковим номером.
    /// </summary>
    public UserTask this[int index]
    {
        get => Tasks[index];
        set => Tasks[index] = value;
    }
}