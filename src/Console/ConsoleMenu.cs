using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application;
using Domain;
using Infrastructure;
using TaskStatus = Domain.TaskStatus;

namespace ConsoleUI;

public class ConsoleMenu
{
    private readonly TaskService _taskService;
    private readonly Project _currentProject;
    private readonly JsonTaskRepository _repository;
    private bool _isRunning = true;

    public ConsoleMenu(TaskService taskService, Project project, JsonTaskRepository repository)
    {
        _taskService = taskService;
        _currentProject = project;
        _repository = repository;
    }

    public async Task RunAsync()
    {
        var menuActions = new Dictionary<string, Action>
        {
            { "1", AddNewTask },
            { "2", ShowAllTasks },
            { "3", ChangeTaskStatus },
            { "4", ShowAnalytics },
            { "5", ExitProgram }
        };

        while (_isRunning)
        {
            Console.Clear();
            Console.WriteLine($"\n Меню: {_currentProject.Name} ");
            Console.WriteLine("1. Додати нову задачу");
            Console.WriteLine("2. Показати всі задачі (Фільтри, Сортування, Пошук)");
            Console.WriteLine("3. Змінити статус задачі");
            Console.WriteLine("4. Аналітика (Дашборд)");
            Console.WriteLine("5. Вийти та Зберегти");
            Console.Write("Ваш вибір: ");

            string choice = Console.ReadLine() ?? "";

            if (menuActions.TryGetValue(choice, out Action? action))
            {
                action.Invoke();
            }
            else
            {
                Console.WriteLine("Помилка: Невідома команда.");
                WaitForKey();
            }
        }

        await _repository.SaveAsync(_currentProject.Tasks);
        Console.WriteLine("Програму безпечно закрито.");
    }

    private void AddNewTask()
    {
        string title = "";
    while (true)
    {
    Console.Write("Введіть назву задачі: ");
    title = Console.ReadLine() ?? "";
    
    try 
    {
        _taskService.ValidateTitle(title);
        break;
    }
    catch (DuplicateTaskTitleException ex)
    {
        Console.WriteLine($"[Error] {ex.Message} Спробуйте ще раз.\n");
    }
    }

        int days;
        while (true)
        {
            Console.Write("Через скільки днів дедлайн? (0 - без дедлайну): ");
            if (int.TryParse(Console.ReadLine(), out days) && days >= 0) break;
            Console.WriteLine("Помилка! Введіть коректне додатне число.");
        }

        TaskPriority priority;
        while (true)
        {
            Console.Write("Пріоритет (0-Low, 1-Medium, 2-High): ");
            if (Enum.TryParse(Console.ReadLine(), out priority) && Enum.IsDefined(typeof(TaskPriority), priority)) break;
            Console.WriteLine("Помилка! Введіть тільки цифру 0, 1 або 2.");
        }

        try 
        {
        _taskService.AddTask(title, days, priority);
        Console.WriteLine("\n[Success] Задачу додано в список. Вона буде збережена у файл при виході.");
        }
        catch (TaskDomainException ex)
        {
            Console.WriteLine($"\n[Помилка Бізнес-Логіки] {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[Критична Помилка] Щось пішло не так: {ex.Message}");
        }

        WaitForKey();
    }

