using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace TrapDungeonGenerator
{
    public class Map
    {
        public List<Room> Rooms { get; set; }
        public List<Pass> Passes { get; set; }

        public Map()
        {
            Rooms = new List<Room>();
            Passes = new List<Pass>();
        }

        public void NormalizeMap()
        {

        }

        public SKRectI GetArea()
        {
            return default;
        }

    }
}
