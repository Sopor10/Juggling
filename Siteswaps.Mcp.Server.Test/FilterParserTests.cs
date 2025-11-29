using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class FilterParserTests
{
    private SiteswapGeneratorInput CreateInput(int period = 3, int numberOfObjects = 3, int minHeight = 1, int maxHeight = 7)
    {
        return new SiteswapGeneratorInput(period, numberOfObjects, minHeight, maxHeight);
    }

    [Test]
    public void ParseOccurrenceFilters_With_Single_Number_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseOccurrenceFilters("3:2");

        // Assert
        result.Should().HaveCount(1);
        result[0].Numbers.Should().BeEquivalentTo([3]);
        result[0].Amount.Should().Be(2);
    }

    [Test]
    public void ParseOccurrenceFilters_With_Multiple_Numbers_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseOccurrenceFilters("3:2,4:1");

        // Assert
        result.Should().HaveCount(2);
        result[0].Numbers.Should().BeEquivalentTo([3]);
        result[0].Amount.Should().Be(2);
        result[1].Numbers.Should().BeEquivalentTo([4]);
        result[1].Amount.Should().Be(1);
    }

    [Test]
    public void ParseOccurrenceFilters_With_Multiple_Numbers_In_One_Entry_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseOccurrenceFilters("3,4:2");

        // Assert
        result.Should().HaveCount(1);
        result[0].Numbers.Should().BeEquivalentTo([3, 4]);
        result[0].Amount.Should().Be(2);
    }

    [Test]
    public void ParseOccurrenceFilters_With_Invalid_Format_Returns_Empty_List()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseOccurrenceFilters("invalid");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ParsePattern_With_Valid_Pattern_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParsePattern("3,3,1");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo([3, 3, 1]);
    }

    [Test]
    public void ParsePattern_With_Invalid_Pattern_Returns_Null()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParsePattern("invalid,pattern");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ParseState_With_Valid_State_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseState("1,1,0,0");

        // Assert
        result.Should().NotBeNull();
        // State ist intern, daher prüfen wir nur, dass es nicht null ist
    }

    [Test]
    public void ParseState_With_True_False_Values_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseState("true,false,1,0");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ParseState_With_Invalid_State_Returns_Null()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseState("");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ParseFlexiblePattern_With_Valid_Pattern_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseFlexiblePattern("3,4;5,6");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo([3, 4]);
        result[1].Should().BeEquivalentTo([5, 6]);
    }

    [Test]
    public void ParseFlexiblePattern_With_Invalid_Pattern_Returns_Null()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseFlexiblePattern("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ParsePersonalizedNumberFilter_With_Valid_Filter_Returns_Correct_Result()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 2, minHeight: 1, maxHeight: 7);

        // Act
        var result = parser.ParsePersonalizedNumberFilter("3,4:2:atleast:0");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ParsePersonalizedNumberFilter_Without_NumberOfJugglers_Returns_Null()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParsePersonalizedNumberFilter("3:2:atleast:0");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ParsePersonalizedNumberFilter_With_Invalid_Format_Returns_Null()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 2);

        // Act
        var result = parser.ParsePersonalizedNumberFilter("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ParseAndBuildNotFilter_With_MinOccurrence_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 1);

        // Act
        var result = parser.ParseAndBuildNotFilter("minOccurrence:3:2");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ParseAndBuildNotFilter_With_Pattern_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 1);

        // Act
        var result = parser.ParseAndBuildNotFilter("pattern:3,3,1");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ParseAndBuildNotFilter_With_State_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseAndBuildNotFilter("state:1,1,0,0");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ParseAndBuildNotFilter_With_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 1);

        // Act
        var result = parser.ParseAndBuildNotFilter("minOccurrence:3:2|maxOccurrence:5:1");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ParseAndBuildNotFilter_With_Invalid_Format_Returns_Null()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.ParseAndBuildNotFilter("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void BuildOccurrenceFilterWithOr_With_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.BuildOccurrenceFilterWithOr(
            "3:2|4:1",
            (builder, numbers, amount) => builder.MinimumOccurence(numbers, amount));

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildOccurrenceFilterWithOr_Without_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.BuildOccurrenceFilterWithOr(
            "3:2",
            (builder, numbers, amount) => builder.MinimumOccurence(numbers, amount));

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildPatternFilterWithOr_With_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 1);

        // Act
        var result = parser.BuildPatternFilterWithOr("3,3,1|4,4,1");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildPatternFilterWithOr_Without_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 1);

        // Act
        var result = parser.BuildPatternFilterWithOr("3,3,1");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildStateFilterWithOr_With_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.BuildStateFilterWithOr("1,1,0|1,0,1");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildStateFilterWithOr_Without_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.BuildStateFilterWithOr("1,1,0");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildFlexiblePatternFilterWithOr_With_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 2);

        // Act
        var result = parser.BuildFlexiblePatternFilterWithOr("3,4;5|4,5;6");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildFlexiblePatternFilterWithOr_Without_OR_Logic_Returns_Filter()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 2);

        // Act
        var result = parser.BuildFlexiblePatternFilterWithOr("3,4;5");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void BuildFilterFromParameters_With_All_Parameters_Returns_FilterBuilder()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 2, minHeight: 1, maxHeight: 7);

        // Act
        var result = parser.BuildFilterFromParameters(
            minOccurrence: "3:2",
            maxOccurrence: "5:1",
            exactOccurrence: "4:1",
            numberOfPasses: 2,
            pattern: "3,3,1",
            state: "1,1,0",
            flexiblePattern: "3,4;5",
            useDefaultFilter: true,
            useNoFilter: false,
            jugglerIndex: 0,
            rotationAwarePattern: "3,4;5",
            personalizedNumberFilter: "3:2:atleast:0",
            notFilter: "minOccurrence:6:1");

        // Assert
        result.Should().NotBeNull();
        var filter = result.Build();
        filter.Should().NotBeNull();
    }

    [Test]
    public void BuildFilterFromParameters_With_Minimal_Parameters_Returns_FilterBuilder()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input);

        // Act
        var result = parser.BuildFilterFromParameters(
            useDefaultFilter: true);

        // Assert
        result.Should().NotBeNull();
        var filter = result.Build();
        filter.Should().NotBeNull();
    }

    [Test]
    public void BuildFilterFromParameters_With_OR_Logic_Returns_FilterBuilder()
    {
        // Arrange
        var input = CreateInput();
        var parser = new FilterParser(input, numberOfJugglers: 1);

        // Act
        var result = parser.BuildFilterFromParameters(
            minOccurrence: "3:2|4:1",
            pattern: "3,3,1|4,4,1",
            state: "1,1,0|1,0,1");

        // Assert
        result.Should().NotBeNull();
        var filter = result.Build();
        filter.Should().NotBeNull();
    }
}

