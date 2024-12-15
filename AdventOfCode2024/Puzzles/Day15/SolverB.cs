namespace AdventOfCode2024.Puzzles.Day15;

using Position = (int y, int x);
using Map = char[][];

internal sealed class SolverB : SolverBase
{
	private const char BoxLeft = '[';
	private const char BoxRight = ']';

	protected override long Solve(Map map, string moves)
	{
		map = map.Select(expand).ToArray();
		return SolveExpanded(map, moves);

		static char[] expand(char[] line) => line.SelectMany(expandChar).ToArray();

		static IEnumerable<char> expandChar(char ch) =>
			ch switch
			{
				Wall => [Wall, Wall],
				Box => [BoxLeft, BoxRight],
				Empty => [Empty, Empty],
				Robot => [Robot, Empty],
				_ => []
			};
	}

	private long SolveExpanded(Map map, string moves)
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
			if (map[y][x] != BoxLeft) continue;
			result += (y * 100 + x);
		}
		return result;
	}

	private static int OrderByDirection(char direction, Position position) =>
		direction switch
		{
			Directions.Up => position.y,
			Directions.Down => -position.y,
			Directions.Left => position.x,
			Directions.Right => -position.x,
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction")
		};

	private static Position Move(Position robot, char direction, Map map)
	{
		(int dy, int dx) = DirectionToVector(direction);
		if (!CanMove(robot, dy, dx, map, out IEnumerable<BoxLocation> boxesToMove)) return robot;

		var boxes = boxesToMove.
			DistinctBy(b => b.Position).
			OrderBy(b => OrderByDirection(direction, b.Position));
		foreach (var box in boxes)
		{
			box.Move(dy, dx, map);
		}

		(int ry, int rx) = robot;
		map[ry][rx] = Empty;
		(int nry, int nrx) = (ry + dy, rx + dx);
		map[nry][nrx] = Robot;
		return (nry, nrx);
	}

	private static bool CanMove(Position from, int dy, int dx, Map map, out IEnumerable<BoxLocation> boxesToMove)
	{
		List<BoxLocation> boxes = new();
		boxesToMove = boxes;
		return dy == 0 ?
			CanMoveHorizontal(from, dx, map, boxes) :
			CanMoveVertical(from, dy, map, boxes);
	}

	private static bool CanMoveVertical(Position from, int dy, Map map, List<BoxLocation> boxesToMove)
	{
		(int y, int x) = from;
		(int ny, int nx) = (y + dy, x);
		char current = map[y][x];
		char next = map[ny][nx];

		if (current == Robot)
		{
			if (next is Wall) return false;
			if (next is Empty) return true;
			return CanMoveVertical((ny, nx), dy, map, boxesToMove);
		}
		if (current is BoxLeft)
		{
			char next1 = map[ny][nx + 1];
			if (next is Wall || next1 is Wall) return false;
			boxesToMove.Add(new BoxLocation((y, x), (y, x + 1)));
			if (next is Empty && next1 is Empty) return true;
			return CanMoveVertical((ny, nx), dy, map, boxesToMove) && CanMoveVertical((ny, nx + 1), dy, map, boxesToMove);
		}
		if (current is BoxRight)
		{
			char next1 = map[ny][nx - 1];
			if (next is Wall || next1 is Wall) return false;
			boxesToMove.Add(new BoxLocation((y, x), (y, x - 1)));
			if (next is Empty && next1 is Empty) return true;
			return CanMoveVertical((ny, nx), dy, map, boxesToMove) && CanMoveVertical((ny, nx - 1), dy, map, boxesToMove);
		}
		return true;
	}

	private static bool CanMoveHorizontal(Position from, int dx, Map map, List<BoxLocation> boxesToMove)
	{
		(int y, int x) = from;
		(int ny, int nx) = (y, x + dx);
		char current = map[y][x];
		char next = map[ny][nx];

		if (current == Robot)
		{
			if (next is Wall) return false;
			if (next is Empty) return true;
			return CanMoveHorizontal((ny, nx), dx, map, boxesToMove);
		}
		if (current is BoxLeft)
		{
			next = map[ny][++nx];
			if (next is Wall) return false;
			boxesToMove.Add(new BoxLocation((y, x), (y, x + 1)));
			if (next is Empty) return true;
			return CanMoveHorizontal((ny, nx), dx, map, boxesToMove);
		}
		if (current is BoxRight)
		{
			next = map[ny][--nx];
			if (next is Wall) return false;
			boxesToMove.Add(new BoxLocation((y, x), (y, x - 1)));
			if (next is Empty) return true;
			return CanMoveHorizontal((ny, nx), dx, map, boxesToMove);
		}
		return true;
	}


	private sealed class BoxLocation
	{
		private readonly Position _position1;
		private readonly Position _position2;

		public BoxLocation(Position position1, Position position2)
		{
			_position1 = position1;
			_position2 = position2;
		}

		public Position Position => _position1.x < _position2.x ? _position1 : _position2;

		public void Move(int dy, int dx, Map map)
		{
			var (y1, x1) = _position1;
			var (y2, x2) = _position2;
			char ch1 = map[y1][x1];
			char ch2 = map[y2][x2];
			map[y1][x1] = Empty;
			map[y2][x2] = Empty;
			map[y1 + dy][x1 + dx] = ch1;
			map[y2 + dy][x2 + dx] = ch2;
		}
	}
}
