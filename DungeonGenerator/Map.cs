using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonGenerator.Extensions;
using SkiaSharp;

namespace DungeonGenerator
{
    public class Map
    {
        #region Properties

        private List<Room> _rooms  = new();
        private List<Pass> _passes = new();
        private byte[,]    _mapArray;

        public int RoomMinWidth            { get; set; }
        public int RoomMaxWidth            { get; set; }
        public int RoomMaxHeight           { get; set; }
        public int RoomMinHeight           { get; set; }
        public int RoomMinCount            { get; set; }
        public int RoomMaxCount            { get; set; }
        public int MinDistanceBetweenRooms { get; set; }
        public int MaxDistanceBetweenRooms { get; set; }
        public int MinPassWidth            { get; set; }
        public int MaxPassWidth            { get; set; }

        private int    _seed;
        private Random _rand;

        #endregion

        #region Ctor

        public Map(int roomMinWidth = 15, int roomMinHeight = 15, int roomMaxWidth = 30, int roomMaxHeight = 30, int roomMinCount = 5, int roomMaxCount = 10, int minDistanceBetweenRooms = 0, int maxDistanceBetweenRooms = 30, int minPassWidth = 1, int maxPassWidth = 3)
        {
            RoomMinWidth            = roomMinWidth;
            RoomMinHeight           = roomMinHeight;
            RoomMaxWidth            = roomMaxWidth;
            RoomMaxHeight           = roomMaxHeight;
            RoomMinCount            = roomMinCount;
            RoomMaxCount            = roomMaxCount;
            MinDistanceBetweenRooms = minDistanceBetweenRooms;
            MaxDistanceBetweenRooms = maxDistanceBetweenRooms;
            MinPassWidth            = minPassWidth;
            MaxPassWidth            = maxPassWidth;
        }

        #endregion

        #region Funcs
        public byte[,]? GenerateMap(int seed = -1)
        {
            _seed                   = seed == -1 ? new Random().Next(int.MinValue, int.MaxValue) : seed;
            _rand                   = new Random(_seed);
            if (GenerateRooms())
            {
                GeneratePasses();
                GenerateMapArray();
                return _mapArray;
            }

            return null;
        }

        public void SetupParams(int roomMinWidth = 15, int roomMinHeight = 15, int roomMaxWidth = 30,  int roomMaxHeight = 30, int roomMinCount = 5, int roomMaxCount = 10, int minDistanceBetweenRooms = 0, int maxDistanceBetweenRooms = 30, int minPassWidth = 1, int maxPassWidth = 3, int seed = -1)
        {
            RoomMinWidth            = roomMinWidth;
            RoomMinHeight           = roomMinHeight;
            RoomMaxWidth            = roomMaxWidth;
            RoomMaxHeight           = roomMaxHeight;
            RoomMinCount            = roomMinCount;
            RoomMaxCount            = roomMaxCount;
            MinDistanceBetweenRooms = minDistanceBetweenRooms;
            MaxDistanceBetweenRooms = maxDistanceBetweenRooms;
            MinPassWidth            = minPassWidth;
            MaxPassWidth            = maxPassWidth;
        }

        #region Room

        private bool GenerateRooms()
        {
            _rooms.Clear();
            var roomCount = GetRand(RoomMinCount, RoomMaxCount);
            while (_rooms.Count < roomCount)
            {
                if (_rooms.Count == 0)
                    _rooms.Add(new Room(0, 0, GetRand(RoomMinWidth, RoomMaxWidth), GetRand(RoomMinHeight, RoomMaxHeight), (_rooms.Count + 1).ToString()));
                else
                {
                    var room = RepeatableCode.RepeatResult(() =>
                    {
                        var room = GenerateRoom((_rooms.Count + 1).ToString());
                        return CheckRoom(room) ? room : null;
                    }, 100000);

                    if (room == null)
                        return false;

                    _rooms.Add(room);
                }

                NormalizeRooms();
            }

            return true;
        }

        private Room GenerateRoom(string name)
        {
            var area = GetGenerateArea();
            var r = new SKRectI { Left = GetRand(area.Left, area.Right), Top = GetRand(area.Top, area.Bottom) };
            r.Right = r.Left + GetRand(RoomMinWidth, RoomMaxWidth);
            r.Bottom = r.Top + GetRand(RoomMinHeight, RoomMaxHeight);
            return new Room(r, name);
        }

        private SKRectI GetGenerateArea()
        {
            var r = _rooms[0].Rect;
            foreach (var x in _rooms)
                r.Union(x.Rect);
            return r.ExpandAll(MaxDistanceBetweenRooms).ExpandLeft(RoomMaxWidth).ExpandTop(RoomMaxHeight);
        }

        private void NormalizeRooms()
        {
            var minX = _rooms.Min(_ => _.Rect.Left);
            var minY = _rooms.Min(_ => _.Rect.Top);
            foreach (var x in _rooms)
                x.Rect.Offset(-minX, -minY);
        }

