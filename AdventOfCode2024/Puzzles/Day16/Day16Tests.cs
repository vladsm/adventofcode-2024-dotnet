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

		return graph;
	}

	private static bool IsGraphVertex(int y, int x, Map map)
	{
		bool notVertex =
			map[y - 1][x] == Empty && map[y + 1][x] == Empty && map[y][x - 1] != Empty && map[y][x + 1] != Empty ||
			map[y - 1][x] != Empty && map[y + 1][x] != Empty && map[y][x - 1] == Empty && map[y][x + 1] == Empty;
		return !notVertex;
	}

	private static void CollectAdjacentVertexes(Vertex vFrom, Graph graph, Map map)
	{
		(Position from, char fromDirection) = vFrom;
		var (cy, cx) = from;
		if (map[cy][cx] == Wall) return;
		if (!IsGraphVertex(cy, cx, map)) return;

		List<(Vertex, int)> toList = new();

		var rotations = _directions.
			Where(d => d != fromDirection).
			Select(d => (new Vertex(from, d), RotateCost(fromDirection, d))).
			ToList();
		toList.AddRange(rotations);

		(int vy, int vx) = DirectionToVector(fromDirection);

		int cost = 0;
		while (true)
		{
			(int ny, int nx) = (cy + vy, cx + vx);

			if (map[ny][nx] == Wall)
			{
				if (cost > 0)
				{
					toList.Add((new Vertex((cy, cx), fromDirection), cost));
				}
				break;
			}

			++cost;
			if (IsGraphVertex(ny, nx, map))
			{
				toList.Add((new Vertex((ny, nx), fromDirection), cost));
				break;
			}

			(cy, cx) = (ny, nx);
		}

		graph.Add(vFrom, toList);
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
			if (vCost == long.MaxValue) break;
			finished.Add(v);

			if (!graph.TryGetValue(v, out var nextVertexes)) continue;

			foreach ((Vertex next, int toNextCost) in nextVertexes.Where(nv => !finished.Contains(nv.to)))
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
		var graph = CreateGraph(map, start);
		Vertex vStart = new Vertex(start, Directions.Right);

		List<PathInfo> paths = [new([vStart], 0L)];
		List<PathInfo> finished = [];

		while (true)
		{
			List<PathInfo> newPaths = [];
			foreach (PathInfo path in paths)
			{
				Vertex last = path.Path.Last();
				List<PathInfo> nextPaths = [];
				foreach ((Vertex next, int nextCost) in graph.GetValueOrDefault(last, []))
				{
					if (path.Path.Contains(next) || nextCost + path.Cost > 101492)
					{
						continue;
					}
					nextPaths.Add(new PathInfo(path.Path.Append(next).ToList(), path.Cost + nextCost));
				}
				if (nextPaths.Count == 0)
				{
					if (last.Position == end)
					{
						finished.Add(path);
					}
				}
				else
				{
					newPaths.AddRange(nextPaths);
				}
			}

			if (newPaths.Count == 0)
			{
				break;
			}

			paths = newPaths;
		}

		throw new NotImplementedException();
	}


	private sealed record PathInfo(List<Vertex> Path, long Cost);
}


// internal sealed class SolverB : SolverBase
// {
// 	protected override long Solve(Map map, Position start, Position end)
// 	{
// 		Graph graph = CreateGraph(map, start);
// 		Vertex vStart = new Vertex(start, Directions.Right);
//
// 		HashSet<Vertex> visited = [];
// 		Dictionary<Vertex, List<PathInfo>> trackingPaths = new() { { vStart, [new([vStart], 0)] } };
// 		List<Vertex> newVertexes = [vStart];
//
// 		while (true)
// 		{
// 			Dictionary<Vertex, List<PathInfo>> newPaths = trackingPaths;
// 			bool hasChanges = true;
// 			List<Vertex> newVertexes1 = [];
// 			foreach (var currentKey in newVertexes)
// 			{
// 				if (!graph.TryGetValue(currentKey, out var nextVertexes)) continue;
// 				List<PathInfo> currentPaths = trackingPaths[currentKey];
// 				foreach (var next in nextVertexes)
// 				{
// 					if (!newPaths.TryGetValue(next.to, out var paths))
// 					{
// 						paths = new List<PathInfo>();
// 						newPaths[next.to] = paths;
// 					}
// 					List<PathInfo> nextPaths = currentPaths.
// 						Where(path => !path.Path.Contains(next.to)).
// 						Select(path => new PathInfo(path.Path.Append(next.to).ToList(), path.Cost + next.cost)).
// 						ToList();
// 					int before = paths.Count;
// 					paths.AddRange(nextPaths);
// 					if (before < paths.Count)
// 					{
// 						hasChanges = true;
// 					}
// 				}
// 			}
//
// 			if (!hasChanges)
// 			{
// 				break;
// 			}
//
// 			//trackingPaths = newPaths;
// 		}
//
//
// 		throw new NotImplementedException();
// 	}
//}
