using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace TrapDungeonGenerator
{
    public class Pass
    {
        public Room          StartRoom { get; set; }
        public Room          EndRoom   { get; set; }
        public List<SKRectI> LineList  { get; set; }

        public Pass(Room startRoom, Room endRoom, params SKRectI[] lineList)
        {
            StartRoom = startRoom;
            EndRoom   = endRoom;
            LineList  = lineList.ToList();
        }
    }
}