        private SKRectI GetArea()
        {
            var rect = new SKRectI();
            foreach (var x in _rooms)
            {
                if (rect.Right < x.Rect.Right)
                    rect.Right = x.Rect.Right;
                if (rect.Bottom < x.Rect.Bottom)
                    rect.Bottom = x.Rect.Bottom;
            }

            return rect;
        }

        private bool CheckRoom(Room r) => _rooms.All(_ => _.Rect.GetDistanceToRect(r.Rect) >= MinDistanceBetweenRooms) &&
                                          _rooms.Any(_ => _.Rect.GetDistanceToRect(r.Rect) <= MaxDistanceBetweenRooms) &&
                                          _rooms.All(_ => !_.Rect.IntersectsWith(r.Rect)) &&
                                          (_rooms.Count < 3 || GetNearestRooms(r).Count > 1);
        public List<Room> GetNearestRooms(Room r) => _rooms.Where(_ => r != _).Where(_ => r.Rect.GetDistanceToRect(_.Rect) <= MaxDistanceBetweenRooms).ToList();

        #endregion

        #region Pass

        private void GeneratePasses()
        {
            _passes.Clear();
            foreach (var x in _rooms)
            {
                foreach (var c in GetNearestRooms(x))
                {
                    if(PassExist(x, c)) continue;
                    if (c.Rect.Right > x.Rect.Left + 2 && c.Rect.Left < x.Rect.Right - 2) // straight pass vertical
                    {
                        var passX = GetRand(Math.Max(x.Rect.Left, c.Rect.Left) + 1, Math.Min(x.Rect.Right, c.Rect.Right) - 1);
                        _passes.Add(x.Rect.Bottom < c.Rect.Top
                                       ? new Pass(x, c, new SKRectI(passX, x.Rect.Bottom, passX, c.Rect.Top - 1))
                                       : new Pass(x, c, new SKRectI(passX, x.Rect.Top                       - 1, passX, c.Rect.Bottom)));
                    }
                    else if (c.Rect.Bottom > x.Rect.Top + 2 && c.Rect.Top < x.Rect.Bottom - 2) // straight pass horizontal
                    {
                        var passY = GetRand(Math.Max(x.Rect.Top, c.Rect.Top) + 1, Math.Min(x.Rect.Bottom, c.Rect.Bottom) - 1);
                        _passes.Add(x.Rect.Right < c.Rect.Left
                                       ? new Pass(x, c, new SKRectI(x.Rect.Right, passY, c.Rect.Left - 1, passY))
                                       : new Pass(x, c, new SKRectI(x.Rect.Left                      - 1, passY, c.Rect.Right, passY)));
                    }
                    else
                    {
                        var line = new SKLineI(x.Rect.MidX, x.Rect.MidY, c.Rect.MidX, c.Rect.MidY);
                        var xp = x.Rect.GetRectIntersectType(line);
                        var cp = c.Rect.GetRectIntersectType(line);
                        var lines = new List<SKRectI>();
                        if (xp.IsOpposite(cp))
                        {
                            var output = GetRand(x.Rect.GetRectSidePos(xp.side.i) - ((float)xp.side.i).CompareTo(1.1f), xp.line.i % 2 == 0 ? x.Rect.MidY : x.Rect.MidX);
                            var input = GetRand(c.Rect.GetRectSidePos(cp.side.i) - ((float)cp.side.i).CompareTo(1.1f), cp.line.i % 2 == 0 ? c.Rect.MidY : c.Rect.MidX);
                            var br = GetRand((xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2) + ((xp.line.i < 2 ? x : c).Rect.GetRectSidePos(xp.line.i % 2) - (xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2)) * (1 / (float)3),
                                             (xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2) + ((xp.line.i < 2 ? x : c).Rect.GetRectSidePos(xp.line.i % 2) - (xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2)) * (2 / (float)3));
                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? x.Rect.GetRectSidePos(xp.line.i) - (xp.line.i == 0 ? 1 : 0) : output, xp.line.i % 2 == 1 ? x.Rect.GetRectSidePos(xp.line.i) - (xp.line.i == 1 ? 1 : 0) : output, xp.line.i % 2 == 0 ? br : output, xp.line.i % 2 == 1 ? br : output));
                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? br : output, xp.line.i % 2 == 1 ? br : output, xp.line.i % 2 == 0 ? br : input, xp.line.i % 2 == 1 ? br : input));
                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? br : input, xp.line.i % 2 == 1 ? br : input, xp.line.i % 2 == 0 ? c.Rect.GetRectSidePos(cp.line.i) - (cp.line.i < 2 ? 1 : 0) : input, xp.line.i % 2 == 1 ? c.Rect.GetRectSidePos(cp.line.i) - (cp.line.i < 2 ? 1 : 0) : input));
                            _passes.Add(new Pass(x, c, lines.ToArray()));
                        }
                        else
                        {
                            
                        }
                    }
                }
            }

            foreach (var x in _passes)
            {
                var pw = GetRand(MinPassWidth, MaxPassWidth) - 1;
                if (pw > 0)
                {
                    var side = Convert.ToBoolean(GetRand(0, 2));
                    switch (x.LineList.Count)
                    {
                        case 1:
                            if (x.LineList[0].Left == x.LineList[0].Right)
                                x.LineList[0] = x.LineList[0] with { Left = x.LineList[0].Left - pw / 2 - (!side ? pw % 2 : 0), Right = x.LineList[0].Right + pw / 2 + (side ? pw % 2 : 0) };
                            else
                                x.LineList[0] = x.LineList[0] with { Top = x.LineList[0].Top - pw / 2 - (!side ? pw % 2 : 0), Bottom = x.LineList[0].Bottom + pw / 2 + (side ? pw % 2 : 0) };
                            break;
                        case 2:
                            break;
                        case 3:
                            if (x.LineList[0].Left == x.LineList[0].Right)
                            {
                                x.LineList[0] = x.LineList[0] with { Left = x.LineList[0].Left - pw / 2 - (x.LineList[0].Left > x.LineList[2].Left ? pw % 2 : 0), Right = x.LineList[0].Right   + pw / 2 + (x.LineList[0].Right < x.LineList[2].Right ? pw % 2 : 0) };
                                x.LineList[1] = x.LineList[1] with { Top = x.LineList[1].Top   - pw / 2 - (!side ? pw                                   % 2 : 0), Bottom = x.LineList[1].Bottom + pw / 2 + (side ? pw                                      % 2 : 0) };
                                x.LineList[2] = x.LineList[2] with { Left = x.LineList[2].Left - pw / 2 - (x.LineList[0].Left < x.LineList[2].Left ? pw % 2 : 0), Right = x.LineList[2].Right   + pw / 2 + (x.LineList[0].Right > x.LineList[2].Right ? pw % 2 : 0) };
                            }
                            else
                            {
                                x.LineList[0] = x.LineList[0] with { Top = x.LineList[0].Top   - pw / 2 - (x.LineList[0].Top > x.LineList[2].Top ? pw % 2 : 0), Bottom = x.LineList[0].Bottom + pw / 2 + (x.LineList[0].Bottom < x.LineList[2].Bottom ? pw % 2 : 0) };
                                x.LineList[1] = x.LineList[1] with { Left = x.LineList[1].Left - pw / 2 - (!side ? pw                                 % 2 : 0), Right = x.LineList[1].Right   + pw / 2 + (side ? pw                                        % 2 : 0) };
                                x.LineList[2] = x.LineList[2] with { Top = x.LineList[2].Top   - pw / 2 - (x.LineList[0].Top < x.LineList[2].Top ? pw % 2 : 0), Bottom = x.LineList[2].Bottom + pw / 2 + (x.LineList[0].Bottom > x.LineList[2].Bottom ? pw % 2 : 0) };
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool PassExist(Room r1, Room r2) => _passes.Count(_ => (_.StartRoom == r1 && _.EndRoom == r2) || (_.StartRoom == r2 && _.EndRoom == r1)) != 0;

        #endregion

        #region PostProcessing

        private void GenerateMapArray()
        {
            var area = GetArea();
            _mapArray = new byte[area.Width + 1, area.Height + 1];
            foreach (var x in _rooms)
                FillRegionWith(x.Rect, 1);
            foreach (var x in _passes.SelectMany(_ => _.LineList))
                FillRegionWith(x, 1);
        }

        private void FillRegionWith(SKRectI r, byte fill) => FillRegionWith(new SKPointI(r.Left, r.Top), new SKPointI(r.Right, r.Bottom), fill);
        private void FillRegionWith(SKLineI  l, byte fill) => FillRegionWith(new SKPointI(l.Start.X, l.Start.Y), new SKPointI(l.End.X, l.End.Y), fill);

        private void FillRegionWith(SKPointI start, SKPointI end, byte fill)
        {
            var ys = start.Y < end.Y ? start.Y : end.Y;
            var ye = start.Y < end.Y ? end.Y : start.Y;
            var xs = start.X < end.X ? start.X : end.X;
            var xe = start.X < end.X ? end.X : start.X;
            for (var i = ys; i <= ye; i++)
            for (var j = xs; j <= xe; j++)
                _mapArray[j, i] = fill;
        }

        #endregion


        private int        GetRand(int          min, int   max) => min <= max ? _rand.Next(min,      max) : _rand.Next(max,           min);
        private int        GetRand(float        min, float max) => min <= max ? _rand.Next((int)min, (int)max) : _rand.Next((int)max, (int)min);

        #endregion
    }
}
