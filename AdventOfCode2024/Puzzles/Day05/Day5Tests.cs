using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day5;

public sealed class Day5Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			47|53
			97|13
			97|61
			97|47
			75|29
			61|13
			75|53
			29|13
			97|29
			53|29
			61|53
			97|53
			61|29
			47|13
			75|47
			97|75
			47|61
			75|61
			47|29
			75|13
			53|13
			
			75,47,61,53,29
			97,61,53,29,13
			75,29,13
			75,97,47,61,53
			61,13,29
			97,13,75,29,47
			""";
		string[] lines = input.Split("\r\n");
		int result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}


	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 5, level: 1).
			SolveUsing<string, int, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 5, level: 2).
			SolveUsing<string, int, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}


internal abstract class SolverBase : SolverWithArrayInput<string, int>
{
	protected override int Solve(string[] lines)
	{
		var (rules, updates) = ParseInput(lines);
		return Solve(new Rules(rules), updates);
	}

	protected abstract int Solve(Rules rules, IReadOnlyCollection<int[]> updates);

	protected static int GetMiddlePage(int[] update) =>
		update[update.Length / 2];


	private static (IReadOnlyCollection<Rule>, IReadOnlyCollection<int[]>) ParseInput(string[] lines)
	{
		List<Rule> rules = new();
		List<int[]> updates = new();

		bool parseRules = true;
		foreach (string line in lines)
		{
			if (line.Length == 0)
			{
				parseRules = false;
				continue;
			}
			if (parseRules)
			{
				AddRule(line, rules);
			}
			else
			{
				AddUpdate(line, updates);
			}
		}
		return (rules, updates);
	}

	private static void AddRule(string line, List<Rule> rules)
	{
		string[] parts = line.Split('|');
		var rule = new Rule(int.Parse(parts[0]), int.Parse(parts[1]));
		rules.Add(rule);
	}

	private static void AddUpdate(string line, List<int[]> updates)
	{
		int[] update = line.
			Split(',').
			Select(int.Parse).
			ToArray();
		updates.Add(update);
	}
}


internal record Rule(int Page, int Successor);


internal sealed class Rules
{
	private readonly Dictionary<int,HashSet<int>> _predecessors;

	public Rules(IReadOnlyCollection<Rule> rules)
	{
		_predecessors = rules.
			GroupBy(
				rule => rule.Successor,
				(successor, successorRules) => (successor, predecessors: successorRules.Select(rule => rule.Page).ToHashSet())
				).
			ToDictionary(s => s.successor, s => s.predecessors);
	}

	public bool IsRightOrderUpdate(int[] update)
	{
		var emptySet = new HashSet<int>();
		for (int i = 0; i < update.Length; i++)
		{
			int page = update[i];
			HashSet<int> pagePredecessors = _predecessors.GetValueOrDefault(page, emptySet);
			for (int j = i + 1; j < update.Length; j++)
			{
				int successor = update[j];
				if (pagePredecessors.Contains(successor)) return false;
			}
		}
		return true;
	}

	public int[] FixUpdate(int[] update)
	{
		var comparer = new PageOrderComparer(_predecessors);
		return update.OrderBy(page => page, comparer).ToArray();
	}

	private sealed class PageOrderComparer(Dictionary<int,HashSet<int>> _predecessors) : IComparer<int>
	{
		private static readonly HashSet<int> _emptySet = new();

		public int Compare(int x, int y)
		{
			if (x == y) return 0;
			HashSet<int> xPredecessors = _predecessors.GetValueOrDefault(x, _emptySet);
			return xPredecessors.Contains(y) ? 1 : -1;
		}
	}
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override int Solve(Rules rules, IReadOnlyCollection<int[]> updates) =>
		updates.
			Where(rules.IsRightOrderUpdate).
			Select(GetMiddlePage).
			Sum();
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override int Solve(Rules rules, IReadOnlyCollection<int[]> updates) =>
		updates.
			Where(update => !rules.IsRightOrderUpdate(update)).
			Select(rules.FixUpdate).
			Select(GetMiddlePage).
			Sum();
}
