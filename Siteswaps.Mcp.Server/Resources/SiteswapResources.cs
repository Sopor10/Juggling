using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Resources;

[McpServerResourceType]
public class SiteswapResources
{
    // ====================================================================================
    // SECTION 1: SITESWAP DEFINITIONS
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:definition:siteswap")]
    public string SiteswapDefinitionSiteswap() =>
        "Siteswap is a numeric notation system for juggling patterns. A siteswap sequence describes the order in which objects are thrown and caught, and only that.";

    [McpServerResource]
    [Description("siteswap:definition:vanilla-siteswap")]
    public string SiteswapDefinitionVanillaSiteswap() =>
        "Vanilla siteswap describes asynchronous patterns where hands alternate throwing, one object per hand per throw time.";

    [McpServerResource]
    [Description("siteswap:definition:throw-value")]
    public string SiteswapDefinitionThrowValue() =>
        "A siteswap value n indicates that the object will be thrown again n beats later. n corresponds to the number of beats that pass before the same object is thrown again.";

    [McpServerResource]
    [Description("siteswap:definition:beat")]
    public string SiteswapDefinitionBeat() =>
        "A beat is a uniformly spaced time unit at which throws occur. All throws happen at strictly regular beats.";

    [McpServerResource]
    [Description("siteswap:definition:period")]
    public string SiteswapDefinitionPeriod() =>
        "The period of a siteswap is the number of beats before the sequence repeats, ignoring which hand throws.";

    [McpServerResource]
    [Description("siteswap:definition:full-period")]
    public string SiteswapDefinitionFullPeriod() =>
        "The full period is the number of beats before the pattern repeats when hands are considered distinguishable but props are not.";

    [McpServerResource]
    [Description("siteswap:definition:throw-0")]
    public string SiteswapDefinitionThrow0() =>
        "A 0 throw is a pause with an empty hand. The hand that would throw on this beat does nothing because it has nothing to throw.";

    [McpServerResource]
    [Description("siteswap:definition:throw-1")]
    public string SiteswapDefinitionThrow1() =>
        "A 1 throw is a quick pass straight across to the other hand, also called a handoff, feed, zip, or vamp.";

    [McpServerResource]
    [Description("siteswap:definition:throw-2")]
    public string SiteswapDefinitionThrow2() =>
        "A passive 2 throw is a pause with an object held in the hand that would otherwise be thrown on this beat.";

    [McpServerResource]
    [Description("siteswap:definition:odd-even-crossing")]
    public string SiteswapDefinitionOddEvenCrossing() =>
        "In vanilla siteswap: Odd numbers represent throws that go to the other hand (crossing). Even numbers represent throws that are caught by the same hand (non-crossing).";

    [McpServerResource]
    [Description("siteswap:definition:alphanumeric-notation")]
    public string SiteswapDefinitionAlphanumericNotation() =>
        "Values greater than 9 are written as letters: 10=a, 11=b, 12=c, etc. This is not hexadecimal, but serves to avoid ambiguous multi-digit numbers.";

    [McpServerResource]
    [Description("siteswap:definition:ground-state")]
    public string SiteswapDefinitionGroundState() =>
        "Ground-state siteswaps can be entered directly from the basic pattern (cascade/fountain) without special transition throws.";

    [McpServerResource]
    [Description("siteswap:definition:excited-state")]
    public string SiteswapDefinitionExcitedState() =>
        "Excited-state siteswaps require special transition throws to reach them from the basic pattern.";

    [McpServerResource]
    [Description("siteswap:definition:orbit")]
    public string SiteswapDefinitionOrbit() =>
        "An orbit is a closed loop of throws that a single object or group of objects follows. Each object stays within its orbit.";

    [McpServerResource]
    [Description("siteswap:definition:juggling-state")]
    public string SiteswapDefinitionJugglingState() =>
        "A juggling state (landing schedule) describes at which future beats objects will land. Marked with x/1 for landing, -/0 for no landing.";

    [McpServerResource]
    [Description("siteswap:definition:synchronous-siteswap")]
    public string SiteswapDefinitionSynchronousSiteswap() =>
        "Synchronous siteswaps describe patterns where both hands throw simultaneously. Throws are grouped in parentheses: (right, left).";

    [McpServerResource]
    [Description("siteswap:definition:multiplex")]
    public string SiteswapDefinitionMultiplex() =>
        "A multiplex throw is when multiple objects are thrown simultaneously from one hand. Notated in square brackets: [33] means two objects are thrown simultaneously.";

    [McpServerResource]
    [Description("siteswap:definition:passing-siteswap")]
    public string SiteswapDefinitionPassingSiteswap() =>
        "Passing siteswap describes patterns with multiple jugglers. Instead of 2 hands in total we have 2 hands per juggler. The normal hand order is Ar Br Cr...Al Bl Cl for juggler A,B,C with hands r and l. A fraction means the throw is passed to a partner.";

    [McpServerResource]
    [Description("siteswap:definition:4-handed-siteswap")]
    public string SiteswapDefinition4HandedSiteswap() =>
        "4-handed siteswap describes fully asynchronous passing patterns where all four hands throw alternately. Even numbers are selfs, odd numbers are passes.";

    [McpServerResource]
    [Description("siteswap:definition:local-siteswap")]
    public string SiteswapDefinitionLocalSiteswap() =>
        "A local siteswap shows only the throws of one person in a passing pattern. The global siteswap shows all throws of both people in order.";

    [McpServerResource]
    [Description("siteswap:definition:global-siteswap")]
    public string SiteswapDefinitionGlobalSiteswap() =>
        "A global siteswap shows all throws of all jugglers in the order they are made. In 4-handed siteswap, the order is typically: Juggler A right, Juggler B right, Juggler A left, Juggler B left.";

