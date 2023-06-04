using System;
using System.Drawing;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using static IntelOrca.PeggleEdit.Tools.Levels.BezierPath;

namespace IntelOrca.PeggleEdit.Designer.Level_Editor
{
    internal class PenEditorTool : EditorTool
    {
        private BrickCurveGenerator _entry;

        public BezierPath Path => _entry?.BezierPath;

        public PenEditorTool()
        {
        }

        private PenEditorTool(PenEditorTool original)
        {
        }

        public override void Activate()
        {
            base.Activate();

            Editor.ClearSelection();
            Editor.UpdateRedraw();
        }

        public override void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
        {
            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (Path?.State)
            {
                case null:
                case PenState.Initial:
                    _entry = new BrickCurveGenerator(Editor.Level);
                    Editor.Level.Entries.Add(_entry);

                    Path.State = PenState.MoveDown;
                    Path.PushPoint(PointKind.MoveTo, virtualLocation);
                    Path.PushPoint(PointKind.CurveVia, virtualLocation);
                    break;
                case PenState.LineUp:
                    Path.State = PenState.LineDown;
                    break;
                case PenState.CurveUp:
                    Path.State = PenState.CurveDown2;
                    break;
            }
            MouseMove(button, location, modifierKeys);
        }

        public override void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
        {
            if (_entry == null)
                return;

            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (Path.State)
            {
                case PenState.Initial:
                    break;
                case PenState.MoveDown:
                    Path.SetPosition(-1, virtualLocation);
                    break;
                case PenState.LineUp:
                    Path.SetPosition(-1, virtualLocation);
                    break;
                case PenState.LineDown:
                    if (Path.GetPosition(-1) != virtualLocation)
                    {
                        Path.SetKind(-1, PointKind.CurveVia);
                        Path.PushPoint(PointKind.CurveTo, virtualLocation);
                        Path.PushPoint(PointKind.CurveVia, virtualLocation);
                        Path.State = PenState.CurveDown;
                    }
                    break;
                case PenState.CurveDown:
                    var toPoint = Path.GetPosition(-2);
                    Path.SetPosition(-1, virtualLocation);
                    var delta = new PointF(virtualLocation.X - toPoint.X, virtualLocation.Y - toPoint.Y);
                    Path.SetPosition(-3, new PointF(
                        toPoint.X - delta.X,
                        toPoint.Y - delta.Y));
                    break;
                case PenState.CurveUp:
                    Path.SetPosition(-1, virtualLocation);
                    break;
                case PenState.CurveDown2:
                    if (Path.GetPosition(-1) != virtualLocation)
                    {
                        Path.SetKind(-1, PointKind.CurveVia);
                        Path.PushPoint(PointKind.CurveTo, virtualLocation);
                        Path.PushPoint(PointKind.CurveVia, virtualLocation);
                        Path.State = PenState.CurveDown;
                    }
                    break;
            }
            Editor.UpdateRedraw();
        }

        public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
        {
            if (_entry == null)
                return;

            if (button == MouseButtons.Right)
            {
                EndDraw();
                Editor.UpdateRedraw();
                return;
            }

            MouseMove(button, location, modifierKeys);

            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (Path.State)
            {
                case PenState.MoveDown:
                    if (virtualLocation == Path.GetPosition(-2))
                    {
                        // Line to
                        Path.SetKind(-1, PointKind.LineTo);
                    }
                    else
                    {
                        // Curve half
                    }
                    Path.State = PenState.LineUp;
                    break;
                case PenState.LineDown:
                    Path.PushPoint(PointKind.LineTo, virtualLocation);
                    Path.State = PenState.LineUp;
                    break;
                case PenState.CurveDown:
                    Path.PushPoint(PointKind.CurveTo, virtualLocation);
                    Path.State = PenState.CurveUp;
                    break;
                case PenState.CurveDown2:
                    Path.PushPoint(PointKind.LineTo, virtualLocation);
                    Path.State = PenState.LineUp;
                    break;
            }
            Editor.UpdateRedraw();
        }

        private void EndDraw()
        {
            switch (Path.State)
            {
                case PenState.LineDown:
                    Path.PopPoint();
                    break;
                case PenState.CurveDown:
                    Path.PopPoint();
                    Path.PopPoint();
                    Path.PopPoint();
                    Path.PopPoint();
                    break;
                case PenState.CurveDown2:
                    Path.PopPoint();
                    Path.PopPoint();
                    break;
            }

            // var lastPosition = new PointF(float.MinValue, float.MinValue);
            // var lastAngle = 0.0f;
            // var elements = GetElements();
            // for (var i = 0; i < elements.Length; i++)
            // {
            //     PegCurve(in elements[i], ref lastPosition, ref lastAngle);
            // }

            Path.State = PenState.Initial;
            _entry = null;
        }

