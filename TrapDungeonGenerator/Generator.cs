using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonGenerator;
using SkiaSharp;
using TrapDungeonGenerator.Extensions;

namespace TrapDungeonGenerator;

public static class Generator
{
    private static Range  _roomWidth;
    private static Range  _roomHeight;
    private static Range  _roomCount;
    private static Range  _distanceBetweenRooms;
    private static Range  _passWidth;
    private static int    _passPercent;
    private static int    _seed;
    private static Random _rand;
    private static Map    Map;

    public static Map GenerateMap(List<ElementRate> elementGenerateList,
                                  Range             roomWidth,
                                  Range             roomHeight,
                                  Range             roomCount,
                                  Range             distanceBetweenRooms,
                                  Range             passWidth,
                                  int               passPercent,
                                  int               seed)
        //int roomMinWidth = 15, 
        //int roomMinHeight = 15, 
        //int roomMaxWidth = 30, 
        //int roomMaxHeight = 30,
        //int roomMinCount = 5, 
        //int roomMaxCount = 10, 
        //int minDistanceBetweenRooms = 0, 
        //int maxDistanceBetweenRooms = 30,
        //int minPassWidth = 1,
        //int maxPassWidth = 3, 
        //int passPercent = 50)
    {
        _roomWidth            = roomWidth;
        _roomHeight           = roomHeight;
        _roomCount            = roomCount;
        _distanceBetweenRooms = distanceBetweenRooms;
        _passWidth            = passWidth;
        _passPercent          = passPercent;
        _seed                 = seed == -1 ? new Random().Next(int.MinValue, int.MaxValue) : seed;
        _rand                 = new Random(_seed);

        return null;
    }

    #region Room

    private static bool GenerateRooms()
    {
        while (Map.Rooms.Count < _rand.GetRand(_roomCount))
        {
            if (Map.Rooms.Count == 0)
                Map.Rooms.Add(new Room(new SKRectI(0, 0, _rand.GetRand(_roomWidth), _rand.GetRand(_roomHeight)), (Map.Rooms.Count + 1).ToString()));
            else
            {
                var room = RepeatableCode.RepeatResult(() =>
                                                       {
                                                           var room = GenerateRoom((Map.Rooms.Count + 1).ToString());
                                                           return CheckRoom(room) ? room : null;
                                                       }, 100000);

                if (room == null)
                    return false;

                Map.Rooms.Add(room);
            }
        }

        return true;
    }

    private static Room GenerateRoom(string name)
    {
        var area = Map.GetArea();
        var r    = new SKRectI { Left = _rand.GetRand(area.Left, area.Right), Top = _rand.GetRand(area.Top, area.Bottom) };
        r.Right  = r.Left + _rand.GetRand(_roomWidth);
        r.Bottom = r.Top  + _rand.GetRand(_roomHeight);
        return new Room(r, name);
    }

    private static bool CheckRoom(Room r) => Map.Rooms.All(_ => _.Bounds.GetDistanceToRect(r.Bounds) >= _distanceBetweenRooms.Start.Value) &&
                                             Map.Rooms.Any(_ => _.Bounds.GetDistanceToRect(r.Bounds) <= _distanceBetweenRooms.End.Value) &&
                                             Map.Rooms.All(_ => !_.Bounds.IntersectsWith(r.Bounds))                              &&
                                             (Map.Rooms.Count < 3 || GetNearestRooms(r).Count > 1);


    public static List<Room> GetNearestRooms(Room r) => Map.Rooms.Where(_ => r != _).Where(_ => r.Bounds.GetDistanceToRect(_.Bounds) <= _distanceBetweenRooms.End.Value).ToList();

    #endregion

    #region Pass



    #endregion

    #region Element



    #endregion



}
