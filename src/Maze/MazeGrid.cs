using System.Collections.Generic;

namespace RoR2Maze.Maze
{
    /// <summary>
    /// Walls present on a single cell. A set bit means the wall EXISTS (blocks passage).
    /// </summary>
    [System.Flags]
    internal enum Wall : byte
    {
        None  = 0,
        North = 1 << 0,   // +y (visual up)
        East  = 1 << 1,   // +x (visual right)
        South = 1 << 2,   // -y (visual down)
        West  = 1 << 3,   // -x (visual left)
        All   = North | East | South | West,
    }

    /// <summary>
    /// Immutable perfect maze generated with iterative DFS (recursive backtracking).
    /// Coordinate system: (0,0) = bottom-left, (Width-1, Height-1) = top-right.
    /// Start = top-left corner, Exit = bottom-right corner.
    /// </summary>
    internal sealed class MazeGrid
    {
        internal readonly int Width;
        internal readonly int Height;

        /// <summary>Walls[x, y] — bitmask of walls present on cell (x, y).</summary>
        internal readonly Wall[,] Walls;

        /// <summary>Player spawn cell — top-left corner.</summary>
        internal readonly (int x, int y) Start;

        /// <summary>Exit trigger cell — bottom-right corner.</summary>
        internal readonly (int x, int y) Exit;

        /// <summary>Cells on the shortest path from Start to Exit, inclusive.</summary>
        internal readonly List<(int x, int y)> Solution;

        // Direction table: (dx, dy, wall-leaving-from, wall-on-neighbour-side)
        private static readonly (int dx, int dy, Wall from, Wall opp)[] s_dirs =
        {
            ( 0,  1, Wall.North, Wall.South),
            ( 1,  0, Wall.East,  Wall.West),
            ( 0, -1, Wall.South, Wall.North),
            (-1,  0, Wall.West,  Wall.East),
        };

        // ── Generation ──────────────────────────────────────────────────────────

        internal static MazeGrid Generate(int width, int height, int seed)
        {
            var rng   = new System.Random(seed);
            var walls = new Wall[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    walls[x, y] = Wall.All;

            var visited = new bool[width, height];
            var stack   = new Stack<(int, int)>();

            visited[0, height - 1] = true;       // carve starting from Start cell
            stack.Push((0, height - 1));

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Peek();

                // Collect unvisited neighbours in a shuffled order.
                var dirs = ShuffleDirs(rng);
                int chosen = -1;
                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dirs[i].dx;
                    int ny = cy + dirs[i].dy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                    {
                        chosen = i;
                        break;
                    }
                }

                if (chosen == -1)
                {
                    stack.Pop();
                    continue;
                }

                int nx2 = cx + dirs[chosen].dx;
                int ny2 = cy + dirs[chosen].dy;

                walls[cx,  cy ] &= ~dirs[chosen].from;
                walls[nx2, ny2] &= ~dirs[chosen].opp;
                visited[nx2, ny2] = true;
                stack.Push((nx2, ny2));
            }

            var start    = (0, height - 1);
            var exit     = (width - 1, 0);
            var solution = FindPath(walls, width, height, start, exit);

            return new MazeGrid(width, height, walls, start, exit, solution);
        }

        // Shuffle the four direction entries and return them (Fisher-Yates).
        private static (int dx, int dy, Wall from, Wall opp)[] ShuffleDirs(System.Random rng)
        {
            var d = (s_dirs.Clone() as (int, int, Wall, Wall)[])!;
            for (int i = 3; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (d[i], d[j]) = (d[j], d[i]);
            }
            return d;
        }

        // BFS to find the shortest path.
        private static List<(int x, int y)> FindPath(
            Wall[,] walls, int w, int h,
            (int x, int y) start, (int x, int y) goal)
        {
            var parent = new Dictionary<(int, int), (int, int)> { [start] = start };
            var queue  = new Queue<(int x, int y)>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                if ((cx, cy) == goal) break;

                foreach (var d in s_dirs)
                {
                    if ((walls[cx, cy] & d.from) != 0) continue;   // wall blocks passage
                    int nx = cx + d.dx, ny = cy + d.dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    if (parent.ContainsKey((nx, ny))) continue;
                    parent[(nx, ny)] = (cx, cy);
                    queue.Enqueue((nx, ny));
                }
            }

            var path = new List<(int, int)>();
            if (!parent.ContainsKey(goal)) return path;

            var cur = goal;
            while (cur != start) { path.Add(cur); cur = parent[cur]; }
            path.Add(start);
            path.Reverse();
            return path;
        }

        private MazeGrid(int w, int h, Wall[,] walls,
            (int, int) start, (int, int) exit, List<(int, int)> solution)
        {
            Width = w; Height = h; Walls = walls;
            Start = start; Exit = exit; Solution = solution;
        }
    }
}
