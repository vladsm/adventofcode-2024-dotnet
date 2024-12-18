﻿using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day17;

public sealed class Day17Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			Register A: 2024
			Register B: 0
			Register C: 0
			
			Program: 0,3,5,4,3,0
			""";

		string[] lines = input.Split("\r\n");
		string result = await new SolverB(_output).Solve(lines.ToAsyncEnumerable());
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
			SolveUsing(new SolverB(_output)).
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
	private long _a;
	private long _b;
	private long _c;
	private int _pointer;
	private readonly List<string> _out = new();

	public Program(byte[] instructions, long registerA, long registerB, long registerC)
	{
		_instructions = instructions;
		_a = registerA;
		_b = registerB;
		_c = registerC;
	}

	public long RegisterA => _a;

	public long RegisterB => _b;

	public long RegisterC => _c;

	public byte[] Instructions => _instructions;

	public void Reset(long registerA, long registerB, long registerC)
	{
		_a = registerA;
		_b = registerB;
		_c = registerC;
		_pointer = 0;
		_out.Clear();
	}

	public string GetOutput() =>
		string.Join(',', _out);

	public void Run()
	{
		while (RunStep());
	}

	public bool RunWithOutputValidation(string[] targetOutput)
	{
		int targetOutCount = targetOutput.Length;
		while (RunStep())
		{
			int outCount = _out.Count;
			if (outCount > targetOutCount || outCount > 0 && targetOutput[outCount - 1] != _out[outCount - 1])
			{
				return false;
			}
		}
		return _out.Count == targetOutCount && targetOutput[targetOutCount - 1] == _out[targetOutCount - 1];
	}

	public bool RunWithOutputValidation(string targetOutput)
	{
		while (RunStep())
		{
			int outCount = _out.Count;
			if (outCount > 0 && _out[0] == targetOutput)
			{
				return true;
			}
		}
		return false;
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
	private readonly ITestOutputHelper _log;

	public SolverB(ITestOutputHelper log)
	{
		_log = log;
	}

	protected override string Solve(Program program)
	{
		long initialRegisterB = program.RegisterB;
		long initialRegisterC = program.RegisterC;

		string[] targetOutput = program.Instructions.Select(i => i.ToString()).ToArray();

		List<long> aComponents = [];
		foreach (string item in targetOutput)
		{
			long componentACandidate = -1;
			while (true)
			{
				program.Reset(++componentACandidate, initialRegisterB, initialRegisterC);
				bool isValid = program.RunWithOutputValidation(item);
				if (isValid) break;
			}
			aComponents.Add(componentACandidate);
		}

		long pow = 1;
		long A = 0;
		for (int i = 0; i < aComponents.Count; ++i)
		{
			long a = aComponents[i];
			A += a * pow;
			pow *= 8;
		}


		throw new NotImplementedException();
	}

	// protected override string Solve(Program program)
	// {
	// 	long initialRegisterB = program.RegisterB;
	// 	long initialRegisterC = program.RegisterC;
	//
	// 	string[] targetOutput = program.Instructions.Select(i => i.ToString()).ToArray();
	// 	//long registerACandidate = -1;
	// 	long registerACandidate = 1999;
	// 	while (true)
	// 	{
	// 		program.Reset(++registerACandidate, initialRegisterB, initialRegisterC);
	// 		bool isValid = program.RunWithOutputValidation(targetOutput);
	// 		if (isValid) break;
	// 		if (registerACandidate % 100_000_000 == 0)
	// 		{
	// 			_log.WriteLine($"A={registerACandidate}");
	// 		}
	// 	}
	//
	// 	return registerACandidate.ToString();
	// }
}
