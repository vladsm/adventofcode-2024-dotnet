using System.Text.RegularExpressions;

using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day14;

public sealed class Day14Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			p=0,4 v=3,-3
			p=6,3 v=-1,-3
			p=10,3 v=-1,2
			p=2,0 v=2,-1
			p=0,0 v=1,3
			p=3,0 v=-2,-2
			p=7,6 v=-1,-3
			p=3,0 v=-1,-2
			p=9,3 v=2,3
			p=7,3 v=-1,2
			p=2,4 v=2,-3
			p=9,5 v=-3,-3
			""";
		string[] lines = input.Split("\r\n");
		long result = await new SolverA(11, 7, _output).Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 14, level: 1).
			SolveUsing(new SolverA(101, 103, _output)).
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 14, level: 2).
			SolveUsing(new SolverB(101, 103, _output)).
			AssertingResult(line => line).
			Run(_output);
	}
}


internal sealed record Position(int X, int Y);
internal sealed record Vector(int X, int Y);

internal sealed record Robot(Position Initial, Vector Velocity)
{
	public Position Current { get; set; } = Initial;
}


internal abstract partial class SolverBase : SolverWithArrayInput<string, long>
{
	protected int SizeX { get; }
	protected int SizeY { get; }
	protected ITestOutputHelper Output { get; }

	[GeneratedRegex("p=(.+),(.+) v=(.+),(.+)")]
	private static partial Regex RobotRegex { get; }

	protected SolverBase(int sizeX, int sizeY, ITestOutputHelper output)
	{
		SizeX = sizeX;
		SizeY = sizeY;
		Output = output;
	}

	protected abstract long Solve(IReadOnlyCollection<Robot> entries);

	protected override long Solve(string[] lines)
	{
		var robots = lines.Select(ParseRobot).ToList();
		return Solve(robots);
	}

	private static Robot ParseRobot(string line)
	{
		var match = RobotRegex.Match(line);
		return new Robot(
			new Position(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)),
			new Vector(int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value))
			);
	}

	protected void Move(IEnumerable<Robot> robots)
	{
		foreach (var robot in robots)
		{
			Move(robot);
		}
	}

	private void Move(Robot robot)
	{
		var (x, y) = robot.Current;
		var (vx, vy) = robot.Velocity;
		int newX = (x + vx + SizeX) % SizeX;
		int newY = (y + vy + SizeY) % SizeY;
		robot.Current = new Position(newX, newY);
	}
}


internal sealed class SolverA : SolverBase
{
	private const int IterationsCount = 100;

	public SolverA(int sizeX, int sizeY, ITestOutputHelper output) : base(sizeX, sizeY, output)
	{
	}

	protected override long Solve(IReadOnlyCollection<Robot> robots)
	{
		for (int i = 0; i < IterationsCount; ++i)
		{
			Move(robots);
		}

		var quadrants = robots.
			GroupBy(r => GetQuadrant(r.Current)).
			Where(g => g.Key > 0).
			ToDictionary(g => g.Key, g => g.Count());

		int result =
			quadrants.GetValueOrDefault(1, 0) *
			quadrants.GetValueOrDefault(2, 0) *
			quadrants.GetValueOrDefault(3, 0) *
			quadrants.GetValueOrDefault(4, 0);
		return result;
	}

	private int GetQuadrant(Position position)
	{
		int halfSizeX = SizeX / 2;
		int halfSizeY = SizeY / 2;

		(int x, int y) = position;
		if (x == halfSizeX || y == halfSizeY) return 0;

		if (x < halfSizeX)
		{
			return y < halfSizeY ? 1 : 3;
		}
		return y < halfSizeY ? 2 : 4;
	}
}


internal sealed class SolverB : SolverBase
{
	public SolverB(int sizeX, int sizeY, ITestOutputHelper output) : base(sizeX, sizeY, output)
	{
	}

	protected override long Solve(IReadOnlyCollection<Robot> robots)
	{
		for (int second = 1; second <= 100000; ++second)
		{
			Move(robots);
			if (CheckChristmasTreePattern(robots))
			{
				DrawRobots(robots);
				return second;
			}
		}

		throw new InvalidOperationException("No result");
	}

	private void DrawRobots(IEnumerable<Robot> robots)
	{
		char[][] canvas = new char[SizeY][];
		for (int y = 0; y < SizeY; ++y)
		{
			canvas[y] = new char[SizeX];
			for (int x = 0; x < SizeX; ++x)
			{
				canvas[y][x] = '.';
			}
		}

		foreach (Robot robot in robots)
		{
			var (x, y) = robot.Current;
			canvas[y][x] = 'x';
		}

		foreach (char[] line in canvas)
		{
			Output.WriteLine(new string(line));
		}
	}

	private bool CheckChristmasTreePattern(IReadOnlyCollection<Robot> robots)
	{
		var robotsPositions = new RobotsPositions(robots);
		for (int x = 3; x < SizeX - 3; ++x)
		for (int y = 0; y < SizeY - 3; ++y)
		{
			if (CheckChristmasTreePatternAt(x, y, robotsPositions)) return true;
		}
		return false;
	}

	private static bool CheckChristmasTreePatternAt(int x, int y, RobotsPositions robots)
	{
		if (!robots.HasAt(x, y)) return false;
		if (!robots.HasAt(x - 1, y + 1) || !robots.HasAt(x + 1, y + 1)) return false;
		if (!robots.HasAt(x - 2, y + 2) || !robots.HasAt(x + 2, y + 2)) return false;
		if (!robots.HasAt(x - 3, y + 3) || !robots.HasAt(x + 3, y + 3)) return false;
		return true;
	}


	private sealed class RobotsPositions
	{
		private readonly HashSet<Position> _positions;

		public RobotsPositions(IReadOnlyCollection<Robot> robots)
		{
			_positions = robots.Select(r => r.Current).ToHashSet();
		}

		public bool HasAt(int x, int y) => _positions.Contains(new Position(x, y));
	}
}
