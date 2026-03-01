using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Resources;

[McpServerResourceType]
public class SiteswapGeneratorGuideResource
{
    [McpServerResource]
    [Description("Overview of the Siteswap Generator - basic parameters and functionality")]
    public string GetGeneratorOverview() =>
        """
            # Siteswap Generator Overview

            The Siteswap Generator creates valid siteswap patterns based on defined parameters.
            It uses a backtracking algorithm with intelligent filtering for efficient search.

            ## Basic Parameters

            | Parameter | Description | Example |
            |-----------|-------------|---------|
            | period | Length of the pattern (number of throws) | 3 for "531" |
            | numberOfObjects | Number of balls | 3 |
            | minHeight | Minimum throw height | 0 (allows empty hands) |
            | maxHeight | Maximum throw height | 9 (typical for solo) |
            | maxResults | Maximum number of results to return | 100 |
            | timeoutSeconds | Time limit in seconds | 30 |

            ## Simple Example

            All 3-ball siteswaps with period 3 and throw heights 0-9:
            - period: 3
            - numberOfObjects: 3
            - minHeight: 0
            - maxHeight: 9

            Result: 423, 441, 522, 531, 603, 612, 711, 900, ...

            ## Passing Siteswaps

            For passing patterns, `numberOfJugglers` is required:
            - 2 jugglers: Throws ≡ 0 mod 2 are selfs, odd throws are passes
            - 3 jugglers: Throws ≡ 0 mod 3 are selfs, others are passes

            Example for 2 jugglers with 7 clubs:
            - period: 4
            - numberOfObjects: 7
            - numberOfJugglers: 2
            - minHeight: 2
            - maxHeight: 10
            """;

    [McpServerResource]
    [Description("Occurrence filters - filtering by frequency of specific throw heights")]
    public string GetOccurrenceFilters() =>
        """
            # Occurrence Filters (Frequency Filters)

            These filters control how often certain throw heights appear in the pattern.

            ## minOccurrence (Minimum Frequency)

            Requires a number to appear at least X times.

            ### Format
            - Simple: `number:count` → `3:2` = number 3 at least 2 times
            - Multiple numbers: `num1,num2:count` → `3,4:2` = 3s or 4s together at least 2 times
            - Multiple rules: `num1:count1,num2:count2` → `3:2,5:1` = 3s at least 2x AND 5s at least 1x
            - OR logic: `rule1|rule2` → `3:2|4:1` = 3s at least 2x OR 4s at least 1x

            ### Examples
            | Filter | Meaning | Possible Results |
            |--------|---------|------------------|
            | `3:2` | At least two 3s | 333, 363, 531, ... |
            | `5:1` | At least one 5 | 531, 552, 645, ... |
            | `3,4:3` | At least 3 throws of height 3 or 4 | 4443, 3444, 3343, ... |
            | `3:1,5:1` | At least one 3 AND at least one 5 | 531, 5334, 5353, ... |
            | `3:2\|5:2` | At least two 3s OR at least two 5s | 333, 555, 5535, 3533, ... |

            ## maxOccurrence (Maximum Frequency)

            Limits how often a number can appear.

            ### Examples
            | Filter | Meaning | Possible Results |
            |--------|---------|------------------|
            | `4:1` | At most one 4 | 531, 612, 333, ... |
            | `5:0` | No 5s allowed | 441, 333, 612, ... |
            | `3,4:2` | Max 2 throws of height 3 or 4 | 531, 612, 534, ... |

            ## exactOccurrence (Exact Frequency)

            Requires exactly X occurrences of a number.

            ### Examples
            | Filter | Meaning | Possible Results |
            |--------|---------|------------------|
            | `5:2` | Exactly two 5s | 5517, 5553, 5562, ... |
            | `3:3` | Exactly three 3s | 333, 3330, 33300, ... |
            | `4,5:3` | Exactly 3 throws of height 4 or 5 | 4455, 5544, 4554, ... |

            ## Combining Filters

            Filters can be combined (AND logic):
            - minOccurrence: `5:1`
            - maxOccurrence: `5:2`
            → Results in patterns with exactly 1 or 2 fives
            """;

