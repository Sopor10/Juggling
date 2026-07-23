using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Resources;

/// <summary>
/// MCP Resources für die Filter-DSL Dokumentation
/// </summary>
[McpServerResourceType]
public class FilterDslResources
{
    // ====================================================================================
    // SECTION 1: SYNTAX REFERENCE
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:syntax:overview")]
    public string FilterDslSyntaxOverview() =>
        """
            The Filter-DSL is a domain-specific language for filtering siteswaps. It supports:
            - Logical operators: AND, OR, NOT (case-insensitive)
            - Parentheses for grouping: (A OR B) AND C
            - Function calls with arguments: minOcc(5, 2)
            - Keywords without arguments: ground, excited
            - Wildcards in patterns: pattern(5, *, 1)
            - Number lists: occ([5,7,9], 2)

            Operator precedence (highest to lowest): NOT > AND > OR
            """;

    [McpServerResource]
    [Description("filter:dsl:syntax:operators")]
    public string FilterDslSyntaxOperators() =>
        """
            Logical Operators:

            AND - Both conditions must be true
                Example: ground AND noZeros
                
            OR - At least one condition must be true
                Example: ground OR excited
                
            NOT - Negates the following condition
                Example: NOT ground

            Operator Precedence:
            1. NOT (highest) - binds tightest
            2. AND
            3. OR (lowest)

            Examples:
            - "A OR B AND C" is parsed as "A OR (B AND C)"
            - "NOT A AND B" is parsed as "(NOT A) AND B"
            - Use parentheses to override: "(A OR B) AND C"
            """;

    [McpServerResource]
    [Description("filter:dsl:syntax:arguments")]
    public string FilterDslSyntaxArguments() =>
        """
            Argument Types:

            1. Numbers - Integer values
               Example: minOcc(5, 2)
               
            2. Wildcards (*) - Matches any value in pattern()
               Example: pattern(5, *, 1) - matches 531, 541, 551, etc.
               
            3. Pass/Self (p/s) - Matches pass or self in pattern() (requires numberOfJugglers)
               Example: pattern(p, s, p) - Pass, Self, Pass sequence
               p: matches pass throws (ungerade Zahlen bei Passing)
               s: matches self throws (gerade Zahlen bei Passing)
               
            4. Number Lists [n1, n2, ...] - Multiple allowed values
               Example: occ([5,7,9], 2) - throw value can be 5, 7, or 9
               
            Whitespace is flexible:
            - minOcc(5,2) and minOcc( 5 , 2 ) are equivalent
            - [5,7,9] and [ 5, 7, 9 ] are equivalent
            """;

    [McpServerResource]
    [Description("filter:dsl:syntax:grammar")]
    public string FilterDslSyntaxGrammar() =>
        """
            Formal Grammar (EBNF-style):

            Expression  = Term { "OR" Term }
            Term        = Factor { "AND" Factor }
            Factor      = "NOT" Factor | Atom | "(" Expression ")"
            Atom        = Identifier [ "(" ArgList ")" ]
            ArgList     = Argument { "," Argument }
            Argument    = Number | Wildcard | NumberList | Identifier
            Number      = Digit+
            Wildcard    = "*"
            NumberList  = "[" Number { "," Number } "]"
            Identifier  = Letter { Letter | Digit | "_" }

            Keywords (AND, OR, NOT) are case-insensitive.
            """;

    // ====================================================================================
    // SECTION 2: FUNCTION REFERENCE - OCCURRENCE FILTERS
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:function:minOcc")]
    public string FilterDslFunctionMinOcc() =>
        """
            minOcc(throw, count)

            Filters siteswaps where a specific throw value appears at least 'count' times.

            Parameters:
            - throw: The siteswap throw value (integer)
            - count: Minimum number of occurrences (integer)

            Examples:
            - minOcc(5, 2) - must contain at least two 5s
            - minOcc(7, 1) - must contain at least one 7

            Use case: Find patterns with multiple high throws.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:maxOcc")]
    public string FilterDslFunctionMaxOcc() =>
        """
            maxOcc(throw, count)