    [McpServerResource]
    [Description("siteswap:definition:hijacking")]
    public string SiteswapDefinitionHijacking() =>
        "Hijacking is a passing technique where one passer actively changes the pattern and passively transitions their partner into a compatible pattern. The active passer gains one club locally (+1).";

    [McpServerResource]
    [Description("siteswap:definition:lowjacking")]
    public string SiteswapDefinitionLowjacking() =>
        "Lowjacking is a passing technique where one passer actively changes the pattern and passively transitions their partner into a compatible pattern. The active passer loses one club locally (-1).";

    [McpServerResource]
    [Description("siteswap:definition:space-time-diagram")]
    public string SiteswapDefinitionSpaceTimeDiagram() =>
        "A space-time diagram (ladder diagram) shows the flight paths of objects over time. The horizontal axis is time, the vertical axis is space between the hands.";

    [McpServerResource]
    [Description("siteswap:definition:prime-siteswap")]
    public string SiteswapDefinitionPrimeSiteswap() =>
        "A prime siteswap is a siteswap whose path in the state diagram does not traverse any state more than once. Siteswaps that are not prime are called composite.";

    [McpServerResource]
    [Description("siteswap:definition:composite-siteswap")]
    public string SiteswapDefinitionCompositeSiteswap() =>
        "A composite siteswap can be split into shorter valid patterns with the same number of props. Example: 44404413 can be split into 4440, 441, and 3.";

    [McpServerResource]
    [Description("siteswap:definition:transition-throw")]
    public string SiteswapDefinitionTransitionThrow() =>
        "A transition throw is a throw that is not part of the pattern being transitioned to or from. It is used to switch between patterns.";

    [McpServerResource]
    [Description("siteswap:definition:programming")]
    public string SiteswapDefinitionProgramming() =>
        "Programming is a collective term for hijacking and lowjacking. A passer 'programs' their partner by actively changing the pattern.";

    // ====================================================================================
    // SECTION 2: SITESWAP RULES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:rule:averaging-theorem")]
    public string SiteswapRuleAveragingTheorem() =>
        "The number of objects in a siteswap pattern is the average of the numbers in the sequence. Formula: Number of objects = (Sum of all values) / (Number of values). The average must be an integer.";

    [McpServerResource]
    [Description("siteswap:rule:validity-condition")]
    public string SiteswapRuleValidityCondition() =>
        "A vanilla siteswap is valid if no two throws land at the same time. Equivalent: At each beat, exactly one object is caught (except for 0 throws, where none is caught).";

    [McpServerResource]
    [Description("siteswap:rule:modular-validity")]
    public string SiteswapRuleModularValidity() =>
        "A vanilla siteswap sequence a₀a₁...aₙ₋₁ with period n is valid if the set S = {(aᵢ + i) mod n | 0 ≤ i < n} has exactly n elements (no duplicates).";

    [McpServerResource]
    [Description("siteswap:rule:orbit-ball-count")]
    public string SiteswapRuleOrbitBallCount() =>
        "The number of objects in an orbit is the sum of the values in that orbit divided by the period of the entire pattern.";

    [McpServerResource]
    [Description("siteswap:rule:state-transition")]
    public string SiteswapRuleStateTransition() =>
        "From a state: Remove the leftmost x (next landing), shift everything one position left, add a - on the right. If an object was caught, a 0 cannot be thrown. Place an x at position n-1 for an n throw, if there is a - there.";

    [McpServerResource]
    [Description("siteswap:rule:sync-even-numbers")]
    public string SiteswapRuleSyncEvenNumbers() =>
        "Synchronous siteswaps contain only even numbers. Odd numbers are not allowed in synchronous patterns.";

    [McpServerResource]
    [Description("siteswap:rule:sync-crossing-x")]
    public string SiteswapRuleSyncCrossingX() =>
        "In synchronous siteswaps: An 'x' after an even number means the throw goes to the other hand. An 'x' after an odd number (only in transitions) means the throw goes to the same hand.";

    [McpServerResource]
    [Description("siteswap:rule:sync-empty-beat")]
    public string SiteswapRuleSyncEmptyBeat() =>
        "In synchronous patterns, two beats are counted for each pair of synchronous throws, even though both throws occur simultaneously. There is an 'empty beat' after each synchronous pair.";

    [McpServerResource]
    [Description("siteswap:rule:sync-asterisk")]
    public string SiteswapRuleSyncAsterisk() =>
        "An asterisk (*) at the end of a synchronous sequence means the roles of the hands reverse on each repetition. (4,2x)* is short for (4,2x)(2x,4).";

    [McpServerResource]
    [Description("siteswap:rule:sync-exclamation")]
    public string SiteswapRuleSyncExclamation() =>
        "An exclamation mark (!) after a synchronous pair means there is no empty beat after it. Used in transitions between sync and async patterns.";

    [McpServerResource]
    [Description("siteswap:rule:multiplex-averaging")]
    public string SiteswapRuleMultiplexAveraging() =>
        "For multiplex siteswaps: Add all numbers inside brackets and outside, divide by the number of throw times (not the number of objects thrown).";

    [McpServerResource]
    [Description("siteswap:rule:multiplex-zero-ignore")]
    public string SiteswapRuleMultiplexZeroIgnore() =>
        "A 0 in multiplex brackets can be ignored. [30] can be simplified to 3.";

    [McpServerResource]
    [Description("siteswap:rule:multiplex-2-hold")]
    public string SiteswapRuleMultiplex2Hold() =>
        "If multiplex brackets contain a 2, it means one object stays in the hand instead of being thrown. It may not be an actual multiplex throw.";

    [McpServerResource]
    [Description("siteswap:rule:multiplex-1-sliced")]
    public string SiteswapRuleMultiplex1Sliced() =>
        "If multiplex brackets contain a 1, it is a sliced throw.";

    [McpServerResource]
    [Description("siteswap:rule:4-handed-even-self")]
    public string SiteswapRule4HandedEvenSelf() =>
        "In 4-handed siteswap: Even numbers are always self throws (not passed), the type of throw that would be notated as half that number in a solo siteswap. Odd numbers are passes.";

