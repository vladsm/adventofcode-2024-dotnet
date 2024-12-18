using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day18;

using Position = (int y, int x);
using Vector = (int dy, int dx);

public sealed class Day18Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			5,4
			4,2
			4,5
			3,0
			2,1
			6,3
			2,4
			1,5
			0,6
			3,3
			2,6
			5,1
			1,2
			5,5
			2,5
			6,5
			1,4
			0,4
			6,4
			1,1
			6,1
			1,0
			0,5
			1,6
			2,0
			""";

		string[] lines = input.Split("\r\n");
		string result = await new SolverB(7, 12).Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 18, level: 1).
			SolveUsing(new SolverA(71, 1024)).
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 18, level: 2).
			SolveUsing(new SolverB(71, 1024)).
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, string>
{
	private const int Init = -1;
	private const int Wall = -2;

	protected int Size { get; }

	protected readonly Vector[] Directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

	protected SolverBase(int size)
	{
		Size = size;
	}

	protected override string Solve(string[] lines)
	{
		Position[] walls = Parse(lines);
		return Solve(walls);
	}

	protected abstract string Solve(Position[] walls);

	private static Position[] Parse(string[] lines)
	{
		return lines.Select(parse).ToArray();

		Position parse(string line)
		{
			string[] parts = line.Split(',');
			return (int.Parse(parts[1]), int.Parse(parts[0]));
		}
	}

	protected int[][] CreateMap(IEnumerable<Position> walls)
	{
		int[][] map = new int[Size][];
		for (int y = 0; y < Size; ++y)
		{
			map[y] = new int[Size];
			for (int x = 0; x < Size; ++x)
			{
				map[y][x] = Init;
			}
		}
		foreach ((int y, int x) in walls)
		{
			map[y][x] = Wall;
		}
		return map;
	}

	protected void ClearMap(int[][] map, Position additionalWall)
	{
		for (int y = 0; y < Size; ++y)
		for (int x = 0; x < Size; ++x)
		{
			if (map[y][x] > 0) map[y][x] = Init;
		}
		map[additionalWall.y][additionalWall.x] = Wall;
	}

	protected int FindPathLength(int[][] map)
	{
		IReadOnlyCollection<Position> from = [(0, 0)];
		for (int step = 0; from.Count > 0; step++)
		{
			HashSet<Position> next = [];
			foreach ((int cy, int cx) in from)
			{
				map[cy][cx] = step;

				if ((cy, cx) == (Size - 1, Size - 1)) return step;

				foreach ((int dy, int dx) in Directions)
				{
					(int ny, int nx) = (cy + dy, cx + dx);
					if (ny < 0 || ny >= Size || nx < 0 || nx >= Size) continue;
					int tile = map[ny][nx];
					if (tile == Wall || tile >= 0) continue;
					next.Add((ny, nx));
				}
			}
			from = next;
		}
		return -1;
	}
}


internal sealed class SolverA : SolverBase
{
	private readonly int _wallsCount;

	public SolverA(int size, int wallsCount) : base(size)
	{
		_wallsCount = wallsCount;
	}

	protected override string Solve(Position[] walls)
	{
		var map = CreateMap(walls.Take(_wallsCount));
		return FindPathLength(map).ToString();
	}
}


internal sealed class SolverB : SolverBase
{
	private readonly int _wallsCount;

	public SolverB(int size, int wallsCount) : base(size)
	{
		_wallsCount = wallsCount;
	}

	protected override string Solve(Position[] walls)
	{
		var map = CreateMap(walls.Take(_wallsCount));
		foreach (Position newWall in walls.Skip(_wallsCount))
		{
			ClearMap(map, newWall);
			int pathLength = FindPathLength(map);
			if (pathLength >= 0) continue;
			return $"{newWall.x},{newWall.y}";
		}
		throw new InvalidOperationException("None byte blocks the path");
	}
}
