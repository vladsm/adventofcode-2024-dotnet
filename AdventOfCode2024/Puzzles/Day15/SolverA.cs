namespace AdventOfCode2024.Puzzles.Day15;

using Map = char[][];
using Position = (int y, int x);

internal sealed class SolverA : SolverBase
{
	protected override long Solve(char[][] map, string moves)
	{
		Position robot = FindRobot(map);
		foreach (char direction in moves)
		{
			robot = Move(robot, direction, map);
		}

		long result = 0;
		for (int y = 0; y < map.Length; ++y)
		for (int x = 0; x < map[y].Length; ++x)
		{
			if (map[y][x] != Box) continue;
			result += (y * 100 + x);
		}
		return result;
	}

	private static Position Move(Position robot, char direction, Map map)
	{
		(int dy, int dx) = DirectionToVector(direction);
		if (!CanMove(robot, dy, dx, map, out var lastBoxToMove)) return robot;

		if (lastBoxToMove is not null)
		{
			(int by, int bx) = lastBoxToMove.Value;
			map[by + dy][bx + dx] = Box;
		}

		(int ry, int rx) = robot;
		map[ry][rx] = Empty;
		(int nry, int nrx) = (ry + dy, rx + dx);
		map[nry][nrx] = Robot;
		return (nry, nrx);
	}

	private static bool CanMove(Position from, int dy, int dx, Map map, out Position? lastBoxToMove)
	{
		lastBoxToMove = null;
		while (true)
		{
			from = (from.y + dy, from.x + dx);
			(int y, int x) = from;
			if (map[y][x] == Wall)
			{
				lastBoxToMove = null;
				return false;
			}
			if (map[y][x] != Box) break;
			lastBoxToMove = from;
		}
		return true;
	}
}
