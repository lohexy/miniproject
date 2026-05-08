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
    M->>U: Запит даних (Назва, дні, пріоритет)
    U->>M: Вводить дані
    M->>S: TryAddTask(title, days, priority)
    S->>F: Create(title, days, priority)
    F-->>S: Об'єкт UserTask
    S->>S: Валідація та додавання в Project
    S-->>M: Результат (Success)
    
    Note over M,FS: При виході з програми (пункт 5)
    M->>R: SaveAsync(tasks)
    R->>FS: Запис у tasks.json
    FS-->>R: Підтвердження
    R-->>M: Успішно збережено