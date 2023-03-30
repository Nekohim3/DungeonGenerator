using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace TrapDungeonGenerator
{
    public enum ElementType
    {
        StartPoint = 0x00,

        Chest1 = 0x10, //Common
        Chest2 = 0x11, //Uncommon
        Chest3 = 0x12, //Rare

        Mirror = 0x20,

        EroTrap       = 0x30,
        DebuffTrap    = 0x32,
        EnemyBuffTrap = 0x33,
        StatusTrap    = 0x34,

        HpCrystal     = 0x40,
        MpCrystal     = 0x41,
        HpMpCrystal   = 0x42,
        FreezeCrystal = 0x43,

        DefenseStatue = 0x50,

        RandomPortal   = 0x60,
        SafeRoomPortal = 0x61,

        Rune = 0x70,

        Lever     = 0xf0,
        FakeLever = 0xf1,

        EndPoint = 0xff,
    }

    public class Element
    {
        public ElementType ElementType;

        public SKPointI Position { get; set; }

        public Element(ElementType et)
        {
            ElementType = et;
        }

        public Element(ElementType elementType, SKPointI position)
        {
            ElementType = elementType;
            Position    = position;
        }
    }

    public class ElementRate
    {
        public  ElementType Type { get; set; }
        private Range       _ratePerMap;
        public Range RatePerMap
        {
            get => _ratePerMap;
            set
            {
                _ratePerMap   = value;
                MinRatePerMap = _ratePerMap.Start.Value;
                MaxRatePerMap = _ratePerMap.End.Value;
            }
        }

        private Range _ratePerRoom;
        public Range RatePerRoom
        {
            get => _ratePerRoom;
            set
            {
                _ratePerRoom   = value;
                MinRatePerRoom = _ratePerRoom.Start.Value;
                MaxRatePerRoom = _ratePerRoom.End.Value;
            }
        }

        private Range _ratePerPass;
        public Range RatePerPass
        {
            get => _ratePerPass;
            set
            {
                _ratePerPass  = value;
                MinRatePerPass = _ratePerPass.Start.Value;
                MaxRatePerPass = _ratePerPass.End.Value;
            }
        }

        private int _minRatePerMap;
        public int MinRatePerMap
        {
            get => _minRatePerMap;
            set
            {
                _minRatePerMap = value;
                RatePerMap     = value.._ratePerMap.End;
            }
        }

        private int _maxRatePerMap;
        public int MaxRatePerMap
        {
            get => _maxRatePerMap;
            set
            {
                _maxRatePerMap = value;
                RatePerMap     = _ratePerMap.Start..value;
            }
        }

        private int _minRatePerRoom;
        public int MinRatePerRoom
        {
            get => _minRatePerRoom;
            set
            {
                _minRatePerRoom = value;
                RatePerRoom     = value.._ratePerRoom.End;
            }
        }

        private int _maxRatePerRoom;
        public int MaxRatePerRoom
        {
            get => _maxRatePerRoom;
            set
            {
                _maxRatePerRoom = value;
                RatePerRoom     = _ratePerRoom.Start..value;
            }
        }

        private int _minRatePerPass;
        public int MinRatePerPass
        {
            get => _minRatePerPass;
            set
            {
                _minRatePerPass = value;
                RatePerPass     = value..RatePerPass.End;
            }
        }

        private int _maxRatePerPass;
        public int MaxRatePerPass
        {
            get => _maxRatePerPass;
            set
            {
                _maxRatePerPass = value;
                RatePerPass     = _ratePerPass.Start..value;
            }
        }

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

        public ElementRate(ElementType type, Range ratePerMap, Range ratePerRoom, Range ratePerPass)
        {
            Type         = type;
            RatePerMap  = ratePerMap;
            RatePerRoom = ratePerRoom;
            RatePerPass = ratePerPass;
        }
    }
}
