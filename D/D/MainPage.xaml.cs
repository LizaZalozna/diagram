using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Threading;

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
        bool draw =false;
        bool useParallel = false;

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
            var info = e.Info;
            canvas.Clear(SKColors.White);

            if (points.Count == 0)
                return;

            if (draw)
            {
                if (!useParallel)
                    DrawD(canvas, info);
                else DrawDParallel(canvas, info);
            }

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

        private void OnGenerateClicked(object sender, EventArgs e)
        {
            for (int i = 0; i < 20; i++)
            {
                var x = rnd.Next(0, (int)canvasView.CanvasSize.Width);
                var y = rnd.Next(0, (int)canvasView.CanvasSize.Height);
                var color = RandomColor();

                points.Add(new Point
                {
                    Position = new SKPoint(x, y),
                    Color = color
                });
            }
            canvasView.InvalidateSurface();
        }

        private void Draw1Clicked(object sender, EventArgs e)
        {
            draw = true;
            useParallel = false;
            canvasView.InvalidateSurface();
        }

        private void DrawD(SKCanvas canvas, SKImageInfo info)
        {
            using (var bitmap = new SKBitmap(info.Width, info.Height))
            {
                for (int y = 0; y < info.Height; y++)
                {
                    for (int x = 0; x < info.Width; x++)
                    {
                        SKPoint current = new SKPoint(x, y);
                        var nearest = points.OrderBy(p => Distance(p, current)).First();
                        bitmap.SetPixel(x, y, nearest.Color);
                    }
                }
                canvas.DrawBitmap(bitmap, 0, 0);
            }
        }

        private void Draw2Clicked(object sender, EventArgs e)
        {
            draw = true;
            useParallel = true;
            canvasView.InvalidateSurface();
        }

        private void DrawDParallel(SKCanvas canvas, SKImageInfo info)
        {
            int width = info.Width;
            int height = info.Height;

            var bitmap = new SKBitmap(width, height);
            Thread[] threads = new Thread[6];
            int rowsPerThread = height / 6;

            for (int i = 0; i < 6; i++)
            {
                int startY = i * rowsPerThread;
                int endY = (i == 5) ? height : (i + 1) * rowsPerThread;

                threads[i] = new Thread(() =>
                {
                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            SKPoint current = new SKPoint(x, y);
                            var nearest = points.OrderBy(p => Distance(p, current)).First();
                            bitmap.SetPixel(x, y, nearest.Color);
                        }
                    }
                });

                threads[i].Start();
            }

            foreach (var t in threads)
                t.Join();

            canvas.DrawBitmap(bitmap, 0, 0);
        }

    }
}
