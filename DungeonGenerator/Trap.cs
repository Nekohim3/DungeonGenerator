using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace DungeonGenerator
{
    public enum ElementType
    {
        StartPoint = 0x00,

        Chest1 = 0x10,//Common
        Chest2 = 0x11,//Uncommon
        Chest3 = 0x12,//Rare
        Mimic1 = 0x13,
        Mimic2 = 0x14,
        Mimic3 = 0x15,
        
        Mirror = 0x20,

        EroTrap = 0x30,
        DebuffTrap = 0x32,
        EnemyBuffTrap = 0x33,
        StatusTrap = 0x34,

        HpCrystal = 0x40,
        MpCrystal = 0x41,
        HpMpCrystal = 0x42,
        FreezeCrystal = 0x43,

        DefenseStatue = 0x50,

        RandomPortal = 0x60,
        SafeRoomPortal = 0x61,

        Rune = 0x70,

        Lever = 0xf0,
        FakeLever = 0xf1,

        EndPoint = 0xff,
    }
    public class Element
    {
        public ElementType ElementType;
        public bool        Passable { get; set; }

        public SKPointI Position { get; set; }

        public Element(ElementType et)
        {
            ElementType = et;
        }

        public void SetPos(SKPointI pos)
        {
            Position = pos;
        }
    }

    public class ElementRate
    {
        public ElementType Type           { get; set; }
        public int         MinRatePerMap  { get; set; }
        public int         MaxRatePerMap  { get; set; }
        public int         MinRatePerRoom { get; set; }
        public int         MaxRatePerRoom { get; set; }
        public int         MinRatePerPass { get; set; }
        public int         MaxRatePerPass { get; set; }

        public ElementRate(ElementType type, int minRatePerMap, int maxRatePerMap, int minRatePerRoom, int maxRatePerRoom, int minRatePerPass, int maxRatePerPass)
        {
            Type           = type;
            MinRatePerMap  = minRatePerMap;
            MaxRatePerMap  = maxRatePerMap;
            MinRatePerRoom = minRatePerRoom;
            MaxRatePerRoom = maxRatePerRoom;
            MinRatePerPass = minRatePerPass;
            MaxRatePerPass = maxRatePerPass;
        }
    }
}
