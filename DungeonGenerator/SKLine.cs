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
    
    public SKPoint Intersect(SKLineI l)
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
