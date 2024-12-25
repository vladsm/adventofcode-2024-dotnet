using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day25;

public sealed class Day25Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			#####
			.####
			.####
			.####
			.#.#.
			.#...
			.....
			
			#####
			##.##
			.#.##
			...##
			...#.
			...#.
			.....
			
			.....
			#....
			#....
			#...#
			#.#.#
			#.###
			#####
			
			.....
			.....
			#.#..
			###..
			###.#
			###.#
			#####
			
			.....
			.....
			.....
			#....
			#.#..
			#.#.#
			#####
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 25, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 25, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected override long Solve(string[] lines)
	{
		var (keys, locks, size, height) = Parse(lines);
		return Solve(keys, locks, size, height);
	}

	protected abstract long Solve(IReadOnlyCollection<int[]> keys, IReadOnlyCollection<int[]> locks, int size, int height);


	private static (IReadOnlyCollection<int[]> keys, IReadOnlyCollection<int[]> locks, int size, int height) Parse(string[] lines)
	{
		int xSize = lines[0].Length;
		int ySize = 0;
		foreach (string line in lines)
		{
			if (line.Length == 0) break;
			++ySize;
		}

		IEnumerable<string> linesEnumerable = lines;
		if (lines[^1].Length != 0)
		{
			linesEnumerable = linesEnumerable.Append("");
		}
		ILookup<bool, int[]> keysAndLocks = linesEnumerable.
			Chunk(ySize + 1).
			Select(chunk => ParseKeyOrLock(chunk, xSize)).
			ToLookup(kl => kl.isKey, kl => kl.pattern);
		return (keysAndLocks[true].ToList(), keysAndLocks[false].ToList(), xSize, ySize - 2);
	}

	private static (int[] pattern, bool isKey) ParseKeyOrLock(string[] lines, int size)
	{
		const char fill = '#';
		const char empty = '.';

		bool isKey = lines[0][0] == empty;
		int[] pattern = new int[size];
		for (int i = 0; i < size; ++i)
		{
			pattern[i] = -1;
		}

		foreach (var line in lines)
		{
			if (line.Length == 0) break;
			for (int i = 0; i < size; ++i)
			{
				if (line[i] == fill) ++pattern[i];
			}
		}

		return (pattern, isKey);
	}
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(IReadOnlyCollection<int[]> keys, IReadOnlyCollection<int[]> locks, int size, int height)
	{
		long result = 0;
		foreach (var key in keys)
		foreach (var @lock in locks)
		{
			if (!Fit(key, @lock, size, height)) continue;
			++result;
		}
		return result;
	}

	private static bool Fit(int[] key, int[] @lock, int size, int height)
	{
		for (int i = 0; i < size; ++i)
		{
			if (key[i] + @lock[i] > height) return false;
		}
		return true;
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(IReadOnlyCollection<int[]> keys, IReadOnlyCollection<int[]> locks, int size, int height)
	{
		throw new NotImplementedException();
	}
}