            Filters siteswaps where a specific throw value appears at most 'count' times.

            Parameters:
            - throw: The siteswap throw value (integer)
            - count: Maximum number of occurrences (integer)

            Examples:
            - maxOcc(2, 1) - at most one 2 (hold)
            - maxOcc(0, 0) - no zeros allowed (same as noZeros)

            Use case: Limit certain throw types.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:exactOcc")]
    public string FilterDslFunctionExactOcc() =>
        """
            exactOcc(throw, count)

            Filters siteswaps where a specific throw value appears exactly 'count' times.

            Parameters:
            - throw: The siteswap throw value (integer)
            - count: Exact number of occurrences (integer)

            Examples:
            - exactOcc(5, 2) - exactly two 5s
            - exactOcc(1, 1) - exactly one 1 (zip)

            Use case: Precise control over pattern composition.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:occ")]
    public string FilterDslFunctionOcc() =>
        """
            occ(throw, min, max)

            Filters siteswaps where a throw value appears between 'min' and 'max' times.

            Parameters:
            - throw: The siteswap throw value (integer or number list)
            - min: Minimum occurrences (integer)
            - max: Maximum occurrences (integer)

            Examples:
            - occ(5, 1, 3) - between 1 and 3 fives
            - occ([5,7], 2, 4) - between 2 and 4 total 5s and 7s

            Use case: Flexible occurrence constraints.
            """;

    // ====================================================================================
    // SECTION 3: FUNCTION REFERENCE - PATTERN FILTERS
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:function:pattern")]
    public string FilterDslFunctionPattern() =>
        """
            pattern(values...)

            Filters siteswaps that match a specific pattern with wildcards.

            Parameters:
            - values: Sequence of throw values, wildcards (*), or pass/self indicators (p/s)

            Wildcards and Indicators:
            - * : matches any throw value
            - p : matches pass (ungerade Zahlen bei Passing-Patterns, requires numberOfJugglers)
            - s : matches self (gerade Zahlen bei Passing-Patterns, requires numberOfJugglers)

            Examples:
            - pattern(5, 3, 1) - matches exactly "531"
            - pattern(5, *, 1) - matches "531", "541", "551", etc.
            - pattern(*, *, 1) - any pattern ending with 1
            - pattern(p, s, p) - pass, self, pass (bei 2 Jongleuren)
            - pattern(5, p, s) - genau 5, dann Pass, dann Self

            Use case: Find patterns with specific structure.
            Note: p and s require numberOfJugglers parameter in generate_siteswaps.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:startsWith")]
    public string FilterDslFunctionStartsWith() =>
        """
            startsWith(values...)

            Filters siteswaps that start with specific throw values.

            Parameters:
            - values: Sequence of throw values at the beginning

            Examples:
            - startsWith(5) - starts with a 5
            - startsWith(5, 3) - starts with 53
            - startsWith(7, 4, 4) - starts with 744

            Use case: Find patterns starting with high/low throws.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:endsWith")]
    public string FilterDslFunctionEndsWith() =>
        """
            endsWith(values...)

            Filters siteswaps that end with specific throw values.

            Parameters:
            - values: Sequence of throw values at the end

            Examples:
            - endsWith(1) - ends with a 1 (zip)
            - endsWith(4, 1) - ends with 41

            Use case: Find patterns with specific endings.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:contains")]
    public string FilterDslFunctionContains() =>
        """
            contains(values...)

            Filters siteswaps that contain a specific subsequence.

            Parameters:
            - values: Sequence of throw values that must appear

            Examples:
            - contains(5, 1) - contains "51" somewhere
            - contains(4, 4) - contains two consecutive 4s

            Use case: Find patterns containing specific throw combinations.
            """;

