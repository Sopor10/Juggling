using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Resources;

[McpServerResourceType]
public class KnowledgeResources
{
    [McpServerResource]
    [Description("siteswap:definition:siteswap")]
    public Task<string> siteswap_definition_siteswap() =>
        Task.FromResult("Siteswap is a numeric notation system for juggling patterns. A siteswap sequence describes the order in which objects are thrown and caught, and only that.");

    [McpServerResource]
    [Description("siteswap:definition:vanilla-siteswap")]
    public Task<string> siteswap_definition_vanilla_siteswap() =>
        Task.FromResult("Vanilla siteswap describes asynchronous patterns where hands alternate throwing, one object per hand per throw time.");

    [McpServerResource]
    [Description("siteswap:definition:throw-value")]
    public Task<string> siteswap_definition_throw_value() =>
        Task.FromResult("A siteswap value n indicates that the object will be thrown again n beats later. n corresponds to the number of beats that pass before the same object is thrown again.");

    [McpServerResource]
    [Description("siteswap:definition:beat")]
    public Task<string> siteswap_definition_beat() =>
        Task.FromResult("A beat is a uniformly spaced time unit at which throws occur. All throws happen at strictly regular beats.");

    [McpServerResource]
    [Description("siteswap:definition:period")]
    public Task<string> siteswap_definition_period() =>
        Task.FromResult("The period of a siteswap is the number of beats before the sequence repeats, ignoring which hand throws.");

    [McpServerResource]
    [Description("siteswap:definition:full-period")]
    public Task<string> siteswap_definition_full_period() =>
        Task.FromResult("The full period is the number of beats before the pattern repeats when hands are considered distinguishable but props are not.");

    [McpServerResource]
    [Description("siteswap:definition:throw-0")]
    public Task<string> siteswap_definition_throw_0() =>
        Task.FromResult("A 0 throw is a pause with an empty hand. The hand that would throw on this beat does nothing because it has nothing to throw.");

    [McpServerResource]
    [Description("siteswap:definition:throw-1")]
    public Task<string> siteswap_definition_throw_1() =>
        Task.FromResult("A 1 throw is a quick pass straight across to the other hand, also called a handoff, feed, zip, or vamp.");

    [McpServerResource]
    [Description("siteswap:definition:throw-2")]
    public Task<string> siteswap_definition_throw_2() =>
        Task.FromResult("A passive 2 throw is a pause with an object held in the hand that would otherwise be thrown on this beat.");

    [McpServerResource]
    [Description("siteswap:definition:odd-even-crossing")]
    public Task<string> siteswap_definition_odd_even_crossing() =>
        Task.FromResult("In vanilla siteswap: Odd numbers represent throws that go to the other hand (crossing). Even numbers represent throws that are caught by the same hand (non-crossing).");

    [McpServerResource]
    [Description("siteswap:definition:alphanumeric-notation")]
    public Task<string> siteswap_definition_alphanumeric_notation() =>
        Task.FromResult("Values greater than 9 are written as letters: 10=a, 11=b, 12=c, etc. This is not hexadecimal, but serves to avoid ambiguous multi-digit numbers.");

    [McpServerResource]
    [Description("siteswap:definition:ground-state")]
    public Task<string> siteswap_definition_ground_state() =>
        Task.FromResult("Ground-state siteswaps can be entered directly from the basic pattern (cascade/fountain) without special transition throws.");

    [McpServerResource]
    [Description("siteswap:definition:excited-state")]
    public Task<string> siteswap_definition_excited_state() =>
        Task.FromResult("Excited-state siteswaps require special transition throws to reach them from the basic pattern.");

    [McpServerResource]
    [Description("siteswap:definition:orbit")]
    public Task<string> siteswap_definition_orbit() =>
        Task.FromResult("An orbit is a closed loop of throws that a single object or group of objects follows. Each object stays within its orbit.");

    [McpServerResource]
    [Description("siteswap:definition:juggling-state")]
    public Task<string> siteswap_definition_juggling_state() =>
        Task.FromResult("A juggling state (landing schedule) describes at which future beats objects will land. Marked with x/1 for landing, -/0 for no landing.");

    [McpServerResource]
    [Description("siteswap:definition:synchronous-siteswap")]
    public Task<string> siteswap_definition_synchronous_siteswap() =>
        Task.FromResult("Synchronous siteswaps describe patterns where both hands throw simultaneously. Throws are grouped in parentheses: (right, left).");

    [McpServerResource]
    [Description("siteswap:definition:multiplex")]
    public Task<string> siteswap_definition_multiplex() =>
        Task.FromResult("A multiplex throw is when multiple objects are thrown simultaneously from one hand. Notated in square brackets: [33] means two objects are thrown simultaneously.");

