using DungeonGenerator.Extensions;
using DungeonViewer.Utils.AStar;
using SkiaSharp;

namespace DungeonGenerator;

public class Map
{
    #region Properties

    private List<Room> _rooms  = new();
    private List<Pass> _passes = new();
    private Tile[,]    _mapArray;

    public int               RoomMinWidth              { get; set; }
    public int               RoomMaxWidth              { get; set; }
    public int               RoomMaxHeight             { get; set; }
    public int               RoomMinHeight             { get; set; }
    public int               RoomMinCount              { get; set; }
    public int               RoomMaxCount              { get; set; }
    public int               MinDistanceBetweenRooms   { get; set; }
    public int               MaxDistanceBetweenRooms   { get; set; }
    public int               MinPassWidth              { get; set; }
    public int               MaxPassWidth              { get; set; }
    public int               LongPathDifferencePercent { get; set; }
    public List<ElementRate> ElementRateList           { get; set; }


    private int    _seed;
    private Random _rand;

    #endregion

    #region Ctor

    public Map(List<ElementRate> elementGenerateList, int roomMinWidth = 15, int roomMinHeight = 15, int roomMaxWidth = 30, int roomMaxHeight = 30, int roomMinCount = 5, int roomMaxCount = 10, int minDistanceBetweenRooms = 0, int maxDistanceBetweenRooms = 30, int minPassWidth = 1, int maxPassWidth = 3, int passPercent = 50)
    {
        RoomMinWidth              = roomMinWidth;
        RoomMinHeight             = roomMinHeight;
        RoomMaxWidth              = roomMaxWidth;
        RoomMaxHeight             = roomMaxHeight;
        RoomMinCount              = roomMinCount;
        RoomMaxCount              = roomMaxCount;
        MinDistanceBetweenRooms   = minDistanceBetweenRooms;
        MaxDistanceBetweenRooms   = maxDistanceBetweenRooms;
        MinPassWidth              = minPassWidth;
        MaxPassWidth              = maxPassWidth;
        LongPathDifferencePercent = passPercent;
        ElementRateList           = elementGenerateList;
    }

    #endregion

    #region Funcs

    public Tile[,]? GenerateMap(int seed = -1)
    {
        _seed = seed == -1 ? new Random().Next(int.MinValue, int.MaxValue) : seed;
        _rand = new Random(_seed);
        if (GenerateRooms())
        {
            GeneratePasses();
            ClearPasses();
            GenerateElements();
            GenerateMapArray();

            return _mapArray;
        }

        return null;
    }