    // ====================================================================================
    // SECTION 4: FUNCTION REFERENCE - HEIGHT FILTERS
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:function:height")]
    public string FilterDslFunctionHeight() =>
        """
            height(min, max)

            Filters siteswaps where all throw values are within a height range.

            Parameters:
            - min: Minimum throw height (integer)
            - max: Maximum throw height (integer)

            Examples:
            - height(3, 7) - all throws between 3 and 7
            - height(1, 5) - no throws higher than 5, none lower than 1

            Use case: Limit difficulty by constraining throw heights.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:maxHeight")]
    public string FilterDslFunctionMaxHeight() =>
        """
            maxHeight(max)

            Filters siteswaps where no throw exceeds the maximum height.

            Parameters:
            - max: Maximum allowed throw height (integer)

            Examples:
            - maxHeight(7) - no throws higher than 7
            - maxHeight(5) - no throws higher than 5

            Use case: Find patterns suitable for skill level.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:minHeight")]
    public string FilterDslFunctionMinHeight() =>
        """
            minHeight(min)

            Filters siteswaps where all throws are at least the minimum height.

            Parameters:
            - min: Minimum throw height (integer)

            Examples:
            - minHeight(3) - no throws lower than 3
            - minHeight(5) - all throws are 5 or higher

            Use case: Find patterns with consistently high throws.
            """;

    // ====================================================================================
    // SECTION 5: FUNCTION REFERENCE - STATE FILTERS
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:function:ground")]
    public string FilterDslFunctionGround() =>
        """
            ground

            Filters for ground-state siteswaps only.

            No parameters.

            A ground-state siteswap can be entered directly from the basic pattern 
            (cascade for odd numbers, fountain for even) without transition throws.

            Examples:
            - ground - only ground-state patterns
            - ground AND minOcc(5, 2) - ground-state with at least two 5s

            Use case: Find patterns easy to enter and exit.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:excited")]
    public string FilterDslFunctionExcited() =>
        """
            excited

            Filters for excited-state siteswaps only.

            No parameters.

            An excited-state siteswap requires special transition throws to enter 
            and exit the pattern. The juggling state differs from the basic pattern.

            Examples:
            - excited - only excited-state patterns
            - excited AND noZeros - excited-state patterns without zeros

            Use case: Find more challenging patterns requiring transitions.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:state")]
    public string FilterDslFunctionState() =>
        """
            state(values...)

            Filters siteswaps with a specific starting state.

            Parameters:
            - values: The state representation (binary or landing schedule)

            Examples:
            - state(1, 1, 1) - 3-ball ground state
            - state(1, 1, 0, 1) - specific 3-ball excited state

            Use case: Find patterns in a specific juggling state.
            """;

    // ====================================================================================
    // SECTION 6: FUNCTION REFERENCE - PROPERTY FILTERS
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:function:noZeros")]
    public string FilterDslFunctionNoZeros() =>
        """
            noZeros

            Filters siteswaps that contain no zero throws.

            No parameters.

            A zero throw means an empty hand pause. Patterns without zeros 
            have continuous throwing from both hands.

            Examples:
            - noZeros - no empty hand pauses
            - noZeros AND ground - ground-state without gaps

            Use case: Find patterns with consistent rhythm.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:hasZeros")]
    public string FilterDslFunctionHasZeros() =>
        """
            hasZeros

            Filters siteswaps that contain at least one zero throw.

            No parameters.

            Patterns with zeros have pauses where one hand is empty.

            Examples:
            - hasZeros - patterns with gaps
            - hasZeros AND ground - ground-state with pauses

            Use case: Find patterns with deliberate pauses.
            """;

    // ====================================================================================
    // SECTION 7: FUNCTION REFERENCE - ORBIT & PASS FILTERS
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:function:passes")]
    public string FilterDslFunctionPasses() =>
        """
            passes(count)

            Filters passing patterns with exactly 'count' passes per period.

            Parameters:
            - count: Number of passes (integer)

            In 4-handed siteswap, odd numbers (5, 7, 9...) are passes.

            Examples:
            - passes(2) - exactly two passes per period
            - passes(1) - one-count passing pattern

            Use case: Find passing patterns with specific pass frequency.
            """;

    [McpServerResource]
    [Description("filter:dsl:function:passRatio")]
    public string FilterDslFunctionPassRatio() =>
        """
            passRatio(min, max)