    [McpServerResource]
    [Description("Pattern filters - filtering by specific pattern templates")]
    public string GetPatternFilters() =>
        """
            # Pattern Filters (Pattern Templates)

            Pattern filters define which throw heights must appear at which positions.

            ## Basic Format

            Comma-separated numbers: `value1,value2,value3,...`

            ## Special Values

            | Value | Meaning | Description |
            |-------|---------|-------------|
            | -1 | Any | Any value allowed |
            | -2 | Any Pass | Any pass (odd numbers with 2 jugglers) |
            | -3 | Any Self | Any self (even numbers with 2 jugglers) |

            ## Examples

            ### Fixed Positions
            | Pattern | Meaning | Possible Results |
            |---------|---------|------------------|
            | `5,3,1` | Exactly 531 | 531 |
            | `5,-1,1` | 5 at pos 0, 1 at pos 2, any in between | 501, 511, 521, 531, 541, 551, ... |
            | `-1,3,-1` | 3 at position 1 | 030, 130, 230, 330, 430, 531, 631, ... |

            ### Passing Patterns (with numberOfJugglers=2)
            | Pattern | Meaning | Possible Results |
            |---------|---------|------------------|
            | `-2,-3` | Pass, then self | 72, 74, 76, 92, 94, ... |
            | `-3,-2,-3,-2` | Self-pass-self-pass | 7474, 7672, 9474, ... |
            | `9,-1,6,-1` | 9 at pos 0, 6 at pos 2 | 9666, 9686, 9696, ... |

            ## OR Logic for Patterns

            Connect multiple patterns with `|` (pipe):

            | Filter | Meaning |
            |--------|---------|
            | `5,3,1\|4,4,1` | 531 OR 441 |
            | `5,-1,-1\|6,-1,-1` | Starts with 5 OR starts with 6 |

            ## Difference from flexiblePattern

            - **pattern**: Positions are fixed (position 0, 1, 2, ...)
            - **flexiblePattern**: Groups can appear at various positions
            """;

    [McpServerResource]
    [Description("Flexible pattern filters - complex pattern templates with groups")]
    public string GetFlexiblePatternFilters() =>
        """
            # Flexible Pattern Filters

            Flexible patterns define groups of values that must appear in the pattern,
            but not at fixed positions.

            ## Basic Format

            Semicolon-separated groups: `group1;group2;...`
            Each group: Comma-separated alternatives

            ## Format Explanation

            `3,4;5,6` means:
            - Group 1: Value 3 OR 4
            - Group 2: Value 5 OR 6
            - These groups appear consecutively in the pattern

            ## Special Values (same as pattern)

            | Value | Meaning |
            |-------|---------|
            | -1 | Any value |
            | -2 | Any pass |
            | -3 | Any self |

            ## Examples

            ### Solo Juggling
            | Flexible Pattern | Meaning | Possible Results |
            |------------------|---------|------------------|
            | `5;3;1` | A 5, a 3, a 1 somewhere | 531, 513, 351, 315, 153, 135 |
            | `5,6;3,4;1,2` | High throw, medium, low | 531, 642, 631, ... |
            | `-1;5;-1` | Any, a 5, any | X5X for any X |

            ### Passing (numberOfJugglers=2)
            | Flexible Pattern | Meaning | Possible Results |
            |------------------|---------|------------------|
            | `-2,-3;-3,-2` | Pass-self or self-pass sequence | 7474, 4747, 9292, ... |
            | `-2;-2;-3;-3` | 2 passes, 2 selfs | 7744, 9944, ... |

            ## OR Logic

            | Filter | Meaning |
            |--------|---------|
            | `5;3;1\|4;4;1` | 531 OR 441 sequence |
            | `-2,-3\|-3,-2` | Pass-self OR self-pass |

            ## Difference from rotationAwarePattern

            - **flexiblePattern**: Global pattern, applies to all jugglers together
            - **rotationAwarePattern**: Pattern from perspective of a specific juggler
            """;

