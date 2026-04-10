using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Tests;

public class TodoServiceTests
{
    private static TodoService CreateService() => new TodoService();

    [Fact]
    public void Add_ValidTitle_ReturnsTodoItem()
    {
        var service = CreateService();
        var item = service.Add("Buy groceries");

        Assert.NotNull(item);
        Assert.Equal("Buy groceries", item.Title);
        Assert.False(item.IsCompleted);
        Assert.Equal(Priority.Medium, item.Priority);
    }

    [Fact]
    public void Add_WithDescription_SetsDescription()
    {
        var service = CreateService();
        var item = service.Add("Study", "Chapter 5", Priority.High);

        Assert.Equal("Study", item.Title);
        Assert.Equal("Chapter 5", item.Description);
        Assert.Equal(Priority.High, item.Priority);
    }

    [Fact]
    public void Add_AssignsUniqueIncrementingIds()
    {
        var service = CreateService();
        var item1 = service.Add("Task 1");
        var item2 = service.Add("Task 2");
        var item3 = service.Add("Task 3");

        Assert.Equal(1, item1.Id);
        Assert.Equal(2, item2.Id);
        Assert.Equal(3, item3.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_EmptyOrWhitespaceTitle_ThrowsArgumentException(string title)
    {
        var service = CreateService();
        Assert.Throws<ArgumentException>(() => service.Add(title));
    }

    [Fact]
    public void Add_TrimsTitleAndDescription()
    {
        var service = CreateService();
        var item = service.Add("  Do homework  ", "  p. 42  ");

        Assert.Equal("Do homework", item.Title);
        Assert.Equal("p. 42", item.Description);
    }

    [Fact]
    public void GetAll_ReturnsAllItems()
    {
        var service = CreateService();
        service.Add("Task A");
        service.Add("Task B");

        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void GetAll_EmptyService_ReturnsEmptyList()
    {
        var service = CreateService();
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void ToggleComplete_ExistingItem_TogglesIsCompleted()
    {
        var service = CreateService();
        var item = service.Add("Finish project");

        bool result = service.ToggleComplete(item.Id);

        Assert.True(result);
        Assert.True(item.IsCompleted);
        Assert.NotNull(item.CompletedAt);
    }

    [Fact]
    public void ToggleComplete_CompletedItem_MarksAsActive()
    {
        var service = CreateService();
        var item = service.Add("Review notes");
        service.ToggleComplete(item.Id);

        service.ToggleComplete(item.Id);

        Assert.False(item.IsCompleted);
        Assert.Null(item.CompletedAt);
    }

    [Fact]
    public void ToggleComplete_NonExistentId_ReturnsFalse()
    {
        var service = CreateService();
        bool result = service.ToggleComplete(999);
        Assert.False(result);
    }

    [Fact]
    public void Delete_ExistingItem_RemovesIt()
    {
        var service = CreateService();
        var item = service.Add("Read book");

        bool result = service.Delete(item.Id);

        Assert.True(result);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Delete_NonExistentId_ReturnsFalse()
    {
        var service = CreateService();
        bool result = service.Delete(42);
        Assert.False(result);
    }

    [Fact]
    public void Update_ExistingItem_UpdatesFields()
    {
        var service = CreateService();
        var item = service.Add("Old title");

        bool result = service.Update(item.Id, "New title", "New desc", Priority.High);

        Assert.True(result);
        Assert.Equal("New title", item.Title);
        Assert.Equal("New desc", item.Description);
        Assert.Equal(Priority.High, item.Priority);
    }

    [Fact]
    public void Update_NonExistentId_ReturnsFalse()
    {
        var service = CreateService();
        bool result = service.Update(999, "Title", null, Priority.Low);
        Assert.False(result);
    }

    [Fact]
    public void Update_EmptyTitle_ThrowsArgumentException()
    {
        var service = CreateService();
        var item = service.Add("Task");
        Assert.Throws<ArgumentException>(() => service.Update(item.Id, "", null, Priority.Low));
    }

    [Fact]
    public void ClearCompleted_RemovesOnlyCompletedItems()
    {
        var service = CreateService();
        var active = service.Add("Active task");
        var completed = service.Add("Done task");
        service.ToggleComplete(completed.Id);

        service.ClearCompleted();

        var remaining = service.GetAll();
        Assert.Single(remaining);
        Assert.Equal(active.Id, remaining[0].Id);
    }

    [Fact]
    public void CountActive_ReturnsCorrectCount()
    {
        var service = CreateService();
        service.Add("Task 1");
        service.Add("Task 2");
        var item3 = service.Add("Task 3");
        service.ToggleComplete(item3.Id);

        Assert.Equal(2, service.CountActive());
    }

    [Fact]
    public void CountCompleted_ReturnsCorrectCount()
    {
        var service = CreateService();
        var item1 = service.Add("Task 1");
        service.Add("Task 2");
        service.ToggleComplete(item1.Id);

        Assert.Equal(1, service.CountCompleted());
    }

    [Fact]
    public void CountTotal_ReturnsAllItems()
    {
        var service = CreateService();
        service.Add("Task 1");
        service.Add("Task 2");
        service.Add("Task 3");

        Assert.Equal(3, service.CountTotal());
    }

    [Fact]
    public void GetByFilter_All_ReturnsAll()
    {
        var service = CreateService();
        var item1 = service.Add("Task 1");
        service.Add("Task 2");
        service.ToggleComplete(item1.Id);

        var result = service.GetByFilter(FilterType.All);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetByFilter_Active_ReturnsOnlyActive()
    {
        var service = CreateService();
        service.Add("Active");
        var done = service.Add("Done");
        service.ToggleComplete(done.Id);

        var result = service.GetByFilter(FilterType.Active);

        Assert.Single(result);
        Assert.False(result[0].IsCompleted);
    }

    [Fact]
    public void GetByFilter_Completed_ReturnsOnlyCompleted()
    {
        var service = CreateService();
        service.Add("Active");
        var done = service.Add("Done");
        service.ToggleComplete(done.Id);

        var result = service.GetByFilter(FilterType.Completed);

        Assert.Single(result);
        Assert.True(result[0].IsCompleted);
    }
}
