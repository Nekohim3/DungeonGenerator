using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DungeonGenerator.Extensions;
using SkiaSharp;

namespace DungeonGenerator;

public struct SKLine
{
    #region Properties

    public SKPoint Start { get; set; }
    public SKPoint End   { get; set; }

    public float Length       => (float) Math.Sqrt(Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));
    public bool  IsHorizontal => Start.Y.IsEqual(End.Y);
    public bool  IsVertical   => Start.X.IsEqual(End.X);
    public bool  IsDiagonal   => !Start.X.IsEqual(End.X) && !Start.Y.IsEqual(End.Y);

    #endregion

    #region Ctor

    public SKLine(SKPoint start, SKPoint end)
    {
        Start = start;
        End   = end;
    }

    public SKLine(float sx, float sy, float ex, float ey)
    {
        Start = new SKPoint(sx, sy);
        End   = new SKPoint(ex, ey);
    }

    public static implicit operator SKLine(SKLineI l) => new(new SKPoint(l.Start.X, l.Start.Y), new SKPoint(l.End.X, l.End.Y));

    #endregion

    #region Funcs

    public SKPoint Intersect(SKLine l)
    {
        var x1 = Start.X;
        var y1 = Start.Y;
        var x2 = End.X;
        var y2 = End.Y;

        var x3 = l.Start.X;
        var y3 = l.Start.Y;
        var x4 = l.End.X;
        var y4 = l.End.Y;

        var det = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (det == 0)
            return default;

        var x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / det;
        var y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / det;
        return new SKPoint(x, y);
    }

    #endregion
}
public struct SKLineI
{
    #region Properties

    public SKPointI Start { get; set; }
    public SKPointI End   { get; set; }

    public float Length       => (float) Math.Sqrt(Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));
    public bool  IsHorizontal => Start.Y == End.Y;
    public bool  IsVertical   => Start.X == End.X;
    public bool  IsDiagonal   => Start.X != End.X && Start.Y != End.Y;

    #endregion

    #region Ctor

    public SKLineI(SKPointI start, SKPointI end)
    {
        Start = start;
        End   = end;
    }

    public SKLineI(int sx, int sy, int ex, int ey)
    {
        Start = new SKPointI(sx, sy);
        End   = new SKPointI(ex, ey);
    }

    public static implicit operator SKLine(SKLineI l) => new(l.Start.X, l.Start.Y, l.End.X, l.End.Y);

    #endregion

    #region Funcs
    public SKPoint Intersect(SKLineI other, float tolerance = 0.00005f)
    {
        var l = this;
        bool IsInsideLine(SKLineI line, SKPoint p, float tol)
        {
            float x = p.X, y = p.Y;

            var leftX = line.Start.X;
            var leftY = line.Start.Y;

            var rightX = line.End.X;
            var rightY = line.End.Y;

            return ((x.IsGreaterThanOrEqual(leftX, tol) && x.IsLessThanOrEqual(rightX, tol))
                    || (x.IsGreaterThanOrEqual(rightX, tol) && x.IsLessThanOrEqual(leftX, tol)))
                   && ((y.IsGreaterThanOrEqual(leftY, tol) && y.IsLessThanOrEqual(rightY, tol))
                       || (y.IsGreaterThanOrEqual(rightY, tol) && y.IsLessThanOrEqual(leftY, tol)));
        }

        if (l.Start == other.Start && l.End == other.End)
            throw new Exception("Both lines are the same.");

        if (l.Start.X.CompareTo(other.Start.X) > 0)
            (l, other) = (other, l);
        else if (l.Start.X.CompareTo(other.Start.X) == 0)
        {
            if (l.Start.Y.CompareTo(other.Start.Y) > 0)
                (l, other) = (other, l);
        }

        float x1 = l.Start.X, y1 = l.Start.Y;
        float x2 = l.End.X, y2 = l.End.Y;
        float x3 = other.Start.X, y3 = other.Start.Y;
        float x4 = other.End.X, y4 = other.End.Y;

        if (x1.IsEqual(x2) && x3.IsEqual(x4) && x1.IsEqual(x3))
        {
            var firstIntersection = new SKPoint(x3, y3);
            if (IsInsideLine(l, firstIntersection, tolerance) &&
                IsInsideLine(other, firstIntersection, tolerance))
                return new SKPoint(x3, y3);
        }

        if (y1.IsEqual(y2) && y3.IsEqual(y4) && y1.IsEqual(y3))
        {
            var firstIntersection = new SKPoint(x3, y3);
            if (IsInsideLine(l, firstIntersection, tolerance) &&
                IsInsideLine(other, firstIntersection, tolerance))
                return new SKPoint(x3, y3);
        }

        if (x1.IsEqual(x2) && x3.IsEqual(x4))
            return default;

        if (y1.IsEqual(y2) && y3.IsEqual(y4))
            return default;

        float x, y;
        if (x1.IsEqual(x2))
        {
            var m2 = (y4 - y3) / (x4 - x3);
            var c2 = -m2 * x3 + y3;
            x = x1;
            y = c2 + m2 * x1;
        }
        else if (x3.IsEqual(x4))
        {
            var m1 = (y2 - y1) / (x2 - x1);
            var c1 = -m1 * x1 + y1;
            x = x3;
            y = c1 + m1 * x3;
        }
        else
        {
            var m1 = (y2 - y1) / (x2 - x1);
            var c1 = -m1 * x1 + y1;
            var m2 = (y4 - y3) / (x4 - x3);
            var c2 = -m2 * x3 + y3;
            x = (c1 - c2) / (m2 - m1);
            y = c2 + m2 * x;

            if (!((-m1 * x + y).IsEqual(c1)
                  && (-m2 * x + y).IsEqual(c2)))
                return default;
        }

        var result = new SKPoint(x, y);

        if (IsInsideLine(l, result, tolerance) &&
            IsInsideLine(other, result, tolerance))
            return result;

        return default;
    }
    //public SKPoint Intersect(SKLineI l)
    //{
    //    var x1 = Start.X;
    //    var y1 = Start.Y;
    //    var x2 = End.X;
    //    var y2 = End.Y;

    //    var x3 = l.Start.X;
    //    var y3 = l.Start.Y;
    //    var x4 = l.End.X;
    //    var y4 = l.End.Y;

    //    var det = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
    //    if (det == 0)
    //        return default;

    //    var x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / det;
    //    var y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / det;
    //    return new SKPoint(x, y);
    //}

    #endregion
}
