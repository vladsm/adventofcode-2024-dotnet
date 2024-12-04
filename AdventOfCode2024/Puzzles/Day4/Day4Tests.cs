using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day4;

public sealed class Day4Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample_1")]
	public async Task Sample_1()
	{
		string input =
			"""
			.M.S......
			..A..MSMS.
			.M.S.MAA..
			..A.ASMSM.
			.M.S.M....
			..........
			S.S.S.S.S.
			.A.A.A.A..
			M.M.M.M.M.
			..........
			""";
		string[] lines = input.Split("\r\n");
		var result = new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 4, level: 1).
			SolveUsing<string, int, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 4, level: 2).
			SolveUsing<string, int, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, int>
{
	protected override int Solve(string[] lines)
	{
		int sizeX = lines[0].Length;
		int sizeY = lines.Length;

		const int borderRadius = 3;
		string fakeLine = new string('.', sizeX + borderRadius * 2);
		lines = lines.
			Select(line => $"...{line}...").
			Prepend(fakeLine).
			Prepend(fakeLine).
			Prepend(fakeLine).
			Append(fakeLine).
			Append(fakeLine).
			Append(fakeLine).
			ToArray();

		int result = 0;
		for (int y = borderRadius; y < sizeY + borderRadius; ++y)
		for (int x = borderRadius; x < sizeX + borderRadius; ++x)
		{
			result += MatchCount(x, y, lines);
		}

		return result;
	}

	protected abstract int MatchCount(int x, int y, string[] lines);
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override int MatchCount(int x, int y, string[] lines)
	{
		int result = 0;
		result += IsHorizontal(x, y, lines, false) ? 1 : 0;
		result += IsHorizontal(x, y, lines, true) ? 1 : 0;
		result += IsVertical(x, y, lines, false) ? 1 : 0;
		result += IsVertical(x, y, lines, true) ? 1 : 0;
		result += IsDiagonalRight(x, y, lines, false) ? 1 : 0;
		result += IsDiagonalRight(x, y, lines, true) ? 1 : 0;
		result += IsDiagonalLeft(x, y, lines, false) ? 1 : 0;
		result += IsDiagonalLeft(x, y, lines, true) ? 1 : 0;
		return result;
	}

	private static readonly string _pattern = "XMAS";
	private static readonly string _backwardPattern = "SAMX";

	private static bool IsHorizontal(int startX, int startY, string[] lines, bool backward)
	{
		string pattern = backward ? _backwardPattern : _pattern;
		return
			lines[startY][startX] == pattern[0] &&
			lines[startY][startX + 1] == pattern[1] &&
			lines[startY][startX + 2] == pattern[2] &&
			lines[startY][startX + 3] == pattern[3];
	}

	private static bool IsVertical(int startX, int startY, string[] lines, bool backward)
	{
		string pattern = backward ? _backwardPattern : _pattern;
		return
			lines[startY][startX] == pattern[0] &&
			lines[startY + 1][startX] == pattern[1] &&
			lines[startY + 2][startX] == pattern[2] &&
			lines[startY + 3][startX] == pattern[3];
	}

	private static bool IsDiagonalRight(int startX, int startY, string[] lines, bool backward)
	{
		string pattern = backward ? _backwardPattern : _pattern;
		return
			lines[startY][startX] == pattern[0] &&
			lines[startY + 1][startX + 1] == pattern[1] &&
			lines[startY + 2][startX + 2] == pattern[2] &&
			lines[startY + 3][startX + 3] == pattern[3];
	}

	private static bool IsDiagonalLeft(int startX, int startY, string[] lines, bool backward)
	{
		string pattern = backward ? _backwardPattern : _pattern;
		return
			lines[startY][startX] == pattern[0] &&
			lines[startY + 1][startX - 1] == pattern[1] &&
			lines[startY + 2][startX - 2] == pattern[2] &&
			lines[startY + 3][startX - 3] == pattern[3];
	}
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override int MatchCount(int x, int y, string[] lines)
	{
		int result = 0;
		result += IsDiagonalRight(x, y, lines, false) && IsDiagonalLeft(x + 2, y, lines, false) ? 1 : 0;
		result += IsDiagonalRight(x, y, lines, false) && IsDiagonalLeft(x + 2, y, lines, true) ? 1 : 0;
		result += IsDiagonalRight(x, y, lines, true) && IsDiagonalLeft(x + 2, y, lines, false) ? 1 : 0;
		result += IsDiagonalRight(x, y, lines, true) && IsDiagonalLeft(x + 2, y, lines, true) ? 1 : 0;
		return result;
	}

	private static readonly string _pattern = "MAS";
	private static readonly string _backwardPattern = "SAM";

	private static bool IsDiagonalRight(int startX, int startY, string[] lines, bool backward)
	{
		string pattern = backward ? _backwardPattern : _pattern;
		return
			lines[startY][startX] == pattern[0] &&
			lines[startY + 1][startX + 1] == pattern[1] &&
			lines[startY + 2][startX + 2] == pattern[2];
	}

	private static bool IsDiagonalLeft(int startX, int startY, string[] lines, bool backward)
	{
		string pattern = backward ? _backwardPattern : _pattern;
		return
			lines[startY][startX] == pattern[0] &&
			lines[startY + 1][startX - 1] == pattern[1] &&
			lines[startY + 2][startX - 2] == pattern[2];
	}
}
