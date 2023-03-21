using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGenerator;

public class Pass
{
    public Room          StartRoom { get; set; }
    public Room          EndRoom   { get; set; }
    public List<SKLineI> LineList  { get; set; }

    public Pass(Room startRoom, Room endRoom, params SKLineI[] lineList)
    {
        StartRoom = startRoom;
        EndRoom   = endRoom;
        LineList  = lineList.ToList();
    }
}