    private void ShowAllTasks()
    {
        string searchQuery = "";
        List<TaskStatus> selectedStatuses = Enum.GetValues<TaskStatus>().ToList();
        bool onlyOverdue = false;
        string sortField = "Date";
        bool sortDescending = false;
        int currentPage = 1;
        const int pageSize = 10;

        bool inViewMenu = true;
        while (inViewMenu)
        {
            var filteredTasks = _taskService.GetFilteredTasks(searchQuery, selectedStatuses, onlyOverdue, sortField, sortDescending);

            int totalTasks = filteredTasks.Count;
            int totalPages = (int)Math.Ceiling(totalTasks / (double)pageSize);
            if (currentPage > totalPages && totalPages > 0) currentPage = totalPages;
            if (currentPage < 1) currentPage = 1;

            var pagedTasks = filteredTasks.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            Console.Clear();
            Console.WriteLine($"ПЕРЕГЛЯД ЗАДАЧ (Сторінка {currentPage} з {Math.Max(1, totalPages)})");
            Console.WriteLine($"Фільтри: Пошук: '{(searchQuery == "" ? "всі" : searchQuery)}', Статусів: {selectedStatuses.Count}, Прострочені: {(onlyOverdue ? "[x]" : "[ ]")}");
            Console.WriteLine("----------------------------------------------------------------------");

            if (!pagedTasks.Any())
            {
                Console.WriteLine("   Задач не знайдено за такими критеріями.");
            }
            else
            {
                for (int i = 0; i < pagedTasks.Count; i++)
                {
                    var t = pagedTasks[i];
                    string dateStr = t.DueDate.HasValue ? t.DueDate.Value.ToString("dd.MM.yyyy") : "-";
                    int displayIndex = ((currentPage - 1) * pageSize) + i + 1;
                    Console.WriteLine($"{displayIndex}. [{t.Status}] {t.Title} | Дедлайн: {dateStr} | Пр: {t.Priority}");
                }
            }

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("1. Пошук за назвою       2. Скинути все (Показати всі)");
            Console.WriteLine("3. Фільтри (Підменю)     4. Сортування (Підменю)");
            Console.WriteLine("5. Головне меню          6. Попер. сторінка   7. Наст. сторінка");
            Console.Write("\nВибір: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    Console.Write("Введіть назву для пошуку: ");
                    searchQuery = Console.ReadLine() ?? "";
                    currentPage = 1;
                    break;
                case "2":
                    searchQuery = "";
                    selectedStatuses = Enum.GetValues<TaskStatus>().ToList();
                    onlyOverdue = false;
                    currentPage = 1;
                    break;
                case "3":
                    ShowFilterSubmenu(ref selectedStatuses, ref onlyOverdue);
                    currentPage = 1;
                    break;
                case "4":
                    ShowSortSubmenu(ref sortField, ref sortDescending);
                    break;
                case "5":
                    inViewMenu = false;
                    break;
                case "6":
                    if (currentPage > 1) currentPage--;
                    break;
                case "7":
                    if (currentPage < totalPages) currentPage++;
                    break;
            }
        }
    }

    private void ChangeTaskStatus()
    {
        if (_currentProject.Tasks.Count == 0)
        {
            Console.WriteLine("\nНемає задач для зміни статусу.");
            WaitForKey();
            return;
        }

        int currentPage = 1;
        int pageSize = 5;

        while (true)
        {
            int totalTasks = _currentProject.Tasks.Count;
            int totalPages = (int)Math.Ceiling(totalTasks / (double)pageSize);
            
            if (currentPage > totalPages) currentPage = totalPages;

            var pageTasks = _currentProject.Tasks
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Console.WriteLine($"\n Зміна статусу задачі (Сторінка {currentPage} з {totalPages}) ");
            for (int i = 0; i < pageTasks.Count; i++)
            {
                var t = pageTasks[i];
                Console.WriteLine($"[{i + 1}] {t.Title} (Поточний статус: {t.Status})");
            }

            Console.WriteLine("-----------------------");
            Console.WriteLine("[6] Попередня сторінка");
            Console.WriteLine("[7] Наступна сторінка");
            Console.WriteLine("[0] Назад до головного меню");
            Console.Write("\nВиберіть номер задачі (1-5) або команду: ");

            string input = Console.ReadLine() ?? "";

            if (input == "0") 
            {
                return;
            }
            else if (input == "6")
            {
                if (currentPage > 1) currentPage--;
                else { Console.WriteLine("[!] Це вже перша сторінка."); WaitForKey(); }
            }
            else if (input == "7")
            {
                if (currentPage < totalPages) currentPage++;
                else { Console.WriteLine("[!] Це остання сторінка."); WaitForKey(); }
            }
            else if (int.TryParse(input, out int localIndex) && localIndex >= 1 && localIndex <= pageTasks.Count)
            {
                int globalIndex = (currentPage - 1) * pageSize + (localIndex - 1);

                Console.Write("Введіть новий статус (0-Todo, 1-InProgress, 2-Done): ");
                if (Enum.TryParse(Console.ReadLine(), out TaskStatus newStatus) && Enum.IsDefined(typeof(TaskStatus), newStatus))
                {
                    if (_taskService.TryChangeStatus(globalIndex, newStatus, out string error))
                    {
                        Console.WriteLine($"\n[Успіх] Статус задачі змінено на {newStatus}.");
                    }
                    else
                    {
                        Console.WriteLine($"\n[Помилка Бізнес-логіки] {error}");
                    }
                }
                else
                {
                    Console.WriteLine("Помилка вводу статусу.");
                }
                WaitForKey();
            }
            else
            {
                Console.WriteLine("Помилка вводу або невідома команда.");
                WaitForKey();
            }
        }
    }

