# Діаграма класів

```mermaid
classDiagram
    class Task {
        +Guid Id
        +string Title
        +string Description
        +DateTime DueDate
        +Priority Priority
        +TaskStatus Status
        +ChangeStatus(TaskStatus newStatus)
    }

    class Project {
        +Guid Id
        +string Name
        +List~Task~ Tasks
        +AddTask(Task task)
        +GetProgress() double
    }

    class User {
        +Guid Id
        +string Name
        +string Email
    }

    class Priority {
        <<enumeration>>
        Low
        Medium
        High
    }

    class TaskStatus {
        <<enumeration>>
        Todo
        InProgress
        Done
    }

    Project "1" *-- "many" Task : містить
    Task "many" --> "1" User : призначена
    Task --> Priority : має
    Task --> TaskStatus : має