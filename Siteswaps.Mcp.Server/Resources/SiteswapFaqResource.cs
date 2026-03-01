using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Resources;

[McpServerResourceType]
public class SiteswapFaqResource
{
    [McpServerResource]
    [Description("Basic concepts of siteswap notation - what it describes and how it works")]
    public string GetBasics() =>
        """
            # Siteswap Basics

            The idea behind siteswap is to keep track of the order that balls are thrown and caught, 
            and ONLY that. Since no matter how you juggle, you're catching and throwing in SOME order, 
            there will always be some siteswap describing it.

            ## What Siteswap Describes
            - The order of throws and catches
            - How long each ball is in the air (relative to other throws)
            - The rhythm of the pattern

            ## What Siteswap Does NOT Describe
            - WHERE balls are thrown from and to (like in Mills' Mess)
            - The visual appearance of the pattern
            - Specific throwing techniques

            ## Key Principle
            While each ball is in its arc, a certain number of other throws occur. 
            In a 3-ball cascade, while each ball is in its arc, exactly TWO other throws occur.
            This is why the cascade is notated as "3" - each throw allows for 2 intervening throws 
            before that hand needs to catch again.
            """;

    [McpServerResource]
    [Description("How to read and write vanilla siteswap notation")]
    public string GetNotation() =>
        """
            # Siteswap Notation (Vanilla)

            ## Numbers and Their Meaning
            Each number represents how many "beats" until that hand needs to be free again.

            - 0: Empty hand (no throw)
            - 1: Quick pass to the other hand (zip)
            - 2: Hold the ball (or very small throw that returns to same hand)
            - 3: Standard cascade throw (crosses to other hand)
            - 4: Fountain throw (returns to same hand, like in 4-ball fountain)
            - 5: High crossing throw
            - 6: High same-hand throw
            - 7, 8, 9...: Even higher throws
            - a, b, c...: For values 10, 11, 12... (hex notation)

            ## Odd vs Even Numbers
            - ODD numbers (1, 3, 5, 7...): Ball crosses to the OTHER hand
            - EVEN numbers (2, 4, 6, 8...): Ball returns to the SAME hand

            ## The Averaging Rule
            The average of all numbers in a valid siteswap equals the number of balls.

            Examples:
            - 3 → average is 3 → 3 balls (cascade)
            - 441 → (4+4+1)/3 = 3 → 3 balls
            - 531 → (5+3+1)/3 = 3 → 3 balls
            - 534 → (5+3+4)/3 = 4 → 4 balls
            - 744 → (7+4+4)/3 = 5 → 5 balls
            """;

    [McpServerResource]
    [Description("Understanding ground state vs excited state patterns")]
    public string GetStates() =>
        """
            # Juggling States

            ## What is a State?
            A "juggling state" or "landing schedule" describes which beats will have balls landing.
            It's represented as a series of x's and -'s:
            - x = a ball will land on this beat
            - - = no ball landing on this beat

            Example: For 3 balls, the ground state is "xx-" meaning:
            - A ball lands on beat 0 (must throw)
            - A ball lands on beat 1 (must throw)
            - No ball lands on beat 2 (hand is free)

            ## Ground State vs Excited State

            ### Ground State
            The "basic" state for a given number of balls. For n balls, it's n consecutive x's.
            - 3 balls: xx-
            - 4 balls: xx--
            - 5 balls: xxx--

            Ground state patterns can be entered and exited easily. You can start and stop 
            juggling them naturally.

            ### Excited State
            Any state that is NOT the ground state. These patterns require a "setup" to enter
            and a "transition" to exit cleanly.

            Example: The pattern "441" keeps you in ground state (xx-), but "531" is also 
            ground state. However, "51" (3-ball shower) cycles through excited states.

            ## Finding Transitions
            To get from one pattern to another, you need to find a transition sequence.
            The state diagram shows all possible transitions between states for a given 
            number of balls.
            """;

    [McpServerResource]
    [Description("Common siteswap patterns with explanations")]
    public string GetExamples() =>
        """
            # Common Siteswap Examples

            ## 3-Ball Patterns
            - 3: Basic cascade
            - 333: Same as "3" (just repeated)
            - 423: One high throw (4), one low crossing throw (2 is hold), cascade throw
            - 441: Two fountain throws followed by a zip
            - 531: High-low-zip combination
            - 51: 3-ball shower (high throws one way, quick passes back)
            - 50505: 3-ball snake (shower with gaps)
            - 4440: "3 out of 4 fountain" - pause in 4-ball pattern

            ## 4-Ball Patterns
            - 4: Basic fountain
            - 534: Adds a crossing 5 and 3 to fountain
            - 633: Two high same-hand throws with cascade
            - 71: 4-ball shower
            - 7531: Popular 4-ball pattern mixing heights

            ## 5-Ball Patterns
            - 5: Basic 5-ball cascade
            - 645: Common 5-ball variation
            - 744: High throw with two fountain throws
            - 91: 5-ball shower

            ## Site Swaps (the original meaning!)
            The name comes from SWAPPING SITES where balls land:

            Starting with: 55 (cascade)
            Swap positions: 55 → 64 (swap landing times)

            More examples:
            - 555 → 753 (swap first and third)
            - 5555 → 8552 (swap first and fourth)
            - 534 → 552 (swap second and third)
            - 534 → 444 (swap first and second)
            """;

    [McpServerResource]
    [Description(
        "Common pitfalls and troubleshooting when working with siteswaps and the generator"
    )]
    public string GetTroubleshooting() =>
        """
            # Common Mistakes & Troubleshooting

            ## 1. Verwechselung von passOrSelf und interface (German Context)
            In the analysis of a siteswap, you'll see two rhythm-related fields:
            - `passOrSelf` (Array): This is the **Throw Sequence** (Pattern).
            - `interface` (String): This is the **Landing Sequence**.

            **Don't call it `interfacePassOrSelf`!** The API field is named `passOrSelf`.

            ## 2. Filter Syntax Errors
            - **Incorrect**: `interface(psspss)` (sent as a single string)
            - **Correct**: `interface(p, s, s, p, s, s)` (comma-separated arguments)

            ## 3. Pattern vs. Interface Compatibility
            - If you want to find siteswaps with the same **throwing rhythm** → use `pattern()`.
            - If you want to find siteswaps with the same **landing rhythm** → use `interface()`.
            They are different! Two siteswaps can have the same pattern but different interfaces.

            ## 4. Validating Combinations
            To check if two jugglers can perform different siteswaps together:
            1. Both siteswaps must be valid (use `validate_siteswap`).
            2. They must share the SAME `interface` to be compatible.
            """;

    [McpServerResource]
    [Description(
        "Information about compatability of siteswaps. The user may be interested in generating patterns of different difficulty."
    )]
    public string GetCompatability() =>
        """
            # Compatability

            ## Pattern vs. Interface Compatibility
            - If you want to find siteswaps with the same **throwing rhythm** → use `pattern()`.
            - If you want to find siteswaps with the same **landing rhythm** → use `interface()`.
            They are different! Two siteswaps can have the same pattern but different interfaces.

            ## Validating Combinations
            To check if two jugglers can perform different siteswaps together:
            1. Both siteswaps must be valid (use `validate_siteswap`).
            2. They must share the SAME `interface` to be compatible.
            """;
}
