﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonGenerator.Extensions;
using SkiaSharp;

namespace DungeonGenerator
{
    public class Room
    {
        public string  Name { get; set; }
        public SKRectI Rect;

        public SKPoint MidPoint => Rect.GetMidPoint();

        public Room(SKRectI r, string name)
        {
            Rect = r;
            Name = name;
        }

        public Room(int l, int t, int r, int b, string name)
        {
            Rect = new SKRectI(l, t, r, b);
            Name = name;
        }
    }
}
