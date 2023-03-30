using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using TrapDungeonGenerator.Extensions;

namespace TrapDungeonGenerator
{
    public class Room
    {
        public string  Name { get; set; }
        public SKRectI Bounds { get; set; }
        public SKPointI MidPoint => Bounds.GetMidPoint();
        public List<Element> Elements { get; set; }

        public Room(SKRectI r, string name)
        {
            Bounds = r;
            Name   = name;
        }

    }
}