            Filters passing patterns by the ratio of passes to total throws.

            Parameters:
            - min: Minimum pass ratio (0.0 to 1.0)
            - max: Maximum pass ratio (0.0 to 1.0)

            Examples:
            - passRatio(0.3, 0.5) - 30% to 50% of throws are passes
            - passRatio(0.5, 1.0) - at least half are passes

            Use case: Find patterns with balanced self/pass ratio.
            """;

    // ====================================================================================
    // SECTION 8: EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:examples:basic")]
    public string FilterDslExamplesBasic() =>
        """
            Basic Filter Examples:

            1. Simple keywords:
               - ground           → Ground-state patterns only
               - noZeros          → No empty hand pauses
               - prime            → Prime (non-composite) patterns

            2. Single function calls:
               - maxHeight(7)     → No throws higher than 7
               - minOcc(5, 2)     → At least two 5s
               - exactOcc(1, 1)   → Exactly one zip

            3. Simple combinations:
               - ground AND noZeros
               - NOT excited
            """;

    [McpServerResource]
    [Description("filter:dsl:examples:pattern")]
    public string FilterDslExamplesPattern() =>
        """
            Pattern Matching Examples:

            1. Exact pattern match:
               - pattern(5, 3, 1)       → Only "531"

            2. Wildcard patterns:
               - pattern(5, *, 1)       → "5x1" where x is any throw
               - pattern(*, 4, *)       → Any pattern with 4 in middle

            3. Pass/Self patterns (requires numberOfJugglers):
               - pattern(p, s, p)       → Pass, Self, Pass sequence
               - pattern(5, p, *)       → 5, then Pass, then any
               - pattern(p, p, s, s)    → Two Passes, then two Selfs

            4. Prefix/suffix matching:
               - startsWith(7)          → Patterns starting with 7
               - endsWith(1)            → Patterns ending with zip
               - contains(4, 4)         → Has consecutive 4s

            5. Combined with filters:
               - pattern(5, *, 1) AND ground
               - pattern(p, s, p) AND minOcc(5,1)
               - startsWith(7) AND maxHeight(9)
            """;

    [McpServerResource]
    [Description("filter:dsl:examples:occurrence")]
    public string FilterDslExamplesOccurrence() =>
        """
            Occurrence Filter Examples:

            1. Minimum occurrences:
               - minOcc(5, 2)           → At least two 5s
               - minOcc(7, 1)           → At least one 7

            2. Maximum occurrences:
               - maxOcc(2, 1)           → At most one hold
               - maxOcc(0, 0)           → No zeros (same as noZeros)

            3. Exact occurrences:
               - exactOcc(4, 2)         → Exactly two 4s

            4. Range occurrences:
               - occ(5, 1, 3)           → Between 1 and 3 fives
               - occ([5,7], 2, 4)       → 2-4 high throws (5s or 7s)

            5. Combined:
               - minOcc(5, 2) AND maxOcc(7, 1)
               - exactOcc(5, 2) AND ground
            """;

    [McpServerResource]
    [Description("filter:dsl:examples:complex")]
    public string FilterDslExamplesComplex() =>
        """
            Complex Filter Examples:

            1. Multiple conditions with AND:
               - ground AND noZeros AND prime
               - maxHeight(7) AND minOcc(5, 2) AND ground

            2. Alternative conditions with OR:
               - ground OR excited
               - minOcc(5, 2) OR minOcc(7, 1)

            3. Negation:
               - NOT ground              → Excited-state only
               - NOT hasZeros            → Same as noZeros
               - NOT (hasZeros OR ground)

            4. Mixed operators with parentheses:
               - (ground OR excited) AND noZeros
               - (minOcc(5, 2) OR minOcc(7, 1)) AND maxHeight(9)
               - ground AND (pattern(5, *, 1) OR pattern(*, 3, 1))

