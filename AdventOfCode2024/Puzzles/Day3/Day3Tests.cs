using System.Text.RegularExpressions;

using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day3;

public sealed class Day3Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 3, level: 1).
			SolveUsing<string, int, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 3, level: 2).
			SolveUsing<string, int, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract partial class SolverBase : SolverWithArrayInput<string, int>
{
	[GeneratedRegex("mul\\((\\d{1,3}),(\\d{1,3})\\)")]
	private static partial Regex MulOpRegex { get; }

	protected abstract IEnumerable<string> GetEffectiveLines(string[] lines);

	protected override int Solve(string[] lines)
	{
		string text = string.Concat(GetEffectiveLines(lines));
		return MulOpRegex.
			Matches(text).
			Select(parseAndApplyOperation).
			Sum();

		static int parseAndApplyOperation(Match match)
		{
			int operand1 = int.Parse(match.Groups[1].Value);
			int operand2 = int.Parse(match.Groups[2].Value);
			return operand1 * operand2;
		}
	}
}

[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override IEnumerable<string> GetEffectiveLines(string[] lines) => lines;
}

[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override IEnumerable<string> GetEffectiveLines(string[] lines)
	{
		string text = string.Concat(lines);
		return text.
			Split("do()", StringSplitOptions.RemoveEmptyEntries).
			Select(part => part.Split("don't()", 2, StringSplitOptions.RemoveEmptyEntries)[0]);
	}
}