        private void PegCurve(in PenPathElement element, ref PointF lastPoint, ref float lastAngle)
        {
            var tStep = 0.005;
            if (lastPoint == new PointF(float.MinValue, float.MinValue))
            {
                lastPoint = element.GetPoint(0);
                lastAngle = GetAngle(element.GetPoint(0), element.GetPoint(tStep));
            }
            var lastPointBackup = lastPoint;

            var lengthDiff = 30;

            {
                var t = 0.0;
                while (t <= 1)
                {
                    var pPrev = element.GetPoint(t - tStep);
                    var p = element.GetPoint(t);
                    var pNext = element.GetPoint(t + tStep);
                    var lengthFromLastPeg = p.GetLength(lastPoint);
                    if (lengthFromLastPeg > lengthDiff)
                    {
                        // var delta = pNext.Subtract(p);
                        // var rotation = (float)(-Math.Atan2(delta.Y, delta.X) + (Math.PI / 2));

                        // CreatePeg(p);
                        // CreateBrick(p, rotation);
                        var currentAngle = GetAngle(p, pNext);
                        CreateBrick(lastPoint, lastAngle, p, currentAngle);
                        lastPoint = p;
                        lastAngle = currentAngle;
                    }
                    t += tStep;
                }
            }
            // {
            //     var lp = lastPointBackup;
            //     var t = 0.0;
            //     while (t <= 1)
            //     {
            //         var p = element.GetPoint(t);
            //         var lengthFromLastPeg = p.GetLength(lp);
            //         if (lengthFromLastPeg > lengthDiff)
            //         {
            //             CreateMarker(lp);
            //             lp = p;
            //         }
            //         t += 0.01;
            //     }
            // }
        }

        private static float GetAngle(PointF p0, PointF p1)
        {
            var delta = p1.Subtract(p0);
            var rotation = (float)(-Math.Atan2(delta.Y, delta.X) + (Math.PI / 2));
            return rotation;
        }

        private void CreatePeg(PointF pos)
        {
            var level = Editor.Level;
            var p = new Circle(level);
            p.PegInfo = new PegInfo(p, true, false);
            p.X = (int)Math.Round(pos.X);
            p.Y = (int)Math.Round(pos.Y);
            level.Entries.Add(p);
        }

        private void CreateMarker(PointF pos)
        {
            var level = Editor.Level;
            var p = new Circle(level);
            p.Radius = 4;
            p.X = (int)Math.Round(pos.X);
            p.Y = (int)Math.Round(pos.Y);
            level.Entries.Add(p);
        }

        private void CreateBrick(PointF pos, float angle)
        {
            var level = Editor.Level;
            var brick = new Brick(level);
            brick.PegInfo = new PegInfo(brick, true, false);
            brick.X = (int)Math.Round(pos.X);
            brick.Y = (int)Math.Round(pos.Y);
            brick.Rotation = MathExt.ToDegrees(angle);
            brick.SectorAngle = 40;
            brick.Curved = true;
            level.Entries.Add(brick);
        }

        private void CreateBrick(PointF left, float leftAngle, PointF right, float rightAngle)
        {
            var level = Editor.Level;
            var brick = new Brick(level);
            brick.PegInfo = new PegInfo(brick, true, false);

            if (Math.Abs(leftAngle - rightAngle) < (1 / 180.0 * Math.PI))
            {
                brick.Length = right.GetLength(left);
                brick.Location = left.Add(right.Subtract(left));
                brick.Rotation = MathExt.ToDegrees(leftAngle);
            }
            else
            {
                var b = left.GetLength(right);
                var sectorAngle = rightAngle - leftAngle;
                var midAngle = leftAngle + (sectorAngle / 2);
                var radius = b / (2 * Math.Sin(sectorAngle / 2));
                var origin = new PointF(
                    (float)(left.X + (Math.Cos(leftAngle) * radius)),
                    (float)(right.Y - (Math.Sin(rightAngle) * radius)));
                var midPoint = new PointF(
                    (float)(origin.X - (Math.Cos(midAngle) * radius)),
                    (float)(origin.Y + (Math.Sin(midAngle) * radius)));

                brick.Location = midPoint;
                brick.Rotation = MathExt.ToDegrees((float)(midAngle + Math.PI));
                brick.SectorAngle = MathExt.ToDegrees(Math.Abs(sectorAngle));
                brick.InnerRadius = (float)(radius - 10);
                brick.Curved = true;
            }
            level.Entries.Add(brick);

            // var rod = new Rod(level);
            // rod.PointA = left;
            // rod.PointB = origin;
            // level.Entries.Add(rod);
        }

