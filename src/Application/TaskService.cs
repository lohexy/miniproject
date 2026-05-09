using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using TaskStatus = Domain.TaskStatus;
using TaskFactory = Domain.TaskFactory;

namespace Application;

public class TaskService
{
    private readonly Project _project;

    public TaskService(Project project)
    {
        _project = project;
    }

    public UserTask AddTask(string title, int daysToDue, TaskPriority priority)
    {
    if (_project.Tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
    {
        throw new DuplicateTaskTitleException(title); 
    }

    var createdTask = Domain.TaskFactory.Create(title, daysToDue, priority);
    _project.AddTask(createdTask);
    
    return createdTask;
    }

    public void ValidateTitle(string title)
    {
    if (_project.Tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
    {
        throw new DuplicateTaskTitleException(title);
    }
    }

    public bool TryChangeStatus(int taskIndex, TaskStatus newStatus, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (taskIndex < 0 || taskIndex >= _project.Tasks.Count)
        {
            errorMessage = "Неправильний номер задачі.";
            return false;
        }

        var task = _project.Tasks[taskIndex];

        if (task.DueDate.HasValue && task.DueDate.Value < DateTime.Now && task.Status != TaskStatus.Done && newStatus == TaskStatus.Done)
        {
            errorMessage = "Не можна завершити прострочену задачу! Спочатку подовжте дедлайн.";
            return false;
        }

        task.Status = newStatus;
        return true;
    }

    public List<UserTask> GetFilteredTasks(string searchQuery, List<TaskStatus> selectedStatuses, bool onlyOverdue, string sortField, bool sortDesc)
    {
        var query = _project.Tasks
            .Where(t => t.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .Where(t => selectedStatuses.Contains(t.Status))
            .Where(t => !onlyOverdue || (t.DueDate.HasValue && t.DueDate.Value < DateTime.Now && t.Status != TaskStatus.Done));

        if (sortField == "Date")
            return sortDesc ? query.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue).ToList() 
                            : query.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ToList();
        
        return sortDesc ? query.OrderByDescending(t => t.Status).ToList() 
                        : query.OrderBy(t => t.Status).ToList();
    }

    public (
        int Total, 
        int Done, 
        double Progress, 
        IEnumerable<IGrouping<TaskPriority, UserTask>> PriorityGroup, 
        IEnumerable<IGrouping<TaskStatus, UserTask>> StatusGroup,
        List<UserTask> OverdueTasks,
        List<UserTask> ClosestDeadlines
    ) GetAnalytics()
    {
        int total = _project.Tasks.Count;
        int done = _project.Tasks.Count(t => t.Status == TaskStatus.Done);
        double progress = total > 0 ? (double)done / total * 100 : 0;
        
        var priorityGroup = _project.Tasks.GroupBy(t => t.Priority);
        var statusGroup = _project.Tasks.GroupBy(t => t.Status);
        
        var overdue = _project.Tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.Now && t.Status != TaskStatus.Done)
            .ToList();
            
        var closest = _project.Tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value >= DateTime.Now && t.Status != TaskStatus.Done)
            .OrderBy(t => t.DueDate)
            .Take(3)
            .ToList();

        return (total, done, progress, priorityGroup, statusGroup, overdue, closest);
    }
}