    public void SetupParams(int roomMinWidth = 15, int roomMinHeight = 15, int roomMaxWidth = 30, int roomMaxHeight = 30, int roomMinCount = 5, int roomMaxCount = 10, int minDistanceBetweenRooms = 0, int maxDistanceBetweenRooms = 30, int minPassWidth = 1, int maxPassWidth = 3, int seed = -1)
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
        var roomCount = _rand.GetRand(RoomMinCount, RoomMaxCount);
        while (_rooms.Count < roomCount)
        {
            if (_rooms.Count == 0)
                _rooms.Add(new Room(0, 0, _rand.GetRand(RoomMinWidth, RoomMaxWidth), _rand.GetRand(RoomMinHeight, RoomMaxHeight), (_rooms.Count + 1).ToString()));
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
        var r    = new SKRectI {Left = _rand.GetRand(area.Left, area.Right), Top = _rand.GetRand(area.Top, area.Bottom)};
        r.Right  = r.Left + _rand.GetRand(RoomMinWidth,  RoomMaxWidth);
        r.Bottom = r.Top  + _rand.GetRand(RoomMinHeight, RoomMaxHeight);
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
                                      _rooms.All(_ => !_.Rect.IntersectsWith(r.Rect))                              &&
                                      (_rooms.Count < 3 || GetNearestRooms(r).Count > 1);

    public List<Room> GetNearestRooms(Room r) => _rooms.Where(_ => r != _).Where(_ => r.Rect.GetDistanceToRect(_.Rect) <= MaxDistanceBetweenRooms).ToList();

    #endregion

    #region Pass

    private void GeneratePasses()
    {
        var area = GetArea();
        _passes.Clear();
        foreach (var x in _rooms)
        {
            foreach (var c in GetNearestRooms(x))
            {
                if (PassExist(x, c))
                    continue;
                var lines = new List<SKRectI>();
                if (c.Rect.Right > x.Rect.Left + 2 && c.Rect.Left < x.Rect.Right - 2) // straight pass vertical
                {
                    var passX = _rand.GetRand(Math.Max(x.Rect.Left, c.Rect.Left) + 1, Math.Min(x.Rect.Right, c.Rect.Right) - 1);
                    lines.Add(x.Rect.Bottom < c.Rect.Top
                                  ? new SKRectI(passX, x.Rect.Bottom, passX, c.Rect.Top - 1).Standardized
                                  : new SKRectI(passX, x.Rect.Top                       - 1, passX, c.Rect.Bottom).Standardized);
                }
                else if (c.Rect.Bottom > x.Rect.Top + 2 && c.Rect.Top < x.Rect.Bottom - 2) // straight pass horizontal
                {
                    var passY = _rand.GetRand(Math.Max(x.Rect.Top, c.Rect.Top) + 1, Math.Min(x.Rect.Bottom, c.Rect.Bottom) - 1);
                    lines.Add(x.Rect.Right < c.Rect.Left
                                  ? new SKRectI(x.Rect.Right, passY, c.Rect.Left - 1, passY).Standardized
                                  : new SKRectI(x.Rect.Left                      - 1, passY, c.Rect.Right, passY).Standardized);
                }
                else
                {
                    var line = new SKLineI(x.Rect.MidX, x.Rect.MidY, c.Rect.MidX, c.Rect.MidY);
                    var xp   = x.Rect.GetRectIntersectType(line);
                    var cp   = c.Rect.GetRectIntersectType(line);
                    if (xp.IsOpposite(cp))
                    {
                        var output = _rand.GetRand(x.Rect.GetRectSidePos(xp.side.i) - ((float) xp.side.i).CompareTo(1.1f), xp.line.i % 2 == 0 ? x.Rect.MidY : x.Rect.MidX);
                        var input  = _rand.GetRand(c.Rect.GetRectSidePos(cp.side.i) - ((float) cp.side.i).CompareTo(1.1f), cp.line.i % 2 == 0 ? c.Rect.MidY : c.Rect.MidX);
                        var br = _rand.GetRand((xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2) + ((xp.line.i < 2 ? x : c).Rect.GetRectSidePos(xp.line.i % 2) - (xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2)) * (1 / (float) 3),
                                               (xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2) + ((xp.line.i < 2 ? x : c).Rect.GetRectSidePos(xp.line.i % 2) - (xp.line.i < 2 ? c : x).Rect.GetRectSidePos(xp.line.i % 2 + 2)) * (2 / (float) 3));

                        lines.Add(new SKRectI(xp.line.i % 2 == 0 ? x.Rect.GetRectSidePos(xp.line.i) - (xp.line.i == 0 ? 1 : 0) : output,
                                              xp.line.i % 2 == 1 ? x.Rect.GetRectSidePos(xp.line.i) - (xp.line.i == 1 ? 1 : 0) : output,
                                              xp.line.i % 2 == 0 ? br : output,
                                              xp.line.i % 2 == 1 ? br : output).Standardized);

                        lines.Add(new SKRectI(xp.line.i % 2 == 0 ? br : output,
                                              xp.line.i % 2 == 1 ? br : output,
                                              xp.line.i % 2 == 0 ? br : input,
                                              xp.line.i % 2 == 1 ? br : input).Standardized);

                        lines.Add(new SKRectI(xp.line.i % 2 == 0 ? br : input,
                                              xp.line.i % 2 == 1 ? br : input,
                                              xp.line.i % 2 == 0 ? c.Rect.GetRectSidePos(cp.line.i) - (cp.line.i == 0 ? 1 : 0) : input,
                                              xp.line.i % 2 == 1 ? c.Rect.GetRectSidePos(cp.line.i) - (cp.line.i == 1 ? 1 : 0) : input).Standardized);
                    }
                    else
                    {
                        var output = _rand.GetRand(x.Rect.GetRectSidePos(xp.side.i) - ((float) xp.side.i).CompareTo(1.1f), xp.line.i % 2 == 0 ? x.Rect.MidY : x.Rect.MidX);
                        var input  = _rand.GetRand(c.Rect.GetRectSidePos(cp.side.i) - ((float) cp.side.i).CompareTo(1.1f), cp.line.i % 2 == 0 ? c.Rect.MidY : c.Rect.MidX);

                        lines.Add(new SKRectI(xp.line.i % 2 == 0 ? x.Rect.GetRectSidePos(xp.line.i) - (xp.line.i == 0 ? 1 : 0) : output,
                                              xp.line.i % 2 == 1 ? x.Rect.GetRectSidePos(xp.line.i) - (xp.line.i == 1 ? 1 : 0) : output,
                                              xp.line.i % 2 == 0 ? input : output,
                                              xp.line.i % 2 == 1 ? input : output).Standardized);

                        lines.Add(new SKRectI(cp.line.i % 2 == 0 ? c.Rect.GetRectSidePos(cp.line.i) - (cp.line.i == 0 ? 1 : 0) : input,
                                              cp.line.i % 2 == 1 ? c.Rect.GetRectSidePos(cp.line.i) - (cp.line.i == 1 ? 1 : 0) : input,
                                              cp.line.i % 2 == 0 ? output : input,
                                              cp.line.i % 2 == 1 ? output : input).Standardized);
                    }
                }

                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Left < 0)
                        lines[i] = lines[i] with {Left = 0};
                    if (lines[i].Right < 0)
                        lines[i] = lines[i] with {Right = 0};
                    if (lines[i].Top < 0)
                        lines[i] = lines[i] with {Top = 0};
                    if (lines[i].Bottom < 0)
                        lines[i] = lines[i] with {Bottom = 0};
                    if (lines[i].Right > area.Right)
                        lines[i] = lines[i] with {Right = area.Right};
                    if (lines[i].Left > area.Right)
                        lines[i] = lines[i] with {Left = area.Right};
                    if (lines[i].Bottom > area.Bottom)
                        lines[i] = lines[i] with {Bottom = area.Bottom};
                    if (lines[i].Top > area.Bottom)
                        lines[i] = lines[i] with {Top = area.Bottom};
                }

                var pw = _rand.GetRand(MinPassWidth, MaxPassWidth) - 1;
                if (pw > 0)
                {
                    var side = Convert.ToBoolean(_rand.GetRand(0, 2));
                    switch (lines.Count)
                    {
                        case 1:
                            if (lines[0].Left == lines[0].Right)
                                lines[0] = lines[0] with {Left = lines[0].Left - pw / 2 - (!side ? pw % 2 : 0), Right = lines[0].Right + pw / 2 + (side ? pw % 2 : 0)};
                            else
                                lines[0] = lines[0] with {Top = lines[0].Top - pw / 2 - (!side ? pw % 2 : 0), Bottom = lines[0].Bottom + pw / 2 + (side ? pw % 2 : 0)};
                            break;
                        case 2:
                            if (lines[0].Left == lines[0].Right)
                            {
                                lines[0] = lines[0] with {Left = lines[0].Left - pw / 2 - (x.MidPoint.X > c.MidPoint.X ? pw % 2 : 0), Right = lines[0].Right   + pw / 2 + (x.MidPoint.X < c.MidPoint.X ? pw % 2 : 0)};
                                lines[1] = lines[1] with {Top = lines[1].Top   - pw / 2 - (x.MidPoint.Y > c.MidPoint.Y ? pw % 2 : 0), Bottom = lines[1].Bottom + pw / 2 + (x.MidPoint.Y < c.MidPoint.Y ? pw % 2 : 0)};
                            }
                            else
                            {
                                lines[0] = lines[0] with {Top = lines[0].Top   - pw / 2 - (x.MidPoint.Y > c.MidPoint.Y ? pw % 2 : 0), Bottom = lines[0].Bottom + pw / 2 + (x.MidPoint.Y < c.MidPoint.Y ? pw % 2 : 0)};
                                lines[1] = lines[1] with {Left = lines[1].Left - pw / 2 - (x.MidPoint.X > c.MidPoint.X ? pw % 2 : 0), Right = lines[1].Right   + pw / 2 + (x.MidPoint.X < c.MidPoint.X ? pw % 2 : 0)};
                            }

                            break;
                        case 3:
                            if (lines[0].Left == lines[0].Right)
                            {
                                lines[0] = lines[0] with
                                           {
                                               Left = lines[0].Left     - pw / 2 - (lines[0].Left  > lines[2].Left ? pw  % 2 : 0),
                                               Right = lines[0].Right   + pw / 2 + (lines[0].Right < lines[2].Right ? pw % 2 : 0),
                                               Top = lines[0].Top       - (lines[0].Top            > lines[2].Top ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                               Bottom = lines[0].Bottom + (lines[0].Top            < lines[2].Top ? pw / 2 + (side ? pw     % 2 : 0) : 0)
                                           };
                                lines[1] = lines[1] with {Top = lines[1].Top - pw / 2 - (!side ? pw % 2 : 0), Bottom = lines[1].Bottom + pw / 2 + (side ? pw % 2 : 0)};
                                lines[2] = lines[2] with
                                           {
                                               Left = lines[2].Left     - pw / 2 - (lines[0].Left  < lines[2].Left ? pw  % 2 : 0),
                                               Right = lines[2].Right   + pw / 2 + (lines[0].Right > lines[2].Right ? pw % 2 : 0),
                                               Top = lines[2].Top       - (lines[0].Top            < lines[2].Top ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                               Bottom = lines[2].Bottom + (lines[0].Top            > lines[2].Top ? pw / 2 + (side ? pw     % 2 : 0) : 0)
                                           };
                            }
                            else
                            {
                                lines[0] = lines[0] with
                                           {
                                               Top = lines[0].Top       - pw / 2 - (lines[0].Top    > lines[2].Top ? pw    % 2 : 0),
                                               Bottom = lines[0].Bottom + pw / 2 + (lines[0].Bottom < lines[2].Bottom ? pw % 2 : 0),
                                               Left = lines[0].Left     - (lines[0].Left            > lines[2].Left ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                               Right = lines[0].Right   + (lines[0].Left            < lines[2].Left ? pw / 2 + (side ? pw     % 2 : 0) : 0)
                                           };
                                lines[1] = lines[1] with {Left = lines[1].Left - pw / 2 - (!side ? pw % 2 : 0), Right = lines[1].Right + pw / 2 + (side ? pw % 2 : 0)};
                                lines[2] = lines[2] with
                                           {
                                               Top = lines[2].Top       - pw / 2 - (lines[0].Top    < lines[2].Top ? pw    % 2 : 0),
                                               Bottom = lines[2].Bottom + pw / 2 + (lines[0].Bottom > lines[2].Bottom ? pw % 2 : 0),
                                               Left = lines[2].Left     - (lines[0].Left            < lines[2].Left ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                               Right = lines[2].Right   + (lines[0].Left            > lines[2].Left ? pw / 2 + (side ? pw     % 2 : 0) : 0)
                                           };
                            }

                            break;
                        default:
                            break;
                    }
                }

                if (!_rooms.Any(_ => lines.Any(__ => __.IntersectsWith(_.Rect))) && !_passes.SelectMany(_ => _.LineList).Any(_ => lines.Any(__ => __.IntersectsWith(_))))
                    _passes.Add(new Pass(x, c, lines.ToArray()));
            }
        }
    }

    private void ClearPasses()
    {
        var p = new AStar.AStar(1000);
        for (var i = 0; i < _passes.Count; i++)
        {
            var x = _passes[i];
            if (x.LineList.Count == 1)
                continue;
            var basePath = p.GetPathDistance(x.StartRoom.MidPoint, x.EndRoom.MidPoint, GetMapArrayExclude());
            if (basePath.IsEqual(-1))
                continue;
            var longPath = p.GetPathDistance(x.StartRoom.MidPoint, x.EndRoom.MidPoint, GetMapArrayExclude(x));
            if (longPath.IsEqual(-1))
                continue;
            if (basePath + basePath * LongPathDifferencePercent / 100 > longPath)
            {
                _passes.RemoveAt(i);
                i--;
            }
        }
    }

    public bool PassExist(Room r1, Room r2) => _passes.Count(_ => (_.StartRoom == r1 && _.EndRoom == r2) || (_.StartRoom == r2 && _.EndRoom == r1)) != 0;

    #endregion

    #region Elements

    private void GenerateElements()
    {
        foreach (var x in ElementRateList)
        {
            var mapRate    = _rand.GetRand(x.MinRatePerMap, x.MaxRatePerMap);
            var mapRateInt = mapRate / 100;
            var mapRateDiv = mapRate % 100;
            if (_rand.GetRand(0, 100) <= mapRateDiv)
                mapRateInt++;
            var arr = _rand.CreateShuffleInt(_rooms.Count);
            var i   = 0;
            RepeatableCode.RepeatResult(() =>
                                        {
                                            var minRand = _rand.GetRand(x.MinRatePerRoom, x.MaxRatePerRoom);
                                            var res = RepeatableCode.RepeatResult(() =>
                                                                        {
                                                                            if (_rand.GetRand(0, 100) <= minRand)
                                                                                return true;
                                                                            i++;
                                                                            return false;
                                                                        }, 100000);
                                            if (!res)
                                                return false;
                                            _rooms[arr[i % _rooms.Count]].AddElement(new Element(x.Type), _rand);
                                            return true;

                                        }, 100);
        }
    }
    

    #endregion

    #region PostProcessing

    private void GenerateMapArray()
    {
        var area = GetArea();
        _mapArray = new Tile[area.Width + 1, area.Height + 1];

        for (var i = 0; i < _mapArray.GetLength(0); i++)
        for (var j = 0; j < _mapArray.GetLength(1); j++)
            _mapArray[i, j] = new Tile(TileType.Wall);

        foreach (var x in _rooms)
        {
            for (var i = x.Rect.Top; i < x.Rect.Bottom; i++)
            for (var j = x.Rect.Left; j < x.Rect.Right; j++)
                _mapArray[j, i].SetTileType(TileType.Floor);
            foreach (var c in x.Elements)
                _mapArray[x.Rect.Left + c.Position.X, x.Rect.Top + c.Position.Y].Element = c;
        }

        foreach (var x in _passes)
        {
            for (var i = 0; i < x.LineList.Count; i++)
            {
                if (i == 0)
                    FillRegionWith(x.LineList[i]);
                if (i == 1)
                    FillRegionWith(x.LineList[i]);
                if (i == 2)
                    FillRegionWith(x.LineList[i]);
            }
        }
        
    }

    private Tile[,] GetMapArrayExclude(Pass? excludePass = null)
    {
        var area = GetArea();
        var arr  = new Tile[area.Width + 1, area.Height + 1];
        for (var i = 0; i < arr.GetLength(0); i++)
        for (var j = 0; j < arr.GetLength(1); j++)
            arr[i, j] = new Tile(TileType.Wall);
        foreach (var x in _rooms)
            for (var i = x.Rect.Top; i < x.Rect.Bottom; i++)
            for (var j = x.Rect.Left; j < x.Rect.Right; j++)
                arr[j, i].SetTileType(TileType.Floor);

        foreach (var x in _passes.Where(x => excludePass != x))
        {
            for (var i = 0; i < x.LineList.Count; i++)
            {
                if (i == 0)
                    FillRegionWith(x.LineList[i], arr);
                if (i == 1)
                    FillRegionWith(x.LineList[i], arr);
                if (i == 2)
                    FillRegionWith(x.LineList[i], arr);
            }
        }

        return arr;
    }

    //private void FillRegionWith(SKRectI r, byte fill) => FillRegionWith(new SKPointI(r.Left,    r.Top),     new SKPointI(r.Right, r.Bottom), fill);
    //private void FillRegionWith(SKLineI l, byte fill) => FillRegionWith(new SKPointI(l.Start.X, l.Start.Y), new SKPointI(l.End.X, l.End.Y),  fill);

    private void FillRegionWith(SKRectI r)
    {
        for (var i = r.Top; i <= r.Bottom; i++)
        for (var j = r.Left; j <= r.Right; j++)
            _mapArray[j, i].SetTileType(TileType.Floor);
    }

    private void FillRegionWith(SKRectI r, Tile[,] arr)
    {
        for (var i = r.Top; i <= r.Bottom; i++)
        for (var j = r.Left; j <= r.Right; j++)
            arr[j, i].SetTileType(TileType.Floor);
    }

    #endregion


    #endregion
}