    [McpServerResource]
    [Description("siteswap:definition:passing-siteswap")]
    public Task<string> siteswap_definition_passing_siteswap() =>
        Task.FromResult("Passing siteswap describes patterns with multiple jugglers. Instead of 2 hands in total we have 2 hands per juggler. The normal hand order is Ar Br Cr...Al Bl Cl for juggler A,B,C with hands r and l. A fraction means the throw is passed to a partner.");

    [McpServerResource]
    [Description("siteswap:definition:4-handed-siteswap")]
    public Task<string> siteswap_definition_4_handed_siteswap() =>
        Task.FromResult("4-handed siteswap describes fully asynchronous passing patterns where all four hands throw alternately. Even numbers are selfs, odd numbers are passes.");

    [McpServerResource]
    [Description("siteswap:definition:local-siteswap")]
    public Task<string> siteswap_definition_local_siteswap() =>
        Task.FromResult("A local siteswap shows only the throws of one person in a passing pattern. The global siteswap shows all throws of both people in order.");

    [McpServerResource]
    [Description("siteswap:definition:global-siteswap")]
    public Task<string> siteswap_definition_global_siteswap() =>
        Task.FromResult("A global siteswap shows all throws of all jugglers in the order they are made. In 4-handed siteswap, the order is typically: Juggler A right, Juggler B right, Juggler A left, Juggler B left.");

    [McpServerResource]
    [Description("siteswap:definition:hijacking")]
    public Task<string> siteswap_definition_hijacking() =>
        Task.FromResult("Hijacking is a passing technique where one passer actively changes the pattern and passively transitions their partner into a compatible pattern. The active passer gains one club locally (+1).");

    [McpServerResource]
    [Description("siteswap:definition:lowjacking")]
    public Task<string> siteswap_definition_lowjacking() =>
        Task.FromResult("Lowjacking is a passing technique where one passer actively changes the pattern and passively transitions their partner into a compatible pattern. The active passer loses one club locally (-1).");

    [McpServerResource]
    [Description("siteswap:definition:space-time-diagram")]
    public Task<string> siteswap_definition_space_time_diagram() =>
        Task.FromResult("A space-time diagram (ladder diagram) shows the flight paths of objects over time. The horizontal axis is time, the vertical axis is space between the hands.");

    [McpServerResource]
    [Description("siteswap:definition:prime-siteswap")]
    public Task<string> siteswap_definition_prime_siteswap() =>
        Task.FromResult("A prime siteswap is a siteswap whose path in the state diagram does not traverse any state more than once. Siteswaps that are not prime are called composite.");

    [McpServerResource]
    [Description("siteswap:definition:composite-siteswap")]
    public Task<string> siteswap_definition_composite_siteswap() =>
        Task.FromResult("A composite siteswap can be split into shorter valid patterns with the same number of props. Example: 44404413 can be split into 4440, 441, and 3.");

    [McpServerResource]
    [Description("siteswap:definition:transition-throw")]
    public Task<string> siteswap_definition_transition_throw() =>
        Task.FromResult("A transition throw is a throw that is not part of the pattern being transitioned to or from. It is used to switch between patterns.");

    [McpServerResource]
    [Description("siteswap:definition:programming")]
    public Task<string> siteswap_definition_programming() =>
        Task.FromResult("Programming is a collective term for hijacking and lowjacking. A passer 'programs' their partner by actively changing the pattern.");

    [McpServerResource]
    [Description("siteswap:rule:averaging-theorem")]
    public Task<string> siteswap_rule_averaging_theorem() =>
        Task.FromResult("The number of objects in a siteswap pattern is the average of the numbers in the sequence. Formula: Number of objects = (Sum of all values) / (Number of values). The average must be an integer.");

    [McpServerResource]
    [Description("siteswap:rule:validity-condition")]
    public Task<string> siteswap_rule_validity_condition() =>
        Task.FromResult("A vanilla siteswap is valid if no two throws land at the same time. Equivalent: At each beat, exactly one object is caught (except for 0 throws, where none is caught).");

    [McpServerResource]
    [Description("siteswap:rule:modular-validity")]
    public Task<string> siteswap_rule_modular_validity() =>
        Task.FromResult("A vanilla siteswap sequence a₀a₁...aₙ₋₁ with period n is valid if the set S = {(aᵢ + i) mod n | 0 ≤ i < n} has exactly n elements (no duplicates).");

    [McpServerResource]
    [Description("siteswap:rule:orbit-ball-count")]
    public Task<string> siteswap_rule_orbit_ball_count() =>
        Task.FromResult("The number of objects in an orbit is the sum of the values in that orbit divided by the period of the entire pattern.");

