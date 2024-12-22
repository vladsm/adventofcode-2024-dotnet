using System.Text;

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
	protected abstract Dictionary<char, Dictionary<char, string>> Moves { get; }

	public string GetMoves(char from, char to) => Moves[from][to];
}

internal sealed class DigitsPad : Pad
{
	protected override Dictionary<char, Dictionary<char, string>> Moves { get; } = new()
	{
		{'0', new() {{'0', ""}, {'A', ">"}, {'1', "^<"}, {'2', "^"}, {'3', "^>"}, {'4', "^^<"}, {'5', "^^"}, {'6', "^^>"}, {'7', "^^^<"}, {'8', "^^^"}, {'9', "^^^>"}}},
		{'A', new() {{'0', "<"}, {'A', ""}, {'1', "^<<"}, {'2', "^<"}, {'3', "^"}, {'4', "^^<<"}, {'5', "^^<"}, {'6', "^^"}, {'7', "^^^<<"}, {'8', "^^^<"}, {'9', "^^^"}}},
		{'1', new() {{'0', ">v"}, {'A', ">>v"}, {'1', ""}, {'2', ">"}, {'3', ">>"}, {'4', "^"}, {'5', "^>"}, {'6', "^>>"}, {'7', "^^"}, {'8', "^^>"}, {'9', "^^>>"}}},
		{'2', new() {{'0', "v"}, {'A', ">v"}, {'1', "<"}, {'2', ""}, {'3', ">"}, {'4', "^<"}, {'5', "^"}, {'6', "^>"}, {'7', "^^<"}, {'8', "^^"}, {'9', "^^>"}}},
		{'3', new() {{'0', "v<"}, {'A', "v"}, {'1', "<<"}, {'2', "<"}, {'3', ""}, {'4', "^<<"}, {'5', "^<"}, {'6', "^"}, {'7', "^^<<"}, {'8', "^^<"}, {'9', "^^"}}},
		{'4', new() {{'0', ">vv"}, {'A', ">>vv"}, {'1', "v"}, {'2', ">v"}, {'3', ">>v"}, {'4', ""}, {'5', ">"}, {'6', ">>"}, {'7', "^"}, {'8', "^>"}, {'9', "^>>"}}},
		{'5', new() {{'0', "vv"}, {'A', ">vv"}, {'1', "<v"}, {'2', "v"}, {'3', ">v"}, {'4', "<"}, {'5', ""}, {'6', ">"}, {'7', "^<"}, {'8', "^"}, {'9', "^>"}}},
		{'6', new() {{'0', "vv<"}, {'A', "vv"}, {'1', "<<v"}, {'2', "<v"}, {'3', "v"}, {'4', "<<"}, {'5', "<"}, {'6', ""}, {'7', "^<<"}, {'8', "^<"}, {'9', "^"}}},
		{'7', new() {{'0', ">vvv"}, {'A', ">>vvv"}, {'1', "vv"}, {'2', ">vv"}, {'3', ">>vv"}, {'4', "v"}, {'5', ">v"}, {'6', ">>v"}, {'7', ""}, {'8', ">"}, {'9', ">>"}}},
		{'8', new() {{'0', "vvv"}, {'A', ">vvv"}, {'1', "<vv"}, {'2', "vv"}, {'3', ">vv"}, {'4', "<v"}, {'5', "v"}, {'6', ">v"}, {'7', "<"}, {'8', ""}, {'9', ">"}}},
		{'9', new() {{'0', "vvv<"}, {'A', "vvv"}, {'1', "<<vv"}, {'2', "<vv"}, {'3', "vv"}, {'4', "<<v"}, {'5', "<v"}, {'6', "v"}, {'7', "<<"}, {'8', "<"}, {'9', ""}}}
	};
}

internal sealed class DirectionsPad : Pad
{
	protected override Dictionary<char, Dictionary<char, string>> Moves { get; } = new()
	{
		{'<', new() {{'<', ""}, {'v', ">"}, {'>', ">>"}, {'^', ">^"}, {'A', ">>^"}}},
		{'v', new() {{'<', "<"}, {'v', ""}, {'>', ">"}, {'^', "^"}, {'A', ">^"}}},
		{'>', new() {{'<', "<<"}, {'v', "<"}, {'>', ""}, {'^', "<^"}, {'A', "^"}}},
		{'^', new() {{'<', "v<"}, {'v', "v"}, {'>', "v>"}, {'^', ""}, {'A', ">"}}},
		{'A', new() {{'<', "v<<"}, {'v', "v<"}, {'>', "v"}, {'^', "<"}, {'A', ""}}}
	};
}


internal sealed class Robot
{
	public Pad Pad { get; }
	public Robot? Driver { get; }
	public char Current { get; set; }

	public Robot(Pad pad, Robot? driver, char current)
	{
		Pad = pad;
		Driver = driver;
		Current = current;
	}

	public void Reset()
	{
		var robot = this;
		while (robot is not null)
		{
			robot.Current = 'A';
			robot = robot.Driver;
		}
	}
}


internal static class PadsExtensions
{
	public static void Press(this Robot robot, char to, StringBuilder tracker)
	{
		string moves = robot.Pad.GetMoves(robot.Current, to);
		robot.Current = to;
		Robot? driver = robot.Driver;
		if (driver is null)
		{
			tracker.Append(moves);
			tracker.Append('A');
			return;
		}

		foreach (char move in moves.Concat(['A']))
		{
			driver.Press(move, tracker);
			driver.Current = move;
		}
	}
}


internal sealed class SolverA : SolverWithArrayInput<string, long>
{
	protected override long Solve(string[] codes)
	{
		var robot = new Robot(
			new DigitsPad(),
			new Robot(
				new DirectionsPad(),
				new Robot(
					new DirectionsPad(),
					new Robot(new DirectionsPad(), null, 'A'),
					'A'
					),
				'A'
				),
			'A'
			);

		var robot1 = new Robot(
			new DigitsPad(),
			new Robot(
				new DirectionsPad(),
				new Robot(new DirectionsPad(), null, 'A'),
				'A'
				),
			'A'
			);


		var robot2 = new Robot(new DirectionsPad(), null, 'A');
		var tracker2 = new StringBuilder();
		foreach (char digit in "<A>A<AAv<AA>>^AvAA^Av<AAA>^A")
		{
			//379A
			//^A^^<<A>>AvvvA
			//<A>A<AAv<AA>>^AvAA^Av<AAA>^A
			//v<<A>>^AvA^Av<<A>>^AAv<A<A>>^AAvAA<^A>Av<A>^AA<A>Av<A<A>>^AAAvA<^A>A
			robot2.Press(digit, tracker2);
		}


		long result = 0L;
		foreach (string code in codes)
		{
			var tracker = new StringBuilder();
			foreach (char digit in code)
			{
				robot1.Press(digit, tracker);
			}
			robot1.Reset();

			result += tracker.Length * NumericPart(code);
		}

		return result;
	}

	private static long NumericPart(string code) =>
		int.Parse(code.AsSpan(0, code.Length - 1));
}


internal sealed class SolverB : SolverWithArrayInput<string, long>
{
	protected override long Solve(string[] codes)
	{
		throw new NotImplementedException();
	}
}
