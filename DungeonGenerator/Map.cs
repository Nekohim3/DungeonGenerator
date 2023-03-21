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

        public Map(int roomMinWidth = 15, int roomMaxWidth = 15, int roomMinHeight = 30, int roomMaxHeight = 30, int roomMinCount = 5, int roomMaxCount = 10, int minDistanceBetweenRooms = 0, int maxDistanceBetweenRooms = 30, int minPassWidth = 1, int maxPassWidth = 3)
        {
            RoomMinWidth            = roomMinWidth;
            RoomMaxWidth            = roomMaxWidth;
            RoomMinHeight           = roomMinHeight;
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

        public void SetupParams(int roomMinWidth = 15, int roomMaxWidth = 15, int roomMaxHeight = 30, int roomMinHeight = 30, int roomMinCount = 5, int roomMaxCount = 10, int minDistanceBetweenRooms = 0, int maxDistanceBetweenRooms = 30, int minPassWidth = 1, int maxPassWidth = 3, int seed = -1)
        {
            RoomMinWidth            = roomMinWidth;
            RoomMaxWidth            = roomMaxWidth;
            RoomMinHeight           = roomMinHeight;
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

        private bool CheckRoom(Room r) => _rooms.All(_ => _.Rect.GetDistanceToRect(r.Rect) >= MinDistanceBetweenRooms) &&
                                          _rooms.Any(_ => _.Rect.GetDistanceToRect(r.Rect) <= MaxDistanceBetweenRooms) &&
                                          _rooms.All(_ => !_.Rect.IntersectsWith(r.Rect)) &&
                                          (_rooms.Count < 3 || GetNearestRooms(r).Count > 1);
        public List<Room> GetNearestRooms(Room r) => _rooms.Where(_ => r != _).Where(_ => r.Rect.GetDistanceToRect(_.Rect) <= MaxDistanceBetweenRooms).ToList();

        #endregion

        #region Pass

        private void GeneratePasses()
        {

        }

        #endregion

        #region PostProcessing

        private void GenerateMapArray()
        {
            foreach (var x in _rooms)
                FillRegionWith(x.Rect, 1);
            foreach (var x in _passes.SelectMany(_ => _.LineList))
                FillRegionWith(x, 1);
        }

        private void FillRegionWith(SKRectI r, byte fill) => FillRegionWith(new SKPointI(r.Left, r.Top), new SKPointI(r.Right, r.Bottom), fill);
        private void FillRegionWith(SKLineI  l, byte fill) => FillRegionWith(new SKPointI(l.Start.X, l.Start.Y), new SKPointI(l.End.X, l.End.Y), fill);
        private void FillRegionWith(SKPointI start, SKPointI end, byte fill)
        {
            for (var i = start.Y; i != end.Y; i += (end.Y - start.Y) / Math.Abs(end.Y - start.Y))
            for (var j = start.X; j != end.X; j += (end.X - start.X) / Math.Abs(end.X - start.X))
                _mapArray[i, j] = fill;

        }

        #endregion


        private int        GetRand(int          min, int   max) => min <= max ? _rand.Next(min,      max) : _rand.Next(max,           min);
        private int        GetRand(float        min, float max) => min <= max ? _rand.Next((int)min, (int)max) : _rand.Next((int)max, (int)min);

        #endregion
    }
}
