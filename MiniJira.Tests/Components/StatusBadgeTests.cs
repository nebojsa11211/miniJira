using Bunit;
using FluentAssertions;
using MiniJira.Components.Shared;
using MiniJira.Models;

namespace MiniJira.Tests.Components;

public class StatusBadgeTests : TestContext
{
    [Theory]
    [InlineData(Models.TaskStatus.ToDo, "To Do", "bg-secondary")]
    [InlineData(Models.TaskStatus.InProgress, "In Progress", "bg-primary")]
    [InlineData(Models.TaskStatus.Done, "Done", "bg-success")]
    public void StatusBadge_ShouldRenderCorrectly(Models.TaskStatus status, string expectedText, string expectedClass)
    {
        // Act
        var cut = RenderComponent<StatusBadge>(parameters => parameters
            .Add(p => p.Status, status));

        // Assert
        var badge = cut.Find(".badge");
        badge.TextContent.Should().Be(expectedText);
        badge.ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void StatusBadge_ShouldRenderBadgeElement_WithCorrectBaseClasses()
    {
        // Act
        var cut = RenderComponent<StatusBadge>(parameters => parameters
            .Add(p => p.Status, Models.TaskStatus.ToDo));

        // Assert
        var badge = cut.Find(".badge");
        badge.Should().NotBeNull();
        badge.ClassList.Should().Contain("badge");
    }
}
