using System.Text.Json;
using TodoApp.Mcp;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Tests;

public class TodoMcpToolsTests
{
    private static TodoService CreateService() => new TodoService();

    [Fact]
    public void ListTodos_EmptyService_ReturnsNoTodosMessage()
    {
        var service = CreateService();
        var result = TodoMcpTools.ListTodos(service);
        Assert.Equal("No todos found.", result);
    }

    [Fact]
    public void ListTodos_WithItems_ReturnsJson()
    {
        var service = CreateService();
        service.Add("Task 1");
        service.Add("Task 2");

        var result = TodoMcpTools.ListTodos(service);
        var items = JsonSerializer.Deserialize<List<TodoItem>>(result);

        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("completed")]
    [InlineData("all")]
    public void ListTodos_WithFilter_ReturnsFilteredResults(string filter)
    {
        var service = CreateService();
        service.Add("Active task");
        var done = service.Add("Done task");
        service.ToggleComplete(done.Id);

        var result = TodoMcpTools.ListTodos(service, filter);
        var items = JsonSerializer.Deserialize<List<TodoItem>>(result);

        Assert.NotNull(items);
        if (filter == "active" || filter == "completed")
            Assert.Single(items);
        else
            Assert.Equal(2, items.Count);
    }

    [Fact]
    public void AddTodo_ValidTitle_ReturnsJsonWithItem()
    {
        var service = CreateService();
        var result = TodoMcpTools.AddTodo(service, "New task");
        var item = JsonSerializer.Deserialize<TodoItem>(result);

        Assert.NotNull(item);
        Assert.Equal("New task", item.Title);
        Assert.Equal(Priority.Medium, item.Priority);
    }

    [Fact]
    public void AddTodo_WithPriority_SetsPriority()
    {
        var service = CreateService();
        var result = TodoMcpTools.AddTodo(service, "Urgent task", priority: "high");
        var item = JsonSerializer.Deserialize<TodoItem>(result);

        Assert.NotNull(item);
        Assert.Equal(Priority.High, item.Priority);
    }

    [Fact]
    public void AddTodo_WithDescription_SetsDescription()
    {
        var service = CreateService();
        var result = TodoMcpTools.AddTodo(service, "Task", description: "Details here");
        var item = JsonSerializer.Deserialize<TodoItem>(result);

        Assert.NotNull(item);
        Assert.Equal("Details here", item.Description);
    }

    [Fact]
    public void RemoveTodo_ExistingItem_ReturnsRemovedMessage()
    {
        var service = CreateService();
        var item = service.Add("Task to delete");

        var result = TodoMcpTools.RemoveTodo(service, item.Id);

        Assert.Equal($"Todo {item.Id} removed.", result);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void RemoveTodo_NonExistent_ReturnsNotFoundMessage()
    {
        var service = CreateService();
        var result = TodoMcpTools.RemoveTodo(service, 999);
        Assert.Equal("Todo 999 not found.", result);
    }

    [Fact]
    public void ToggleTodo_ExistingItem_ReturnsToggledMessage()
    {
        var service = CreateService();
        var item = service.Add("Toggle me");

        var result = TodoMcpTools.ToggleTodo(service, item.Id);

        Assert.Equal($"Todo {item.Id} toggled.", result);
        Assert.True(item.IsCompleted);
    }

    [Fact]
    public void ToggleTodo_NonExistent_ReturnsNotFoundMessage()
    {
        var service = CreateService();
        var result = TodoMcpTools.ToggleTodo(service, 999);
        Assert.Equal("Todo 999 not found.", result);
    }

    [Fact]
    public void BackupTodos_ReturnsJsonArray()
    {
        var service = CreateService();
        service.Add("Task A");
        service.Add("Task B");

        var result = TodoMcpTools.BackupTodos(service);
        var items = JsonSerializer.Deserialize<List<TodoItem>>(result);

        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void BackupTodos_EmptyService_ReturnsEmptyArray()
    {
        var service = CreateService();
        var result = TodoMcpTools.BackupTodos(service);
        var items = JsonSerializer.Deserialize<List<TodoItem>>(result);

        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void RestoreTodos_ValidJson_RestoresItems()
    {
        var service = CreateService();
        service.Add("Existing task");

        var backup = JsonSerializer.Serialize(new[]
        {
            new TodoItem { Id = 10, Title = "Restored 1", Priority = Priority.High },
            new TodoItem { Id = 20, Title = "Restored 2", Priority = Priority.Low }
        });

        var result = TodoMcpTools.RestoreTodos(service, backup);

        Assert.Equal("Restored 2 todo(s).", result);
        Assert.Equal(2, service.CountTotal());
    }

    [Fact]
    public void RestoreTodos_InvalidJson_ReturnsErrorMessage()
    {
        var service = CreateService();
        var result = TodoMcpTools.RestoreTodos(service, "not valid json");
        Assert.StartsWith("Invalid JSON:", result);
    }

    [Fact]
    public void RestoreTodos_ClearsExistingTodos()
    {
        var service = CreateService();
        service.Add("Old task 1");
        service.Add("Old task 2");

        var backup = JsonSerializer.Serialize(new[]
        {
            new TodoItem { Title = "New task" }
        });

        TodoMcpTools.RestoreTodos(service, backup);

        var all = service.GetAll();
        Assert.Single(all);
        Assert.Equal("New task", all[0].Title);
    }
}
