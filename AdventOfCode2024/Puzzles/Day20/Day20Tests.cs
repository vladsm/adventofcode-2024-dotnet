using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day20;

using Position = (int y, int x);
using Vector = (int dy, int dx);
using Map = int[][];

public sealed class Day20Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			###############
			#...#...#.....#
			#.#.#.#.#.###.#
			#S#...#.#.#...#
			#######.#.#.###
			#######.#.#...#
			#######.#.###.#
			###..E#...#...#
			###.#######.###
			#...###...#...#
			#.#####.#.###.#
			#.#...#.#.#...#
			#.#.#.#.#.#.###
			#...#...#...###
			###############
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 20, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 20, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	private const int Init = -1;
	private const int Wall = -2;

	protected readonly Vector[] Directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

	protected override long Solve(string[] lines)
	{
		(Map map, Position start, Position end) = Parse(lines);
		return Solve(map, start, end);
	}

	protected abstract long Solve(Map map, Position start, Position end);

	protected static bool OutOfInnerMap(Position position, int sizeY, int sizeX)
	{
		(int y, int x) = position;
		return x <= 0 || y <= 0 || x >= sizeX - 1 || y >= sizeY - 1;
	}

	protected int MarkMapWithDistancesFromStartToEnd(int[][] map, Position start, Position end)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		IReadOnlyCollection<Position> from = [start];
		for (int step = 0; from.Count > 0; step++)
		{
			HashSet<Position> next = [];
			foreach ((int cy, int cx) in from)
			{
				map[cy][cx] = step;

				if ((cy, cx) == end) return step;

				foreach ((int dy, int dx) in Directions)
				{
					(int ny, int nx) = (cy + dy, cx + dx);
					if (ny < 0 || ny >= sizeY || nx < 0 || nx >= sizeX) continue;
					int tile = map[ny][nx];
					if (tile == Wall || tile >= 0) continue;
					next.Add((ny, nx));
				}
			}
			from = next;
		}
		return -1;
	}

	protected long CalculateCheatsToSaveAtLeast(Map map, int maxCheatDistance, int targetToSave)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		long result = 0L;
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			if (OutOfInnerMap((y, x), sizeY, sizeX)) continue;

			int distance = map[y][x];
			if (distance < 0) continue;

			foreach ((_, int cheatLength, int fromCheatExitDistance) in CheatExits(y, x, maxCheatDistance, map))
			{
				int saved = distance - fromCheatExitDistance - cheatLength;
				if (saved >= targetToSave) ++result;
			}
		}
		return result;
	}

	private IEnumerable<(Position exit, int length, int fromExitDistance)> CheatExits(int fromY, int fromX, int maxDistance, Map map)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		for (int dy = -maxDistance; dy <= maxDistance; ++dy)
		for (int dx = -maxDistance; dx <= maxDistance; ++dx)
		{
			if (dy == 0 && dx == 0) continue;

			int length = Math.Abs(dy) + Math.Abs(dx);
			if (length > maxDistance) continue;

			(int ey, int ex) = (fromY + dy, fromX + dx);
			if (OutOfInnerMap((ey, ex), sizeY, sizeX)) continue;

			int fromExitDistance = map[ey][ex];
			if (fromExitDistance < 0) continue;

			yield return ((ey, ex), length, fromExitDistance);
		}
	}

	private static (Map map, Position start, Position end) Parse(string[] lines)
	{
		const char startCh = 'S';
		const char endCh = 'E';
		const char emptyCh = '.';
		const char wallCh = '#';

		const int startInt = 1;
		const int endInt = 2;

		List<int[]> mapList = new();
		foreach (string line in lines)
		{
			mapList.Add(line.Select(parseTile).ToArray());
		}
		Map map = mapList.ToArray();

		static int parseTile(char ch) =>
			ch switch
			{
				emptyCh => Init,
				wallCh => Wall,
				startCh => startInt,
				endCh => endInt,
				_ => throw new ArgumentException($"Invalid character '{ch}'")
			};

		Position start = (-1, -1), end = (-1, -1);
		bool hasStart = false, hasEnd = false;
		for (int y = 0; y < map.Length; ++y)
		for (int x = 0; x < map[y].Length; ++x)
		{
			int tile = map[y][x];
			if (tile == startInt)
			{
				start = new Position(y, x);
				hasStart = true;
				map[y][x] = Init;
			}
			if (tile == endInt)
			{
				end = new Position(y, x);
				hasEnd = true;
				map[y][x] = Init;
			}
			if (hasStart && hasEnd) break;
		}

		return (map, start, end);
	}
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(Map map, Position start, Position end)
	{
		MarkMapWithDistancesFromStartToEnd(map, start, end);
		return CalculateCheatsToSaveAtLeast(map, 2, 100);
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(Map map, Position start, Position end)
	{
		MarkMapWithDistancesFromStartToEnd(map, start, end);
		return CalculateCheatsToSaveAtLeast(map, 20, 100);
	}
}
