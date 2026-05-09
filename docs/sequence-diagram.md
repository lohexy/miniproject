#Діаграма послідовності

```mermaid
sequenceDiagram
    participant U as Користувач
    participant M as ConsoleMenu
    participant S as TaskService
    participant F as TaskFactory
    participant R as JsonTaskRepository
    participant FS as File System

    U->>M: Вибір "Додати задачу"
    M->>U: Запит назви
    U->>M: Вводить назву
    M->>S: ValidateTitle(title)
    
    alt Назва вже існує
        S-->>M: throw DuplicateTaskTitleException
        M->>U: Вивід помилки (спробуйте ще раз)
    else Назва унікальна
        M->>S: AddTask(title, days, priority)
        S->>F: Create(title, days, priority)
        F-->>S: Об'єкт UserTask
        S->>S: Додавання в Project
        S-->>M: Результат (Success)
    end
    
    Note over M,FS: При виході з програми (пункт 0)
    M->>R: SaveAsync(tasks)
    R->>FS: Запис у tasks.json (та бекап tasks.csv)