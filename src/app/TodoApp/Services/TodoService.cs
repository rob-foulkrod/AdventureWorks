using TodoApp.Models;

namespace TodoApp.Services;

public class TodoService
{
    private readonly List<TodoItem> _todos = [];
    private int _nextId = 1;

    public IReadOnlyList<TodoItem> GetAll() => _todos.AsReadOnly();

    public IReadOnlyList<TodoItem> GetByFilter(FilterType filter) => filter switch
    {
        FilterType.Active => _todos.Where(t => !t.IsCompleted).ToList().AsReadOnly(),
        FilterType.Completed => _todos.Where(t => t.IsCompleted).ToList().AsReadOnly(),
        _ => _todos.AsReadOnly()
    };

    public TodoItem Add(string title, string? description = null, Priority priority = Priority.Medium)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var item = new TodoItem
        {
            Id = _nextId++,
            Title = title.Trim(),
            Description = description?.Trim(),
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };
        _todos.Add(item);
        return item;
    }

    public bool ToggleComplete(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item is null) return false;

        item.IsCompleted = !item.IsCompleted;
        item.CompletedAt = item.IsCompleted ? DateTime.UtcNow : null;
        return true;
    }

    public bool Delete(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item is null) return false;

        _todos.Remove(item);
        return true;
    }

    public bool Update(int id, string title, string? description, Priority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item is null) return false;

        item.Title = title.Trim();
        item.Description = description?.Trim();
        item.Priority = priority;
        return true;
    }

    public void ClearCompleted()
    {
        _todos.RemoveAll(t => t.IsCompleted);
    }

    public int CountActive() => _todos.Count(t => !t.IsCompleted);
    public int CountCompleted() => _todos.Count(t => t.IsCompleted);
    public int CountTotal() => _todos.Count;
}

public enum FilterType
{
    All,
    Active,
    Completed
}
