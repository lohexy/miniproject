using System;

namespace Domain;

public class TaskDomainException : Exception
{
    public TaskDomainException(string message) : base(message) { }
}

public class DuplicateTaskTitleException : TaskDomainException
{
    public DuplicateTaskTitleException(string title) 
        : base($"Задача з назвою '{title}' вже існує!") { }
}