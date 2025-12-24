using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test.FilterDsl;

/// <summary>
/// End-to-End Integrationstests für die Filter-DSL
/// </summary>
public class IntegrationTests
{
    private readonly GenerateSiteswapsTool _tool = new();

    [Test]
    public async Task GenerateSiteswaps_With_Dsl_MinOcc_Works()
    {
        // Act - minOcc(5,1) für mindestens eine 5
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 10,
            filter: "minOcc(5,1)"
        );

        // Assert
        result.Should().NotBeEmpty();
        // Alle Ergebnisse sollten mindestens eine 5 enthalten
        foreach (var siteswap in result)
        {
            siteswap.Count(c => c == '5').Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Dsl_Ground_Works()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 10,
            filter: "ground"
        );

        // Assert
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task GenerateSiteswaps_With_Dsl_NoZeros_Works()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 0, // 0 erlauben
            maxHeight: 7,
            maxResults: 10,
            filter: "noZeros"
        );

        // Assert
        result.Should().NotBeEmpty();
        // Keine Ergebnisse sollten 0 enthalten
        foreach (var siteswap in result)
        {
            siteswap.Should().NotContain("0");
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Dsl_And_Combination_Works()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 0,
            maxHeight: 7,
            maxResults: 10,
            filter: "minOcc(5,1) AND noZeros"
        );

        // Assert
        result.Should().NotBeEmpty();
        foreach (var siteswap in result)
        {
            siteswap.Count(c => c == '5').Should().BeGreaterThanOrEqualTo(1);
            siteswap.Should().NotContain("0");
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Dsl_Or_Combination_Works()
    {
        // Act - OR von zwei minOcc Filtern
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 20,
            filter: "minOcc(5,1) OR minOcc(7,1)"
        );

        // Assert
        result.Should().NotBeEmpty();
        // Jedes Ergebnis sollte entweder 1+ 5er ODER 1+ 7er haben
        foreach (var siteswap in result)
        {
            var has5 = siteswap.Contains('5');
            var has7 = siteswap.Contains('7');
            (has5 || has7).Should().BeTrue($"Siteswap {siteswap} sollte 5 oder 7 haben");
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Dsl_Complex_Expression_Works()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 0,
            maxHeight: 7,
            maxResults: 10,
            filter: "(minOcc(5,1) OR minOcc(7,1)) AND noZeros"
        );

        // Assert
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task GenerateSiteswaps_With_Invalid_Dsl_Throws()
    {
        // Act & Assert
        var action = async () =>
            await _tool.GenerateSiteswaps(
                period: 3,
                numberOfObjects: 3,
                minHeight: 1,
                maxHeight: 7,
                filter: "unknownFunction(5)"
            );

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*Unbekannte Funktion*");
    }

    [Test]
    public async Task GenerateSiteswaps_With_Syntax_Error_Dsl_Throws()
    {
        // Act & Assert
        var action = async () =>
            await _tool.GenerateSiteswaps(
                period: 3,
                numberOfObjects: 3,
                minHeight: 1,
                maxHeight: 7,
                filter: "minOcc(5,2 AND ground" // Fehlende Klammer
            );

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*Syntaxfehler*");
    }

    [Test]
    public async Task GenerateSiteswaps_With_MaxOcc_Filter()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 10,
            filter: "maxOcc(5,1)"
        );

        // Assert
        result.Should().NotBeEmpty();
        foreach (var siteswap in result)
        {
            siteswap.Count(c => c == '5').Should().BeLessThanOrEqualTo(1);
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_ExactOcc_Filter()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 10,
            filter: "exactOcc(5,1)"
        );

        // Assert
        result.Should().NotBeEmpty();
        foreach (var siteswap in result)
        {
            siteswap.Count(c => c == '5').Should().Be(1);
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Not_Filter()
    {
        // Act
        var result = await _tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 10,
            filter: "NOT minOcc(5,2)" // Nicht mehr als 1 Fünfer
        );

        // Assert
        result.Should().NotBeEmpty();
        foreach (var siteswap in result)
        {
            siteswap.Count(c => c == '5').Should().BeLessThan(2);
        }
    }
}
