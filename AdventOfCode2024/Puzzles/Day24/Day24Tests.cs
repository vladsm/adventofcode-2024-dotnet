using System.Text.RegularExpressions;

using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day24;

using VarValue = (string var, bool value);
using Operation = (string in1, string in2, Op op, string result);
using OperationsMap = Dictionary<string, Dictionary<string, (Op op, string result)[]>>;
using VariablesMap = Dictionary<string, bool>;

public sealed class Day24Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			x00: 1
			x01: 0
			x02: 1
			x03: 1
			x04: 0
			y00: 1
			y01: 1
			y02: 1
			y03: 1
			y04: 1
			
			ntg XOR fgs -> mjb
			y02 OR x01 -> tnw
			kwq OR kpj -> z05
			x00 OR x03 -> fst
			tgd XOR rvg -> z01
			vdt OR tnw -> bfw
			bfw AND frj -> z10
			ffh OR nrd -> bqk
			y00 AND y03 -> djm
			y03 OR y00 -> psh
			bqk OR frj -> z08
			tnw OR fst -> frj
			gnj AND tgd -> z11
			bfw XOR mjb -> z00
			x03 OR x00 -> vdt
			gnj AND wpb -> z02
			x04 AND y00 -> kjc
			djm OR pbm -> qhw
			nrd AND vdt -> hwm
			kjc AND fst -> rvg
			y04 OR y02 -> fgs
			y01 AND x02 -> pbm
			ntg OR kjc -> kwq
			psh XOR fgs -> tgd
			qhw XOR tgd -> z09
			pbm OR djm -> kpj
			x03 XOR y03 -> ffh
			x00 XOR y04 -> ntg
			bfw OR bqk -> z06
			nrd XOR fgs -> wpb
			frj XOR qhw -> z04
			bqk OR frj -> z07
			y03 OR x01 -> nrd
			hwm AND bqk -> z03
			tgd XOR rvg -> z12
			tnw OR pbm -> gnj
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 24, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 24, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal enum Op
{
	And,
	Or,
	Xor
}


internal abstract partial class SolverBase : SolverWithArrayInput<string, long>
{
	[GeneratedRegex(@"(\w+): ([01])")]
	private static partial Regex VarValueRegex { get; }

	[GeneratedRegex(@"(\w+) (AND|OR|XOR) (\w+) -> (\w+)")]
	private static partial Regex OperationRegex { get; }


	protected override long Solve(string[] lines)
	{
		(VarValue[] initials, Operation[] operations) = Parse(lines);
		return Solve(initials, operations);
	}

	protected abstract long Solve(VarValue[] initials, Operation[] operations);

	private (VarValue[] initials, Operation[] operations) Parse(string[] lines)
	{
		List<VarValue> initials = new();
		List<Operation> operations = new();
		bool readOperations = false;
		foreach (string line in lines)
		{
			if (line.Length == 0)
			{
				readOperations = true;
				continue;
			}

			if (!readOperations)
			{
				initials.Add(ParseVarValue(line));
			}
			else
			{
				operations.Add(ParseOperation(line));
			}
		}
		return (initials.ToArray(), operations.ToArray());
	}

	private static VarValue ParseVarValue(string line)
	{
		var match = VarValueRegex.Match(line);
		return (match.Groups[1].Value, toBool(match.Groups[2].Value));

		static bool toBool(string v) => v != "0";
	}

	private static Operation ParseOperation(string line)
	{
		var match = OperationRegex.Match(line);
		return (match.Groups[1].Value, match.Groups[3].Value, toOp(match.Groups[2].Value), match.Groups[4].Value);

		static Op toOp(string op) =>
			op switch
			{
				"AND" => Op.And,
				"OR" => Op.Or,
				"XOR" => Op.Xor,
				_ => throw new ArgumentException($"Unknown operation: {op}")
			};
	}
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(VarValue[] initials, Operation[] operations)
	{
		OperationsMap operationsMap = operations.
			//Concat(operations.Select(o => (Operation)(o.in2, o.in1, o.op, o.result))).
			GroupBy(
				o => o.in1,
				(in1, in1Operations) => (in1, in2ops: in1Operations.GroupBy(o => o.in2).ToDictionary(g => g.Key, g => g.Select(i => (i.op, i.result)).ToArray()))
				).
			ToDictionary(o => o.in1, o => o.in2ops);
		VariablesMap variables = initials.ToDictionary(i => i.var, i => i.value);

		bool canStop = false;
		while (!canStop)
		{
			canStop = true;
			foreach (Operation operation in operations)
			{
				bool executed = ExecuteOperations(operation, operationsMap, variables);
				if (!executed && operation.result[0] == 'z')
				{
					canStop = false;
				}
			}
		}

		bool[] bits = variables.
			Where(kvp => kvp.Key[0] == 'z').
			OrderBy(kvp => kvp.Key.Substring(1)).
			Select(kvp => kvp.Value).
			ToArray();
		long result = 0L;
		for (int i = 0; i < bits.Length; ++i)
		{
			bool bit = bits[i];
			if (!bit) continue;
			result += (1L << i);
		}
		return result;
	}

	private bool ExecuteOperations(Operation initialOperation, OperationsMap operationsMap, VariablesMap variables)
	{
		if (!ExecuteOperation(initialOperation, variables, out bool newResult)) return false;
		if (!newResult) return true;

		string result = initialOperation.result;
		if (!operationsMap.TryGetValue(result, out var fromResultOperations)) return true;

		bool allExecuted = true;
		foreach (var (fromResultIn2, fromResultIn2Operations) in fromResultOperations)
		{
			foreach (var (fromResultOp, fromResultResult) in fromResultIn2Operations)
			{
				if (!ExecuteOperation((result, fromResultIn2, fromResultOp, fromResultResult), variables, out _))
				{
					allExecuted = false;
				}
			}
		}
		return allExecuted;
	}

	private bool ExecuteOperation(Operation operation, VariablesMap variables, out bool newResult)
	{
		newResult = false;
		(string in1, string in2, Op op, string result) = operation;
		if (variables.ContainsKey(result)) return true;
		if (!variables.TryGetValue(in1, out bool in1Value) || !variables.TryGetValue(in2, out bool in2Value)) return false;
		bool resultValue = op switch
		{
			Op.And => in1Value && in2Value,
			Op.Or => in1Value || in2Value,
			Op.Xor => in1Value ^ in2Value,
			_ => throw new InvalidOperationException($"Unknown operation: {op}")
		};
		variables[result] = resultValue;
		newResult = true;
		return true;
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(VarValue[] initials, Operation[] operations)
	{
		throw new NotImplementedException();
	}
}
