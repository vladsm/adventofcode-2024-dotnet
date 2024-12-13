using System.Text.RegularExpressions;

using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day13;

public sealed class Day13Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			Button A: X+94, Y+34
			Button B: X+22, Y+67
			Prize: X=8400, Y=5400
			
			Button A: X+26, Y+66
			Button B: X+67, Y+21
			Prize: X=12748, Y=12176
			
			Button A: X+17, Y+86
			Button B: X+84, Y+37
			Prize: X=7870, Y=6450
			
			Button A: X+69, Y+23
			Button B: X+27, Y+71
			Prize: X=18641, Y=10279
			""";
		string[] lines = input.Split("\r\n");
		long result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 13, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 13, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal sealed record Button(int X, int Y);
internal sealed record Prize(int X, int Y);

internal sealed record Machine(Button A, Button B, Prize Prize);


internal abstract partial class SolverBase : SolverWithArrayInput<string, long>
{
	[GeneratedRegex("X\\+(\\d+), Y\\+(\\d+)")]
	private static partial Regex ButtonRegex { get; }

	[GeneratedRegex("X=(\\d+), Y=(\\d+)")]
	private static partial Regex PrizeRegex { get; }


	protected abstract long FindMinimumTokensToWinPrize(Machine machine);

	protected override long Solve(string[] lines)
	{
		IEnumerable<Machine> machines = Parse(lines);
		return machines.
			Select(FindMinimumTokensToWinPrize).
			Where(t => t != long.MaxValue).
			Sum();
	}


	private static IEnumerable<Machine> Parse(string[] lines)
	{
		return lines.Chunk(4).Select(toMachine);

		static Machine toMachine(string[] lines)
		{
			var a = toButton(ButtonRegex.Match(lines[0]));
			var b = toButton(ButtonRegex.Match(lines[1]));
			var prize = toPrize(PrizeRegex.Match(lines[2]));
			return new Machine(a, b, prize);
		}

		static Button toButton(Match match) =>
			new(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));

		static Prize toPrize(Match match) =>
			new(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
	}
}


internal sealed class SolverA : SolverBase
{
	protected override long FindMinimumTokensToWinPrize(Machine machine)
	{
		var ((aX, aY), (bX, bY), (prizeX, prizeY)) = machine;
		long minTokens = long.MaxValue;

		for (int a = 0; a <= 100; ++a)
		{
			for (int b = 0; b <= 100; ++b)
			{
				int xUnits = aX*a + bX*b;
				if (xUnits > prizeX) break;
				int yUnits = aY*a + bY*b;
				if (yUnits > prizeY) break;
				if (xUnits > prizeX && yUnits > prizeY) return minTokens;

				if (xUnits == prizeX && yUnits == prizeY)
				{
					int tokens = 3*a + b;
					minTokens = Math.Min(minTokens, tokens);
				}
			}
		}
		return minTokens;
	}
}


internal sealed class SolverB : SolverBase
{
	private const long Correction = 10000000000000;

	protected override long FindMinimumTokensToWinPrize(Machine machine)
	{
		long aX = machine.A.X;
		long aY = machine.A.Y;
		long bX = machine.B.X;
		long bY = machine.B.Y;
		long prizeX = machine.Prize.X + Correction;
		long prizeY = machine.Prize.Y + Correction;

		long b = (prizeY * aX - prizeX * aY) / (aX*bY - aY*bX);
		long a = (prizeX - b * bX) / aX;

		if (aX * a + bX * b != prizeX || aY * a + bY * b != prizeY)
		{
			return long.MaxValue;
		}

		return 3 * a + b;
	}
}
