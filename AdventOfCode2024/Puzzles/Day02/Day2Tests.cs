using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day2;

public sealed class Day2Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 2, level: 1).
			SolveUsing<int[], int, SolverA>().
			AssertingResult(ParseLine).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 2, level: 2).
			SolveUsing<int[], int, SolverB>().
			AssertingResult(ParseLine).
			Run(_output);
	}


	private static int[] ParseLine(string line)
	{
		string[] levels = line.Split(' ');
		return levels.Select(int.Parse).ToArray();
	}
}


[UsedImplicitly]
file sealed class SolverA : SolverWithArrayInput<int[], int>
{
	protected override int Solve(int[][] entries) =>
		entries.Where(IsSafe).Count();

	private static bool IsSafe(int[] levels)
	{
		int prev = levels[0];
		bool asc = levels[1] > prev;
		foreach (int current in levels.Skip(1))
		{
			int delta = current - prev;
			if (delta is < -3 or > 3 or 0) return false;
			if (asc && delta < 0 || !asc && delta > 0) return false;

			prev = current;
		}
		return true;
	}
}

[UsedImplicitly]
file sealed class SolverB : SolverWithArrayInput<int[], int>
{
	protected override int Solve(int[][] entries) =>
		entries.Where(IsSafeWithTolerance).Count();

	private static bool IsSafeWithTolerance(int[] levels)
	{
		for (int i = -1; i < levels.Length; i++)
		{
			if (IsSafe(levels, i)) return true;
		}
		return false;
	}

	private static bool IsSafe(int[] levels, int indexToSkip)
	{
		int prev = indexToSkip == 0 ? levels[1] : levels[0];
		int second = indexToSkip == 1 ? levels[2] : levels[3];
		bool asc = second > prev;
		int initialIndex = indexToSkip == 0 ? 2 : 1;

		for (int i = initialIndex; i < levels.Length; ++i)
		{
			if (i == indexToSkip) continue;

			int current = levels[i];
			int delta = current - prev;
			bool error =
				delta is < -3 or > 3 or 0 ||
				asc && delta < 0 ||
				!asc && delta > 0;
			if (error) return false;

			prev = current;
		}
		return true;
	}
}
