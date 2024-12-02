using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day1;

using Line = (int left, int right);

public sealed class Day1Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 1, level: 1).
			SolveUsing<Line, long, SolverA>().
			AssertingResult(ParseLine).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 1, level: 2).
			SolveUsing<Line, long, SolverB>().
			AssertingResult(ParseLine).
			Run(_output);
	}


	private static Line ParseLine(string line)
	{
		string[] parts = line.Split("   ");
		return (int.Parse(parts[0]), int.Parse(parts[1]));
	}
}


[UsedImplicitly]
file sealed class SolverA : SolverWithArrayInput<Line, long>
{
	protected override long Solve(Line[] entries)
	{
		List<int> left = new();
		List<int> right = new();
		foreach ((int l, int r) in entries)
		{
			left.Add(l);
			right.Add(r);
		}

		left.Sort();
		right.Sort();

		int result = 0;
		for (int i = 0; i < left.Count; i++)
		{
			int delta = Math.Abs(left[i] - right[i]);
			result += delta;
		}

		return result;
	}
}

[UsedImplicitly]
file sealed class SolverB : SolverWithArrayInput<Line, long>
{
	protected override long Solve(Line[] entries)
	{
		List<int> left = new();
		List<int> right = new();
		foreach ((int l, int r) in entries)
		{
			left.Add(l);
			right.Add(r);
		}

		Dictionary<int, int> rightCounts = right.CountBy(id => id).ToDictionary();
		
		int result = left.Sum(id => rightCounts.TryGetValue(id, out int count) ? id * count : 0);

		return result;
	}
}
