using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day7;

using Operator = Func<ulong, ulong, ulong>;

public sealed class Day7Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			190: 10 19
			3267: 81 40 27
			83: 17 5
			156: 15 6
			7290: 6 8 6 15
			161011: 16 10 13
			192: 17 8 14
			21037: 9 7 18 13
			292: 11 6 16 20
			""";
		string[] lines = input.Split("\r\n");
		ulong result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}


	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 7, level: 1).
			SolveUsing<string, ulong, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 7, level: 2).
			SolveUsing<string, ulong, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal record Equation(ulong[] Operands, ulong Result);


internal abstract class SolverBase : SolverWithArrayInput<string, ulong>
{
	protected abstract Operator[] AvailableOperators { get; }

	protected override ulong Solve(string[] lines)
	{
		var equations = lines.Select(ParseLine).ToList();
		ulong sum = 0;
		foreach (var equation in equations.Where(IsResolvable))
		{
			sum += equation.Result;
		}
		return sum;
	}


	private static Equation ParseLine(string line)
	{
		string[] sides = line.Split(':', StringSplitOptions.TrimEntries);
		ulong equationResult = ulong.Parse(sides[0]);
		ulong[] operands = sides[1].Split(' ').Select(ulong.Parse).ToArray();
		return new Equation(operands, equationResult);
	}

	private bool IsResolvable(Equation equation)
	{
		(ulong[] operands, ulong expectedResult) = equation;
		var operatorsCombinations = GenerateOperatorsCombinations(equation.Operands.Length);
		foreach (Operator[] operators in operatorsCombinations)
		{
			ulong actualResult = operators.
				Prepend((agg, operand) => agg + operand).
				Zip(operands).
				Aggregate(
					0UL,
					(agg, pair) =>
					{
						(Operator op, ulong operand) = pair;
						return op(agg, operand);
					}
					);
			if (actualResult == expectedResult) return true;
		}
		return false;
	}

	private IEnumerable<Operator[]> GenerateOperatorsCombinations(int size)
	{
		var availableOperators = AvailableOperators;

		byte[] emptyPattern = new byte[size];
		foreach (byte[] ops in generate(emptyPattern, 0, availableOperators))
		{
			yield return ops.Select(op => availableOperators[op]).ToArray();
		}

		static IEnumerable<byte[]> generate(byte[] pattern, int index, Operator[] availableOperators)
		{
			for (int op = 0; op < availableOperators.Length; ++op)
			{
				pattern[index] = (byte)op;
				if (index == pattern.Length - 1)
				{
					yield return pattern;
				}
				else
				{
					foreach (var ops in generate(pattern, index + 1, availableOperators))
					{
						yield return ops;
					}
				}
			}
		}
	}
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override Operator[] AvailableOperators => _operators;

	private static readonly Operator[] _operators =
	[
		static (agg, operand) => agg * operand,
		static (agg, operand) => agg + operand
	];
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override Operator[] AvailableOperators => _operators;

	private static readonly Operator[] _operators =
	[
		static (agg, operand) => agg * operand,
		static (agg, operand) => agg + operand,
		Concat
	];

	private static ulong Concat(ulong a, ulong b) => ulong.Parse($"{a}{b}");
}
