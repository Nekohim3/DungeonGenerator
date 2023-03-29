using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace DungeonViewer.Utils
{
    //public static class AStar1
    //{
    //    private static float                           Sqr = (float)Math.Sqrt(2);
    //    private static byte[,]                         _map;
    //    private static Dictionary<SKPointI, AStarNode> _nodes = new();
    //    private static int                             _width;
    //    private static int                             _height;
    //    public static List<AStarNode> FindPath(byte[,] map, SKPointI start, SKPointI end)
    //    {
            
    //        _map    = map;
    //        _height = _map.GetLength(0);
    //        _width = _map.GetLength(1);
    //        var startNode = new AStarNode(start, 0);
    //        _nodes.Add(startNode.Cost, startNode);
    //        return null;
    //    }

    //    private static void ExpandNearestNodes()
    //    {
    //        var node = _nodes.First();
    //        void CheckPoint(int x, int y, bool diag)
    //        {
    //            var p = new SKPointI(x, y);
    //            if (_map[p.X, p.Y] == 1)
    //            {
    //                var n = new AStarNode(p, node.Value.Cost + (diag ? Sqr : 1));
    //                _nodes.Add(n.Cost, n);
    //            }
    //        }
    //        CheckPoint(-1, -1, true);
    //        CheckPoint(0, -1, true);
    //        CheckPoint(1, -1, true);
    //        CheckPoint(1, 0, true);
    //        CheckPoint(1, 1, true);
    //        CheckPoint(0, 1, true);
    //        CheckPoint(-1, -1, true);
    //        CheckPoint(-1, 0, true);
    //    }

    //    private static void InsertToDict(AStarNode node)
    //    {
    //        for (var i = 0; i < _nodes.Count; i++)
    //        {
    //            if (_nodes.ElementAt(i).Value.Cost > node.Cost)
    //                _nodes.
    //        }
    //    }
    //}

    //public class AStarNode
    //{
    //    public SKPointI Point    { get; set; }
    //    public float    Cost     { get; set; }

    //    public AStarNode(SKPointI point, float cost)
    //    {
    //        Point = point;
    //        Cost  = cost;
    //    }
    //}
}