        public override void Draw(Graphics g)
        {
            /*
            var elements = GetElements();
            for (int i = 0; i < elements.Length; i++)
            {
                var pen = i == elements.Length - 1 ? Pens.Red : Pens.White;
                var el = elements[i];
                if (el.IsLine)
                {
                    g.DrawLine(pen,
                        GetVisualLocation(el.P0),
                        GetVisualLocation(el.P1));
                }
                else
                {
                    g.DrawBezier(pen,
                        GetVisualLocation(el.P0),
                        GetVisualLocation(el.P1),
                        GetVisualLocation(el.P2),
                        GetVisualLocation(el.P3));
                }
            }

            if (_state == PenState.CurveDown)
            {
                // Cv Ct Cv
                var p0 = GetVisualLocation(_points[_points.Count - 3]);
                var p1 = GetVisualLocation(_points[_points.Count - 2]);
                var p2 = GetVisualLocation(_points[_points.Count - 1]);

                g.FillEllipse(Brushes.Cyan, p0.X - 4, p0.Y - 4, 8, 8);
                g.FillEllipse(Brushes.Cyan, p2.X - 4, p2.Y - 4, 8, 8);
                g.DrawLine(Pens.Cyan, p0, p2);
            }
            else if (_state == PenState.CurveUp)
            {
                // Cv Ct Cv Ct
                var p0 = GetVisualLocation(_points[_points.Count - 4]);
                var p1 = GetVisualLocation(_points[_points.Count - 3]);
                var p2 = GetVisualLocation(_points[_points.Count - 2]);
                var p3 = GetVisualLocation(_points[_points.Count - 1]);

                g.FillEllipse(Brushes.Cyan, p0.X - 4, p0.Y - 4, 8, 8);
                g.FillEllipse(Brushes.Cyan, p2.X - 4, p2.Y - 4, 8, 8);
                g.DrawLine(Pens.Cyan, p0, p2);
            }

            // for (int i = 0; i < _points.Count; i++)
            // {
            //     var c = Color.White;
            //     switch (_pointKinds[i])
            //     {
            //         case PointKind.MoveTo:
            //             c = Color.Green;
            //             break;
            //         case PointKind.LineTo:
            //             c = Color.White;
            //             break;
            //         case PointKind.CurveTo:
            //             c = Color.Red;
            //             break;
            //         case PointKind.CurveVia:
            //             c = Color.Cyan;
            //             break;
            //     }
            //     var b = new SolidBrush(c);
            //     var p = GetVisualLocation(_points[i]);
            //     g.FillEllipse(b, p.X - 4, p.Y - 4, 8, 8);
            // }
            */
        }

        public override object Clone()
        {
            return new PenEditorTool(this);
        }

        private PointF GetVisualLocation(PointF location)
        {
            return Editor.Level.GetActualXY(location);
        }

        private PointF GetVirtualLocation(Point location, Keys modifierKeys)
        {
            // Snap
            var result = (PointF)Editor.Level.GetVirtualXY(location);
            if (Settings.Default.ShowGrid & Settings.Default.SnapToGrid)
            {
                result = new PointF(Editor.SnapToGrid(location.X), Editor.SnapToGrid(location.Y));
            }

            if ((modifierKeys & Keys.Control) != 0)
            {
                switch (Path?.State)
                {
                    case PenState.LineDown:
                    case PenState.LineUp:
                        result = SnapAngle(result, Path.GetPosition(-2));
                        break;
                    case PenState.CurveDown:
                        result = SnapAngle(result, Path.GetPosition(-2));
                        break;
                }
            }
            return result;
        }

        private static PointF SnapAngle(PointF point, PointF origin)
        {
            const int snapPoints = 16;
            var delta = point.Subtract(origin);
            var magnitude = delta.GetLength();
            var angle = Math.Atan2(delta.Y, delta.X);
            var snapAngle = ((int)Math.Round(((angle + Math.PI) / (2 * Math.PI)) * snapPoints) / (double)snapPoints) * 2 * Math.PI;
            delta = new PointF(
                -(int)(Math.Cos(snapAngle) * magnitude),
                -(int)(Math.Sin(snapAngle) * magnitude));
            var newX = origin.X + delta.X;
            var newY = origin.Y + delta.Y;
            return new PointF((int)Math.Round(newX), (int)Math.Round(newY));
        }
    }

    static class GraphicsExtensions
    {
        public static void DrawBezier(this Graphics g, Pen pen, PointF p0, PointF p1, PointF p2)
        {
            // [P1, (C*2/3 + P1 * 1/3), (C*2/3 + P2 * 1/3), P2]
            var newP0 = p0;
            var newP1 = new PointF(p1.X * 2 / 3 + p0.X * 1 / 3, p1.Y * 2 / 3 + p0.Y * 1 / 3);
            var newP2 = new PointF(p1.X * 2 / 3 + p2.X * 1 / 3, p1.Y * 2 / 3 + p2.Y * 1 / 3);
            var newP3 = p2;
            g.DrawBezier(pen, newP0, newP1, newP2, newP3);
        }
    }
}
