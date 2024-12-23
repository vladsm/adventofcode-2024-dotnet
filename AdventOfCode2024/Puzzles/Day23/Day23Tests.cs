using AdventOfCode;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day23;

using Link = (string computer1, string computer2);
using LinkMap = Dictionary<string, HashSet<string>>;

public sealed class Day23Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			kh-tc
			qp-kh
			de-cg
			ka-co
			yn-aq
			qp-ub
			cg-tb
			vc-aq
			tb-ka
			wh-tc
			yn-cg
			kh-ub
			ta-co
			de-co
			tc-td
			tb-wq
			wh-td
			ta-ka
			td-qp
			aq-cg
			wq-ub
			ub-vc
			de-ta
			wq-aq
			wq-vc
			wh-yn
			ka-de
			kh-ta
			co-tc
			wh-qp
			tb-vc
			td-yn
			""";

		string[] lines = input.Split("\r\n");
		long result = await new SolverA().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 23, level: 1).
			SolveUsing<string, long, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 23, level: 2).
			SolveUsing<string, long, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, long>
{
	protected override long Solve(string[] lines)
	{
		Link[] links = lines.Select(parse).ToArray();
		return Solve(links);

		static Link parse(string line)
		{
			string[] parts = line.Split('-');
			return (parts[0], parts[1]);
		}
	}

	protected abstract long Solve(Link[] links);
}


internal sealed class SolverA : SolverBase
{
	protected override long Solve(Link[] links)
	{
		var allComputers = links.SelectMany<Link, string>(l => [l.computer1, l.computer2]).ToHashSet();

		LinkMap map = links.
			Concat(links.Select(l => (Link)(l.computer2, l.computer1))).
			GroupBy(l => l.computer1).
			ToDictionary(g => g.Key, g => g.Select(l => l.computer2).ToHashSet());

		HashSet<LinkedComputers> resultSet = new();
		foreach (var computer1 in allComputers)
		foreach (var computer2 in allComputers)
		foreach (var computer3 in allComputers)
		{
			if (computer1 == computer2 || computer1 == computer3 || computer2 == computer3) continue;
			if (!map.TryGetValue(computer1, out var linked1)) continue;
			if (!linked1.Contains(computer2) || !linked1.Contains(computer3)) continue;
			if (!map.TryGetValue(computer2, out var linked2)) continue;
			if (!linked2.Contains(computer3)) continue;
			if (computer1[0] != 't' && computer2[0] != 't' && computer3[0] != 't') continue;
			resultSet.Add(new LinkedComputers([computer1, computer2, computer3]));
		}

		return resultSet.Count;
	}

	private sealed class LinkedComputers
	{
		private readonly string[] _computers;

		public LinkedComputers(IEnumerable<string> computers)
		{
			_computers = computers.OrderBy(c => c).ToArray();
		}

		private bool Equals(LinkedComputers other)
		{
			var computers = _computers;
			var otherComputers = other._computers;
			if (computers.Length != otherComputers.Length) return false;
			for (int i = 0; i < computers.Length; ++i)
			{
				if (computers[i] != otherComputers[i]) return false;
			}
			return true;
		}

		public override bool Equals(object? obj) =>
			ReferenceEquals(this, obj) || obj is LinkedComputers other && Equals(other);

		public override int GetHashCode()
		{
			HashCode hc = new();
			foreach (string computer in _computers)
			{
				hc.Add(computer);
			}
			return hc.ToHashCode();
		}
	}
}


internal sealed class SolverB : SolverBase
{
	protected override long Solve(Link[] links)
	{
		throw new NotImplementedException();
	}
}
