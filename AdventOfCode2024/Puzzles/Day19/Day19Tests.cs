using System.Text;

using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day19;

public sealed class Day19Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			r, wr, b, g, bwu, rb, gb, br
			
			brwrr
			bggr
			gbbr
			rrbgbr
			ubwu
			bwurrg
			brgr
			bbrgwb
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 19, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 19, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected abstract long Solve(IReadOnlyCollection<byte[]> patterns, IReadOnlyCollection<byte[]> designs);

	protected override long Solve(string[] lines)
	{
		var (patterns, designs) = Parse(lines);
		return Solve(patterns, designs);
	}

	private static (IReadOnlyCollection<byte[]> patterns, IReadOnlyCollection<byte[]> designs) Parse(string[] lines)
	{
		IReadOnlyCollection<byte[]> patterns = lines[0].
			Split(',', StringSplitOptions.TrimEntries).
			Select(toColors).
			ToList();
		IReadOnlyCollection<byte[]> designs = lines.
			Skip(2).
			Select(toColors).
			ToList();
		return (patterns, designs);

		static byte[] toColors(string str) => str.Select(toColor).ToArray();

		static byte toColor(char ch) =>
			ch switch
			{
				'w' => 0,
				'u' => 1,
				'b' => 2,
				'r' => 3,
				'g' => 4,
				_ => throw new ArgumentOutOfRangeException(nameof(ch), $"Unknown color '{ch}'")
			};
	}
}

internal sealed class ColorPatternsIndexNode
{
	private static ColorPatternsIndexNode Empty() => new([]);

	private bool _isPattern;
	private ColorPatternsIndexNode[] _next;

	public ColorPatternsIndexNode() : this([Empty(), Empty(), Empty(), Empty(), Empty()])
	{
	}

	private ColorPatternsIndexNode(ColorPatternsIndexNode[] next)
	{
		_next = next;
	}

	public void AddPattern(byte[] pattern)
	{
		ColorPatternsIndexNode node = this;
		ColorPatternsIndexNode[] nodes = _next;
		foreach (byte color in pattern)
		{
			if (nodes.Length == 0)
			{
				nodes = [Empty(), Empty(), Empty(), Empty(), Empty()];
				node._next = nodes;
			}
			node = nodes[color];
			nodes = node._next;
		}
		node._isPattern = true;
	}

	public bool Match(ReadOnlySpan<byte> pattern)
	{
		bool result = false;
		ColorPatternsIndexNode node = this;
		foreach (byte color in pattern)
		{
			result = false;

			var nodes = node._next;
			if (nodes.Length == 0) return false;

			node = nodes[color];
			if (node._isPattern)
			{
				result = true;
			}
		}
		return result;
	}
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(IReadOnlyCollection<byte[]> patterns, IReadOnlyCollection<byte[]> designs)
	{
		ColorPatternsIndexNode index = new();
		foreach (byte[] pattern in patterns)
		{
			index.AddPattern(pattern);
		}
		return designs.Count(design => CanDisplay(design, index));
	}

	private static bool CanDisplay(ReadOnlySpan<byte> design, ColorPatternsIndexNode index)
	{
		if (design.Length == 0) return true;
		for (int prefixLength = 1; prefixLength <= design.Length; ++prefixLength)
		{
			ReadOnlySpan<byte> prefix = design[..prefixLength];
			if (!index.Match(prefix)) continue;

			bool canDisplay = CanDisplay(design[prefixLength..], index);
			if (canDisplay) return true;
		}
		return false;
	}

	// private static bool CanDisplay(ReadOnlySpan<byte> design, ColorPatternsIndexNode index, int level = 0)
	// {
	// 	string indent = new string(' ', level * 4);
	// 	Console.WriteLine($"{indent}Cheching '{AsString(design)}':");
	//
	// 	if (design.Length == 0) return true;
	// 	for (int prefixLength = 1; prefixLength <= design.Length; ++prefixLength)
	// 	{
	// 		ReadOnlySpan<byte> prefix = design[..prefixLength];
	// 		if (!index.Match(prefix))
	// 		{
	// 			Console.WriteLine($"{indent}Match '{AsString(prefix)}' --> {false}");
	// 			continue;
	// 		}
	// 		else
	// 		{
	// 			Console.WriteLine($"{indent}Match '{AsString(prefix)}' --> {true}");
	// 		}
	//
	// 		bool canDisplay = CanDisplay(design[prefixLength..], index, level + 1);
	// 		Console.WriteLine($"{indent}{AsString(design[prefixLength..])}, canDisplay = {canDisplay}");
	// 		if (canDisplay) return true;
	// 	}
	// 	Console.WriteLine($"{indent}{AsString(design)}, canDisplay = {false}");
	// 	return false;
	// }
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(IReadOnlyCollection<byte[]> patterns, IReadOnlyCollection<byte[]> designs)
	{
		ColorPatternsIndexNode index = new();
		foreach (byte[] pattern in patterns)
		{
			index.AddPattern(pattern);
		}

		Dictionary<string, long> memo = [];
		return designs.Sum(design => CalculateDisplaysQuantity(design, memo, index));
	}

	private static long CalculateDisplaysQuantity(ReadOnlySpan<byte> design, Dictionary<string, long> memo, ColorPatternsIndexNode index)
	{
		if (design.Length == 0) return 1;

		string memoKey = AsString(design);
		if (memo.TryGetValue(memoKey, out long resultFromMemo)) return resultFromMemo;

		long result = 0;
		for (int prefixLength = 1; prefixLength <= design.Length; ++prefixLength)
		{
			ReadOnlySpan<byte> prefix = design[..prefixLength];
			if (!index.Match(prefix)) continue;

			result += CalculateDisplaysQuantity(design[prefixLength..], memo, index);
		}

		memo[memoKey] = result;
		return result;
	}

	private static string AsString(ReadOnlySpan<byte> colors)
	{
		StringBuilder sb = new();
		foreach (byte color in colors)
		{
			sb.Append(toChar(color));
		}
		return sb.ToString();

		static char toChar(byte color) =>
			color switch
			{
				0 => 'w',
				1 => 'u',
				2 => 'b',
				3 => 'r',
				4 => 'g',
				_ => throw new ArgumentOutOfRangeException(nameof(color), $"Unknown color '{color}'")
			};
	}
}
