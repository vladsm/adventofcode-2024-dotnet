using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day15;

using Position = (int y, int x);
using Map = char[][];

public sealed class Day15Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			##########
			#..O..O.O#
			#......O.#
			#.OO..O.O#
			#..O@..O.#
			#O#..O...#
			#O..O..O.#
			#.OO.O.OO#
			#....O...#
			##########
		
			<vv>^<v^>v>^vv^v>v<>v^v<v<^vv<<<^><<><>>v<vvv<>^v^>^<<<><<v<<<v^vv^v>^
			vvv<<^>^v^^><<>>><>^<<><^vv^^<>vvv<>><^^v>^>vv<>v<<<<v<^v>^<^^>>>^<v<v
			><>vv>v^v^<>><>>>><^^>vv>v<^^^>>v^v^<^^>v^^>v^<^v>v<>>v^v^<v>v^^<^^vv<
			<<v<^>>^^^^>>>v^<>vvv^><v<<<>^^^vv^<vvv>^>v<^^^^v<>^>vvvv><>>v^<<^^^^^
			^><^><>>><>^^<<^^v>>><^<v>^<vv>>v>>>^v><>^v><<<<v>>v<v<v>vvv>^<><<>^><
			^>><>^v<><^vvv<^^<><v<<<<<><^v<<<><<<^^<v<^^^><^>>^<v^><<<^>>^v<v^v<v^
			>^>>^v>vv>^<<^v<>><<><<v<<v><>v<^vv<<<>^^v^>^^>>><<^v>>v^v><^^>>^<>vv^
			<><^^>^^^<><vvvvv^v<v<<>^v<v>v<<^><<><<><<<^^<<<^<<>><<><^^^>^^<>^>v<>
			^^>vv<^v^v<vv>^<><v<^v>^^^>>>^^vvv^>vvv<>>>^<^>>>>>^<<^v>^vvv<>^<><<v>
			v^^>>><<^^<>>^v^<v^vv<>v^<<>^<^v^v><^<<<><<^<v><v<>vv>>v><v^<vv<>v^<<^
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 15, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 15, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal static class Directions
{
	public const char Up = '^';
	public const char Down = 'v';
	public const char Left = '<';
	public const char Right = '>';
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected const char Robot = '@';
	protected const char Empty = '.';
	protected const char Wall = '#';
	protected const char Box = 'O';

	protected override long Solve(string[] lines)
	{
		var (map, moves) = Parse(lines);
		return Solve(map, moves);
	}

	protected abstract long Solve(Map map, string moves);

	private static (Map map, string moves) Parse(string[] lines)
	{
		List<char[]> map = new();
		int i = 0;
		string line = lines[i];
		while (line.Length > 0)
		{
			map.Add(line.ToArray());
			line = lines[++i];
		}

		string moves = string.Concat(lines.Skip(i));

		return (map.ToArray(), moves);
	}

	protected static Position FindRobot(Map map)
	{
		for (int y = 0; y < map.Length; ++y)
		for (int x = 0; x < map[y].Length; ++x)
		{
			if (map[y][x] == Robot) return (y, x);
		}
		throw new InvalidOperationException("No robot found");
	}

	protected static (int dy, int dx) DirectionToVector(char direction)
	{
		return direction switch
		{
			Directions.Up => (-1, 0),
			Directions.Down => (1, 0),
			Directions.Left => (0, -1),
			Directions.Right => (0, 1),
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction")
		};
	}
}

internal static class MapExtensions
{
	public static string[] Show(this Map map) =>
		map.Select(line => new string(line)).ToArray();
}
