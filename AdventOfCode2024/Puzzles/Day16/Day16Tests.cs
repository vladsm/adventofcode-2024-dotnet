using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day16;

using Position = (int y, int x);
using Map = char[][];
using Graph = Dictionary<Vertex, IReadOnlyCollection<(Vertex to, int cost)>>;

public sealed class Day16Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			###############
			#.......#....E#
			#.#.###.#.###.#
			#.....#.#...#.#
			#.###.#####.#.#
			#.#.#.......#.#
			#.#.#####.###.#
			#...........#.#
			###.#.#####.#.#
			#...#.....#.#.#
			#.#.#.###.#.#.#
			#.....#...#.#.#
			#.###.#.#.#.#.#
			#S..#.....#...#
			###############
			""";

		// string input =
		// 	"""
		// 	#################
		// 	#...#...#...#..E#
		// 	#.#.#.#.#.#.#.#.#
		// 	#.#.#.#...#...#.#
		// 	#.#.#.#.###.#.#.#
		// 	#...#.#.#.....#.#
		// 	#.#.#.#.#.#####.#
		// 	#.#...#.#.#.....#
		// 	#.#.#####.#.###.#
		// 	#.#.#.......#...#
		// 	#.#.###.#####.###
		// 	#.#.#...#.....#.#
		// 	#.#.#.#####.###.#
		// 	#.#.#.........#.#
		// 	#.#.#.#########.#
		// 	#S#.............#
		// 	#################
		// 	""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
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
	private const char Empty = '.';
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
		(Directions.Up, Directions.Down) => 2000,
		(Directions.Up, Directions.Right) => 1000,
		(Directions.Up, Directions.Left) => 1000,
		(Directions.Down, Directions.Up) => 2000,
		(Directions.Down, Directions.Down) => 0,
		(Directions.Down, Directions.Right) => 1000,
		(Directions.Down, Directions.Left) => 1000,
		(Directions.Left, Directions.Up) => 1000,
		(Directions.Left, Directions.Down) => 1000,
		(Directions.Left, Directions.Right) => 2000,
		(Directions.Left, Directions.Left) => 0,
		(Directions.Right, Directions.Up) => 1000,
		(Directions.Right, Directions.Down) => 1000,
		(Directions.Right, Directions.Right) => 0,
		(Directions.Right, Directions.Left) => 2000,
		_ => throw new InvalidOperationException("Invalid directions pair")
	};

	protected static Graph CreateGraph(Map map, Position start)
	{
		var vStart = new Vertex(start, Directions.Right);
		var graph = new Graph();

		int sizeY = map.Length;
		int sizeX = map[0].Length;
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			foreach (char direction in _directions)
			{
				CollectAdjacentVertexes(new Vertex((y, x), direction), graph, map);
			}
		}

		//CollectAdjacentVertexes(vStart, graph, map);
		return graph;
	}

	private static void CollectAdjacentVertexes(Vertex vFrom, Graph graph, Map map)
	{
		(Position from, char fromDirection) = vFrom;
		var (cy, cx) = from;
		if (map[cy][cx] == Wall) return;

		// int cways = 0;
		// if (map[cy + 1][cx] == Empty) ++cways;
		// if (map[cy - 1][cx] == Empty) ++cways;
		// if (map[cy][cx + 1] == Empty) ++cways;
		// if (map[cy][cx - 1] == Empty) ++cways;
		// if (cways  2) return;

		List<(Vertex, int)> toList = new();

		var rotations = _directions.
			Where(d => d != fromDirection).
			Select(d => (new Vertex(from, d), RotateCost(fromDirection, d))).
			ToList();
		toList.AddRange(rotations);

		(int vy, int vx) = DirectionToVector(fromDirection);
		// (int ny, int nx) = (cy + vy, cx + vx);
		// if (map[ny][nx] != Wall)
		// {
		// 	toList.Add((new Vertex((ny, nx), fromDirection), 1));
		// }

		int cost = 0;
		while (true)
		{
			(int ny, int nx) = (cy + vy, cx + vx);

			bool finishEdge = map[ny][nx] == Wall;
			if (!finishEdge)
			{
				int ways = 0;
				if (map[ny + 1][nx] == Empty) ++ways;
				if (map[ny - 1][nx] == Empty) ++ways;
				if (map[ny][nx + 1] == Empty) ++ways;
				if (map[ny][nx - 1] == Empty) ++ways;
				if (ways > 2) finishEdge = true;
			}

			if (finishEdge)
			{
				if (cost > 0)
				{
					toList.Add((new Vertex((cy, cx), fromDirection), cost));
				}
				break;
			}

			++cost;
			(cy, cx) = (ny, nx);
		}

		graph.Add(vFrom, toList);
	}

	private static void CollectAdjacentVertexesOneThatWorks(Vertex vFrom, Graph graph, Map map)
	{
		(Position from, char fromDirection) = vFrom;
		var (cy, cx) = from;
		if (map[cy][cx] == Wall) return;

		List<(Vertex, int)> toList = new();

		var rotations = _directions.
			Where(d => d != fromDirection).
			Select(d => (new Vertex(from, d), RotateCost(fromDirection, d))).
			ToList();
		toList.AddRange(rotations);

		(int vy, int vx) = DirectionToVector(fromDirection);
		(int ny, int nx) = (cy + vy, cx + vx);
		try
		{
			if (map[ny][nx] != Wall)
			{
				toList.Add((new Vertex((ny, nx), fromDirection), 1));
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

		graph.Add(vFrom, toList);
	}

	private static void CollectAdjacentVertexesOld(Vertex vFrom, Graph graph, Map map)
	{
		if (graph.ContainsKey(vFrom)) return;

		List<(Vertex, int)> toList = new();

		(Position from, char fromDirection) = vFrom;

		var rotations = _directions.
			Where(d => d != fromDirection).
			Select(d => (new Vertex(from, d), RotateCost(fromDirection, d))).
			ToList();
		toList.AddRange(rotations);

		(int vy, int vx) = DirectionToVector(fromDirection);

		var (cy, cx) = from;
		(int ny, int nx) = (cy + vy, cx + vx);
		if (map[ny][nx] != Wall)
		{
			toList.Add((new Vertex((ny, nx), fromDirection), 1));
		}

		// int cost = 0;
		// var (cy, cx) = from;
		// while (true)
		// {
		// 	(int ny, int nx) = (cy + vy, cx + vx);
		//
		// 	if (map[ny][nx] == Wall)
		// 	{
		// 		if (cost > 0)
		// 		{
		// 			toList.Add((new Vertex((cy, cx), fromDirection), cost));
		// 		}
		// 		break;
		// 	}
		//
		// 	++cost;
		// 	(cy, cx) = (ny, nx);
		// }

		graph.Add(vFrom, toList);
		foreach ((Vertex vTo, _) in toList)
		{
			CollectAdjacentVertexes(vTo, graph, map);
		}
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
		var graph = CreateGraph(map, start);

		Dictionary<Vertex, long> costs = new();
		foreach (Vertex v in graph.Keys)
		{
			costs.Add(v, long.MaxValue - 1);
		}

		var vStart = new Vertex(start, Directions.Right);
		costs[vStart] = 0;

		HashSet<Vertex> finished = new();
		while (true)
		{
			long vCost = long.MaxValue;
			Vertex v = new();
			foreach (var kvp in costs.Where(kvp => !finished.Contains(kvp.Key)))
			{
				if (kvp.Value >= vCost) continue;
				vCost = kvp.Value;
				v = kvp.Key;
			}
			finished.Add(v);

			if (!graph.TryGetValue(v, out var nextVertexes)) break;

			foreach ((Vertex next, int toNextCost) in nextVertexes)
			{
				long nextCost = costs.GetValueOrDefault(next, long.MaxValue);
				costs[next] = Math.Min(nextCost, vCost + toNextCost);
			}
		}

		long result = costs.Where(v => v.Key.Position == end).Min(v => v.Value);
		return result;
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(Map map, Position start, Position end)
	{
		throw new NotImplementedException();
	}
}
