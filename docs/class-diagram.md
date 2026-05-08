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
        +TryAddTask(String title, int days, TaskPriority p) bool
        +GetAnalytics() Stats
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
    }

    TaskService --> Project
    Project "1" *-- "*" UserTask
    TaskFactory ..> UserTask : creates
    JsonTaskRepository ..|> IDataStore
    TaskService ..> IDataStore : uses