    [McpServerResource]
    [Description("State filters - filtering by juggling states")]
    public string GetStateFilters() =>
        """
            # State Filters

            State filters limit the search to patterns that pass through a specific state.

            ## What is a State?

            A state describes the "landing schedule" - which beats will have a ball landing:
            - 1 or true: A ball will land on this beat
            - 0 or false: No ball lands on this beat

            ## Basic Format

            Comma-separated 0/1 values: `1,1,0,0` or `1,1,0,0,0`

            ## Important States

            ### Ground State
            | Balls | Ground State | Description |
            |-------|--------------|-------------|
            | 3 | `1,1,1,0,0` | 3 balls in first 3 slots |
            | 4 | `1,1,1,1,0` | 4 balls in first 4 slots |
            | 5 | `1,1,1,1,1,0` | 5 balls in first 5 slots |

            ### Excited States
            | State | Example | Description |
            |-------|---------|-------------|
            | `1,0,1,1,0` | 3-ball | Gap in second slot |
            | `1,1,0,1,0` | 3-ball | Gap in third slot |
            | `1,0,1,1,1,0` | 4-ball | Gap in second slot |

            ## Examples

            ### Ground State Patterns (3 balls)
            - state: `1,1,1,0,0`
            - Results: 441, 531, 333, 522, 450, ...

            ### Excited State Patterns (3 balls)
            - state: `1,0,1,1,0`
            - Results: Patterns that pass through this state

            ## OR Logic for States

            | Filter | Meaning |
            |--------|---------|
            | `1,1,1,0,0\|1,0,1,1,0` | Ground state OR specific excited state |

            ## Practical Applications

            1. **Finding transitions**: 
               State filters help find patterns that switch between specific states

            2. **Learnability**: 
               Ground state patterns are easier to start and stop

            3. **Combination with other filters**: 
               State + Occurrence = Specific patterns in desired state
            """;

    [McpServerResource]
    [Description("Passing filters - special filters for passing siteswaps")]
    public string GetPassingFilters() =>
        """
            # Passing Filters

            These filters are specifically designed for passing siteswaps (multiple jugglers).

            ## Basics

            In passing siteswaps, the throw height modulo number of jugglers 
            determines the target juggler:

            | Jugglers | Self | Pass to J1 | Pass to J2 |
            |----------|------|------------|------------|
            | 2 | Even (0,2,4,6,...) | Odd (1,3,5,7,...) | - |
            | 3 | ≡0 mod 3 | ≡1 mod 3 | ≡2 mod 3 |

            ## numberOfPasses (Number of Passes)

            Exact number of passes in the pattern.

            ### Examples (2 jugglers)
            | Parameter | Meaning | Examples |
            |-----------|---------|----------|
            | numberOfPasses: 0 | Only selfs | 8888, 6666, 4444 |
            | numberOfPasses: 1 | Exactly 1 pass | 7772, 9944, 7744 |
            | numberOfPasses: 2 | Exactly 2 passes | 7474, 7744, 9292 |
            | numberOfPasses: 4 | All passes (for period 4) | 7777, 9999, 5555 |

            ## jugglerIndex (Juggler Index)

            For personalized filters - which juggler is being considered (0-based).

            | Jugglers | jugglerIndex | Meaning |
            |----------|--------------|---------|
            | 2 | 0 | Juggler A |
            | 2 | 1 | Juggler B |
            | 3 | 0, 1, 2 | Juggler A, B, C |

            ## rotationAwarePattern

            Pattern from the perspective of a specific juggler (considers rotation).

            ### Example
            - numberOfJugglers: 2
            - jugglerIndex: 0
            - rotationAwarePattern: `-2,-3;-3,-2`

            Finds patterns where juggler 0 throws the sequence pass-self-self-pass.

            ## personalizedNumberFilter

            Filter for specific throws by a juggler.

            ### Format
            `numbers:count:type:from` where:
            - numbers: Comma-separated throw heights
            - count: Number of occurrences
            - type: `exact`, `atleast`, or `atmost`
            - from: Juggler index (0-based)

            ### Examples
            | Filter | Meaning |
            |--------|---------|
            | `7:2:exact:0` | Juggler 0 throws exactly two 7s |
            | `9:1:atleast:1` | Juggler 1 throws at least one 9 |
            | `5,7:3:atmost:0` | Juggler 0 throws max 3 fives or sevens |

            ## Combination Example

            Find 7-club passing with 4 passes per person:
            - period: 8
            - numberOfObjects: 7
            - numberOfJugglers: 2
            - numberOfPasses: 4
            - minOccurrence: `7:4` (at least 4 singles)
            """;

