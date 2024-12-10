using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day10;

using Position = (int y, int x);

public sealed class Day10Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			89010123
			78121874
			87430965
			96549874
			45678903
			32019012
			01329801
			10456732
			""";
		string[] lines = input.Split("\r\n");
		int result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 10, level: 1).
			SolveUsing<string, int, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 10, level: 2).
			SolveUsing<string, int, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, int>
{
	protected override int Solve(string[] lines)
	{
		var map = Parse(lines);
		return Solve(map);
	}

	protected abstract int Solve(sbyte[][] map);


	private static sbyte[][] Parse(string[] lines)
	{
		return lines.Select(parseLine).ToArray();

		static sbyte[] parseLine(string line) => line.Select(toInt).ToArray();

		static sbyte toInt(char ch) =>
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
				_ => -1
			};
	}
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override int Solve(sbyte[][] map)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		sbyte height = 0;
		var visited = new Dictionary<Position, HashSet<Position>>();
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			if (map[y][x] != 0) continue;
			visited[(y, x)] = [(y, x)];
		}

		while (++height <= 9)
		{
			var newVisited = new Dictionary<Position, HashSet<Position>>();
			foreach ((Position pos, HashSet<Position> trailheads) in visited)
			{
				foreach (Position next in NextPositions(pos.y, pos.x, map))
				{
					if (map[next.y][next.x] != height) continue;
					if (!newVisited.TryGetValue(next, out HashSet<Position>? nextTrailheads))
					{
						nextTrailheads = [..trailheads];
						newVisited.Add(next, nextTrailheads);
					}
					else
					{
						foreach (Position trailhead in trailheads)
						{
							nextTrailheads.Add(trailhead);
						}
					}
				}
			}
			visited = newVisited;
		}

		return visited.Values.SelectMany(trailheads => trailheads).CountBy(trailhead => trailhead).Sum(g => g.Value);
	}


	private static IEnumerable<Position> NextPositions(int y, int x, sbyte[][] map)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		if (isValid(y - 1, x)) yield return (y - 1, x);
		if (isValid(y + 1, x)) yield return (y + 1, x);
		if (isValid(y, x - 1)) yield return (y, x - 1);
		if (isValid(y, x + 1)) yield return (y, x + 1);

		bool isValid(int yy, int xx) => yy >= 0 && yy < sizeY && xx >= 0 && xx < sizeX;
	}
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override int Solve(sbyte[][] map)
	{
		throw new NotImplementedException();
	}
}
