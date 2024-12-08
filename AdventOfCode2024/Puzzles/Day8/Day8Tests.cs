using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day8;

using Position = (int y, int x);

public sealed class Day8Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			............
			........0...
			.....0......
			.......0....
			....0.......
			......A.....
			............
			............
			........A...
			.........A..
			............
			............
			""";
		string[] lines = input.Split("\r\n");
		int result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 8, level: 1).
			SolveUsing<string, int, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 8, level: 2).
			SolveUsing<string, int, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal static class MapExtensions
{
	public static IEnumerable<Position> NextSimilarAntennas(
		this string[] map,
		Position antennaPosition,
		char antenna
		)
	{
		(int fromY, int fromX) = antennaPosition;
		for (int y = fromY; y < map.Length; ++y)
		for (int x = 0; x < map[y].Length; ++x)
		{
			if (y == fromY && x <= fromX) continue;
			char newAntenna = map[y][x];
			if (newAntenna == antenna) yield return (y, x);
		}
	}

	public static bool InMap(this string[] map, Position pos)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;
		return pos.y >= 0 && pos.y < sizeY && pos.x >= 0 && pos.x < sizeX;
	}


	public static bool IsAntenna(this char ch) =>
		char.IsDigit(ch) || char.IsAsciiLetter(ch);
}


internal abstract class SolverBase : SolverWithArrayInput<string, int>
{
	protected override int Solve(string[] map) =>
		map.
			SelectMany((line, y) => line.Select((type, x) => (pos: (y, x), type))).
			Where(a => a.type.IsAntenna()).
			SelectMany(a => map.NextSimilarAntennas(a.pos, a.type).Select(next => (a.pos, next))).
			SelectMany(a => GetAntinodesFor(a.pos, a.next, map)).
			Distinct().
			Count();

	protected abstract IEnumerable<Position> GetAntinodesFor(Position a1, Position a2, string[] map);
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override IEnumerable<Position> GetAntinodesFor(Position a1, Position a2, string[] map)
	{
		(int dy, int dx) = (a2.y - a1.y, a2.x - a1.x);

		Position antinode = (a1.y - dy, a1.x - dx);
		if (map.InMap(antinode)) yield return antinode;

		antinode = (a2.y + dy, a2.x + dx);
		if (map.InMap(antinode)) yield return antinode;
	}
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override IEnumerable<Position> GetAntinodesFor(Position a1, Position a2, string[] map)
	{
		(int dy, int dx) = (a2.y - a1.y, a2.x - a1.x);

		Position antinode = a1;
		while (map.InMap(antinode))
		{
			yield return antinode;
			antinode = (antinode.y - dy, antinode.x - dx);
		}

		antinode = a2;
		while (map.InMap(antinode))
		{
			yield return antinode;
			antinode = (antinode.y + dy, antinode.x + dx);
		}
	}
}
