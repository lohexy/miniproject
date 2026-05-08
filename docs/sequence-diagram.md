#Діаграма послідовності

```mermaid
sequenceDiagram
    actor User
    participant UI as ConsoleUI
    participant App as TaskService (Application)
    participant Domain as Task (Domain)
    participant Repo as TaskRepository (Infrastructure)

    User->>UI: Вводить дані задачі (Title, DueDate)
    UI->>App: CreateTask(title, dueDate)
    App->>Domain: new Task(title, dueDate)
    Domain-->>App: task Object
    App->>Repo: Save(task)
    Repo-->>App: Success/Confirmation
    App-->>UI: Show success message
    UI-->>User: "Задача успішно створена"