    [McpServerResource]
    [Description("Not filter and logical combinations - negation and complex logic")]
    public string GetNotFilterAndLogic() =>
        """
            # Not Filter and Logical Combinations

            ## Not Filter (Negation Filter)

            The not filter excludes certain patterns.

            ### Format
            `filterType:value` or with OR: `filterType:value|filterType:value`

            ### Available Filter Types
            | Type | Description |
            |------|-------------|
            | minOccurrence | Negates minimum frequency |
            | maxOccurrence | Negates maximum frequency |
            | exactOccurrence | Negates exact frequency |
            | pattern | Negates pattern |
            | state | Negates state |
            | flexiblePattern | Negates flexible pattern |
            | numberOfPasses | Negates pass count |

            ### Examples
            | Not Filter | Meaning | Effect |
            |------------|---------|--------|
            | `minOccurrence:5:2` | NOT at least 2 fives | Max 1 five or none |
            | `pattern:5,3,1` | NOT 531 | Everything except 531 |
            | `exactOccurrence:3:3` | NOT exactly 3 threes | 0,1,2 or 4+ threes |
            | `state:1,1,1,0,0` | NOT ground state | Only excited state patterns |

            ### OR in Not Filter
            | Not Filter | Meaning |
            |------------|---------|
            | `pattern:5,3,1\|pattern:4,4,1` | NOT (531 OR 441) |
            | `minOccurrence:5:2\|minOccurrence:6:2` | NOT (2+ fives OR 2+ sixes) |

            ## Logical Combinations (Overview)

            ### AND Logic (Default)
            All top-level filters are combined with AND:
            - minOccurrence: `5:1` + maxOccurrence: `5:2` 
            - → Exactly 1 or 2 fives

            ### OR Logic (with Pipe)
            Within a filter using `|`:
            - minOccurrence: `5:2|6:2`
            - → At least 2 fives OR at least 2 sixes

            ## Complex Examples

            ### "Find 3-ball patterns without shower"
            - period: 3
            - numberOfObjects: 3
            - notFilter: `pattern:5,1,1|pattern:1,5,1|pattern:1,1,5`

            ### "Excited state, but not specific ones"
            - state: `1,0,1,1,0|1,1,0,1,0` (OR for multiple excited states)
            - notFilter: `state:1,0,1,1,0` (excludes one)

            ### "No boring patterns"
            - minOccurrence: `3:1,5:1` (at least one 3 AND one 5)
            - notFilter: `exactOccurrence:3:3` (not only 333)
            """;

    [McpServerResource]
    [Description("Special filters and advanced options")]
    public string GetSpecialFilters() =>
        """
            # Special Filters and Advanced Options

            ## useDefaultFilter

            Activates the "RightAmountOfBalls" filter (default: true).

            | Value | Effect |
            |-------|--------|
            | true | Only valid siteswaps with correct ball count |
            | false | Invalid combinations also possible |

            **Recommendation**: Always keep true, except for special analyses.

            ## useNoFilter

            Deactivates all automatic filters.

            | Value | Effect |
            |-------|--------|
            | true | Accepts all combinations |
            | false | Normal filtering (default) |

            **Warning**: Produces many invalid results!

            ## Efficient Filtering

            ### Filter Evaluation Order
            1. Occurrence filters (fast)
            2. State filters (medium)
            3. Pattern filters (slow)
            4. Flexible pattern (slow)

            ### Performance Tips
            - Set strict occurrence filters early → faster search
            - Limit maxHeight → exponentially fewer possibilities
            - Keep period short → fewer combinations

            ### Example: Efficient Search
            Search for 5-ball patterns with exactly one 9:
            ```
            Good:
            - exactOccurrence: 9:1 (limits early)
            - maxHeight: 9 (reduces search space)

            Bad:
            - Only pattern: -1,-1,-1,-1,9 (must search all)
            ```

            ## Timeout and Limits

            | Parameter | Recommended Value | Description |
            |-----------|-------------------|-------------|
            | maxResults | 100-1000 | More = longer |
            | timeoutSeconds | 30-60 | Abort for complex searches |

            ## Debugging

            If you get unexpectedly few results:
            1. Test filters individually
            2. Reduce period/maxHeight
            3. Loosen occurrence filters
            4. Check NOT filters (may exclude too much)
            """;
}
