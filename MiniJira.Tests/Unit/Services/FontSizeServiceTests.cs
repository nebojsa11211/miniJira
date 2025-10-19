using FluentAssertions;
using MiniJira.Services;

namespace MiniJira.Tests.Unit.Services;

public class FontSizeServiceTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultFontSizeToNormal()
    {
        // Act
        var service = new FontSizeService();

        // Assert
        service.CurrentFontSizeMultiplier.Should().Be(1.0m);
    }

    [Fact]
    public void AvailableOptions_ShouldContainSixOptions()
    {
        // Arrange
        var service = new FontSizeService();

        // Act
        var options = service.AvailableOptions;

        // Assert
        options.Should().HaveCount(6);
        options.Should().Contain(o => o.Multiplier == 1.0m && o.DisplayKey == "FontSize.Normal");
        options.Should().Contain(o => o.Multiplier == 1.1m && o.DisplayKey == "FontSize.Bigger10");
        options.Should().Contain(o => o.Multiplier == 1.2m && o.DisplayKey == "FontSize.Bigger20");
        options.Should().Contain(o => o.Multiplier == 1.3m && o.DisplayKey == "FontSize.Bigger30");
        options.Should().Contain(o => o.Multiplier == 1.4m && o.DisplayKey == "FontSize.Bigger40");
        options.Should().Contain(o => o.Multiplier == 1.5m && o.DisplayKey == "FontSize.Bigger50");
    }

    [Fact]
    public void SetFontSize_ShouldUpdateCurrentFontSizeMultiplier_WhenMultiplierIsValid()
    {
        // Arrange
        var service = new FontSizeService();

        // Act
        service.SetFontSize(1.2m);

        // Assert
        service.CurrentFontSizeMultiplier.Should().Be(1.2m);
    }

    [Fact]
    public void SetFontSize_ShouldRaiseFontSizeChangedEvent()
    {
        // Arrange
        var service = new FontSizeService();
        var eventRaised = false;

        service.FontSizeChanged += (sender, args) => eventRaised = true;

        // Act
        service.SetFontSize(1.3m);

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void SetFontSize_ShouldThrowException_WhenMultiplierIsNotSupported()
    {
        // Arrange
        var service = new FontSizeService();

        // Act
        var action = () => service.SetFontSize(2.0m); // Not in available options

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*not supported*");
    }

    [Fact]
    public void SetFontSize_ShouldRaiseEventForEachChange()
    {
        // Arrange
        var service = new FontSizeService();
        var eventCount = 0;

        service.FontSizeChanged += (sender, args) => eventCount++;

        // Act
        service.SetFontSize(1.1m);
        service.SetFontSize(1.2m);
        service.SetFontSize(1.3m);

        // Assert
        eventCount.Should().Be(3);
    }

    [Fact]
    public void SetFontSize_ShouldWorkWithAllAvailableOptions()
    {
        // Arrange
        var service = new FontSizeService();

        // Act & Assert - Should not throw for any available option
        foreach (var option in service.AvailableOptions)
        {
            var action = () => service.SetFontSize(option.Multiplier);
            action.Should().NotThrow();
            service.CurrentFontSizeMultiplier.Should().Be(option.Multiplier);
        }
    }

    [Fact]
    public void FontSizeOption_ShouldHaveCorrectMultiplierAndDisplayKey()
    {
        // Arrange
        var service = new FontSizeService();

        // Act
        var normalOption = service.AvailableOptions.First(o => o.Multiplier == 1.0m);

        // Assert
        normalOption.Multiplier.Should().Be(1.0m);
        normalOption.DisplayKey.Should().Be("FontSize.Normal");
    }
}
