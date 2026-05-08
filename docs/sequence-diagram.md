#Діаграма послідовності

```mermaid
sequenceDiagram
    actor User as Користувач
    participant Console as UI (Program.cs)
    participant Factory as TaskFactory
    participant Project as Project
    participant Repo as InMemoryTaskRepository

    User->>Console: Вибирає "1" (Додати задачу)
    Console->>User: Запитує дані (Назва, Дедлайн, Пріоритет)
    User->>Console: Вводить валідні дані
    
    Note over Console, Factory: Використання патерну Factory
    Console->>Factory: Create(title, days, priority)
    Factory-->>Console: повертає UserTask
    
    Console->>Project: AddTask(UserTask)
    
    Console->>Repo: Add(UserTask)
    Note over Repo: Підготовка до Observer
    Repo-->>Console: Тригерить подію OnTaskAdded
    Console-->>User: Виводить лог: "Задачу успішно збережено"