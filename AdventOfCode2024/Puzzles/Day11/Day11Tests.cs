using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day11;

public sealed class Day11Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			125 17
			""";
		string[] lines = input.Split("\r\n");
		ulong result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 11, level: 1).
			SolveUsing<string, ulong, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 11, level: 2).
			SolveUsing<string, ulong, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}

internal abstract class SolverBase : SolverWithArrayInput<string, ulong>
{
	protected override ulong Solve(string[] lines)
	{
		List<ulong> numbers = lines[0].Split(' ').Select(ulong.Parse).ToList();
		return Solve(numbers);
	}

	protected abstract int IterationsCount { get; }

	private ulong Solve(IReadOnlyCollection<ulong> numbers)
	{
		ulong count = 0;
		foreach (ulong number in numbers)
		{
			Console.WriteLine(number);
			count += TransformAndCalculate(number, IterationsCount);
		}
		return count;
	}

	private readonly Dictionary<(ulong number, int iteration), ulong> _memo = [];

	private ulong TransformAndCalculate(ulong initial, int iteration)
	{
		if (iteration == 0) return 1;

		if (_memo.TryGetValue((initial, iteration), out ulong result)) return result;

		ulong count = 0;
		(bool both, ulong n1, ulong n2) = Transform(initial);
		count += TransformAndCalculate(n1, iteration - 1);
		if (both)
		{
			count += TransformAndCalculate(n2, iteration - 1);
		}

		if (initial < 10_000_000)
		{
			_memo[(initial, iteration)] = count;
		}

		return count;
	}

	private static (bool, ulong, ulong) Transform(ulong number)
	{
		return number switch
		{
			0 => (false, 1, 0),
			_ when isEvenSize(number, out ulong left, out ulong right) => (true, left, right),
			_ => (false, number * 2024, 0)
		};

		static bool isEvenSize(ulong n, out ulong left, out ulong right)
		{
			Assert.True(n > 0);

			left = right = 0;

			int digits = 1;
			ulong pow10 = 1;
			ulong halfPow10 = 1;
			while (true)
			{
				pow10 *= 10;
				if (pow10 > n) break;

				if (++digits % 2 == 0)
				{
					halfPow10 *= 10;
				}
			}
			if (digits % 2 != 0) return false;

			left = n / halfPow10;
			right = n % halfPow10;
			return true;
		}
	}
}

[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override int IterationsCount => 25;
}

[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override int IterationsCount => 75;
}
