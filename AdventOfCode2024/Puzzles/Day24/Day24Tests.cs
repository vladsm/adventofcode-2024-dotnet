using System.Collections;
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
		string result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 24, level: 1).
			SolveUsing<string, string, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 24, level: 2).
			SolveUsing<string, string, SolverB>().
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


internal abstract partial class SolverBase : SolverWithArrayInput<string, string>
{
	[GeneratedRegex(@"(\w+): ([01])")]
	private static partial Regex VarValueRegex { get; }

	[GeneratedRegex(@"(\w+) (AND|OR|XOR) (\w+) -> (\w+)")]
	private static partial Regex OperationRegex { get; }


	protected override string Solve(string[] lines)
	{
		(VarValue[] initials, Operation[] operations) = Parse(lines);
		return Solve(initials, operations);
	}

	protected abstract string Solve(VarValue[] initials, Operation[] operations);

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


internal sealed class Calculator
{
	private readonly Operation[] _operations;
	private readonly int _size;
	private readonly OperationsMap _operationsMap;

	public Calculator(Operation[] operations, int size)
	{
		_operations = operations;
		_size = size;
		_operationsMap = operations.
			GroupBy(
				o => o.in1,
				(in1, in1Operations) => (in1, in2ops: in1Operations.GroupBy(o => o.in2).ToDictionary(g => g.Key, g => g.Select(i => (i.op, i.result)).ToArray()))
				).
			ToDictionary(o => o.in1, o => o.in2ops);
	}

	public long Calculate(long x, long y)
	{
		var initials = CreateInitials(x, "x", _size).Concat(CreateInitials(y, "y", _size));
		return Calculate(initials);
	}

