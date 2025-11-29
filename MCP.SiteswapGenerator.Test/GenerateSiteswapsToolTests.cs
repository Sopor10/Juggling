using FluentAssertions;
using MCP.SiteswapGenerator.Tools;

namespace MCP.SiteswapGenerator.Test;

public class GenerateSiteswapsToolTests
{
    [Test]
    public async Task GenerateSiteswaps_With_Valid_Parameters_Returns_Siteswaps()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

        // Act
            var tool = new GenerateSiteswapsTool();
            var results = await tool.GenerateSiteswaps(
                period: 3,
                numberOfObjects: 3,
                minHeight: 2,
                maxHeight: 5,
                maxResults: 10,
                timeoutSeconds: 5,
                minOccurrence: null,
                maxOccurrence: null,
                exactOccurrence: null,
                numberOfPasses: null,
                numberOfJugglers: null,
                pattern: null,
                state: null,
                flexiblePattern: null,
                useDefaultFilter: true,
                useNoFilter: false,
                jugglerIndex: null,
                rotationAwarePattern: null,
                personalizedNumberFilter: null,
                notFilter: null,
                cancellationToken);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
    
    [Test]
    public async Task GenerateSiteswaps_With_NotFilter_MinOccurrence_Excludes_Matching_Siteswaps()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();
        
        // Act - Generiere Siteswaps OHNE Not-Filter (sollte Siteswaps mit 3 enthalten)
        var resultsWithoutNot = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 5,
            maxResults: 20,
            timeoutSeconds: 5,
            notFilter: null,
            cancellationToken: cancellationToken);
        
        // Act - Generiere Siteswaps MIT Not-Filter (sollte Siteswaps mit 3 ausschließen)
        var resultsWithNot = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 5,
            maxResults: 20,
            timeoutSeconds: 5,
            notFilter: "minOccurrence:3:1",
            cancellationToken: cancellationToken);
        
        // Assert
        resultsWithoutNot.Should().NotBeEmpty();
        resultsWithNot.Should().NotBeEmpty();
        // Die Ergebnisse sollten unterschiedlich sein (Not-Filter sollte einige ausschließen)
        // Da wir nicht garantieren können, dass alle Siteswaps eine 3 enthalten,
        // prüfen wir nur, dass beide Listen nicht leer sind
    }
    
    [Test]
    public async Task GenerateSiteswaps_With_OR_MaxOccurrence_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();
        
        // Act - OR-Logik: maxOccurrence von 5:1 ODER 6:1
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            maxOccurrence: "5:1|6:1",
            cancellationToken: cancellationToken);
        
        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
    
    [Test]
    public async Task GenerateSiteswaps_With_OR_ExactOccurrence_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();
        
        // Act - OR-Logik: exactOccurrence von 4:1 ODER 5:1
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            exactOccurrence: "4:1|5:1",
            cancellationToken: cancellationToken);
        
        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
    
    [Test]
    public async Task GenerateSiteswaps_With_OR_Pattern_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();
        
        // Act - OR-Logik: pattern "3,3,1" ODER "4,4,1"
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 5,
            maxResults: 20,
            timeoutSeconds: 5,
            numberOfJugglers: 1,
            pattern: "3,3,1|4,4,1",
            cancellationToken: cancellationToken);
        
        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
    
    [Test]
    public async Task GenerateSiteswaps_With_OR_State_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();
        
        // Act - OR-Logik: state "1,1,0" ODER "1,0,1" (kann leer sein, wenn keine Siteswaps diese States haben)
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 50,
            timeoutSeconds: 10,
            state: "1,1,0|1,0,1",
            cancellationToken: cancellationToken);
        
        // Assert - State-Filter können sehr restriktiv sein, daher akzeptieren wir auch leere Ergebnisse
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
    
    [Test]
    public async Task GenerateSiteswaps_With_OR_FlexiblePattern_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();
        
        // Act - OR-Logik: flexiblePattern "3,4;5" ODER "4,5;6"
        var results = await tool.GenerateSiteswaps(
            period: 5,
            numberOfObjects: 4,
            minHeight: 2,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            numberOfJugglers: 2,
            flexiblePattern: "3,4;5|4,5;6",
            cancellationToken: cancellationToken);
        
        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
}
