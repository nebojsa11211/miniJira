using FluentAssertions;
using Microsoft.JSInterop;
using MiniJira.Services;
using NSubstitute;

namespace MiniJira.Tests.Unit.Services;

public class ThemeServiceTests
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IThemeService _service;

    public ThemeServiceTests()
    {
        _jsRuntime = Substitute.For<IJSRuntime>();
        _service = new ThemeService(_jsRuntime);
    }

    [Fact]
    public void Constructor_ShouldSetDefaultThemeToLight()
    {
        // Assert
        _service.CurrentTheme.Should().Be("light");
    }

    [Fact]
    public async Task InitializeAsync_ShouldLoadThemeFromLocalStorage()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(Task.FromResult("dark"));

        // Act
        await _service.InitializeAsync();

        // Assert
        _service.CurrentTheme.Should().Be("dark");
    }

    [Fact]
    public async Task InitializeAsync_ShouldDefaultToLight_WhenLocalStorageReturnsNull()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(Task.FromResult<string>(null!));

        // Act
        await _service.InitializeAsync();

        // Assert
        _service.CurrentTheme.Should().Be("light");
    }

    [Fact]
    public async Task InitializeAsync_ShouldDefaultToLight_WhenExceptionOccurs()
    {
        // Arrange
        _jsRuntime.InvokeAsync<string>("localStorage.getItem", Arg.Any<object[]>())
            .Returns<string>(_ => throw new JSException("Test exception"));

        // Act
        await _service.InitializeAsync();

        // Assert
        _service.CurrentTheme.Should().Be("light");
    }

    [Fact]
    public async Task ToggleThemeAsync_ShouldSwitchFromLightToDark()
    {
        // Arrange - Start with light theme
        var eventRaised = false;
        _service.OnThemeChanged += () => eventRaised = true;

        // Act
        await _service.ToggleThemeAsync();

        // Assert
        _service.CurrentTheme.Should().Be("dark");
        eventRaised.Should().BeTrue();
        await _jsRuntime.Received(1).InvokeVoidAsync("localStorage.setItem", Arg.Any<object[]>());
    }

    [Fact]
    public async Task ToggleThemeAsync_ShouldSwitchFromDarkToLight()
    {
        // Arrange - Toggle once to get to dark
        await _service.ToggleThemeAsync();
        var eventRaised = false;
        _service.OnThemeChanged += () => eventRaised = true;

        // Act
        await _service.ToggleThemeAsync();

        // Assert
        _service.CurrentTheme.Should().Be("light");
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public async Task SetThemeAsync_ShouldSetThemeToDark()
    {
        // Arrange
        var eventRaised = false;
        _service.OnThemeChanged += () => eventRaised = true;

        // Act
        await _service.SetThemeAsync("dark");

        // Assert
        _service.CurrentTheme.Should().Be("dark");
        eventRaised.Should().BeTrue();
        await _jsRuntime.Received(1).InvokeVoidAsync("localStorage.setItem", Arg.Any<object[]>());
    }

    [Fact]
    public async Task SetThemeAsync_ShouldSetThemeToLight()
    {
        // Arrange
        var eventRaised = false;
        _service.OnThemeChanged += () => eventRaised = true;

        // Act
        await _service.SetThemeAsync("light");

        // Assert
        _service.CurrentTheme.Should().Be("light");
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public async Task SetThemeAsync_ShouldIgnoreInvalidTheme()
    {
        // Arrange
        var initialTheme = _service.CurrentTheme;
        var eventRaised = false;
        _service.OnThemeChanged += () => eventRaised = true;

        // Act
        await _service.SetThemeAsync("invalid");

        // Assert
        _service.CurrentTheme.Should().Be(initialTheme);
        eventRaised.Should().BeFalse();
    }

    [Fact]
    public async Task OnThemeChanged_ShouldRaiseEventOnToggle()
    {
        // Arrange
        var eventCount = 0;
        _service.OnThemeChanged += () => eventCount++;

        // Act
        await _service.ToggleThemeAsync();
        await _service.ToggleThemeAsync();

        // Assert
        eventCount.Should().Be(2);
    }

    [Fact]
    public async Task OnThemeChanged_ShouldRaiseEventOnSetTheme()
    {
        // Arrange
        var eventCount = 0;
        _service.OnThemeChanged += () => eventCount++;

        // Act
        await _service.SetThemeAsync("dark");
        await _service.SetThemeAsync("light");

        // Assert
        eventCount.Should().Be(2);
    }
}
