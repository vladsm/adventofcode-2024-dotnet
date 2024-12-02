using Xunit.Abstractions;

namespace AdventOfCode2024;

using AdventOfCode;

public static class PuzzlesTestsExtensions
{
	public static AdventOfCode.IObserversBuilder<int, TResult> AssertingResult<TResult>(
		this SiteRunner.IResultCorrectnessHandlerBuilder<int, TResult> builder
		)
	{
		return builder.AssertingResult(line => Convert.ToInt32(line));
	}

	public static AdventOfCode.IObserversBuilder<TEntry, TResult> AssertingResult<TEntry, TResult>(
		this SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> builder,
		Func<string, TEntry> inputLineParser
		)
	{
		return builder.AssertingResult((line, _) => inputLineParser(line));
	}

	public static AdventOfCode.IObserversBuilder<TEntry, TResult> AssertingResult<TEntry, TResult>(
		this SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> builder,
		Func<string, int, TEntry> inputLineParser
		)
	{
		return builder.
			HandlingResultCorrectness(assert).
			ParsingInputWith(inputLineParser);

		static void assert(TResult result, bool isCorrect)
		{
			Assert.True(isCorrect);
		}
	}

	public static async ValueTask Run<TEntry, TResult>(
		this AdventOfCode.IObserversBuilder<TEntry, TResult> builder,
		ITestOutputHelper output
		)
	{
		await builder.
			ObservingResultWith(result => output.WriteLine($"Result: {result}")).
			Run();
	}
}
