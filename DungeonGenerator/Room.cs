using System;
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

        public List<Element> Elements { get; set; } = new(); 

        public SKPointI MidPoint => Rect.GetMidPoint();

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

        public void AddElement(Element e, Random r)
        {
            var res = RepeatableCode.RepeatResult(() =>
                                        {
                                            var p = new SKPointI(r.GetRand(0, Rect.Width), r.GetRand(0, Rect.Height));
                                            if (Elements.Any(_ => _.Position == p))
                                                return false;
                                            e.Position = p;
                                            Elements.Add(e);
                                            return true;
                                        }, 100000);
            if (!res)
                throw new Exception();

        }
    }
}
