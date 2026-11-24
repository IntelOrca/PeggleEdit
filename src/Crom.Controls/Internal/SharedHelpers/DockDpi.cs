using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace Crom.Controls
{
   /// <summary>
   /// Scales the docking library's layout metrics, which are written for 96 DPI, to the DPI of the display.
   /// </summary>
   /// <remarks>
   /// The application is system DPI aware, so the layout is worked out once for the system DPI. In a process that is
   /// not DPI aware Windows reports 96 DPI and the factor is 1.
   /// </remarks>
   internal static class DockDpi
   {
      private static float? _factor;

      /// <summary>
      /// Gets the ratio of the display's DPI to 96 DPI, e.g. 1.75 at 168 DPI.
      /// </summary>
      public static float Factor
      {
         get
         {
            if (_factor == null)
            {
               try
               {
                  using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                  {
                     _factor = Math.Max(1f, g.DpiX / 96f);
                  }
               }
               catch
               {
                  _factor = 1f;
               }
            }
            return _factor.Value;
         }
      }

      public static int Scale(int value)
      {
         return (int)Math.Round(value * Factor);
      }

      public static Size Scale(Size size)
      {
         return new Size(Scale(size.Width), Scale(size.Height));
      }

      /// <summary>
      /// Draws glyphs that are written in 96 DPI pixel coordinates, relative to <paramref name="origin"/>, at the
      /// display's scale. Dispose the result to restore the graphics state.
      /// </summary>
      public static IDisposable ScaleGlyphs(Graphics graphics, Point origin)
      {
         GraphicsState state = graphics.Save();
         graphics.TranslateTransform(origin.X, origin.Y);
         if (Factor > 1)
         {
            graphics.ScaleTransform(Factor, Factor);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
         }
         return new GraphicsRestorer(graphics, state);
      }

      /// <summary>
      /// Draws an icon into the given bounds; an icon smaller than the bounds is enlarged with a high quality filter
      /// (once, then cached) rather than being stretched blockily.
      /// </summary>
      public static void DrawIcon(Graphics graphics, Icon icon, Rectangle bounds)
      {
         if (icon == null || bounds.Width <= 0 || bounds.Height <= 0)
            return;

         // Unscaled, the icon is drawn as it always was
         if (Factor == 1 || (icon.Width >= bounds.Width && icon.Height >= bounds.Height))
         {
            graphics.DrawIcon(icon, bounds);
            return;
         }

         Bitmap scaled;
         if (!_scaledIcons.TryGetValue(icon, out scaled) || scaled.Size != bounds.Size)
         {
            using (Bitmap source = icon.ToBitmap())
            {
               scaled = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
               using (Graphics g = Graphics.FromImage(scaled))
               using (ImageAttributes attributes = new ImageAttributes())
               {
                  g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                  g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                  attributes.SetWrapMode(WrapMode.TileFlipXY);
                  g.DrawImage(source, new Rectangle(Point.Empty, bounds.Size), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
               }
            }
            _scaledIcons.Remove(icon);
            _scaledIcons.Add(icon, scaled);
         }
         graphics.DrawImage(scaled, bounds);
      }

      private static readonly ConditionalWeakTable<Icon, Bitmap> _scaledIcons = new ConditionalWeakTable<Icon, Bitmap>();

      private sealed class GraphicsRestorer : IDisposable
      {
         private readonly Graphics _graphics;
         private readonly GraphicsState _state;

         public GraphicsRestorer(Graphics graphics, GraphicsState state)
         {
            _graphics = graphics;
            _state = state;
         }

         public void Dispose()
         {
            _graphics.Restore(_state);
         }
      }
   }
}
