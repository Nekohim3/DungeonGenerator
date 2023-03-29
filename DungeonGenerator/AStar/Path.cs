using DungeonViewer.Utils.AStar;
using SkiaSharp;

namespace DungeonGenerator.AStar;

public class AStar
{
    private const    int        MaxNeighbours = 8;
    private readonly PathNode[] neighbours    = new PathNode[MaxNeighbours];

    private readonly int                                 maxSteps;
    private readonly IBinaryHeap<Vector2Int, PathNode>   frontier;
    private readonly HashSet<Vector2Int>                 ignoredPositions;
    private readonly List<Vector2Int>                    output;
    private readonly IDictionary<Vector2Int, Vector2Int> links;
    private          List<PathNode>                      _allNodes;

    /// <summary>
    /// Creation of new path finder.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public AStar(int maxSteps = int.MaxValue, int initialCapacity = 0)
    {
        if (maxSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSteps));
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));

        this.maxSteps    = maxSteps;
        frontier         = new BinaryHeap<Vector2Int, PathNode>(a => a.Position, initialCapacity);
        ignoredPositions = new HashSet<Vector2Int>(initialCapacity);
        output           = new List<Vector2Int>(initialCapacity);
        links            = new Dictionary<Vector2Int, Vector2Int>(initialCapacity);
        _allNodes        = new List<PathNode>();
    }

    public List<Vector2Int>? Calculate(SKPointI start, SKPointI end, byte[,] map)
    {
        var lst = new List<Vector2Int>();
        for (var i = 0; i < map.GetLength(1); i++)
        for (var j = 0; j < map.GetLength(0); j++)
            if (map[j, i] == 0)
                lst.Add(new Vector2Int(j, i));

        return Calculate(new Vector2Int(start), new Vector2Int(end), lst);
    }

    /// <summary>
    /// Calculate a new path between two points.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public List<Vector2Int>? Calculate(Vector2Int start, Vector2Int target, List<Vector2Int> obstacles)
    {
        var path = new List<Vector2Int>();
        if (obstacles == null)
            throw new ArgumentNullException(nameof(obstacles));

        if (!GenerateNodes(start, target, obstacles))
            return null;

        output.Clear();
        output.Add(target);

        while (links.TryGetValue(target, out target))
            output.Add(target);
        path = output;
        return path;
    }

    public float GetPathDistance(SKPointI start, SKPointI end, Tile[,] map)
    {
        var lst = new List<Vector2Int>();
        for (var i = 0; i < map.GetLength(1); i++)
        for (var j = 0; j < map.GetLength(0); j++)
            if (!map[j, i].Passable)
                lst.Add(new Vector2Int(j, i));

        var l = Calculate(new Vector2Int(start), new Vector2Int(end), lst);
        if (l == null || l.Count == 0)
            return -1;
        return (float)_allNodes.FirstOrDefault(_ => _.Position.X == end.X && _.Position.Y == end.Y).TraverseDistance;
    }

    private bool GenerateNodes(Vector2Int start, Vector2Int target, IReadOnlyCollection<Vector2Int> obstacles)
    {
        _allNodes.Clear();
        frontier.Clear();
        ignoredPositions.Clear();
        links.Clear();
        frontier.Enqueue(new PathNode(start, target, 0));
        ignoredPositions.UnionWith(obstacles);
        var step = 0;
        while (frontier.Count > 0 && step++ <= maxSteps)
        {
            var current = frontier.Dequeue();
            ignoredPositions.Add(current.Position);

            if (current.Position.Equals(target))
                return true;

            GenerateFrontierNodes(current, target);
        }

        // All nodes analyzed - no path detected.
        return false;
    }

    private void GenerateFrontierNodes(PathNode parent, Vector2Int target)
    {
        neighbours.Fill(parent, target);
        foreach (var newNode in neighbours)
        {
            _allNodes.Add(newNode);
            // Position is already checked or occupied by an obstacle.
            if (ignoredPositions.Contains(newNode.Position))
                continue;

            // Node is not present in queue.
            if (!frontier.TryGet(newNode.Position, out var existingNode))
            {
                frontier.Enqueue(newNode);
                links[newNode.Position] = parent.Position;
            }

            // Node is present in queue and new optimal path is detected.
            else if (newNode.TraverseDistance < existingNode.TraverseDistance)
            {
                frontier.Modify(newNode);
                links[newNode.Position] = parent.Position;
            }
        }
    }
}