            5. Deeply nested expressions:
               - ((ground AND prime) OR excited) AND noZeros
               - NOT ((minOcc(5, 3) AND ground) OR hasZeros)
            """;

    [McpServerResource]
    [Description("filter:dsl:examples:passing")]
    public string FilterDslExamplesPassing() =>
        """
            Passing Pattern Filter Examples:

            1. Basic passing filters:
               - passes(2)              → Two passes per period

            2. Pass ratio:
               - passRatio(0.3, 0.5)    → 30-50% passes

            3. Combined passing filters:
               - passes(1) AND maxHeight(9)
               - passRatio(0.4, 0.6) AND noZeros
               - passes(2) AND ground
            """;

    [McpServerResource]
    [Description("filter:dsl:examples:use-cases")]
    public string FilterDslExamplesUseCases() =>
        """
            Practical Use Case Examples:

            1. Beginner-friendly patterns:
               - ground AND maxHeight(5) AND noZeros

            2. Intermediate patterns with specific throws:
               - minOcc(5, 2) AND maxHeight(7) AND ground

            3. Advanced excited-state patterns:
               - excited AND prime AND minHeight(5)

            4. Patterns for specific practice:
               - pattern(5, *, 1) AND ground     → 5x1 variations
               - contains(4, 4) AND noZeros      → Double-4 patterns

            5. Passing patterns for mixed skill:
               - passes(1) AND ground AND maxHeight(7)

            6. Excluding unwanted patterns:
               - ground AND NOT hasZeros AND NOT pattern(3)
            """;

    // ====================================================================================
    // SECTION 9: QUICK REFERENCE
    // ====================================================================================

    [McpServerResource]
    [Description("filter:dsl:reference:all-functions")]
    public string FilterDslReferenceAllFunctions() =>
        """
            Filter-DSL Function Quick Reference:

            OCCURRENCE FILTERS:
            - minOcc(throw, count)     → At least count occurrences
            - maxOcc(throw, count)     → At most count occurrences
            - exactOcc(throw, count)   → Exactly count occurrences
            - occ(throw, min, max)     → Between min and max occurrences

            PATTERN FILTERS:
            - pattern(values...)       → Match pattern with wildcards (*)
            - startsWith(values...)    → Pattern starts with values
            - endsWith(values...)      → Pattern ends with values
            - contains(values...)      → Pattern contains subsequence

            HEIGHT FILTERS:
            - height(min, max)         → All throws in range
            - maxHeight(max)           → No throw exceeds max
            - minHeight(min)           → All throws at least min

            STATE FILTERS:
            - ground                   → Ground-state only
            - excited                  → Excited-state only
            - state(values...)         → Specific starting state

            PROPERTY FILTERS:
            - noZeros                  → No zero throws
            - hasZeros                 → Has zero throws

            ORBIT & PASS FILTERS:
            - orbits(count)            → Exactly count orbits
            - passes(count)            → Exactly count passes
            - passRatio(min, max)      → Pass ratio in range

            OPERATORS:
            - AND                      → Both conditions
            - OR                       → Either condition
            - NOT                      → Negate condition
            - ( )                      → Group expressions
            """;

    [McpServerResource]
    [Description("filter:dsl:reference:cheat-sheet")]
    public string FilterDslReferenceCheatSheet() =>
        """
            Filter-DSL Cheat Sheet:

            SYNTAX:
            condition AND condition    → Both must be true
            condition OR condition     → At least one true
            NOT condition              → Condition must be false
            (condition)                → Group for precedence

            ARGUMENTS:
            5                          → Number
            *                          → Wildcard (any value)
            [5,7,9]                    → Number list (any of these)

            COMMON PATTERNS:
            ground                     → Easy to enter/exit
            ground AND noZeros         → Continuous throwing
            maxHeight(7)               → Manageable difficulty
            minOcc(5, 2)               → Multiple high throws
            pattern(5, *, 1)           → Specific structure
            prime AND ground           → Fundamental patterns

            TIPS:
            - Keywords are case-insensitive (AND = and = And)
            - Spaces around operators are required (A AND B, not AandB)
            - Use parentheses to clarify complex expressions
            - Wildcards (*) only work in pattern()
            """;
}
