using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day19;

public sealed class Day19Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			r, wr, b, g, bwu, rb, gb, br
			
			brwrr
			bggr
			gbbr
			rrbgbr
			ubwu
			bwurrg
			brgr
			bbrgwb
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 19, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 19, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected abstract long Solve(IReadOnlyCollection<string> patterns, IReadOnlyCollection<string> designs);

	protected override long Solve(string[] lines)
	{
		var (patterns, designs) = Parse(lines);
		return Solve(patterns, designs);
	}

	private static (IReadOnlyCollection<string> patterns, IReadOnlyCollection<string> designs) Parse(string[] lines)
	{
		IReadOnlyCollection<string> patterns = lines[0].
			Split(',', StringSplitOptions.TrimEntries).
			ToList();
		IReadOnlyCollection<string> designs = lines.
			Skip(2).
			ToList();
		return (patterns, designs);
	}
}


internal sealed class ColorPatternsIndex
{
	private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _patterns;

	public ColorPatternsIndex(IEnumerable<string> patterns)
	{
		_patterns = patterns.ToHashSet().GetAlternateLookup<ReadOnlySpan<char>>();
	}

	public bool Match(ReadOnlySpan<char> pattern) => _patterns.Contains(pattern);
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(IReadOnlyCollection<string> patterns, IReadOnlyCollection<string> designs)
	{
		ColorPatternsIndex index = new(patterns);
		return designs.Count(design => CanDisplay(design, index));
	}

	private static bool CanDisplay(ReadOnlySpan<char> design, ColorPatternsIndex index)
	{
		if (design.Length == 0) return true;
		for (int prefixLength = 1; prefixLength <= design.Length; ++prefixLength)
		{
			ReadOnlySpan<char> prefix = design[..prefixLength];
			if (!index.Match(prefix)) continue;

			bool canDisplay = CanDisplay(design[prefixLength..], index);
			if (canDisplay) return true;
		}
		return false;
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(IReadOnlyCollection<string> patterns, IReadOnlyCollection<string> designs)
	{
		ColorPatternsIndex index = new(patterns);
		Dictionary<string, long> memo = [];
		Dictionary<string, long>.AlternateLookup<ReadOnlySpan<char>> memoSpan = memo.GetAlternateLookup<ReadOnlySpan<char>>();
		return designs.Sum(design => CalculateDisplaysQuantity(design, memoSpan, index));
	}

	private static long CalculateDisplaysQuantity(
		ReadOnlySpan<char> design,
		Dictionary<string, long>.AlternateLookup<ReadOnlySpan<char>> memo,
		ColorPatternsIndex index
		)
	{
		if (design.Length == 0) return 1;

		if (memo.TryGetValue(design, out long resultFromMemo)) return resultFromMemo;

		long result = 0;
		for (int prefixLength = 1; prefixLength <= design.Length; ++prefixLength)
		{
			ReadOnlySpan<char> prefix = design[..prefixLength];
			if (!index.Match(prefix)) continue;

			result += CalculateDisplaysQuantity(design[prefixLength..], memo, index);
		}

		memo[design] = result;
		return result;
	}
}
