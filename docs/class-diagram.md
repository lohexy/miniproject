# Діаграма класів

```mermaid
classDiagram
    class UserTask {
        +Guid Id
        +string Title
        +DateTime DueDate
        +TaskPriority Priority
        +TaskStatus Status
    }

    class Project {
        +Guid Id
        +string Name
        +List~UserTask~ Tasks
        +AddTask(UserTask task)
        +this[int index] UserTask
    }

    class User {
        +Guid Id
        +string Name
        +string Email
    }
    
    class Category {
        +Guid Id
        +string Name
    }

    class TaskFactory {
        <<static>>
        +Create(string title, int days, TaskPriority priority) UserTask
    }

    class InMemoryTaskRepository {
        -List~UserTask~ _tasks
        +event Action~UserTask~ OnTaskAdded
        +Add(UserTask task)
        +GetAll() List~UserTask~
        +Dispose()
    }

    Project "1" *-- "many" UserTask : містить
    UserTask "many" --> "1" User : призначена
    TaskFactory ..> UserTask : створює
    InMemoryTaskRepository o-- UserTask : зберігає