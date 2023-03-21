using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Avalonia.Media;
using JetBrains.Annotations;
using SkiaSharp;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = System.Drawing.Color;

namespace DungeonViewer.Utils
{
    public class Sbmp : IDisposable
    {
        private SKImageInfo _imageInfo;
        private SKSurface _surface;
        private SKCanvas _canvas => _surface.Canvas;
        public int Width => _imageInfo.Width;
        public int Height => _imageInfo.Height;

        public Sbmp(int width, int height)
        {
            _imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(_imageInfo);
        }

        public Sbmp(SKImage image)
        {
            _imageInfo = new SKImageInfo(image.Width, image.Height);
            _surface = SKSurface.Create(image.PeekPixels());
        }

        #region DrawFuncs

        public void Fill(SKColor color) => DrawFillRectangle(new SKRectI(0, 0, Width, Height), color);

        #region Rectangles

        public void DrawRectangle(SKRectI rect, SKColor fillColor, SKColor strokeColor, int strokeThickness, int gapX = 0, int gapY = 0)
        {
            var fillRect = new SKRectI(rect.Left + gapX + strokeThickness, rect.Top + gapY + strokeThickness, rect.Right - gapX - strokeThickness, rect.Bottom - gapY - strokeThickness);
            var paintFill = new SKPaint() { Style = SKPaintStyle.Fill, Color = fillColor };
            _canvas.DrawRect(fillRect, paintFill);
            //_canvas.DrawRoundRect(fillRect, 50,50, paintFill);

            if (strokeThickness == 0)
                return;

            var strokeRect = new SKRectI(rect.Left + strokeThickness / 2, rect.Top + strokeThickness / 2, rect.Right - 1 - strokeThickness / 2, rect.Bottom - 1 - strokeThickness / 2);
            var paintStroke = new SKPaint() { Style = SKPaintStyle.Stroke, Color = strokeColor, StrokeWidth = strokeThickness };
            _canvas.DrawRect(strokeRect, paintStroke);
        }

        public void DrawRectangle(int left, int top, int right, int bottom, SKColor fillColor, SKColor strokeColor, int strokeThickness, int gapX = 0, int gapY = 0) =>
            DrawRectangle(new SKRectI(left, top, right, bottom), fillColor, strokeColor, strokeThickness, gapX, gapY);

        public void DrawFillRectangle(SKRectI rect, SKColor fillColor)
        {
            var fillRect = new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
            var paintFill = new SKPaint() { Style = SKPaintStyle.Fill, Color = fillColor };
            _canvas.DrawRect(fillRect, paintFill);
            //_canvas.DrawRoundRect(fillRect, 5, 5, paintFill);
        }

        public void DrawFillRectangle(int left, int top, int right, int bottom, SKColor fillColor) =>
            DrawFillRectangle(new SKRectI(left, top, right, bottom), fillColor);

        public void DrawOutlinedRectangle(SKRectI rect, SKColor strokeColor, int strokeThickness)
        {
            if (strokeThickness == 0)
                return;

            var strokeRect = new SKRectI(rect.Left + strokeThickness / 2, rect.Top + strokeThickness / 2, rect.Right - 1 - strokeThickness / 2, rect.Bottom - 1 - strokeThickness / 2);
            var paintStroke = new SKPaint() { Style = SKPaintStyle.Stroke, Color = strokeColor, StrokeWidth = strokeThickness };
            _canvas.DrawRect(strokeRect, paintStroke);
        }

        public void DrawOutlinedRectangle(int left, int top, int right, int bottom, SKColor strokeColor, int strokeThickness) =>
            DrawOutlinedRectangle(new SKRectI(left, top, right, bottom), strokeColor, strokeThickness);

        #endregion

        #region Line

        public void DrawLine(SKPointI start, SKPointI end, SKColor color, int strokeThickness)
        {
            var paintStroke = new SKPaint() { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = strokeThickness, StrokeCap = SKStrokeCap.Square };
            _canvas.DrawLine(start, end, paintStroke);
        }

        #endregion

        #region Pixel

        public void DrawPixel(SKPointI point, SKColor color)
        {
            _canvas.DrawPoint(point, color);
        }

        #endregion

        #region Text

        public void DrawText(string text, SKPoint pos, SKColor color, int size, SKFont? font = null)
        {
            font ??= new SKFont(SKTypeface.FromFamilyName("verdana"));
            _canvas.DrawText(text, pos, new SKPaint(font) { Color = color, TextSize = size });
        }
        #endregion

        #endregion

        public Sbmp Crop(SKRectI rect)
        {
            if (rect.Left < 0)
                rect.Left = 0;
            if (rect.Top < 0)
                rect.Top = 0;
            if (rect.Right > Width)
                rect.Right = Width;
            if (rect.Bottom > Height)
                rect.Bottom = Height;
            return new Sbmp(_surface.Snapshot(rect));
        }

        public SKImage GetImage() => _surface.Snapshot();
        public SKData GetData() => GetImage().Encode();
        public byte[] GetBytes() => GetData().ToArray();
        public MemoryStream GetStream => new(GetBytes());
        public Bitmap GetBitmap => new(GetStream);

        public void Save(string path)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var buf = GetBytes();
            fs.Write(buf, 0, buf.Length);
        }

        public void Dispose()
        {
        }
    }
}
