using AdventOfCode;

using JetBrains.Annotations;

using Xunit.Abstractions;

namespace AdventOfCode2024.Puzzles.Day12;

using Position = (int y, int x);

public sealed class Day12Tests(ITestOutputHelper _output) : PuzzleTestsBase
{
	[Fact(DisplayName = "Sample 1")]
	public async Task Sample_1()
	{
		string input =
			"""
			RRRRIICCFF
			RRRRIICCCF
			VVRRRCCFFF
			VVRCCCJFFF
			VVVVCJJCFE
			VVIVCCJJEE
			VVIIICJJEE
			MIIIIIJJEE
			MIIISIJEEE
			MMMISSJEEE
			""";
		string[] lines = input.Split("\r\n");
		ulong result = await new SolverB().Solve(lines.ToAsyncEnumerable());
		_output.WriteLine($"Result: {result}");
	}

	[Fact(DisplayName = "Resolves and tests puzzle A")]
	public async Task Resolves_and_tests_puzzle_A()
	{
		await Runner.
			Puzzle(2024, day: 12, level: 1).
			SolveUsing<string, ulong, SolverA>().
			AssertingResult(line => line).
			Run(_output);
	}

	[Fact(DisplayName = "Resolves and tests puzzle B")]
	public async Task Resolves_and_tests_puzzle_B()
	{
		await Runner.
			Puzzle(2024, day: 12, level: 2).
			SolveUsing<string, ulong, SolverB>().
			AssertingResult(line => line).
			Run(_output);
	}
}

internal abstract class SolverBase : SolverWithArrayInput<string, ulong>
{
}


[UsedImplicitly]
internal sealed class SolverA : SolverBase
{
	protected override ulong Solve(string[] map)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		bool[][] visited = new bool[sizeY][];
		for (int y = 0; y < sizeY; ++y)
		{
			visited[y] = new bool[sizeX];
		}

		ulong result = 0;

		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			int square = 0, perimeter = 0;
			Traverse(y, x, map[y][x], map, visited, ref square, ref perimeter);
			result += ((ulong)square * (ulong)perimeter);
		}

		return result;
	}


	private static bool Traverse(int y, int x, char currentType, string[] map, bool[][] visited, ref int square, ref int perimeter)
	{
		char type = map[y][x];
		if (visited[y][x]) return type == currentType;

		if (type != currentType) return false;

		visited[y][x] = true;
		++square;

		int sizeY = map.Length;
		int sizeX = map[0].Length;

		if (y - 1 >= 0)
		{
			if (!Traverse(y - 1, x, currentType, map, visited, ref square, ref perimeter)) ++perimeter;
		}
		else
		{
			++perimeter;
		}

		if (y + 1 < sizeY)
		{
			if (!Traverse(y + 1, x, currentType, map, visited, ref square, ref perimeter)) ++perimeter;
		}
		else
		{
			++perimeter;
		}

		if (x - 1 >= 0)
		{
			if (!Traverse(y, x - 1, currentType, map, visited, ref square, ref perimeter)) ++perimeter;
		}
		else
		{
			++perimeter;
		}

		if (x + 1 < sizeX)
		{
			if (!Traverse(y, x + 1, currentType, map, visited, ref square, ref perimeter)) ++perimeter;
		}
		else
		{
			++perimeter;
		}

		return true;
	}
}


