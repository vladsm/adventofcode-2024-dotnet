using AdventOfCode;

namespace AdventOfCode2024;

public static class SiteRunnerExtensions
{
	public static SiteRunner.ISiteRunnerBuilder PuzzleFirstLevel(this SiteRunner runner, int year, int day)
	{
		return runner.Puzzle(year, day, 1);
	}

	public static SiteRunner.ISiteRunnerBuilder PuzzleSecondLevel(this SiteRunner runner, int year, int day)
	{
		return runner.Puzzle(year, day, 2);
	}

	public static SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> SolveFirstLevel<TEntry, TResult>(
		this SiteRunner runner,
		int year,
		int day
		)
	{
		return runner.Solve<TEntry, TResult>(year, day, 1);
	}

	public static SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> SolveSecondLevel<TEntry, TResult>(
		this SiteRunner runner,
		int year,
		int day
		)
	{
		return runner.Solve<TEntry, TResult>(year, day, 2);
	}

	public static SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> Solve<TEntry, TResult>(
		this SiteRunner runner,
		int year,
		int day,
		int level
		)
	{
		string solverTypeName = $"AdventOfCode.Year{year}.Solvers.Day{day}.Day{day:00}Level{level}Solver, AdventOfCode2021.Solvers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type? solverType = Type.GetType(solverTypeName, false);
		if (solverType is null)
		{
			throw new InvalidOperationException(
				$"Can not find solver type {solverTypeName} for the puzzle {year} day {day} level {level}"
				);
		}

		return runner.Puzzle(year, day, level).SolveUsing<TEntry, TResult>(solverType);
	}
}
