using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day17;

public sealed class Day17Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			Register A: 729
			Register B: 0
			Register C: 0
			
			Program: 0,1,5,4,3,0
			""";

		string[] lines = input.Split("\r\n");
		string result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 17, level: 1).
			SolveUsing<string, string, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 17, level: 2).
			SolveUsing<string, string, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, string>
{
	protected abstract string Solve(Program program);

	protected override string Solve(string[] lines) =>
		Solve(Parse(lines));

	private static Program Parse(string[] lines)
	{
		long a = long.Parse(lines[0].AsSpan(12));
		long b = long.Parse(lines[1].AsSpan(12));
		long c = long.Parse(lines[2].AsSpan(12));

		ReadOnlySpan<char> instructionsStr = lines[4].AsSpan(9);
		List<byte> instructions = [];
		foreach (Range segment in instructionsStr.Split(','))
		{
			byte instruction = byte.Parse(instructionsStr[segment]);
			instructions.Add(instruction);
		}

		return new Program(instructions.ToArray(), a, b, c);
	}
}


internal sealed class Program
{
	private readonly byte[] _instructions;
	private long _b;
	private long _c;
	private long _a;
	private int _pointer;
	private readonly List<string> _out = new();

	public Program(byte[] instructions, long registerA, long registerB, long registerC)
	{
		_instructions = instructions;
		_a = registerA;
		_b = registerB;
		_c = registerC;
	}

	public string GetOutput() =>
		string.Join(',', _out);

	public void Run()
	{
		while (RunStep());
	}


	private bool RunStep()
	{
		if (!TryReadInstruction(out byte instruction)) return false;
		return instruction switch
		{
			0 => Adv(),
			1 => Bxl(),
			2 => Bst(),
			3 => Jnz(),
			4 => Bxc(),
			5 => Out(),
			6 => Bdv(),
			7 => Cdv(),
			_ => throw new InvalidOperationException("Invalid instruction")
		};
	}

	private bool Adv()
	{
		if (!TryReadComboOperand(out long operand)) return false;
		long denominator = 1L << (int)operand;
		_a /= denominator;
		return true;
	}

	private bool Bxl()
	{
		if (!TryReadOperand(out byte operand)) return false;
		_b ^= operand;
		return true;
	}

	private bool Bst()
	{
		if (!TryReadComboOperand(out long operand)) return false;
		_b = operand % 8;
		return true;
	}

	private bool Jnz()
	{
		if (!TryReadOperand(out byte operand)) return false;
		if (_a != 0)
		{
			_pointer = operand;
		}
		return true;
	}

	private bool Bxc()
	{
		if (!TryReadOperand(out _)) return false;
		_b ^= _c;
		return true;
	}

	private bool Out()
	{
		if (!TryReadComboOperand(out long operand)) return false;
		_out.Add((operand % 8).ToString());
		return true;
	}

	private bool Bdv()
	{
		if (!TryReadComboOperand(out long operand)) return false;
		long denominator = 1L << (int)operand;
		_b = _a / denominator;
		return true;
	}

	private bool Cdv()
	{
		if (!TryReadComboOperand(out long operand)) return false;
		long denominator = 1L << (int)operand;
		_c = _a / denominator;
		return true;
	}

	private bool TryReadInstruction(out byte instruction)
	{
		if (_pointer >= _instructions.Length)
		{
			instruction = 0;
			return false;
		}
		instruction = _instructions[_pointer++];
		return true;
	}

	private bool TryReadOperand(out byte operand)
	{
		if (_pointer >= _instructions.Length)
		{
			operand = 0;
			return false;
		}
		operand = _instructions[_pointer++];
		return true;
	}

	private bool TryReadComboOperand(out long comboOperand)
	{
		if (!TryReadOperand(out byte operand))
		{
			comboOperand = 0;
			return false;
		}
		comboOperand = operand switch
		{
			4 => _a,
			5 => _b,
			6 => _c,
			_ when operand <= 3 => operand,
			_ => throw new InvalidOperationException("Unexpected operand")
		};
		return true;
	}
}


internal sealed class SolverA : SolverBase
{
	protected override string Solve(Program program)
	{
		program.Run();
		return program.GetOutput();
	}
}


internal sealed class SolverB : SolverBase
{
	protected override string Solve(Program program)
	{
		throw new NotImplementedException();
	}
}