[UsedImplicitly]
internal sealed class SolverB : SolverBase
{
	protected override ulong Solve(string[] map)
	{
		int sizeY = map.Length;
		int sizeX = map[0].Length;

		bool[][] visited = new bool[sizeY][];
		for (int y = 0; y < sizeY; ++y)
		{
			visited[y] = new bool[sizeX];
		}

		Dictionary<Position, Region> regions = [];
		for (int y = 0; y < sizeY; ++y)
		for (int x = 0; x < sizeX; ++x)
		{
			Region region = new()
			{
				Type = map[y][x]
			};
			Traverse(y, x, map[y][x], region, map, visited, regions);
		}

		// top sides
		for (int y = 0; y < sizeY; ++y)
		{
			var currentRegion = regions[(y, 0)];
			int side = 0;
			for (int x = 0; x < sizeX; ++x)
			{
				var region = regions[(y, x)];
				if (currentRegion != region)
				{
					if (side > 0)
					{
						++currentRegion.Sides;
						side = 0;
					}
					currentRegion = region;
				}

				if (y - 1 < 0 || regions[(y - 1, x)] != currentRegion)
				{
					side++;
				}
				else if (side > 0)
				{
					++currentRegion.Sides;
					side = 0;
				}
			}
			if (side > 0) ++currentRegion.Sides;
		}

		// bottom sides
		for (int y = 0; y < sizeY; ++y)
		{
			var currentRegion = regions[(y, 0)];
			int side = 0;
			for (int x = 0; x < sizeX; ++x)
			{
				var region = regions[(y, x)];
				if (currentRegion != region)
				{
					if (side > 0)
					{
						++currentRegion.Sides;
						side = 0;
					}
					currentRegion = region;
				}

				if (y + 1 >= sizeY || regions[(y + 1, x)] != currentRegion)
				{
					side++;
				}
				else if (side > 0)
				{
					++currentRegion.Sides;
					side = 0;
				}
			}
			if (side > 0) ++currentRegion.Sides;
		}

		// left sides
		for (int x = 0; x < sizeX; ++x)
		{
			var currentRegion = regions[(0, x)];
			int side = 0;
			for (int y = 0; y < sizeY; ++y)
			{
				var region = regions[(y, x)];
				if (currentRegion != region)
				{
					if (side > 0)
					{
						++currentRegion.Sides;
						side = 0;
					}
					currentRegion = region;
				}

				if (x - 1 < 0 || regions[(y, x - 1)] != currentRegion)
				{
					side++;
				}
				else if (side > 0)
				{
					++currentRegion.Sides;
					side = 0;
				}
			}
			if (side > 0) ++currentRegion.Sides;
		}

		// right sides
		for (int x = 0; x < sizeX; ++x)
		{
			var currentRegion = regions[(0, x)];
			int side = 0;
			for (int y = 0; y < sizeY; ++y)
			{
				var region = regions[(y, x)];
				if (currentRegion != region)
				{
					if (side > 0)
					{
						++currentRegion.Sides;
						side = 0;
					}
					currentRegion = region;
				}

				if (x + 1 >= sizeX || regions[(y, x + 1)] != currentRegion)
				{
					side++;
				}
				else if (side > 0)
				{
					++currentRegion.Sides;
					side = 0;
				}
			}
			if (side > 0) ++currentRegion.Sides;
		}

		foreach (var region in regions.Values.Distinct())
		{
			Console.WriteLine($"{region.Type}: {region.Sides}");
		}

		return regions.Values.Distinct().Aggregate(0UL, (sum, r) => sum + (ulong)r.Sides * (ulong)r.Square);
	}

	private sealed class Region
	{
		public char Type = '-';
		public int Square = 0;
		public int Sides = 0;
	}


	private static void Traverse(int y, int x, char currentType, Region currentRegion, string[] map, bool[][] visited, Dictionary<Position, Region> regions)
	{
		char type = map[y][x];
		if (visited[y][x]) return;

		if (type != currentType) return;

		visited[y][x] = true;

		currentRegion.Square++;
		regions[(y, x)] = currentRegion;

		int sizeY = map.Length;
		int sizeX = map[0].Length;

		if (y - 1 >= 0)
		{
			Traverse(y - 1, x, currentType, currentRegion, map, visited, regions);
		}
		if (y + 1 < sizeY)
		{
			Traverse(y + 1, x, currentType, currentRegion, map, visited, regions);
		}
		if (x - 1 >= 0)
		{
			Traverse(y, x - 1, currentType, currentRegion, map, visited, regions);
		}
		if (x + 1 < sizeX)
		{
			Traverse(y, x + 1, currentType, currentRegion, map, visited, regions);
		}
	}
}
