using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace D
{
    public partial class MainPage : ContentPage
    {
        class Point
        {
            public SKPoint Position;
            public SKColor Color;
        }

        List<Point> points = new List<Point>();
        Random rnd = new Random();

        public MainPage()
        {
            InitializeComponent();
        }

        private double Distance(Point a, SKPoint b)
        {
            return Math.Sqrt(Math.Pow(a.Position.X - b.X, 2) + Math.Pow(a.Position.Y - b.Y, 2));
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            if (points.Count == 0)
                return;
            foreach (var p in points)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black
                })
                {
                    canvas.DrawCircle(p.Position, 5, paint);
                }
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2,
                    Color = SKColors.White
                })
                {
                    canvas.DrawCircle(p.Position, 5, paint);
                }
            }
        }

        private void OnCanvasViewTouch(object sender, SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Released)
            {
                var pos = e.Location;
                var nearest = points.OrderBy(p => Distance(p, pos)).FirstOrDefault();
                if (nearest != null && Distance(nearest, pos) < 20)
                {
                    points.Remove(nearest);
                }
                else
                {
                    points.Add(new Point { Position = pos, Color = RandomColor() });
                }
                canvasView.InvalidateSurface();
                e.Handled = true;
            }
        }

        private SKColor RandomColor()
        {
            return new SKColor(
                (byte)rnd.Next(50, 256),
                (byte)rnd.Next(50, 256),
                (byte)rnd.Next(50, 256));
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            points = new List<Point>();
            canvasView.InvalidateSurface();
        }
    }
}
