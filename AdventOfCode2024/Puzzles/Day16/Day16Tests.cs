using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day16;

using Position = (int y, int x);
using Map = char[][];
using Costs = Dictionary<(int y, int x), Dictionary<char, long>>;

public sealed class Day16Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		// string input =
		// 	"""
		// 	###############
		// 	#.......#....E#
		// 	#.#.###.#.###.#
		// 	#.....#.#...#.#
		// 	#.###.#####.#.#
		// 	#.#.#.......#.#
		// 	#.#.#####.###.#
		// 	#...........#.#
		// 	###.#.#####.#.#
		// 	#...#.....#.#.#
		// 	#.#.#.###.#.#.#
		// 	#.....#...#.#.#
		// 	#.###.#.#.#.#.#
		// 	#S..#.....#...#
		// 	###############
		// 	""";

		string input =
			"""
			#################
			#...#...#...#..E#
			#.#.#.#.#.#.#.#.#
			#.#.#.#...#...#.#
			#.#.#.#.###.#.#.#
			#...#.#.#.....#.#
			#.#.#.#.#.#####.#
			#.#...#.#.#.....#
			#.#.#####.#.###.#
			#.#.#.......#...#
			#.#.###.#####.###
			#.#.#...#.....#.#
			#.#.#.#####.###.#
			#.#.#.........#.#
			#.#.#.#########.#
			#S#.............#
			#################
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 16, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 16, level: 2).
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


internal record struct Vertex(Position Position, char Direction);


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	private const char Start = 'S';
	private const char End = 'E';
	protected const char Empty = '.';
	private const char Wall = '#';

	private static readonly char[] _directions = [Directions.Up, Directions.Down, Directions.Right, Directions.Left];

	protected override long Solve(string[] lines)
	{
		(Map map, Position start, Position end) = Parse(lines);
		return Solve(map, start, end);
	}

	protected abstract long Solve(Map map, Position start, Position end);

	private static (Map map, Position start, Position end) Parse(string[] lines)
	{
		List<char[]> mapList = new();
		foreach (string line in lines)
		{
			mapList.Add(line.ToArray());
		}
		Map map = mapList.ToArray();

		Position start = (-1, -1), end = (-1, -1);
		bool hasStart = false, hasEnd = false;
		for (int y = 0; y < map.Length; ++y)
		for (int x = 0; x < map[y].Length; ++x)
		{
			char ch = map[y][x];
			if (ch == Start)
			{
				start = new Position(y, x);
				hasStart = true;
				map[y][x] = Empty;
			}
			if (ch == End)
			{
				end = new Position(y, x);
				hasEnd = true;
				map[y][x] = Empty;
			}
			if (hasStart && hasEnd) break;
		}

		return (map, start, end);
	}

	private static (int dy, int dx) DirectionToVector(char direction)
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

	private static int RotateCost(char from, char to) =>
		(from, to) switch
		{
			(Directions.Up, Directions.Up) => 0,
			(Directions.Up, Directions.Right) => 1000,
			(Directions.Up, Directions.Left) => 1000,
			(Directions.Down, Directions.Down) => 0,
			(Directions.Down, Directions.Right) => 1000,
			(Directions.Down, Directions.Left) => 1000,
			(Directions.Left, Directions.Up) => 1000,
			(Directions.Left, Directions.Down) => 1000,
			(Directions.Left, Directions.Left) => 0,
			(Directions.Right, Directions.Up) => 1000,
			(Directions.Right, Directions.Down) => 1000,
			(Directions.Right, Directions.Right) => 0,
			_ => -1
		};

	protected sealed record PosDir(Position Position, char Direction);

	protected static long GetCostTo(Position to, Costs costs) =>
		costs[to].Values.Min();

	protected static Costs CalculateCosts(Position from, char fromDirection, Map map)
	{
		Costs result = [];
		foreach (char direction in _directions)
		{
			for (int y = 0; y < map.Length; ++y)
			for (int x = 0; x < map[y].Length; ++x)
			{
				if (map[y][x] != Empty) continue;
				if (result.TryGetValue((y, x), out Dictionary<char, long>? directionsResult))
				{
					directionsResult[direction] = long.MaxValue;
				}
				else
				{
					result.Add((y, x), new Dictionary<char, long>{{direction, long.MaxValue}});
				}
			}
		}
		result[from][fromDirection] = 0;

		PriorityQueue<PosDir, long> states = new();
		HashSet<PosDir> finished = [];
		states.Enqueue(new(from, fromDirection), 0);

		while (states.Count > 0)
		{
			var vState = states.Dequeue();
			if (!finished.Add(vState)) continue;

			(Position v, char vDirection) = vState;
			(int vy, int vx) = v;
			long vCost = result[v][vDirection];

			foreach (char direction in _directions)
			{
				int rotateCost = RotateCost(vDirection, direction);
				if (rotateCost < 0) continue;
				(int dy, int dx) = DirectionToVector(direction);
				(int ny, int nx) = (vy + dy, vx + dx);
				char ch = map[ny][nx];
				if (ch == Wall) continue;
				long nCost = vCost + rotateCost + 1;
				long nPrevCost = result[(ny, nx)][direction];
				if (nPrevCost > nCost)
				{
					result[(ny, nx)][direction] = nCost;
				}
				else
				{
					nCost = nPrevCost;
				}
				var next = new PosDir((ny, nx), direction);
				states.Enqueue(next, nCost);
			}
		}

		return result;
	}
}


internal static class MapExtensions
{
	public static string[] Show(this Map map) =>
		map.Select(line => new string(line)).ToArray();
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(Map map, Position start, Position end)
	{
		Costs costs = CalculateCosts(start, Directions.Right, map);
		return GetCostTo(end, costs);
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(Map map, Position start, Position end)
	{
		Costs costsFromStart = CalculateCosts(start, Directions.Right, map);
		long startToEndCost = GetCostTo(end, costsFromStart);

		int result = 0;
		for (int y = 0; y < map.Length; ++y)
		for (int x = 0; x < map[y].Length; ++x)
		{
			if (map[y][x] != Empty) continue;

			foreach ((char fromDir, long fromStart) in costsFromStart[(y, x)])
			{
				if (fromStart > startToEndCost) continue;

				Costs costsToEnd = CalculateCosts((y, x), fromDir, map);
				long toEnd = GetCostTo(end, costsToEnd);
				if (startToEndCost == fromStart + toEnd)
				{
					++result;
					break;
				}
			}
		}
		return result;
	}
}
