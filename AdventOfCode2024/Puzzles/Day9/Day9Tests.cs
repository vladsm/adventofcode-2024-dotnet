using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day9;

public sealed class Day9Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input = "2333133121414131402";
		string[] lines = input.Split("\r\n");
		long result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 9, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 9, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected override long Solve(string[] lines)
	{
		var idsMap = ToIdsMap(lines[0]);
		Compact(idsMap);
		return idsMap.Select((id, index) => id == -1 ? 0 : id * index).Sum();
	}

	protected abstract void Compact(long[] idsMap);

	private static long[] ToIdsMap(string line)
	{
		return line.SelectMany(toIdsBlock).ToArray();

		static IEnumerable<long> toIdsBlock(char ch, int index)
		{
			int size = toInt(ch);
			long id = index % 2 == 0 ? index / 2 : -1;
			for (int i = 0; i < size; i++)
			{
				yield return id;
			}
		}

		static int toInt(char ch) =>
			ch switch
			{
				'0' => 0,
				'1' => 1,
				'2' => 2,
				'3' => 3,
				'4' => 4,
				'5' => 5,
				'6' => 6,
				'7' => 7,
				'8' => 8,
				'9' => 9,
				_ => throw new ArgumentException($"Invalid character '{ch}'")
			};
	}
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override void Compact(long[] idsMap)
	{
		int i = 0;
		int j = idsMap.Length - 1;
		long id = idsMap[j];
		while (i < j)
		{
			if (idsMap[i] > -1)
			{
				++i;
				continue;
			}
			if (id < 0)
			{
				id = idsMap[--j];
				continue;
			}

			idsMap[i++] = id;
			idsMap[j] = -1;
			id = idsMap[--j];
		}
	}
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override void Compact(long[] idsMap)
	{
		int endPos = idsMap.Length - 1;
		while (idsMap.ReadFile(ref endPos, out var file))
		{
			int blockStart = idsMap.FindFreeBlock(file.size);
			if (blockStart < 0 || blockStart >= file.start) continue;

			for (int i = 0; i < file.size; ++i)
			{
				idsMap[blockStart + i] = file.id;
				idsMap[file.start + i] = -1;
			}
		}
	}
}


internal static class FilesExtensions
{
	public static bool ReadFile(this long[] idsMap, ref int endPos, out (long id, int start, int size) file)
	{
		file = default;
		if (endPos < 0) return false;
		long id = idsMap[endPos];
		while (id < 0)
		{
			if (endPos <= 0) return false;
			id = idsMap[--endPos];
		}

		int size = 1;
		int start = endPos;
		while (--endPos >= 0 && id == idsMap[endPos])
		{
			++size;
			start = endPos;
		}

		file = (id, start, size);
		return true;
	}

	public static int FindFreeBlock(this long[] idsMap, int size)
	{
		int start = -1;
		for (int i = 0; i < idsMap.Length; ++i)
		{
			if (idsMap[i] == -1)
			{
				if (start == -1)
				{
					start = i;
				}
				if (i - start + 1 >= size) return start;
			}
			else
			{
				start = -1;
			}
		}
		return -1;
	}
}
