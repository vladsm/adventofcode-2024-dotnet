using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day21;

public sealed class Day21Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			029A
			980A
			179A
			456A
			379A
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 21, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 21, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class Pad
{
	private static readonly Dictionary<(int cy, int cx), string[]> _permutations = new()
	{
		{(3, 2), ["yyyxx", "yyxyx", "yyxxy", "yxyyx", "yxyxy", "yxxyy", "xyyyx", "xyyxy", "xyxyy", "xxyyy"]},
		{(2, 2), ["yyxx", "yxyx", "yxxy", "xyyx", "xyxy", "xxyy"]},
		{(1, 2), ["yxx", "xyx", "xxy"]},
		{(3, 1), ["yyyx", "yyxy", "yxyy", "xyyy"]},
		{(2, 1), ["yyx", "yxy", "xyy"]},
		{(1, 1), ["yx", "xy"]}
	};

	protected abstract string[] Lines { get; }

	public IEnumerable<string> GetMovesOptions(char from, char to)
	{
		string[] pad = Lines;

		int sizeY = pad.Length;
		int sizeX = pad[0].Length;

		(int fromY, int fromX) = (-1, -1);
		(int toY, int toX) = (-1, -1);
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			char ch = pad[y][x];
			if (from == ch) (fromY, fromX) = (y, x);
			if (to == ch) (toY, toX) = (y, x);
		}
		if (fromY == -1 || fromX == -1 || toY == -1 || toX == -1) yield break;

		(int dy, int dx) = (toY - fromY, toX - fromX);
		if (dy == 0 && dx == 0)
		{
			yield return "";
			yield break;
		}

		char yMoveCh = dy > 0 ? '^' : 'v';
		char xMoveCh = dx > 0 ? '>' : '<';
		if (dy == 0)
		{
			yield return new string(xMoveCh, Math.Abs(dx));
			yield break;
		}
		if (dx == 0)
		{
			yield return new string(yMoveCh, Math.Abs(dy));
			yield break;
		}

		int yMove = dy > 0 ? 1 : -1;
		int xMove = dx > 0 ? 1 : -1;
		foreach (string permutation in _permutations[(Math.Abs(dy), Math.Abs(dx))])
		{
			if (!IsValidPermutation(permutation, fromY, fromX, yMove, xMove, pad)) continue;
			yield return PermutationToMoves(permutation, yMoveCh, xMoveCh);
		}
	}

	private static string PermutationToMoves(string permutation, char yCh, char xCh) =>
		new(permutation.Select(ch => ch == 'y' ? yCh : xCh).ToArray());

	private static bool IsValidPermutation(string permutation, int fromY, int fromX, int dy, int dx, string[] pad)
	{
		(int y, int x) = (fromY, fromX);
		foreach (char ch in permutation)
		{
			(y, x) = ch == 'y' ? (y + dy, x) : (y, x + dx);
			if (pad[y][x] == ' ') return false;
		}
		return true;
	}
}

internal sealed class DigitsPad : Pad
{
	protected override string[] Lines { get; } =
	[
		" 0A",
		"123",
		"456",
		"789"
	];
}

internal sealed class DirectionsPad : Pad
{
	protected override string[] Lines { get; } =
	[
		"<v>",
		" ^A"
	];
}


internal sealed class Robot
{
	private Dictionary<(char from, char to), long> _memo = [];

	public Pad Pad { get; }
	public Robot? Driver { get; set; }
	public char Current { get; set; }

	public Robot(Pad pad, Robot? driver, char current)
	{
		Pad = pad;
		Driver = driver;
		Current = current;
	}

	public long FromMemo(char from, char to) =>
		_memo.GetValueOrDefault((from, to), -1);

	public void ToMemo(char from, char to, long length) =>
		_memo[(from, to)] = length;

	public void Reset()
	{
		var robot = this;
		while (robot is not null)
		{
			robot.Current = 'A';
			robot._memo = [];
			robot = robot.Driver;
		}
	}

	public Robot Clone()
	{
		var cloned = new Robot(Pad, Driver?.Clone(), Current)
		{
			_memo = _memo
		};
		return cloned;
	}
}


internal static class PadsExtensions
{
	public static long Enter(this Robot robot, string code)
	{
		long result = 0L;
		foreach (char digit in code)
		{
			result += robot.Press(digit);
		}
		return result;
	}

	public static long Press(this Robot robot, char to)
	{
		var from = robot.Current;
		long fromMemo = robot.FromMemo(from, to);
		if (fromMemo >= 0) return fromMemo;

		string[] movesOptions = robot.Pad.GetMovesOptions(from, to).ToArray();
		robot.Current = to;
		Robot? driver = robot.Driver;
		if (driver is null)
		{
			if (movesOptions.Length == 0) return 0;
			long result = movesOptions.MinBy(m => m.Length)!.Length + 1;
			robot.ToMemo(from, to, result);
			return result;
		}

		long resultMovesLength = -1;
		Robot? resultDriver = null;
		foreach (string moves in movesOptions)
		{
			driver = driver.Clone();
			long length = 0;
			foreach (char move in moves.Concat(['A']))
			{
				length += driver.Press(move);
				driver.Current = move;
			}
			if (resultMovesLength < 0 || resultMovesLength > length)
			{
				resultMovesLength = length;
				resultDriver = driver;
			}
		}
		if (resultDriver is not null)
		{
			robot.Driver = resultDriver;
		}
		robot.ToMemo(from, to, resultMovesLength < 0 ? 0L : resultMovesLength);
		return resultMovesLength < 0 ? 0L : resultMovesLength;
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected abstract Robot CreateRobot();

	protected override long Solve(string[] codes)
	{
		Robot robot = CreateRobot();

		long result = 0L;
		foreach (string code in codes)
		{
			long length = robot.Enter(code);
			result += length * NumericPart(code);
			robot.Reset();
		}
		return result;
	}

	protected static Robot? CreateRobot(int depth) =>
		depth == 0 ? null : new Robot(new DirectionsPad(), CreateRobot(depth - 1), 'A');

	private static long NumericPart(string code) =>
		int.Parse(code.AsSpan(0, code.Length - 1));
}

internal sealed class SolverA : SolverBase
{
	protected override Robot CreateRobot() => new(new DigitsPad(), CreateRobot(2), 'A');
}

internal sealed class SolverB : SolverBase
{
	protected override Robot CreateRobot() => new(new DigitsPad(), CreateRobot(25), 'A');
}
