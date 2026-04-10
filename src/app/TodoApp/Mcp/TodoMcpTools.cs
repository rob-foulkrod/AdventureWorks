using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Mcp;

[McpServerToolType]
public class TodoMcpTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "list_todos"), Description("List all todos, optionally filtered by status (all, active, completed).")]
    public static string ListTodos(TodoService todoService, [Description("Filter: 'all', 'active', or 'completed'. Defaults to 'all'.")] string filter = "all")
    {
        var filterType = filter.ToLowerInvariant() switch
        {
            "active" => FilterType.Active,
            "completed" => FilterType.Completed,
            _ => FilterType.All
        };

        var todos = todoService.GetByFilter(filterType);
        if (todos.Count == 0)
            return "No todos found.";

        return JsonSerializer.Serialize(todos, JsonOptions);
    }

    [McpServerTool(Name = "add_todo"), Description("Add a new todo item.")]
    public static string AddTodo(
        TodoService todoService,
        [Description("Title of the todo item.")] string title,
        [Description("Optional description.")] string? description = null,
        [Description("Priority: 'low', 'medium', or 'high'. Defaults to 'medium'.")] string priority = "medium")
    {
        var p = priority.ToLowerInvariant() switch
        {
            "low" => Priority.Low,
            "high" => Priority.High,
            _ => Priority.Medium
        };

        var item = todoService.Add(title, description, p);
        return JsonSerializer.Serialize(item, JsonOptions);
    }

    [McpServerTool(Name = "remove_todo", Destructive = true), Description("Remove a todo item by its ID.")]
    public static string RemoveTodo(TodoService todoService, [Description("The ID of the todo to remove.")] int id)
    {
        var deleted = todoService.Delete(id);
        return deleted ? $"Todo {id} removed." : $"Todo {id} not found.";
    }

    [McpServerTool(Name = "toggle_todo"), Description("Toggle the completed status of a todo item by its ID.")]
    public static string ToggleTodo(TodoService todoService, [Description("The ID of the todo to toggle.")] int id)
    {
        var toggled = todoService.ToggleComplete(id);
        return toggled ? $"Todo {id} toggled." : $"Todo {id} not found.";
    }

    [McpServerTool(Name = "backup_todos", ReadOnly = true), Description("Export all todos as a JSON backup string.")]
    public static string BackupTodos(TodoService todoService)
    {
        var todos = todoService.GetAll();
        return JsonSerializer.Serialize(todos, JsonOptions);
    }

    [McpServerTool(Name = "restore_todos", Destructive = true), Description("Restore todos from a JSON backup string. Replaces all current todos.")]
    public static string RestoreTodos(TodoService todoService, [Description("JSON array of todo items to restore.")] string json)
    {
        List<TodoItem>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<TodoItem>>(json);
        }
        catch (JsonException ex)
        {
            return $"Invalid JSON: {ex.Message}";
        }

        if (items is null)
            return "Invalid JSON: deserialized to null.";

        // Clear existing todos and restore from backup
        foreach (var existing in todoService.GetAll().ToList())
        {
            todoService.Delete(existing.Id);
        }

        foreach (var item in items)
        {
            var added = todoService.Add(item.Title, item.Description, item.Priority);
            if (item.IsCompleted)
            {
                todoService.ToggleComplete(added.Id);
            }
        }

        return $"Restored {items.Count} todo(s).";
    }
}