    [McpServerResource]
    [Description("siteswap:rule:state-transition")]
    public Task<string> siteswap_rule_state_transition() =>
        Task.FromResult("From a state: Remove the leftmost x (next landing), shift everything one position left, add a - on the right. If an object was caught, a 0 cannot be thrown. Place an x at position n-1 for an n throw, if there is a - there.");

    [McpServerResource]
    [Description("siteswap:rule:sync-even-numbers")]
    public Task<string> siteswap_rule_sync_even_numbers() =>
        Task.FromResult("Synchronous siteswaps contain only even numbers. Odd numbers are not allowed in synchronous patterns.");

    [McpServerResource]
    [Description("siteswap:rule:sync-crossing-x")]
    public Task<string> siteswap_rule_sync_crossing_x() =>
        Task.FromResult("In synchronous siteswaps: An 'x' after an even number means the throw goes to the other hand. An 'x' after an odd number (only in transitions) means the throw goes to the same hand.");

    [McpServerResource]
    [Description("siteswap:rule:sync-empty-beat")]
    public Task<string> siteswap_rule_sync_empty_beat() =>
        Task.FromResult("In synchronous patterns, two beats are counted for each pair of synchronous throws, even though both throws occur simultaneously. There is an 'empty beat' after each synchronous pair.");

    [McpServerResource]
    [Description("siteswap:rule:sync-asterisk")]
    public Task<string> siteswap_rule_sync_asterisk() =>
        Task.FromResult("An asterisk (*) at the end of a synchronous sequence means the roles of the hands reverse on each repetition. (4,2x)* is short for (4,2x)(2x,4).");

    [McpServerResource]
    [Description("siteswap:rule:sync-exclamation")]
    public Task<string> siteswap_rule_sync_exclamation() =>
        Task.FromResult("An exclamation mark (!) after a synchronous pair means there is no empty beat after it. Used in transitions between sync and async patterns.");

    [McpServerResource]
    [Description("siteswap:rule:multiplex-averaging")]
    public Task<string> siteswap_rule_multiplex_averaging() =>
        Task.FromResult("For multiplex siteswaps: Add all numbers inside brackets and outside, divide by the number of throw times (not the number of objects thrown).");

    [McpServerResource]
    [Description("siteswap:rule:multiplex-zero-ignore")]
    public Task<string> siteswap_rule_multiplex_zero_ignore() =>
        Task.FromResult("A 0 in multiplex brackets can be ignored. [30] can be simplified to 3.");

    [McpServerResource]
    [Description("siteswap:rule:multiplex-2-hold")]
    public Task<string> siteswap_rule_multiplex_2_hold() =>
        Task.FromResult("If multiplex brackets contain a 2, it means one object stays in the hand instead of being thrown. It may not be an actual multiplex throw.");

    [McpServerResource]
    [Description("siteswap:rule:multiplex-1-sliced")]
    public Task<string> siteswap_rule_multiplex_1_sliced() =>
        Task.FromResult("If multiplex brackets contain a 1, it is a sliced throw.");

    [McpServerResource]
    [Description("siteswap:rule:4-handed-even-self")]
    public Task<string> siteswap_rule_4_handed_even_self() =>
        Task.FromResult("In 4-handed siteswap: Even numbers are always self throws (not passed), the type of throw that would be notated as half that number in a solo siteswap. Odd numbers are passes.");

    [McpServerResource]
    [Description("siteswap:rule:4-handed-diagonal-straight")]
    public Task<string> siteswap_rule_4_handed_diagonal_straight() =>
        Task.FromResult("In 4-handed siteswap: If the global throwing order is Juggler #1 right, #2 right, #1 left, #2 left, then Juggler #1 passes diagonally on 1s, 5s, etc. and straight on 3s, 7s, etc. Juggler #2 does the opposite.");

    [McpServerResource]
    [Description("siteswap:rule:passing-p-notation")]
    public Task<string> siteswap_rule_passing_p_notation() =>
        Task.FromResult("A 'p' after a number in passing siteswap means the throw is passed to the partner. Without 'p' it is a self throw.");

    [McpServerResource]
    [Description("siteswap:rule:passing-multiple-jugglers")]
    public Task<string> siteswap_rule_passing_multiple_jugglers() =>
        Task.FromResult("With more than two jugglers, a number after the 'p' can specify which juggler to pass to. Juggler #1 is the leftmost in |, #2 is next, etc.");

    [McpServerResource]
    [Description("siteswap:rule:passing-fraction")]
    public Task<string> siteswap_rule_passing_fraction() =>
        Task.FromResult("Fractions in passing siteswap mean the juggler after | throws half a beat later, and all fractions are passes. Example: <4.5 3 3|3 4 3.5>.");

