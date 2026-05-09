using Application;
using Domain;
using Infrastructure;
using ConsoleUI;

using var repository = new JsonTaskRepository("tasks.json");
var currentProject = new Project { Name = "Система управління задачами" };

currentProject.Tasks = await repository.LoadAsync();

var taskService = new TaskService(currentProject);

var menu = new ConsoleMenu(taskService, currentProject, repository);
await menu.RunAsync();