    [McpServerResource]
    [Description("siteswap:rule:4-handed-diagonal-straight")]
    public string SiteswapRule4HandedDiagonalStraight() =>
        "In 4-handed siteswap: If the global throwing order is Juggler #1 right, #2 right, #1 left, #2 left, then Juggler #1 passes diagonally on 1s, 5s, etc. and straight on 3s, 7s, etc. Juggler #2 does the opposite.";

    [McpServerResource]
    [Description("siteswap:rule:passing-p-notation")]
    public string SiteswapRulePassingPNotation() =>
        "A 'p' after a number in passing siteswap means the throw is passed to the partner. Without 'p' it is a self throw.";

    [McpServerResource]
    [Description("siteswap:rule:passing-multiple-jugglers")]
    public string SiteswapRulePassingMultipleJugglers() =>
        "With more than two jugglers, a number after the 'p' can specify which juggler to pass to. Juggler #1 is the leftmost in |, #2 is next, etc.";

    [McpServerResource]
    [Description("siteswap:rule:passing-fraction")]
    public string SiteswapRulePassingFraction() =>
        "Fractions in passing siteswap mean the juggler after | throws half a beat later, and all fractions are passes. Example: <4.5 3 3|3 4 3.5>.";

    [McpServerResource]
    [Description("siteswap:rule:height-formula")]
    public string SiteswapRuleHeightFormula() =>
        "The actual height of a throw (from hand height to peak) is h = g × (st - 2dt)² / 8, where g = acceleration due to gravity (9.8 m/s²), s = siteswap number, t = time between throws, d = dwell ratio.";