	public long Calculate(IEnumerable<VarValue> initials)
	{
		VariablesMap variables = initials.ToDictionary(i => i.var, i => i.value);

		bool canStop = false;
		while (!canStop)
		{
			canStop = true;
			int variablesCountBefore = variables.Count;
			foreach (Operation operation in _operations)
			{
				bool executed = ExecuteOperations(operation, variables);
				if (!executed && operation.result[0] == 'z')
				{
					canStop = false;
				}
			}
			if (variables.Count <= variablesCountBefore) return -1;
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

	private static IEnumerable<VarValue> CreateInitials(long n, string prefix, int size)
	{
		int low = (int)(n & 0b11111111_11111111_11111111_11111111);
		int high = (int)(n >> 32);
		var bits = new BitArray([low, high]);
		for (int i = 0; i < size; ++i)
		{
			bool bit = bits[i];
			yield return ($"{prefix}{i:00}", bit);
		}
	}

	private bool ExecuteOperations(Operation initialOperation, VariablesMap variables)
	{
		if (!ExecuteOperation(initialOperation, variables, out bool newResult)) return false;
		if (!newResult) return true;

		string result = initialOperation.result;
		if (!_operationsMap.TryGetValue(result, out var fromResultOperations)) return true;

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

	private static bool ExecuteOperation(Operation operation, VariablesMap variables, out bool newResult)
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


internal sealed class SolverA : SolverBase
{
	protected override string Solve(VarValue[] initials, Operation[] operations)
	{
		var calculator = new Calculator(operations, 45);
		return calculator.Calculate(initials).ToString();
	}
}


internal sealed class SolverB : SolverBase
{
	private const int Size = 45;

	private static readonly Random _random = new();

	private bool Check(Operation[] operations, int checkUntil)
	{
		var calculator = new Calculator(operations, Size);
		for (int i = 0; i <= checkUntil; ++i)
		{
			long x = 0;
			long y = 1L << i;

			long expected = x + y;
			long actual = calculator.Calculate(x, y);
			if (expected != actual) return false;
		}

		for (int i = 0; i <= 10; ++i)
		{
			long x = _random.NextInt64() & 0b1111_11111_11111_11111_11111_11111_11111_11111_11111;
			long y = _random.NextInt64() & 0b1111_11111_11111_11111_11111_11111_11111_11111_11111;

			long expected = x + y;
			long actual = calculator.Calculate(x, y);
			if (expected != actual) return false;
		}

		return true;
	}

	private bool CheckFast(Operation[] operations, int bit)
	{
		var calculator = new Calculator(operations, Size);
		long x = 0;
		long y = 1L << bit;

		long expected = y;
		long actual = calculator.Calculate(x, y);
		return expected == actual;
	}

	private Operation[] Swap(string out1, string out2, Operation[] operations)
	{
		return operations.Select(swapIfNeeded).ToArray();

		Operation swapIfNeeded(Operation operation)
		{
			if (operation.result == out1) return operation with { result = out2 };
			if (operation.result == out2) return operation with { result = out1 };
			return operation;
		}
	}

	protected override string Solve(VarValue[] initials, Operation[] operations)
	{
		var calculator = new Calculator(operations, Size);

		string[] variables = operations.Select(o => o.result).ToArray();

		Dictionary<string, string[]> toLinks = operations.
			SelectMany(GetLinks).
			GroupBy(l => l.to).
			ToDictionary(g => g.Key, g => g.Select(l => l.from).ToArray());

		Dictionary<string, string[][]> toPaths = toLinks.Keys.
			Where(to => to[0] is 'z').
			Select(to => (to, froms: GetPathsTo(to, toLinks).Where(path => path.Last()[0] is 'x' or 'y'))).
			ToDictionary(p => p.to, p => p.froms.Select(path => path.Prepend(p.to).ToArray()).ToArray());

		// I'm using these bits indexes to split checks into 4 parts (see below)
		// ReSharper disable once CollectionNeverQueried.Local
		List<int> wrongs = new();
		for (int i = 0; i < Size; ++i)
		{
			long x = 0;
			long y = 1L << i;

			long expected = x + y;
			long actual = calculator.Calculate(x, y);
			if (expected != actual)
			{
				wrongs.Add(i);
			}
		}

		List<(string, string)[]> testCandidates = [];
		foreach (string p11 in GetWiresUntilZIndex(10, toPaths).Except(GetWiresUntilZIndex(9, toPaths)))
		{
			foreach (string p12 in variables)
			{
				var swappedOperations1 = Swap(p11, p12, operations);
				var success1 = CheckFast(swappedOperations1, 10);
				if (!success1) continue;

				foreach (string p21 in GetWiresUntilZIndex(14, toPaths).Except(GetWiresUntilZIndex(13, toPaths)))
				foreach (string p22 in variables)
				{
					var swappedOperations2 = Swap(p21, p22, swappedOperations1);
					var success2 = CheckFast(swappedOperations2, 14);
					if (!success2) continue;

					foreach (string p31 in GetWiresUntilZIndex(25, toPaths).Except(GetWiresUntilZIndex(24, toPaths)))
					foreach (string p32 in variables)
					{
						var swappedOperations3 = Swap(p31, p32, swappedOperations2);
						var success3 = CheckFast(swappedOperations3, 25);
						if (!success3) continue;

						foreach (string p41 in GetWiresUntilZIndex(34, toPaths).Except(GetWiresUntilZIndex(33, toPaths)))
						foreach (string p42 in variables)
						{
							var swappedOperations4 = Swap(p41, p42, swappedOperations3);
							var success4 = CheckFast(swappedOperations4, 34);
							if (!success4) continue;

							bool success = Check(swappedOperations4, Size - 1);
							if (!success) continue;

							testCandidates.Add([(p11, p12), (p21, p22), (p31, p32), (p41, p42)]);
						}
					}
				}
			}
		}

		var swaps = testCandidates.First().SelectMany(p => new[] {p.Item1, p.Item2}).OrderBy(w => w);
		return string.Join(",", swaps);
	}

	private static HashSet<string> GetWiresUntilZIndex(int index, Dictionary<string, string[][]> toPaths)
	{
		HashSet<string> result = [];
		for (int i = 0; i <= index; i++)
		{
			foreach (string wire in toPaths[$"z{i:00}"].SelectMany(w => w))
			{
				if (wire[0] is 'x' or 'y') continue;
				result.Add(wire);
			}
		}
		return result;
	}

	private static IEnumerable<(string from, string to)> GetLinks(Operation operation)
	{
		(string in1, string in2, _, string result) = operation;
		yield return (in1, result);
		yield return (in2, result);
	}

	private static List<List<string>> GetPathsTo(string to, Dictionary<string, string[]> toLinks)
	{
		if (!toLinks.TryGetValue(to, out string[]? froms)) return [];

		List<List<string>> result = new();
		foreach (string from in froms)
		{
			var paths = GetPathsTo(from, toLinks);
			if (paths.Count == 0)
			{
				result.Add([from]);
			}
			else
			{
				string[] fromArray = [from];
				result.AddRange(paths.Select(path => fromArray.Concat(path).ToList()));
			}
		}
		return result;
	}
}
