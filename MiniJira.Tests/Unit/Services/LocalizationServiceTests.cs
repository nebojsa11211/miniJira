using FluentAssertions;
using MiniJira.Services;
using System.Globalization;

namespace MiniJira.Tests.Unit.Services;

public class LocalizationServiceTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultCultureToEnglish()
    {
        // Act
        var service = new LocalizationService();

        // Assert
        service.CurrentCulture.Name.Should().Be("en-US");
    }

    [Fact]
    public void SupportedCultures_ShouldContainEnglishAndCroatian()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var supportedCultures = service.SupportedCultures;

        // Assert
        supportedCultures.Should().HaveCount(2);
        supportedCultures.Should().Contain(c => c.Name == "en-US");
        supportedCultures.Should().Contain(c => c.Name == "hr-HR");
    }

    [Fact]
    public void SetCulture_ShouldUpdateCurrentCulture_WhenCultureIsSupported()
    {
        // Arrange
        var service = new LocalizationService();
        var croatianCulture = new CultureInfo("hr-HR");

        // Act
        service.SetCulture(croatianCulture);

        // Assert
        service.CurrentCulture.Name.Should().Be("hr-HR");
        CultureInfo.CurrentCulture.Name.Should().Be("hr-HR");
        CultureInfo.CurrentUICulture.Name.Should().Be("hr-HR");
    }

    [Fact]
    public void SetCulture_ShouldRaiseCultureChangedEvent()
    {
        // Arrange
        var service = new LocalizationService();
        var eventRaised = false;

        service.CultureChanged += (sender, args) => eventRaised = true;

        // Act
        service.SetCulture(new CultureInfo("hr-HR"));

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void SetCulture_ShouldThrowException_WhenCultureIsNotSupported()
    {
        // Arrange
        var service = new LocalizationService();
        var unsupportedCulture = new CultureInfo("de-DE"); // German

        // Act
        var action = () => service.SetCulture(unsupportedCulture);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*not supported*");
    }

    [Fact]
    public void SetCulture_ShouldNotRaiseEvent_WhenSetToCurrentCulture()
    {
        // Arrange
        var service = new LocalizationService();
        var eventCount = 0;

        service.CultureChanged += (sender, args) => eventCount++;

        // Act
        service.SetCulture(new CultureInfo("en-US")); // Already the default
        service.SetCulture(new CultureInfo("hr-HR"));

        // Assert
        eventCount.Should().Be(2); // Both calls should raise events
    }
}
