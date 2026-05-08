using Domain;
using Infrastructure;
using TaskStatus = Domain.TaskStatus;
using TaskFactory = Domain.TaskFactory;
using var repository = new InMemoryTaskRepository();

var currentProject = new Project { Name = "Система управління задачами" };

repository.OnTaskAdded += (task) => 
    Console.WriteLine($"[Логгер подій] Задачу '{task.Title}' успішно збережено в базі!");

bool isRunning = true;

var menuActions = new Dictionary<string, Action>
{
    { "1", AddNewTask },
    { "2", ShowAllTasks },
    { "3", ExitProgram }
};

while (isRunning)
{
    Console.WriteLine($"\nМеню: {currentProject.Name}");
    Console.WriteLine("1. Додати нову задачу");
    Console.WriteLine("2. Показати всі задачі");
    Console.WriteLine("3. Вийти");
    Console.Write("Ваш вибір: ");

    string choice = Console.ReadLine() ?? "";

    if (menuActions.TryGetValue(choice, out Action? action))
    {
        action.Invoke();
    }
    else
    {
        Console.WriteLine("Помилка: Невідома команда.");
    }
}


void AddNewTask()
{
    Console.Write("\nВведіть назву: ");
    string title = Console.ReadLine() ?? "Без назви";

    int days;
    while (true)
    {
        Console.Write("Через скільки днів дедлайн? (введіть додатне число): ");
        string? input = Console.ReadLine();
        
        if (int.TryParse(input, out days) && days >= 0)
        {
            break;
        }
        Console.WriteLine("Помилка! Введіть коректне додатне число (без літер і мінусів).");
    }

    TaskPriority priority;
    while (true)
    {
        Console.Write("Пріоритет (0-Low, 1-Medium, 2-High): ");
        string? input = Console.ReadLine();

        if (Enum.TryParse(input, out priority) && Enum.IsDefined(typeof(TaskPriority), priority))
        {
            break;
        }
        Console.WriteLine("Помилка! Введіть тільки цифру 0, 1 або 2.");
    }

    var task = TaskFactory.Create(title, days, priority);
    
    currentProject.AddTask(task);
    repository.Add(task); 
}

void ShowAllTasks()
{
    var tasks = repository.GetAll();
    if (tasks.Count == 0)
    {
        Console.WriteLine("\nЗадач поки немає.");
        return;
    }

    Console.WriteLine("\nСписок задач");
    for (int i = 0; i < currentProject.Tasks.Count; i++)
    {
        var t = currentProject[i];
        Console.WriteLine($"[{i + 1}] {t.Title} | Дедлайн: {t.DueDate:dd.MM.yyyy} | Пріоритет: {t.Priority}");
    }
}

void ExitProgram()
{
    isRunning = false;
    Console.WriteLine("Завершення роботи...");
}