using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGenerator.Extensions
{
    public static class GeometricExtension
    {
        #region Rectangle

        public static SKRectI        ExpandLeft(this    SKRectI r, int left)                                 => r with { Left = r.Left     - left };
        public static SKRectI        ExpandTop(this     SKRectI r, int top)                                  => r with { Top = r.Top       - top };
        public static SKRectI        ExpandRight(this   SKRectI r, int right)                                => r with { Right = r.Right   + right };
        public static SKRectI        ExpandBottom(this  SKRectI r, int bottom)                               => r with { Bottom = r.Bottom + bottom };
        public static SKRectI        ExpandWidth(this   SKRectI r, int width)                                => r.ExpandLeft(width).ExpandRight(width);
        public static SKRectI        ExpandHeight(this  SKRectI r, int height)                               => r.ExpandTop(height).ExpandBottom(height);
        public static SKRectI        ExpandAll(this     SKRectI r, int n)                                    => r.ExpandWidth(n).ExpandHeight(n);
        public static SKRectI        Expand(this        SKRectI r, int left, int top, int right, int bottom) => r.ExpandLeft(left).ExpandTop(top).ExpandRight(right).ExpandBottom(bottom);
        public static SKPointI        GetMidPoint(this   SKRectI r) => new(r.MidX, r.MidY);
        public static List<SKLineI>  GetRectLines(this  SKRectI r) => new() { new SKLineI(r.Left, r.Top, r.Left, r.Bottom), new SKLineI(r.Left, r.Top, r.Right, r.Top), new SKLineI(r.Right, r.Top, r.Right, r.Bottom), new SKLineI(r.Left, r.Bottom, r.Right, r.Bottom) };
        public static List<SKPointI> GetRectPoints(this SKRectI r) => new() { new SKPointI(r.Left, r.Top), new SKPointI(r.Right, r.Top), new SKPointI(r.Right, r.Bottom), new SKPointI(r.Left, r.Bottom) };

        public static float GetDistanceToRect(this SKRectI r, SKRectI r1)
        {
            if (r1.Right > r.Left + 2 && r1.Left < r.Right - 2)
                return Math.Min(Math.Abs(r.Top - r1.Bottom), Math.Abs(r1.Top - r.Bottom));
            if (r1.Bottom > r.Top + 2 && r1.Top < r.Bottom - 2)
                return Math.Min(Math.Abs(r.Left - r1.Right), Math.Abs(r1.Left - r.Right));
            return r.GetRectPoints().SelectMany(x => r1.GetRectPoints().Select(c => (x - c).Length)).Min();
        }

        public static int GetRectSidePos(this SKRectI r, RectangleSideDirection d) => r.GetRectSidePos((int)d);
        public static int GetRectSidePos(this SKRectI r, int d) => d switch
        {
            0 => r.Left,
            1 => r.Top,
            2 => r.Right,
            3 => r.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };

        #endregion


        #region Point

        public static SKPoint OffsetPoint(this SKPoint  p, float    x, float y) => p with { X = p.X + x, Y = p.Y + y };
        public static float   DistanceToPoint(this  SKPointI p, SKPointI pp) => new SKLineI(p, pp).Length;

        #endregion

        #region Other

        public static bool IsOpposite(this (RectangleSideDirection line, RectangleSideDirection side)                       a, (RectangleSideDirection line, RectangleSideDirection side)                       b) => (int)a.line % 2 == (int)b.line % 2 && a.line   != b.line;
        public static bool IsOpposite(this ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) a, ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) b) => a.line.i    % 2 == b.line.i    % 2 && a.line.i != b.line.i;
        public static ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) GetRectIntersectType(this SKRectI r, SKLineI l)
        {
            var intersections = r.GetRectLines().Select(_ => _.Intersect(l)).ToList();
            var point         = intersections.FirstOrDefault(_ => _ != default);
            if (point == default)
                throw new Exception();
            var index = intersections.IndexOf(point);
            return (((RectangleSideDirection)index, index), index % 2 == 0 ? r.MidY > point.Y ? (RectangleSideDirection.Top, 1) : (RectangleSideDirection.Bottom, 3) : r.MidX > point.X ? (RectangleSideDirection.Left, 0) : (RectangleSideDirection.Right, 2));
        }

        #endregion
    }
}