    private void ShowAnalytics()
    {
        if (_currentProject.Tasks.Count == 0)
        {
            Console.WriteLine("\nНемає даних для аналітики.");
            WaitForKey();
            return;
        }

        Console.WriteLine("\n АНАЛІТИКА ТА ДАШБОРД (LINQ) ");
        
        var stats = _taskService.GetAnalytics();

        Console.WriteLine($"\n1. Загальний прогрес: {Math.Round(stats.Progress, 1)}% ({stats.Done} з {stats.Total} задач виконано)");
     
        Console.WriteLine("\n2. Розподіл за статусами:");
        foreach (var group in stats.StatusGroup)
        {
            Console.WriteLine($"   - {group.Key}: {group.Count()} шт.");
        }

        Console.WriteLine("\n3. Статистика за пріоритетами:");
        foreach (var group in stats.PriorityGroup)
        {
            Console.WriteLine($"   - {group.Key}: {group.Count()} шт.");
        }

        Console.WriteLine("\n4. Топ-3 найближчих дедлайнів (активні):");
        if (stats.ClosestDeadlines.Any())
        {
            foreach (var t in stats.ClosestDeadlines)
                Console.WriteLine($"   - {t.Title} (до {t.DueDate!.Value:dd.MM.yyyy})");
        }
        else Console.WriteLine("   - Немає майбутніх дедлайнів.");

        Console.WriteLine($"\n5. Прострочені задачі ({stats.OverdueTasks.Count}):");
        if (stats.OverdueTasks.Any())
        {
            foreach (var t in stats.OverdueTasks)
                Console.WriteLine($"   - [УВАГА] {t.Title} (дедлайн був {t.DueDate!.Value:dd.MM.yyyy})");
        }
        else Console.WriteLine("   - Все йде за планом! Прострочених немає.");

        WaitForKey();
    }

    private void ExitProgram()
    {
        _isRunning = false;
        Console.WriteLine("\nПідготовка до збереження...");
    }

    private void ShowFilterSubmenu(ref List<TaskStatus> selectedStatuses, ref bool onlyOverdue)
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            Console.WriteLine(" НАЛАШТУВАННЯ ФІЛЬТРІВ ");
            Console.WriteLine($"1. {(selectedStatuses.Contains(TaskStatus.Todo) ? "[x]" : "[ ]")} Todo");
            Console.WriteLine($"2. {(selectedStatuses.Contains(TaskStatus.InProgress) ? "[x]" : "[ ]")} In Progress");
            Console.WriteLine($"3. {(selectedStatuses.Contains(TaskStatus.Done) ? "[x]" : "[ ]")} Done");
            Console.WriteLine($"4. {(onlyOverdue ? "[x]" : "[ ]")} Тільки прострочені");
            Console.WriteLine("5. Назад");
            Console.Write("\nВибір: ");

            string c = Console.ReadLine() ?? "";
            if (c == "1") ToggleStatus(selectedStatuses, TaskStatus.Todo);
            else if (c == "2") ToggleStatus(selectedStatuses, TaskStatus.InProgress);
            else if (c == "3") ToggleStatus(selectedStatuses, TaskStatus.Done);
            else if (c == "4") onlyOverdue = !onlyOverdue;
            else if (c == "5") back = true;
        }
    }

    private void ToggleStatus(List<TaskStatus> list, TaskStatus status)
    {
        if (list.Contains(status)) list.Remove(status);
        else list.Add(status);
    }

    private void ShowSortSubmenu(ref string field, ref bool desc)
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            Console.WriteLine(" НАЛАШТУВАННЯ СОРТУВАННЯ ");
            Console.WriteLine($"1. {(field == "Date" ? "[x]" : "[ ]")} За датою");
            Console.WriteLine($"2. {(field == "Status" ? "[x]" : "[ ]")} За статусом");
            Console.WriteLine($"3. {(desc ? "[x]" : "[ ]")} За спаданням (інакше - зростання)");
            Console.WriteLine("4. Назад");
            Console.Write("\nВибір: ");

            string c = Console.ReadLine() ?? "";
            if (c == "1") field = "Date";
            else if (c == "2") field = "Status";
            else if (c == "3") desc = !desc;
            else if (c == "4") back = true;
        }
    }

    private void WaitForKey()
    {
        Console.WriteLine("\n[Натисніть будь-яку клавішу для повернення...]");
        Console.ReadKey();
    }
}