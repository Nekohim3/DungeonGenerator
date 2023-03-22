using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGenerator
{
    public enum ElementType
    {
        StartPoint = 0x00,

        Chest1 = 0x10,
        Chest2 = 0x11,
        Chest3 = 0x12,
        Mimic = 0x13,

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
    }
}