    [McpServerResource]
    [Description("siteswap:rule:relative-heights")]
    public string SiteswapRuleRelativeHeights() =>
        "To compare relative heights of two siteswap numbers: Subtract 1 from each number, square the results, divide the squares. Example: (9-1)²/(5-1)² = 64/16 = 4, so a 9 is about four times as high as a 5.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-period-pass")]
    public string SiteswapRuleHijackingPeriodPass() =>
        "A 'pass' for hijacking is defined as period + 2. In period-5 patterns, a pass is a 7, in period-3 patterns a 5 (zap).";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-empty-hand-zip")]
    public string SiteswapRuleHijackingEmptyHandZip() =>
        "If you have an empty hand because you did not receive a 'pass', zip into the empty hand.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-zip-to-pass")]
    public string SiteswapRuleHijackingZipToPass() =>
        "If you were going to zip into a hand, but a pass is coming to that hand, throw a 'pass' instead.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-period-constant")]
    public string SiteswapRuleHijackingPeriodConstant() =>
        "The period of the pattern before and after a hijacking transition must be the same.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-club-change")]
    public string SiteswapRuleHijackingClubChange() =>
        "A hijacking transition results in one passer gaining one club locally, the other losing one club locally.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-transition-throws")]
    public string SiteswapRuleHijackingTransitionThrows() =>
        "There can be 0 or 1 active transition throws (a throw that is not part of the pattern being transitioned to or from).";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-passive-throws")]
    public string SiteswapRuleHijackingPassiveThrows() =>
        "All throws on the passive side must belong to a pattern being transitioned to or from.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-minimal-change")]
    public string SiteswapRuleHijackingMinimalChange() =>
        "The passive responder must make a causally minimal change to their juggling in response to an active transition.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-global-validity")]
    public string SiteswapRuleHijackingGlobalValidity() =>
        "All global patterns with twice the local period must be valid 4-handed siteswaps.";

    [McpServerResource]
    [Description("siteswap:rule:hijacking-passes-required")]
    public string SiteswapRuleHijackingPassesRequired() =>
        "All patterns must have one or more passes. Not just period+2 passes, but any pass satisfies this condition.";

    // ====================================================================================
    // SECTION 3: SITESWAP TRANSFORMATIONS
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:transformation:rotation")]
    public string SiteswapTransformationRotation() =>
        "Move a throw from the beginning of the notation to the end to get another representation of the same pattern. Example: 423 becomes 234.";

    [McpServerResource]
    [Description("siteswap:transformation:repetition")]
    public string SiteswapTransformationRepetition() =>
        "Repeat the same sequence of numbers to get another way of writing the same pattern. Example: 3 becomes 33.";

    [McpServerResource]
    [Description("siteswap:transformation:period-addition")]
    public string SiteswapTransformationPeriodAddition() =>
        "Add the period to a number (or subtract it) to get a pattern with one more (or less) object. Example: 42 becomes 62 or 22 when the first throw is changed.";

    [McpServerResource]
    [Description("siteswap:transformation:uniform-increment")]
    public string SiteswapTransformationUniformIncrement() =>
        "Add 1 to each number (or subtract 1) to get a pattern with one more (or less) object. Example: 441 becomes 552 or 330.";

    [McpServerResource]
    [Description("siteswap:transformation:swap-property")]
    public string SiteswapTransformationSwapProperty() =>
        "Swap two consecutive numbers, add 1 to the first and subtract 1 from the second. This corresponds to swapping the landing times of those two throws. Example: 522 becomes 531 when the 2s are swapped.";

    [McpServerResource]
    [Description("siteswap:transformation:swap-non-consecutive")]
    public string SiteswapTransformationSwapNonConsecutive() =>
        "Swap two non-consecutive numbers, add the distance to the first and subtract the distance from the second. Example: In 5511, the second 5 and second 1 are two beats apart. Swap them (to 5115), add 2 to the first, subtract 2 from the second: becomes 5313.";

    [McpServerResource]
    [Description("siteswap:transformation:time-reversal")]
    public string SiteswapTransformationTimeReversal() =>
        "Move each number that many positions to the right (with wrap-around), then read the resulting sequence backwards to get the time-reversed version. Example: 603 becomes 360.";

    [McpServerResource]
    [Description("siteswap:transformation:dual")]
    public string SiteswapTransformationDual() =>
        "Subtract each number from twice the number of objects, then read the sequence backwards to get the dual. Example: Subtract each number in 504 from 6: becomes 162, read backwards: 261 (normally written as 612).";

    [McpServerResource]
    [Description("siteswap:transformation:state-replacement")]
    public string SiteswapTransformationStateReplacement() =>
        "Replace a section of a siteswap with a different sequence of throws that starts and ends in the same state as that section. Example: 531 becomes 73131 (both sequences 5 and 731 end in state 11001, starting from ground state 111).";

    [McpServerResource]
    [Description("siteswap:transformation:state-combination")]
    public string SiteswapTransformationStateCombination() =>
        "Combine two patterns that visit the same state by switching to the other pattern when you reach that state. Example: 51 visits state 10101 after the 5, 60 visits this state before the 6, combined: 5601.";

    [McpServerResource]
    [Description("siteswap:transformation:sync-swap")]
    public string SiteswapTransformationSyncSwap() =>
        "Swap a pair of synchronous throws and change each of those throws to a crossing throw if it was non-crossing, or vice versa. Example: (6x,4)(2,4x) becomes (4x,6)(2,4x) or (6x,4)(4,2x).";

    [McpServerResource]
    [Description("siteswap:transformation:slide-property")]
    public string SiteswapTransformationSlideProperty() =>
        "Convert a synchronous sequence (a₀,a₁)(a₂,a₃)... into two vanilla sequences b₀b₁... and c₀c₁..., where bᵢ = aᵢ+1 if i is even and aᵢ crosses, aᵢ-1 if i is odd and aᵢ crosses, otherwise aᵢ. cᵢ = aᵢ+2 if i is even and aᵢ crosses, aᵢ-2 if i is odd and aᵢ crosses, aᵢ+1 if i is even and aᵢ does not cross, aᵢ-1 if i is odd and aᵢ does not cross.";

    [McpServerResource]
    [Description("siteswap:transformation:shower-conversion")]
    public string SiteswapTransformationShowerConversion() =>
        "Turn a vanilla siteswap into a showered version by replacing each number with the notation for a shower with that number of objects. Example: 534 becomes 915171.";

    [McpServerResource]
    [Description("siteswap:transformation:one-handed-conversion")]
    public string SiteswapTransformationOneHandedConversion() =>
        "Turn a vanilla siteswap into a one-handed version by replacing each number with the notation for a one-handed pattern with that number of objects. Example: 534 becomes a06080.";

    // ====================================================================================
    // SECTION 4: SITESWAP EXAMPLES (Solo Patterns)
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:cascade-3")]
    public string SiteswapExampleCascade3() =>
        "3 is the 3-ball cascade. Every throw is a 3, which goes to the other hand. Sequence: ...333333...";

    [McpServerResource]
    [Description("siteswap:example:shower-51")]
    public string SiteswapExampleShower51() =>
        "51 is the 3-ball shower. One hand throws high 5s, the other hand throws quick 1s (handoffs). Sequence: ...515151...";

    [McpServerResource]
    [Description("siteswap:example:fountain-4")]
    public string SiteswapExampleFountain4() =>
        "4 is the 4-ball fountain. Every throw is a 4, which returns to the same hand. Sequence: ...444444...";

    [McpServerResource]
    [Description("siteswap:example:pattern-441")]
    public string SiteswapExamplePattern441() =>
        "441 is a 3-ball pattern. Mostly non-crossing 4s are thrown, every three beats a ball is passed directly to the other hand (1). Average: (4+4+1)/3 = 3.";

    [McpServerResource]
    [Description("siteswap:example:pattern-531")]
    public string SiteswapExamplePattern531() =>
        "531 is a 3-ball pattern. High crossing throw (5), lower crossing throw (3), pass straight to the other hand (1). Average: (5+3+1)/3 = 3.";

    [McpServerResource]
    [Description("siteswap:example:pattern-423")]
    public string SiteswapExamplePattern423() =>
        "423 is a 3-ball pattern. Non-crossing throw (4), pause with ball in hand (2), lower crossing throw (3). Average: (4+2+3)/3 = 3.";

    [McpServerResource]
    [Description("siteswap:example:pattern-53")]
    public string SiteswapExamplePattern53() =>
        "53 is the 4-ball half-shower. One hand throws like a 5-ball cascade, the other like a 3-ball cascade. Average: (5+3)/2 = 4.";

    [McpServerResource]
    [Description("siteswap:example:pattern-20")]
    public string SiteswapExamplePattern20() =>
        "20: One hand holds a ball (2), the other hand is empty (0). Average: (2+0)/2 = 1.";

    [McpServerResource]
    [Description("siteswap:example:pattern-31")]
    public string SiteswapExamplePattern31() =>
        "31 is the 2-ball shower. One hand throws 3s, the other throws 1s. Average: (3+1)/2 = 2.";

    [McpServerResource]
    [Description("siteswap:example:pattern-330")]
    public string SiteswapExamplePattern330() =>
        "330: Mostly 3s are thrown like a 3-ball cascade, but every three beats one hand is empty (0), because only 2 balls are used. Average: (3+3+0)/3 = 2.";

    [McpServerResource]
    [Description("siteswap:example:pattern-411")]
    public string SiteswapExamplePattern411() =>
        "411 is a 3-ball pattern. Throw like a 4-ball fountain, then pass to the other hand, then pass back. Average: (4+1+1)/3 = 2, but actually 3 balls.";

    [McpServerResource]
    [Description("siteswap:example:pattern-522")]
    public string SiteswapExamplePattern522() =>
        "522 is a 3-ball pattern. A ball is only thrown every three beats (5), the other two beats are 2s (ball is held). Actually a slower version of 3.";

    [McpServerResource]
    [Description("siteswap:example:pattern-633")]
    public string SiteswapExamplePattern633() =>
        "633 is a 3-ball pattern. One high non-crossing throw like a 6-ball fountain, then two lower crossing throws like a 3-ball cascade.";

    [McpServerResource]
    [Description("siteswap:example:pattern-201")]
    public string SiteswapExamplePattern201() =>
        "201: One hand holds a ball (2), the other is empty (0), then the ball is passed to the other hand (1). A slower version of 1.";

    [McpServerResource]
    [Description("siteswap:example:pattern-312")]
    public string SiteswapExamplePattern312() =>
        "312 is a 3-ball pattern. Crossing throw (3), pass to the other hand (1), pause with ball in hand (2).";

    [McpServerResource]
    [Description("siteswap:example:pattern-420")]
    public string SiteswapExamplePattern420() =>
        "420 is a 2-ball pattern. Non-crossing throw (4), pause with ball (2), empty hand (0). A slower version of 2 with active 2s.";

    [McpServerResource]
    [Description("siteswap:example:pattern-642")]
    public string SiteswapExamplePattern642() =>
        "642 is a 4-ball pattern. High non-crossing throw (6), lower non-crossing throw (4), pause with ball (2). Average: (6+4+2)/3 = 4.";

    [McpServerResource]
    [Description("siteswap:example:pattern-4400")]
    public string SiteswapExamplePattern4400() =>
        "4400 is a 2-ball pattern. Two non-crossing throws (4,4), then two empty hands (0,0). Average: (4+4+0+0)/4 = 2.";

    [McpServerResource]
    [Description("siteswap:example:pattern-5511")]
    public string SiteswapExamplePattern5511() =>
        "5511 is a 4-ball pattern. Each hand makes a high non-crossing throw (5), then each hand passes a ball directly (1). Average: (5+5+1+1)/4 = 3, but actually 4 balls.";

    [McpServerResource]
    [Description("siteswap:example:pattern-6622")]
    public string SiteswapExamplePattern6622() =>
        "6622 is a 4-ball pattern. Two high non-crossing throws (6,6), alternating with two beats where balls are held (2,2). Average: (6+6+2+2)/4 = 4.";

    [McpServerResource]
    [Description("siteswap:example:pattern-744")]
    public string SiteswapExamplePattern744() =>
        "744 is a 5-ball pattern. Average: (7+4+4)/3 = 5. Ground-state, can be entered directly from the cascade.";

    [McpServerResource]
    [Description("siteswap:example:pattern-771")]
    public string SiteswapExamplePattern771() =>
        "771 is a 5-ball pattern. Average: (7+7+1)/3 = 5. Excited-state, requires transition throws to enter.";

    [McpServerResource]
    [Description("siteswap:example:pattern-55550")]
    public string SiteswapExamplePattern55550() =>
        "55550 is a 4-ball pattern. Like a 5-ball cascade, except the last ball is missing, so an empty hand (0) instead of a 5. Average: (5+5+5+5+0)/5 = 4.";

    [McpServerResource]
    [Description("siteswap:example:pattern-55500")]
    public string SiteswapExamplePattern55500() =>
        "55500 is a 3-ball pattern. A 5-ball cascade with two balls missing. Average: (5+5+5+0+0)/5 = 3.";

    [McpServerResource]
    [Description("siteswap:example:pattern-50505")]
    public string SiteswapExamplePattern50505() =>
        "50505 is a 3-ball pattern. A 5-ball cascade with two balls missing, but not two consecutive ones. Average: (5+0+5+0+5)/5 = 3.";

    // ====================================================================================
    // SECTION 5: SYNCHRONOUS SITESWAP EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:sync-shower")]
    public string SiteswapExampleSyncShower() =>
        "(4x,2x) is the synchronous 3-ball shower. Both hands throw simultaneously: one hand throws high crossing 4s, the other quick crossing 2s.";

    [McpServerResource]
    [Description("siteswap:example:sync-box")]
    public string SiteswapExampleSyncBox() =>
        "(4,2x)(2x,4) or (4,2x)* is the 3-ball box pattern. One hand throws high non-crossing 4s, the other quick crossing 2s, then roles switch.";

    [McpServerResource]
    [Description("siteswap:example:sync-2x-2x")]
    public string SiteswapExampleSync2x2x() =>
        "(2x,2x) is a synchronous pattern. Every throw is a 2x: two throws are made (simultaneously) before the same ball is thrown again (because of the 2), and the ball goes to the other hand (because of the x).";

    [McpServerResource]
    [Description("siteswap:example:sync-4-0")]
    public string SiteswapExampleSync40() =>
        "(4,0) is a synchronous pattern. One hand throws like a 4-ball fountain, the other hand is empty. This is half of a 4-ball pattern.";

    [McpServerResource]
    [Description("siteswap:example:sync-6-4")]
    public string SiteswapExampleSync64() =>
        "(6,4) is a synchronous pattern. One side throws like a 6-ball fountain (3 in one hand), the other side like a 4-ball fountain (2 in one hand).";

    [McpServerResource]
    [Description("siteswap:example:sync-4x-2x-star")]
    public string SiteswapExampleSync4x2xStar() =>
        "(4x,2x)* or (4x,2x)(2x,4) is a synchronous pattern. One hand throws high crossing 4s, the other quick crossing 2s, then roles switch.";

    // ====================================================================================
    // SECTION 6: MULTIPLEX EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:multiplex-duplex")]
    public string SiteswapExampleMultiplexDuplex() =>
        "[33]33 is a 3-ball cascade with duplex stacks. A pair of balls is always thrown together ([33]), then a single ball (3), then the pair again.";

    [McpServerResource]
    [Description("siteswap:example:multiplex-43-23")]
    public string SiteswapExampleMultiplex4323() =>
        "[43]23 is a 4-ball multiplex pattern. Average: (4+3+2+3)/4 = 12/4 = 3, but actually 4 balls, since [43] counts as one throw time.";

    // ====================================================================================
    // SECTION 7: PASSING EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:passing-2-count")]
    public string SiteswapExamplePassing2Count() =>
        "<3p 3|3p 3> is a 6-prop 2-count passing pattern. All left-hand throws are passes (3p), all right-hand throws are selfs (3).";

    [McpServerResource]
    [Description("siteswap:example:passing-social")]
    public string SiteswapExamplePassingSocial() =>
        "<4p 3|3 4p> becomes 4p 3 as a social siteswap, since both jugglers juggle the same pattern, only time-shifted.";

    [McpServerResource]
    [Description("siteswap:example:hijacking-855")]
    public string SiteswapExampleHijacking855() =>
        "855 is a period-3 passing pattern. Transition out of 855: [855][885] active transition goes up 1 club (hijack), [585][582] passive response goes down 1 club.";

    [McpServerResource]
    [Description("siteswap:example:hijacking-975")]
    public string SiteswapExampleHijacking975() =>
        "975 (Holy Grail) is a period-3 passing pattern. Transition out of 975: [579][879] active transition goes up 1 club (hijack), [957][927] passive response goes down 1 club.";

    [McpServerResource]
    [Description("siteswap:example:hijacking-period-5")]
    public string SiteswapExampleHijackingPeriod5() =>
        "In period-5 patterns, the hijackable passes are singles: 5+2=7. Classic hijacks by Aidan Burns fall into this category.";

    [McpServerResource]
    [Description("siteswap:example:hijacking-period-7")]
    public string SiteswapExampleHijackingPeriod7() =>
        "In period-7 patterns, the hijackable passes are: 7+2=9. Example: 9788827 can transition to 9797226 vs 9797888.";

    // ====================================================================================
    // SECTION 8: ORBIT EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:orbit-561")]
    public string SiteswapExampleOrbit561() =>
        "561 has two orbits: (5,1) with 2 balls (sum 6, period 3, 6/3=2) and (6) with 2 balls (sum 6, period 3, 6/3=2).";

    [McpServerResource]
    [Description("siteswap:example:orbit-72312")]
    public string SiteswapExampleOrbit72312() =>
        "72312 has two orbits: (7,3) with 2 balls and (2,1,2) with 1 ball. The (7,3) orbit has 2 balls going in opposite directions.";

    [McpServerResource]
    [Description("siteswap:example:orbit-5241")]
    public string SiteswapExampleOrbit5241() =>
        "5241 has two orbits: (5,2,1) with 2 balls (sum 8, period 4, 8/4=2) and (4) with 1 ball (sum 4, period 4, 4/4=1).";

    [McpServerResource]
    [Description("siteswap:example:orbit-5551")]
    public string SiteswapExampleOrbit5551() =>
        "5551 has only one orbit (5,5,5,1) with 4 balls (sum 16, period 4, 16/4=4). This pattern mixes the balls completely.";

    [McpServerResource]
    [Description("siteswap:example:orbit-55514")]
    public string SiteswapExampleOrbit55514() =>
        "55514 has four orbits, each with 1 ball: (5), (5), (5), (1,4). Each ball stays in its own orbit.";

    // ====================================================================================
    // SECTION 9: STATE EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:state-xxx")]
    public string SiteswapExampleStateXxx() =>
        "State xxx means: One ball lands now (next beat), one ball lands in one beat, one ball lands in two beats. This is the ground state for 3 balls.";

    [McpServerResource]
    [Description("siteswap:example:state-transition-3")]
    public string SiteswapExampleStateTransition3() =>
        "From state xxx: Throw a 3. Remove leftmost x, shift: xx-, place x at position 2: xxx. The state remains xxx (ground state).";

    [McpServerResource]
    [Description("siteswap:example:state-transition-4")]
    public string SiteswapExampleStateTransition4() =>
        "From state xxx: Throw a 4. Remove leftmost x, shift: xx-, place x at position 3: xx-x. New state: xx-x.";

    [McpServerResource]
    [Description("siteswap:example:state-binary")]
    public string SiteswapExampleStateBinary() =>
        "A state can be represented as a binary number: 1 for ball lands, 0 for no landing. Example: xxx-x becomes 11101 (binary) or 29 (decimal).";

    [McpServerResource]
    [Description("siteswap:example:state-multiplex")]
    public string SiteswapExampleStateMultiplex() =>
        "In multiplex states, the binary representation can contain numbers greater than 1: 2 means 2 balls land. Example: State 2-1-0 means 2 balls land now, 1 in one beat, 0 in two beats.";

    // ====================================================================================
    // SECTION 10: VALIDITY EXAMPLES
    // ====================================================================================

    [McpServerResource]
    [Description("siteswap:example:invalid-543")]
    public string SiteswapExampleInvalid543() =>
        "543 is invalid despite integer average (4+3)/2=4. All three throws land at the same time: 5 lands at beat 5, 4 at beat 4, 3 at beat 3, but all are part of the same 3-beat sequence and collide.";

    [McpServerResource]
    [Description("siteswap:example:invalid-513")]
    public string SiteswapExampleInvalid513() =>
        "513 is invalid. Calculation: (5+0) mod 3 = 2, (1+1) mod 3 = 2, (3+2) mod 3 = 2. All values are 2, therefore duplicates. The sequence is invalid.";

    [McpServerResource]
    [Description("siteswap:example:valid-531")]
    public string SiteswapExampleValid531() =>
        "531 is valid. Calculation: (5+0) mod 3 = 2, (3+1) mod 3 = 1, (1+2) mod 3 = 0. Values 2, 1, 0 are all unique, therefore valid.";

    // ====================================================================================
    // SECTION 11: FOUR-HANDED SITESWAP THROW DEFINITIONS
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:throw:2-zip")]
    public string Passing4HswThrow2Zip() =>
        "In four-handed siteswap, a 2 is called a 'Zip'. It's a hand-across without rotation, equivalent to 1 in solo siteswap. The club moves directly from one hand to the other of the same juggler.";

    [McpServerResource]
    [Description("passing:4hsw:throw:4-flip")]
    public string Passing4HswThrow4Flip() =>
        "In four-handed siteswap, a 4 is called a 'Flip'. It means holding or flipping a club in the same hand, equivalent to 2 in solo siteswap. The club stays in the same hand for the next beat.";

    [McpServerResource]
    [Description("passing:4hsw:throw:5-zap")]
    public string Passing4HswThrow5Zap() =>
        "In four-handed siteswap, a 5 is called a 'Zap'. It's a fast, low pass with a half rotation. Crossing for passer A, straight for passer B. Zaps are quick and require precise timing.";

    [McpServerResource]
    [Description("passing:4hsw:throw:6-self")]
    public string Passing4HswThrow6Self() =>
        "In four-handed siteswap, a 6 is a normal 'Self'. Equivalent to 3 in solo siteswap, it's a crossing throw to your other hand at normal cascade height.";

    [McpServerResource]
    [Description("passing:4hsw:throw:7-single")]
    public string Passing4HswThrow7Single() =>
        "In four-handed siteswap, a 7 is called a 'Single'. It's a lofty pass with a single rotation. Straight for passer A, crossing for passer B. The most common pass in four-handed siteswaps.";

    [McpServerResource]
    [Description("passing:4hsw:throw:8-heff")]
    public string Passing4HswThrow8Heff() =>
        "In four-handed siteswap, an 8 is called a 'Heff'. It's a double-spin self to the same hand, like in a basic four-club solo fountain. Equivalent to 4 in solo siteswap.";

    [McpServerResource]
    [Description("passing:4hsw:throw:9-double")]
    public string Passing4HswThrow9Double() =>
        "In four-handed siteswap, a 9 is called a 'Double'. It's a double-spin pass. Crossing for passer A, straight for passer B. Higher and slower than a single pass.";

    [McpServerResource]
    [Description("passing:4hsw:throw:a-trelf")]
    public string Passing4HswThrowATrelf() =>
        "In four-handed siteswap, an 'a' (10) is called a 'Trelf'. It's a triple-spin self. A high self throw with three rotations, giving extra time for complex sequences.";

    [McpServerResource]
    [Description("passing:4hsw:throw:b-triple")]
    public string Passing4HswThrowBTriple() =>
        "In four-handed siteswap, a 'b' (11) is a triple pass. Very high pass with three rotations. Less common due to difficulty, but used in advanced patterns like 9ab5678.";

    // ====================================================================================
    // SECTION 12: FOUR-HANDED SITESWAP NOTATION AND READING
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:notation:reading-sequence")]
    public string Passing4HswNotationReadingSequence() =>
        "In four-handed siteswap, the number sequence describes alternating actions of TWO passers: first digit = first action of passer A, second digit = first action of passer B, third digit = second action of passer A, and so forth. Both passers do the same sequence but start at different positions.";

    [McpServerResource]
    [Description("passing:4hsw:notation:two-row-format")]
    public string Passing4HswNotationTwoRowFormat() =>
        "To read a four-handed siteswap, repeat it twice and alternate between two rows. Example for 75864: Passer A does 7-8-4-5-6, Passer B does 5-6-4-7-8. Both do the same sequence (7-5-8-6-4) starting at different points.";

    [McpServerResource]
    [Description("passing:4hsw:notation:starting-pattern")]
    public string Passing4HswNotationStartingPattern() =>
        "Starting a four-handed siteswap: Passer A always starts with a right-hand action (first digit). Passer B follows slightly afterward with a right-hand action (second digit). Technically B starts between A's first and second action, but timing can be fudged slightly.";

    [McpServerResource]
    [Description("passing:4hsw:notation:pattern-shifting")]
    public string Passing4HswNotationPatternShifting() =>
        "A siteswap can start on any beat of the sequence. For example, 786 = 867 = 678 are all the same pattern with different starting points. Shifting doesn't change the pattern, only where you enter it.";

    [McpServerResource]
    [Description("passing:4hsw:notation:crossing-rules")]
    public string Passing4HswNotationCrossingRules() =>
        "In four-handed siteswap, one passer throws crossing passes, the other straight. For 5s (zaps) and 9s (doubles): crossing for passer A, straight for passer B. For 7s (singles): straight for passer A, crossing for passer B. This alternation is fundamental to the pattern structure.";

    // ====================================================================================
    // SECTION 13: FOUR-HANDED SITESWAP CORE RULES
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:rule:even-odd")]
    public string Passing4HswRuleEvenOdd() =>
        "In four-handed siteswap: Even numbers (2, 4, 6, 8, a) are always SELF throws - they stay with the same juggler. Odd numbers (3, 5, 7, 9, b) are always PASSES - they go to the partner.";

    [McpServerResource]
    [Description("passing:4hsw:rule:solo-conversion")]
    public string Passing4HswRuleSoloConversion() =>
        "Four-handed siteswap numbers are double the solo siteswap equivalents: 2→1 (zip), 4→2 (flip/hold), 6→3 (self), 8→4 (heff). This is because each hand throws every second beat rather than alternating.";

    [McpServerResource]
    [Description("passing:4hsw:rule:asynchronous-rhythm")]
    public string Passing4HswRuleAsynchronousRhythm() =>
        "Four-handed siteswaps are fully asynchronous: all four hands throw at different times, creating an interleaved rhythm. Order is typically: A right, B right, A left, B left, then repeating.";

    // ====================================================================================
    // SECTION 14: FOUR-HANDED SITESWAP BEGINNER PATTERNS
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:pattern:744")]
    public string Passing4HswPattern744() =>
        "744 is a 5-club four-handed siteswap. Pattern: single pass (7), flip (4), flip (4). Both passers do the same sequence. The basic one-count for 5 clubs. Ground state, easy to start. Often used as the simpler half in compatible siteswap pairs.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:726")]
    public string Passing4HswPattern726() =>
        "726 is a 5-club four-handed siteswap. Pattern: single pass (7), zip (2), self (6). Introduces the zip throw while maintaining regular passing rhythm. Ground state.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:786")]
    public string Passing4HswPattern786() =>
        "786 (French Three-Count) is a 7-club four-handed siteswap. Pattern: single pass (7), heff (8), self (6). One of the most popular intermediate patterns. Compatible with 744 for mixed-skill passing.";

    // ====================================================================================
    // SECTION 15: FOUR-HANDED SITESWAP INTERMEDIATE PATTERNS
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:pattern:966")]
    public string Passing4HswPattern966() =>
        "966 is a 7-club four-handed siteswap. Pattern: double pass (9), self (6), self (6). Features double passes with selfs. Good for learning doubles in a 4-handed context.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:77722")]
    public string Passing4HswPattern77722() =>
        "77722 is a 5-club four-handed siteswap with period 5. Three single passes followed by two zips. Creates a rhythm of pass-pass-pass-zip-zip that's satisfying once mastered.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:756")]
    public string Passing4HswPattern756() =>
        "756 is a 6-club four-handed siteswap. Pattern: single (7), zap (5), self (6). Introduces zaps - the fast, low passes that require precise timing.";

    // ====================================================================================
    // SECTION 16: FOUR-HANDED SITESWAP ADVANCED PATTERNS
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:pattern:975-holygrail")]
    public string Passing4HswPattern975HolyGrail() =>
        "975 (Holy Grail) is a 7-club four-handed siteswap. Pattern: double (9), single (7), zap (5). Creates a 'stack' - three different passes landing on the same hand in sequence. One of the most celebrated advanced patterns.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:95678")]
    public string Passing4HswPattern95678() =>
        "95678 is the best known sequence pattern. This 7-club pattern produces a large stack: zap (5), self (6), single (7), heff (8), double (9) - a sequence of passes all to the same hand. Permutations: 97586 (counting up), 96857 (counting down).";

    [McpServerResource]
    [Description("passing:4hsw:pattern:56784-suicide-bunny")]
    public string Passing4HswPattern56784SuicideBunny() =>
        "56784 (Suicide Bunny) is a 6-club sequence pattern from 4 to 8. Challenging for remembering the many different throws rather than physical difficulty. Permutations: 75864 and 57468.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:789a6")]
    public string Passing4HswPattern789a6() =>
        "789a6 is an 8-club pattern from 6 to a (trelf). Combines a stack with the common solo siteswap 345. Features heffs and trelfs with passes. Permutations: 97a86 and a7968.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:9968926-why-not")]
    public string Passing4HswPattern9968926WhyNot() =>
        "9968926 (Why Not) is a 7-club pattern that integrates heffs and doubles. A named pattern in the four-handed siteswap repertoire.";

    [McpServerResource]
    [Description("passing:4hsw:pattern:9969788-poem")]
    public string Passing4HswPattern9969788Poem() =>
        "9969788 (Poem) is a challenging 8-club pattern. One of the harder named patterns requiring solid control of all throw types.";

    // ====================================================================================
    // SECTION 17: COMPATIBLE SITESWAPS
    // ====================================================================================

    [McpServerResource]
    [Description("passing:4hsw:compatible:definition")]
    public string Passing4HswCompatibleDefinition() =>
        "Compatible siteswaps are different four-handed patterns that can be combined, with each passer doing a different pattern. Two patterns are compatible if they have: 1) Same length (period), 2) Same sequence of pass/non-pass catches. This allows mixed-skill passing.";

    [McpServerResource]
    [Description("passing:4hsw:compatible:744-786")]
    public string Passing4HswCompatible744786() =>
        "744 + 786: Classic compatible pair. 744 (5 clubs, easier) with 786 'French Three-Count' (7 clubs, harder). Total: 6 clubs between both passers. Perfect for teaching or mixed-skill sessions.";

    [McpServerResource]
    [Description("passing:4hsw:compatible:benefits")]
    public string Passing4HswCompatibleBenefits() =>
        "Benefits of compatible siteswaps: 1) Mixed skill levels can pass together, 2) Learning tool - practice hard pattern while helper does easy version, 3) Teaching aid, 4) Creates variety from existing patterns.";

    [McpServerResource]
    [Description("passing:4hsw:compatible:finding")]
    public string Passing4HswCompatibleFinding() =>
        "To find compatible patterns: 1) List patterns with same period, 2) Analyze pass positions (odd numbers), 3) Match patterns where passes occur at same positions, 4) Verify club counts work. For period-3: 744, 786, 966, 948, 969, 996 are all potentially compatible.";

    // ====================================================================================
    // SECTION 18: MODERN PASSING CONCEPTS
    // ====================================================================================

    [McpServerResource]
    [Description("passing:modern:definition")]
    public string PassingModernDefinition() =>
        "Modern club passing is: ambidextrous, combining passes at different heights (zaps, singles, doubles) with zips, flips, heffs, and triple selfs. It emphasizes control, embraces manipulator and walking patterns with 3+ people, and is for the passer's challenge rather than audience entertainment.";

    [McpServerResource]
    [Description("passing:modern:ambidextrous")]
    public string PassingModernAmbidextrous() =>
        "Modern passing is ambidextrous: every pattern is performed equally with both hands. This is a core principle that distinguishes modern passing from traditional patterns where one hand dominates.";

    [McpServerResource]
    [Description("passing:modern:source")]
    public string PassingModernSource() =>
        "Source for four-handed siteswap patterns and modern passing concepts: Christian Kästner's 'Modern Club Passing' book at https://ckaestne.github.io/modernpassing/";
}
