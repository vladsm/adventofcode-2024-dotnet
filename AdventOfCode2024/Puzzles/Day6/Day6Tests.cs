using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day6;

using Direction = (int y, int x);
using Board = byte[,];

public sealed class Day6Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			....#.....
			.........#
			..........
			..#.......
			.......#..
			..........
			.#..^.....
			........#.
			#.........
			......#...
			""";
		string[] lines = input.Split("\r\n");
		int result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}


	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 6, level: 1).
			SolveUsing<string, int, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 6, level: 2).
			SolveUsing<string, int, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}

internal static class Cell
{
	public const byte Empty = 0;
	public const byte Visited = 1;
	public const byte Obstacle = 2;
}


internal abstract class SolverBase : SolverWithArrayInput<string, int>
{
	protected override int Solve(string[] lines)
	{
		var (board, (startY, startX)) = ParseInput(lines);
		return Solve(board, startY, startX);
	}

	protected abstract int Solve(Board board, int startY, int startX);

	private static (Board board, (int y, int x) start) ParseInput(string[] lines)
	{
		int sizeX = lines[0].Length;
		int sizeY = lines.Length;
		var board = new byte[sizeX, sizeY];

		(int, int) start = (0, 0);
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			char ch = lines[y][x];
			board[y, x] = ch == '#' ? Cell.Obstacle : Cell.Empty;
			if (ch == '^')
			{
				start = (y, x);
			}
		}

		return (board, start);
	}


	private static readonly Direction _up = (-1, 0);
	private static readonly Direction _down = (1, 0);
	private static readonly Direction _left = (0, -1);
	private static readonly Direction _right = (0, 1);

	protected static Direction InitialDirection = _up;

	protected static Direction NextDirectionClockwise(Direction direction)
	{
		if (direction == _up) return _right;
		if (direction == _right) return _down;
		if (direction == _down) return _left;
		if (direction == _left) return _up;
		throw new ArgumentOutOfRangeException(nameof(direction), "Unexpected direction");
	}
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override int Solve(Board board, int startY, int startX)
	{
		(int fromY, int fromX) = (startY, startX);
		Direction direction = InitialDirection;
		while (Move(ref fromY, ref fromX, direction, board))
		{
			direction = NextDirectionClockwise(direction);
		}

		int sizeY = board.GetLength(0);
		int sizeX = board.GetLength(1);
		int result = 0;
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			if (board[y, x] == Cell.Visited)
			{
				++result;
			}
		}

		return result;
	}

	private static bool Move(ref int fromY, ref int fromX, Direction direction, Board board)
	{
		int sizeY = board.GetLength(0);
		int sizeX = board.GetLength(1);

		board[fromY, fromX] = Cell.Visited;
		while (true)
		{
			(int moveY, int moveX) = direction;
			int newY = fromY + moveY;
			int newX = fromX + moveX;

			if (newY < 0 || newY >= sizeY) return false;
			if (newX < 0 || newX >= sizeX) return false;
			if (board[newY, newX] == Cell.Obstacle) return true;

			board[newY, newX] = Cell.Visited;
			fromY = newY;
			fromX = newX;
		}
	}
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override int Solve(Board board, int startY, int startX)
	{
		var newObstacles = new List<(int, int)>();
		for (int y = 0; y < board.GetLength(0); ++y)
		for (int x = 0; x < board.GetLength(1); ++x)
		{
			bool canNotAddObstacle =
				(y, x) == (startY, startX) ||
				board[y, x] == Cell.Obstacle;
			if (canNotAddObstacle) continue;
			newObstacles.Add((y, x));
		}

		return newObstacles.Count(obstacle => IsCircularRoute(obstacle, board, startY, startX));
	}

	private static bool IsCircularRoute((int y, int x) newObstacle, Board board, int startY, int startX)
	{
		board[newObstacle.y, newObstacle.x] = Cell.Obstacle;

		var visitedPoints = new HashSet<(int, int)>();

		(int fromY, int fromX) = (startY, startX);
		Direction direction = InitialDirection;
		bool result = false;
		while (Move(ref fromY, ref fromX, direction, board))
		{
			if (visitedPoints.Contains((fromY, fromX)))
			{
				result = true;
				break;
			}

			direction = NextDirectionClockwise(direction);
			visitedPoints.Add((fromY, fromX));
		}

		board[newObstacle.y, newObstacle.x] = Cell.Empty;
		return result;
	}

	private static bool Move(ref int fromY, ref int fromX, Direction direction, Board board)
	{
		int sizeY = board.GetLength(0);
		int sizeX = board.GetLength(1);

		int steps = 0;
		while (true)
		{
			(int moveY, int moveX) = direction;
			int newY = fromY + moveY;
			int newX = fromX + moveX;

			if (newY < 0 || newY >= sizeY) return false;
			if (newX < 0 || newX >= sizeX) return false;
			if (board[newY, newX] == Cell.Obstacle)
			{
				return steps > 0;
			}

			fromY = newY;
			fromX = newX;
			++steps;
		}
	}
}
