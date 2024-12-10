using System.Collections;

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
		int result = await new SolverB().Solve(lines.ToAsyncEnumerable());
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

internal interface IPathsTracker : IEnumerable<(Position pos, IReadOnlyCollection<Position> trailheads)>
{
	void AddStep(Position initial);
	void AddStep(Position next, IReadOnlyCollection<Position> trailheads);
	int CalculateTotalScore();
}


internal abstract class SolverBase<TPathsTracker> : SolverWithArrayInput<string, int>
	where TPathsTracker : IPathsTracker, new()
{
	protected override int Solve(string[] lines)
	{
		var map = Parse(lines);
		return Solve(map);
	}


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

	private int Solve(sbyte[][] map)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		sbyte height = 0;
		var visited = new TPathsTracker();
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			if (map[y][x] != 0) continue;
			visited.AddStep((y, x));
		}

		while (++height <= 9)
		{
			var newVisited = new TPathsTracker();
			foreach ((Position pos, IReadOnlyCollection<Position> trailheads) in visited)
			{
				foreach (Position next in NextPositions(pos.y, pos.x, map))
				{
					if (map[next.y][next.x] != height) continue;
					newVisited.AddStep(next, trailheads);
				}
			}
			visited = newVisited;
		}

		return visited.CalculateTotalScore();
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
internal sealed class SolverA : SolverBase<TrackerA>;

[UsedImplicitly]
internal sealed class SolverB : SolverBase<TrackerB>;


internal sealed class TrackerA : IPathsTracker
{
	private readonly Dictionary<Position, HashSet<Position>> _visited = new();

	public void AddStep(Position initial)
	{
		_visited[(initial.y, initial.x)] = [(initial.y, initial.x)];
	}

	public void AddStep(Position next, IReadOnlyCollection<Position> trailheads)
	{
		if (!_visited.TryGetValue(next, out HashSet<Position>? nextTrailheads))
		{
			nextTrailheads = [..trailheads];
			_visited.Add(next, nextTrailheads);
		}
		else
		{
			foreach (Position trailhead in trailheads)
			{
				nextTrailheads.Add(trailhead);
			}
		}
	}

	public int CalculateTotalScore() =>
		_visited.Values.
			SelectMany(trailheads => trailheads).
			CountBy(trailhead => trailhead).
			Sum(g => g.Value);

	public IEnumerator<(Position pos, IReadOnlyCollection<Position> trailheads)> GetEnumerator()
	{
		foreach (var (pos, trailheads) in _visited)
		{
			yield return (pos, trailheads);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


internal sealed class TrackerB : IPathsTracker
{
	private readonly Dictionary<Position, List<Position>> _visited = new();

	public void AddStep(Position initial)
	{
		_visited[(initial.y, initial.x)] = [(initial.y, initial.x)];
	}

	public void AddStep(Position next, IReadOnlyCollection<Position> trailheads)
	{
		if (!_visited.TryGetValue(next, out List<Position>? nextTrailheads))
		{
			nextTrailheads = [..trailheads];
			_visited.Add(next, nextTrailheads);
		}
		else
		{
			nextTrailheads.AddRange(trailheads);
		}
	}

	public int CalculateTotalScore() =>
		_visited.Values.
			SelectMany(trailheads => trailheads).
			Count();

	public IEnumerator<(Position pos, IReadOnlyCollection<Position> trailheads)> GetEnumerator()
	{
		foreach (var (pos, trailheads) in _visited)
		{
			yield return (pos, trailheads);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
