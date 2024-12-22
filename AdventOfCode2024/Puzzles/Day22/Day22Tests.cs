using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day22;

using Changes = (short, short, short, short);

public sealed class Day22Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			1
			10
			100
			2024
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 22, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 22, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected override long Solve(string[] lines)
	{
		long[] initial = lines.Select(long.Parse).ToArray();
		return Solve(initial);
	}

	protected abstract long Solve(long[] initial);

	protected static long GenerateNext(long initial)
	{
		long secret = prune(mix(initial, initial * 64));
		secret = prune(mix(secret, secret / 32));
		secret = prune(mix(secret, secret * 2048));
		return secret;

		static long mix(long s, long n) => s ^ n;

		static long prune(long s) => s % 16777216;
	}
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(long[] initial)
	{
		var result = initial.Select(s => GenerateNext(s, 2000)).Sum();
		return result;
	}

	private static long GenerateNext(long initial, int steps)
	{
		long secret = initial;
		for (int i = 0; i < steps; ++i)
		{
			secret = GenerateNext(secret);
		}
		return secret;
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(long[] initial)
	{
		//var testPrices = GeneratePrices(2024, 2000).ToArray();
		//var test = FindPrice(testPrices, [-2, 1, -1, 3]);

		Price[][] byers = initial.Select(s => GeneratePrices(s, 2000).ToArray()).ToArray();
		Dictionary<Changes, HashSet<int>> changesSet = new();
		for (int i = 1; i < byers.Length; ++i)
		{
			foreach (var changes in GetChangesOptions(byers[i]))
			{
				var set = changesSet.GetValueOrDefault(changes, []);
				set.Add(i);
				changesSet[changes] = set;
			}
		}
		var targetChangesOptions = changesSet.
			Where(kvp => kvp.Value.Count > 200).
			Select(kvp => kvp.Key).
			ToList();

		long maxPrice = -1L;
		foreach (Changes targetChanges in targetChangesOptions)
		{
			long totalPrice = 0L;
			foreach (Price[] byer in byers)
			{
				long price = FindPrice(byer, targetChanges);
				totalPrice += price;
			}

			if (totalPrice > maxPrice)
			{
				maxPrice = totalPrice;
			}
		}

		return maxPrice;
	}

	private static IEnumerable<Price> GeneratePrices(long initial, int steps)
	{
		long secret = initial;
		short prevPrice = (short)(initial % 10);
		for (int i = 0; i < steps; ++i)
		{
			secret = GenerateNext(secret);
			byte price = (byte)(secret % 10);
			short change = (short)(price - prevPrice);
			yield return new Price(price, change);
			prevPrice = price;
		}
	}

	private static IEnumerable<(short, short, short, short)> GetChangesOptions(Price[] prices)
	{
		for (int i = 3; i < prices.Length; ++i)
		{
			yield return (prices[i-3].Change, prices[i-2].Change, prices[i-1].Change, prices[i].Change);
		}
	}

	private static long FindPrice(Price[] prices, Changes targetChanges)
	{
		var (c1, c2, c3, c4) = targetChanges;
		for (int i = 3; i < prices.Length; ++i)
		{
			bool match =
				prices[i].Change == c4 &&
				prices[i - 1].Change == c3 &&
				prices[i - 2].Change == c2 &&
				prices[i - 3].Change == c1;
			if (match) return prices[i].Value;
		}
		return 0;
	}
}


internal record struct Price(byte Value, short Change);