    [McpServerResource]
    [Description("siteswap:rule:height-formula")]
    public Task<string> siteswap_rule_height_formula() =>
        Task.FromResult("The actual height of a throw (from hand height to peak) is h = g × (st - 2dt)² / 8, where g = acceleration due to gravity (9.8 m/s²), s = siteswap number, t = time between throws, d = dwell ratio.");

    [McpServerResource]
    [Description("siteswap:rule:relative-heights")]
    public Task<string> siteswap_rule_relative_heights() =>
        Task.FromResult("To compare relative heights of two siteswap numbers: Subtract 1 from each number, square the results, divide the squares. Example: (9-1)²/(5-1)² = 64/16 = 4, so a 9 is about four times as high as a 5.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-period-pass")]
    public Task<string> siteswap_rule_hijacking_period_pass() =>
        Task.FromResult("A 'pass' for hijacking is defined as period + 2. In period-5 patterns, a pass is a 7, in period-3 patterns a 5 (zap).");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-empty-hand-zip")]
    public Task<string> siteswap_rule_hijacking_empty_hand_zip() =>
        Task.FromResult("If you have an empty hand because you did not receive a 'pass', zip into the empty hand.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-zip-to-pass")]
    public Task<string> siteswap_rule_hijacking_zip_to_pass() =>
        Task.FromResult("If you were going to zip into a hand, but a pass is coming to that hand, throw a 'pass' instead.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-period-constant")]
    public Task<string> siteswap_rule_hijacking_period_constant() =>
        Task.FromResult("The period of the pattern before and after a hijacking transition must be the same.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-club-change")]
    public Task<string> siteswap_rule_hijacking_club_change() =>
        Task.FromResult("A hijacking transition results in one passer gaining one club locally, the other losing one club locally.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-transition-throws")]
    public Task<string> siteswap_rule_hijacking_transition_throws() =>
        Task.FromResult("There can be 0 or 1 active transition throws (a throw that is not part of the pattern being transitioned to or from).");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-passive-throws")]
    public Task<string> siteswap_rule_hijacking_passive_throws() =>
        Task.FromResult("All throws on the passive side must belong to a pattern being transitioned to or from.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-minimal-change")]
    public Task<string> siteswap_rule_hijacking_minimal_change() =>
        Task.FromResult("The passive responder must make a causally minimal change to their juggling in response to an active transition.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-global-validity")]
    public Task<string> siteswap_rule_hijacking_global_validity() =>
        Task.FromResult("All global patterns with twice the local period must be valid 4-handed siteswaps.");

    [McpServerResource]
    [Description("siteswap:rule:hijacking-passes-required")]
    public Task<string> siteswap_rule_hijacking_passes_required() =>
        Task.FromResult("All patterns must have one or more passes. Not just period+2 passes, but any pass satisfies this condition.");

    [McpServerResource]
    [Description("siteswap:transformation:rotation")]
    public Task<string> siteswap_transformation_rotation() =>
        Task.FromResult("Move a throw from the beginning of the notation to the end to get another representation of the same pattern. Example: 423 becomes 234.");

    [McpServerResource]
    [Description("siteswap:transformation:repetition")]
    public Task<string> siteswap_transformation_repetition() =>
        Task.FromResult("Repeat the same sequence of numbers to get another way of writing the same pattern. Example: 3 becomes 33.");

    [McpServerResource]
    [Description("siteswap:transformation:period-addition")]
    public Task<string> siteswap_transformation_period_addition() =>
        Task.FromResult("Add the period to a number (or subtract it) to get a pattern with one more (or less) object. Example: 42 becomes 62 or 22 when the first throw is changed.");

    [McpServerResource]
    [Description("siteswap:transformation:uniform-increment")]
    public Task<string> siteswap_transformation_uniform_increment() =>
        Task.FromResult("Add 1 to each number (or subtract 1) to get a pattern with one more (or less) object. Example: 441 becomes 552 or 330.");

    [McpServerResource]
    [Description("siteswap:transformation:swap-property")]
    public Task<string> siteswap_transformation_swap_property() =>
        Task.FromResult("Swap two consecutive numbers, add 1 to the first and subtract 1 from the second. This corresponds to swapping the landing times of those two throws. Example: 522 becomes 531 when the 2s are swapped.");

    [McpServerResource]
    [Description("siteswap:transformation:swap-non-consecutive")]
    public Task<string> siteswap_transformation_swap_non_consecutive() =>
        Task.FromResult("Swap two non-consecutive numbers, add the distance to the first and subtract the distance from the second. Example: In 5511, the second 5 and second 1 are two beats apart. Swap them (to 5115), add 2 to the first, subtract 2 from the second: becomes 5313.");

    [McpServerResource]
    [Description("siteswap:transformation:time-reversal")]
    public Task<string> siteswap_transformation_time_reversal() =>
        Task.FromResult("Move each number that many positions to the right (with wrap-around), then read the resulting sequence backwards to get the time-reversed version. Example: 603 becomes 360.");

    [McpServerResource]
    [Description("siteswap:transformation:dual")]
    public Task<string> siteswap_transformation_dual() =>
        Task.FromResult("Subtract each number from twice the number of objects, then read the sequence backwards to get the dual. Example: Subtract each number in 504 from 6: becomes 162, read backwards: 261 (normally written as 612).");

    [McpServerResource]
    [Description("siteswap:transformation:state-replacement")]
    public Task<string> siteswap_transformation_state_replacement() =>
        Task.FromResult("Replace a section of a siteswap with a different sequence of throws that starts and ends in the same state as that section. Example: 531 becomes 73131 (both sequences 5 and 731 end in state 11001, starting from ground state 111).");

    [McpServerResource]
    [Description("siteswap:transformation:state-combination")]
    public Task<string> siteswap_transformation_state_combination() =>
        Task.FromResult("Combine two patterns that visit the same state by switching to the other pattern when you reach that state. Example: 51 visits state 10101 after the 5, 60 visits this state before the 6, combined: 5601.");

    [McpServerResource]
    [Description("siteswap:transformation:sync-swap")]
    public Task<string> siteswap_transformation_sync_swap() =>
        Task.FromResult("Swap a pair of synchronous throws and change each of those throws to a crossing throw if it was non-crossing, or vice versa. Example: (6x,4)(2,4x) becomes (4x,6)(2,4x) or (6x,4)(4,2x).");

    [McpServerResource]
    [Description("siteswap:transformation:slide-property")]
    public Task<string> siteswap_transformation_slide_property() =>
        Task.FromResult("Convert a synchronous sequence (a₀,a₁)(a₂,a₃)... into two vanilla sequences b₀b₁... and c₀c₁..., where bᵢ = aᵢ+1 if i is even and aᵢ crosses, aᵢ-1 if i is odd and aᵢ crosses, otherwise aᵢ. cᵢ = aᵢ+2 if i is even and aᵢ crosses, aᵢ-2 if i is odd and aᵢ crosses, aᵢ+1 if i is even and aᵢ does not cross, aᵢ-1 if i is odd and aᵢ does not cross.");

    [McpServerResource]
    [Description("siteswap:transformation:shower-conversion")]
    public Task<string> siteswap_transformation_shower_conversion() =>
        Task.FromResult("Turn a vanilla siteswap into a showered version by replacing each number with the notation for a shower with that number of objects. Example: 534 becomes 915171.");

    [McpServerResource]
    [Description("siteswap:transformation:one-handed-conversion")]
    public Task<string> siteswap_transformation_one_handed_conversion() =>
        Task.FromResult("Turn a vanilla siteswap into a one-handed version by replacing each number with the notation for a one-handed pattern with that number of objects. Example: 534 becomes a06080.");

    [McpServerResource]
    [Description("siteswap:example:cascade-3")]
    public Task<string> siteswap_example_cascade_3() =>
        Task.FromResult("3 is the 3-ball cascade. Every throw is a 3, which goes to the other hand. Sequence: ...333333...");

    [McpServerResource]
    [Description("siteswap:example:shower-51")]
    public Task<string> siteswap_example_shower_51() =>
        Task.FromResult("51 is the 3-ball shower. One hand throws high 5s, the other hand throws quick 1s (handoffs). Sequence: ...515151...");

    [McpServerResource]
    [Description("siteswap:example:fountain-4")]
    public Task<string> siteswap_example_fountain_4() =>
        Task.FromResult("4 is the 4-ball fountain. Every throw is a 4, which returns to the same hand. Sequence: ...444444...");

    [McpServerResource]
    [Description("siteswap:example:pattern-441")]
    public Task<string> siteswap_example_pattern_441() =>
        Task.FromResult("441 is a 3-ball pattern. Mostly non-crossing 4s are thrown, every three beats a ball is passed directly to the other hand (1). Average: (4+4+1)/3 = 3.");

    [McpServerResource]
    [Description("siteswap:example:pattern-531")]
    public Task<string> siteswap_example_pattern_531() =>
        Task.FromResult("531 is a 3-ball pattern. High crossing throw (5), lower crossing throw (3), pass straight to the other hand (1). Average: (5+3+1)/3 = 3.");

    [McpServerResource]
    [Description("siteswap:example:pattern-423")]
    public Task<string> siteswap_example_pattern_423() =>
        Task.FromResult("423 is a 3-ball pattern. Non-crossing throw (4), pause with ball in hand (2), lower crossing throw (3). Average: (4+2+3)/3 = 3.");

    [McpServerResource]
    [Description("siteswap:example:pattern-53")]
    public Task<string> siteswap_example_pattern_53() =>
        Task.FromResult("53 is the 4-ball half-shower. One hand throws like a 5-ball cascade, the other like a 3-ball cascade. Average: (5+3)/2 = 4.");

    [McpServerResource]
    [Description("siteswap:example:pattern-20")]
    public Task<string> siteswap_example_pattern_20() =>
        Task.FromResult("20: One hand holds a ball (2), the other hand is empty (0). Average: (2+0)/2 = 1.");

    [McpServerResource]
    [Description("siteswap:example:pattern-31")]
    public Task<string> siteswap_example_pattern_31() =>
        Task.FromResult("31 is the 2-ball shower. One hand throws 3s, the other throws 1s. Average: (3+1)/2 = 2.");

    [McpServerResource]
    [Description("siteswap:example:pattern-330")]
    public Task<string> siteswap_example_pattern_330() =>
        Task.FromResult("330: Mostly 3s are thrown like a 3-ball cascade, but every three beats one hand is empty (0), because only 2 balls are used. Average: (3+3+0)/3 = 2.");

    [McpServerResource]
    [Description("siteswap:example:pattern-411")]
    public Task<string> siteswap_example_pattern_411() =>
        Task.FromResult("411 is a 3-ball pattern. Throw like a 4-ball fountain, then pass to the other hand, then pass back. Average: (4+1+1)/3 = 2, but actually 3 balls.");

    [McpServerResource]
    [Description("siteswap:example:pattern-522")]
    public Task<string> siteswap_example_pattern_522() =>
        Task.FromResult("522 is a 3-ball pattern. A ball is only thrown every three beats (5), the other two beats are 2s (ball is held). Actually a slower version of 3.");

    [McpServerResource]
    [Description("siteswap:example:pattern-633")]
    public Task<string> siteswap_example_pattern_633() =>
        Task.FromResult("633 is a 3-ball pattern. One high non-crossing throw like a 6-ball fountain, then two lower crossing throws like a 3-ball cascade.");

    [McpServerResource]
    [Description("siteswap:example:pattern-201")]
    public Task<string> siteswap_example_pattern_201() =>
        Task.FromResult("201: One hand holds a ball (2), the other is empty (0), then the ball is passed to the other hand (1). A slower version of 1.");

    [McpServerResource]
    [Description("siteswap:example:pattern-312")]
    public Task<string> siteswap_example_pattern_312() =>
        Task.FromResult("312 is a 3-ball pattern. Crossing throw (3), pass to the other hand (1), pause with ball in hand (2).");

    [McpServerResource]
    [Description("siteswap:example:pattern-420")]
    public Task<string> siteswap_example_pattern_420() =>
        Task.FromResult("420 is a 2-ball pattern. Non-crossing throw (4), pause with ball (2), empty hand (0). A slower version of 2 with active 2s.");

    [McpServerResource]
    [Description("siteswap:example:pattern-642")]
    public Task<string> siteswap_example_pattern_642() =>
        Task.FromResult("642 is a 4-ball pattern. High non-crossing throw (6), lower non-crossing throw (4), pause with ball (2). Average: (6+4+2)/3 = 4.");

    [McpServerResource]
    [Description("siteswap:example:pattern-4400")]
    public Task<string> siteswap_example_pattern_4400() =>
        Task.FromResult("4400 is a 2-ball pattern. Two non-crossing throws (4,4), then two empty hands (0,0). Average: (4+4+0+0)/4 = 2.");

    [McpServerResource]
    [Description("siteswap:example:pattern-5511")]
    public Task<string> siteswap_example_pattern_5511() =>
        Task.FromResult("5511 is a 4-ball pattern. Each hand makes a high non-crossing throw (5), then each hand passes a ball directly (1). Average: (5+5+1+1)/4 = 3, but actually 4 balls.");

    [McpServerResource]
    [Description("siteswap:example:pattern-6622")]
    public Task<string> siteswap_example_pattern_6622() =>
        Task.FromResult("6622 is a 4-ball pattern. Two high non-crossing throws (6,6), alternating with two beats where balls are held (2,2). Average: (6+6+2+2)/4 = 4.");

    [McpServerResource]
    [Description("siteswap:example:pattern-744")]
    public Task<string> siteswap_example_pattern_744() =>
        Task.FromResult("744 is a 5-ball pattern. Average: (7+4+4)/3 = 5. Ground-state, can be entered directly from the cascade.");

    [McpServerResource]
    [Description("siteswap:example:pattern-771")]
    public Task<string> siteswap_example_pattern_771() =>
        Task.FromResult("771 is a 5-ball pattern. Average: (7+7+1)/3 = 5. Excited-state, requires transition throws to enter.");

    [McpServerResource]
    [Description("siteswap:example:pattern-55550")]
    public Task<string> siteswap_example_pattern_55550() =>
        Task.FromResult("55550 is a 4-ball pattern. Like a 5-ball cascade, except the last ball is missing, so an empty hand (0) instead of a 5. Average: (5+5+5+5+0)/5 = 4.");

    [McpServerResource]
    [Description("siteswap:example:pattern-55500")]
    public Task<string> siteswap_example_pattern_55500() =>
        Task.FromResult("55500 is a 3-ball pattern. A 5-ball cascade with two balls missing. Average: (5+5+5+0+0)/5 = 3.");

    [McpServerResource]
    [Description("siteswap:example:pattern-50505")]
    public Task<string> siteswap_example_pattern_50505() =>
        Task.FromResult("50505 is a 3-ball pattern. A 5-ball cascade with two balls missing, but not two consecutive ones. Average: (5+0+5+0+5)/5 = 3.");

    [McpServerResource]
    [Description("siteswap:example:sync-shower")]
    public Task<string> siteswap_example_sync_shower() =>
        Task.FromResult("(4x,2x) is the synchronous 3-ball shower. Both hands throw simultaneously: one hand throws high crossing 4s, the other quick crossing 2s.");

    [McpServerResource]
    [Description("siteswap:example:sync-box")]
    public Task<string> siteswap_example_sync_box() =>
        Task.FromResult("(4,2x)(2x,4) or (4,2x)* is the 3-ball box pattern. One hand throws high non-crossing 4s, the other quick crossing 2s, then roles switch.");

    [McpServerResource]
    [Description("siteswap:example:sync-2x-2x")]
    public Task<string> siteswap_example_sync_2x_2x() =>
        Task.FromResult("(2x,2x) is a synchronous pattern. Every throw is a 2x: two throws are made (simultaneously) before the same ball is thrown again (because of the 2), and the ball goes to the other hand (because of the x).");

    [McpServerResource]
    [Description("siteswap:example:sync-4-0")]
    public Task<string> siteswap_example_sync_4_0() =>
        Task.FromResult("(4,0) is a synchronous pattern. One hand throws like a 4-ball fountain, the other hand is empty. This is half of a 4-ball pattern.");

    [McpServerResource]
    [Description("siteswap:example:sync-6-4")]
    public Task<string> siteswap_example_sync_6_4() =>
        Task.FromResult("(6,4) is a synchronous pattern. One side throws like a 6-ball fountain (3 in one hand), the other side like a 4-ball fountain (2 in one hand).");

    [McpServerResource]
    [Description("siteswap:example:sync-4x-2x-star")]
    public Task<string> siteswap_example_sync_4x_2x_star() =>
        Task.FromResult("(4x,2x)* or (4x,2x)(2x,4) is a synchronous pattern. One hand throws high crossing 4s, the other quick crossing 2s, then roles switch.");

    [McpServerResource]
    [Description("siteswap:example:multiplex-duplex")]
    public Task<string> siteswap_example_multiplex_duplex() =>
        Task.FromResult("[33]33 is a 3-ball cascade with duplex stacks. A pair of balls is always thrown together ([33]), then a single ball (3), then the pair again.");

    [McpServerResource]
    [Description("siteswap:example:multiplex-43-23")]
    public Task<string> siteswap_example_multiplex_43_23() =>
        Task.FromResult("[43]23 is a 4-ball multiplex pattern. Average: (4+3+2+3)/4 = 12/4 = 3, but actually 4 balls, since [43] counts as one throw time.");

    [McpServerResource]
    [Description("siteswap:example:passing-2-count")]
    public Task<string> siteswap_example_passing_2_count() =>
        Task.FromResult("<3p 3|3p 3> is a 6-prop 2-count passing pattern. All left-hand throws are passes (3p), all right-hand throws are selfs (3).");

    [McpServerResource]
    [Description("siteswap:example:passing-social")]
    public Task<string> siteswap_example_passing_social() =>
        Task.FromResult("<4p 3|3 4p> becomes 4p 3 as a social siteswap, since both jugglers juggle the same pattern, only time-shifted.");

    [McpServerResource]
    [Description("siteswap:example:hijacking-855")]
    public Task<string> siteswap_example_hijacking_855() =>
        Task.FromResult("855 is a period-3 passing pattern. Transition out of 855: [855][885] active transition goes up 1 club (hijack), [585][582] passive response goes down 1 club.");

    [McpServerResource]
    [Description("siteswap:example:hijacking-975")]
    public Task<string> siteswap_example_hijacking_975() =>
        Task.FromResult("975 (Holy Grail) is a period-3 passing pattern. Transition out of 975: [579][879] active transition goes up 1 club (hijack), [957][927] passive response goes down 1 club.");

    [McpServerResource]
    [Description("siteswap:example:hijacking-period-5")]
    public Task<string> siteswap_example_hijacking_period_5() =>
        Task.FromResult("In period-5 patterns, the hijackable passes are singles: 5+2=7. Classic hijacks by Aidan Burns fall into this category.");

    [McpServerResource]
    [Description("siteswap:example:hijacking-period-7")]
    public Task<string> siteswap_example_hijacking_period_7() =>
        Task.FromResult("In period-7 patterns, the hijackable passes are: 7+2=9. Example: 9788827 can transition to 9797226 vs 9797888.");

    [McpServerResource]
    [Description("siteswap:example:orbit-561")]
    public Task<string> siteswap_example_orbit_561() =>
        Task.FromResult("561 has two orbits: (5,1) with 2 balls (sum 6, period 3, 6/3=2) and (6) with 2 balls (sum 6, period 3, 6/3=2).");

    [McpServerResource]
    [Description("siteswap:example:orbit-72312")]
    public Task<string> siteswap_example_orbit_72312() =>
        Task.FromResult("72312 has two orbits: (7,3) with 2 balls and (2,1,2) with 1 ball. The (7,3) orbit has 2 balls going in opposite directions.");

    [McpServerResource]
    [Description("siteswap:example:orbit-5241")]
    public Task<string> siteswap_example_orbit_5241() =>
        Task.FromResult("5241 has two orbits: (5,2,1) with 2 balls (sum 8, period 4, 8/4=2) and (4) with 1 ball (sum 4, period 4, 4/4=1).");

    [McpServerResource]
    [Description("siteswap:example:orbit-5551")]
    public Task<string> siteswap_example_orbit_5551() =>
        Task.FromResult("5551 has only one orbit (5,5,5,1) with 4 balls (sum 16, period 4, 16/4=4). This pattern mixes the balls completely.");

    [McpServerResource]
    [Description("siteswap:example:orbit-55514")]
    public Task<string> siteswap_example_orbit_55514() =>
        Task.FromResult("55514 has four orbits, each with 1 ball: (5), (5), (5), (1,4). Each ball stays in its own orbit.");

    [McpServerResource]
    [Description("siteswap:example:state-xxx")]
    public Task<string> siteswap_example_state_xxx() =>
        Task.FromResult("State xxx means: One ball lands now (next beat), one ball lands in one beat, one ball lands in two beats. This is the ground state for 3 balls.");

    [McpServerResource]
    [Description("siteswap:example:state-transition-3")]
    public Task<string> siteswap_example_state_transition_3() =>
        Task.FromResult("From state xxx: Throw a 3. Remove leftmost x, shift: xx-, place x at position 2: xxx. The state remains xxx (ground state).");

    [McpServerResource]
    [Description("siteswap:example:state-transition-4")]
    public Task<string> siteswap_example_state_transition_4() =>
        Task.FromResult("From state xxx: Throw a 4. Remove leftmost x, shift: xx-, place x at position 3: xx-x. New state: xx-x.");

    [McpServerResource]
    [Description("siteswap:example:state-binary")]
    public Task<string> siteswap_example_state_binary() =>
        Task.FromResult("A state can be represented as a binary number: 1 for ball lands, 0 for no landing. Example: xxx-x becomes 11101 (binary) or 29 (decimal).");

    [McpServerResource]
    [Description("siteswap:example:state-multiplex")]
    public Task<string> siteswap_example_state_multiplex() =>
        Task.FromResult("In multiplex states, the binary representation can contain numbers greater than 1: 2 means 2 balls land. Example: State 2-1-0 means 2 balls land now, 1 in one beat, 0 in two beats.");

    [McpServerResource]
    [Description("siteswap:example:invalid-543")]
    public Task<string> siteswap_example_invalid_543() =>
        Task.FromResult("543 is invalid despite integer average (4+3)/2=4. All three throws land at the same time: 5 lands at beat 5, 4 at beat 4, 3 at beat 3, but all are part of the same 3-beat sequence and collide.");

    [McpServerResource]
    [Description("siteswap:example:invalid-513")]
    public Task<string> siteswap_example_invalid_513() =>
        Task.FromResult("513 is invalid. Calculation: (5+0) mod 3 = 2, (1+1) mod 3 = 2, (3+2) mod 3 = 2. All values are 2, therefore duplicates. The sequence is invalid.");

    [McpServerResource]
    [Description("siteswap:example:valid-531")]
    public Task<string> siteswap_example_valid_531() =>
        Task.FromResult("531 is valid. Calculation: (5+0) mod 3 = 2, (3+1) mod 3 = 1, (1+2) mod 3 = 0. Values 2, 1, 0 are all unique, therefore valid.");

}
