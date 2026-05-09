# Діаграма класів

```mermaid
classDiagram
    class Project {
        +String Name
        +List~UserTask~ Tasks
        +AddTask(UserTask task)
    }
    class UserTask {
        +Guid Id
        +String Title
        +TaskStatus Status
        +TaskPriority Priority
        +DateTime? DueDate
    }
    class TaskFactory {
        +Create(String title, int days, TaskPriority p) UserTask
    }
    class TaskService {
        -Project _project
        +AddTask(String title, int days, TaskPriority p) UserTask
        +ValidateTitle(String title)
        +TryChangeStatus(int index, TaskStatus status)
    }
    class IDataStore {
        <<interface>>
        +LoadAsync()
        +SaveAsync()
    }
    class JsonTaskRepository {
        -String _filePath
        +LoadAsync()
        +SaveAsync()
        -ImportFromCsvAsync()
    }
    class TaskDomainException {
        <<Exception>>
    }
    class DuplicateTaskTitleException {
        <<Exception>>
    }

    TaskService --> Project
    Project "1" *-- "*" UserTask
    TaskFactory ..> UserTask : creates
    JsonTaskRepository ..|> IDataStore
    TaskService ..> IDataStore : uses
    TaskDomainException <|-- DuplicateTaskTitleException
    TaskService ..> DuplicateTaskTitleException : throws