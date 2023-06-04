using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using static CSJ2K.j2k.codestream.HeaderInfo;

namespace IntelOrca.PeggleEdit.Designer.Level_Editor
{
    internal class PenEditorTool : EditorTool
    {
        private List<PointF> _points = new List<PointF>();
        private List<PointKind> _pointKinds = new List<PointKind>();
        private PenState _state;

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
            switch (_state)
            {
                case PenState.Initial:
                    _state = PenState.MoveDown;
                    _points.Add(virtualLocation);
                    _pointKinds.Add(PointKind.MoveTo);
                    _points.Add(virtualLocation);
                    _pointKinds.Add(PointKind.CurveVia);
                    break;
                case PenState.LineUp:
                    _state = PenState.LineDown;
                    break;
                case PenState.CurveUp:
                    _state = PenState.CurveDown2;
                    break;
            }
            MouseMove(button, location, modifierKeys);
        }

        public override void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
        {
            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (_state)
            {
                case PenState.Initial:
                    break;
                case PenState.MoveDown:
                    _points[_points.Count - 1] = virtualLocation;
                    break;
                case PenState.LineUp:
                    _points[_points.Count - 1] = virtualLocation;
                    break;
                case PenState.LineDown:
                    if (_points[_points.Count - 1] != virtualLocation)
                    {
                        _pointKinds[_pointKinds.Count - 1] = PointKind.CurveVia;
                        _points.Add(virtualLocation);
                        _pointKinds.Add(PointKind.CurveTo);
                        _points.Add(virtualLocation);
                        _pointKinds.Add(PointKind.CurveVia);
                        _state = PenState.CurveDown;
                    }
                    break;
                case PenState.CurveDown:
                    var toPoint = _points[_points.Count - 2];
                    _points[_points.Count - 1] = virtualLocation;
                    var delta = new PointF(virtualLocation.X - toPoint.X, virtualLocation.Y - toPoint.Y);
                    _points[_points.Count - 3] = new PointF(
                        toPoint.X - delta.X,
                        toPoint.Y - delta.Y);
                    break;
                case PenState.CurveUp:
                    _points[_points.Count - 1] = virtualLocation;
                    break;
                case PenState.CurveDown2:
                    if (_points[_points.Count - 1] != virtualLocation)
                    {
                        _pointKinds[_pointKinds.Count - 1] = PointKind.CurveVia;
                        _points.Add(virtualLocation);
                        _pointKinds.Add(PointKind.CurveTo);
                        _points.Add(virtualLocation);
                        _pointKinds.Add(PointKind.CurveVia);
                        _state = PenState.CurveDown;
                    }
                    break;
            }
            Editor.UpdateRedraw();
        }

        public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
        {
            if (button == MouseButtons.Right)
            {
                EndDraw();
                return;
            }

            MouseMove(button, location, modifierKeys);

            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (_state)
            {
                case PenState.MoveDown:
                    if (virtualLocation == _points[_points.Count - 2])
                    {
                        // Line to
                        _pointKinds[_points.Count - 1] = PointKind.LineTo;
                    }
                    else
                    {
                        // Curve half
                    }
                    _state = PenState.LineUp;
                    break;
                case PenState.LineDown:
                    _pointKinds.Add(PointKind.LineTo);
                    _points.Add(virtualLocation);
                    _state = PenState.LineUp;
                    break;
                case PenState.CurveDown:
                    _points.Add(virtualLocation);
                    _pointKinds.Add(PointKind.CurveTo);
                    _state = PenState.CurveUp;
                    break;
                case PenState.CurveDown2:
                    _pointKinds.Add(PointKind.LineTo);
                    _points.Add(virtualLocation);
                    _state = PenState.LineUp;
                    break;
            }
            Editor.UpdateRedraw();
        }

        private void EndDraw()
        {
            switch (_state)
            {
                case PenState.LineDown:
                    _points.RemoveAt(_points.Count - 1);
                    _pointKinds.RemoveAt(_pointKinds.Count - 1);
                    break;
                case PenState.CurveDown2:
                    _points.RemoveAt(_points.Count - 1);
                    _pointKinds.RemoveAt(_pointKinds.Count - 1);
                    _points.RemoveAt(_points.Count - 1);
                    _pointKinds.RemoveAt(_pointKinds.Count - 1);
                    break;
            }

            var lastPeg = new PointF(float.MinValue, float.MinValue);
            var elements = GetElements();
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                if (element.IsLine)
                {
                    PegLine(element.P0, element.P1, ref lastPeg);
                }
                else
                {
                    PegCurve(element.P0, element.P1, element.P2, element.P3, ref lastPeg);
                }
            }

            _points.Clear();
            _pointKinds.Clear();
            _state = PenState.Initial;
        }

        private void PegLine(PointF from, PointF to, ref PointF lastPeg)
        {
            var delta = to.Subtract(from);
            var count = (int)(delta.GetLength() / 30) + 1;
            var stepX = delta.X / (count - 1);
            var stepY = delta.Y / (count - 1);
            var posX = (double)from.X;
            var posY = (double)from.Y;

            for (int i = 0; i < count; i++)
            {
                var p = new PointF((float)posX, (float)posY);
                if (p.GetLength(lastPeg) > 30)
                {
                    CreatePeg(p);
                    lastPeg = p;
                }
                posX += stepX;
                posY += stepY;
            }
        }

        private void PegCurve(PointF p0, PointF p1, PointF p2, PointF p3, ref PointF lastPeg)
        {
            var t = 0.0;
            while (t <= 1)
            {
                var p = CalculateCurvePoint(t, p0, p1, p2, p3);
                var lengthFromLastPeg = p.GetLength(lastPeg);
                if (lengthFromLastPeg > 30)
                {
                    CreatePeg(p);
                    lastPeg = p;
                }
                t += 0.01;
            }
        }

        private PointF CalculateCurvePoint(double t, PointF p0, PointF p1, PointF p2, PointF p3)
        {
            var xA = Math.Pow(1 - t, 3) * p0.X;
            var xB = 3 * t * Math.Pow(1 - t, 2) * p1.X;
            var xC = 3 * Math.Pow(t, 2) * (1 - t) * p2.X;
            var xD = Math.Pow(t, 3) * p3.X;
            var x = xA + xB + xC + xD;

            var yA = Math.Pow(1 - t, 3) * p0.Y;
            var yB = 3 * t * Math.Pow(1 - t, 2) * p1.Y;
            var yC = 3 * Math.Pow(t, 2) * (1 - t) * p2.Y;
            var yD = Math.Pow(t, 3) * p3.Y;
            var y = yA + yB + yC + yD;

            return new PointF((float)x, (float)y);
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

        public override void Draw(Graphics g)
        {
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
        }

        public override object Clone()
        {
            return new PenEditorTool(this);
        }

        private PenPathElement[] GetElements()
        {
            var result = new List<PenPathElement>();
            var lastPosition = new PointF();
            var bezier1 = default(PointF?);
            var bezier2 = default(PointF?);
            for (int i = 0; i < _points.Count; i++)
            {
                var pos = _points[i];
                var kind = _pointKinds[i];
                switch (kind)
                {
                    case PointKind.MoveTo:
                        lastPosition = pos;
                        break;
                    case PointKind.CurveVia:
                        if (bezier1 == null)
                        {
                            bezier1 = pos;
                        }
                        else
                        {
                            bezier2 = pos;
                        }
                        break;
                    case PointKind.LineTo:
                        result.Add(new PenPathElement(lastPosition, pos));
                        lastPosition = pos;
                        break;
                    case PointKind.CurveTo:
                        if (bezier2 == null)
                        {
                            result.Add(new PenPathElement(lastPosition, bezier1.Value, pos));
                        }
                        else
                        {
                            result.Add(new PenPathElement(lastPosition, bezier1.Value, bezier2.Value, pos));
                        }
                        lastPosition = pos;
                        bezier1 = null;
                        bezier2 = null;
                        break;
                }
            }
            return result.ToArray();
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
                switch (_state)
                {
                    case PenState.LineDown:
                    case PenState.LineUp:
                        result = SnapAngle(result, _points[_points.Count - 2]);
                        break;
                    case PenState.CurveDown:
                        result = SnapAngle(result, _points[_points.Count - 2]);
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

        private enum PenState
        {
            Initial,    //
            MoveDown,   //       M  Cv
            LineUp,     // ?? ?? ?? Lt
            LineDown,   // ?? ?? ?? Lt
            CurveDown,  // ?? Cv Ct Cv
            CurveUp,    // ?? Cv Cv Ct
            CurveDown2, // ?? Cv Cv Ct
        }

        private enum PointKind : byte
        {
            MoveTo,
            LineTo,
            CurveVia,
            CurveTo,
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

    internal static class PointExtensions
    {
        public static PointF Subtract(this PointF lhs, PointF rhs) => new PointF(lhs.X - rhs.X, lhs.Y - rhs.Y);
        public static float GetLength(this PointF a, PointF b) => b.Subtract(a).GetLength();
        public static float GetLength(this PointF p) => (float)Math.Sqrt((p.X * p.X) + (p.Y * p.Y));
    }

    internal struct PenPathElement
    {
        public bool IsLine { get; }
        public PointF P0 { get; }
        public PointF P1 { get; }
        public PointF P2 { get; }
        public PointF P3 { get; }

        public PenPathElement(PointF p0, PointF p1)
        {
            IsLine = true;
            P0 = p0;
            P1 = p1;
            P2 = default(PointF);
            P3 = default(PointF);
        }

        public PenPathElement(PointF p0, PointF p1, PointF p2)
            : this(
                  p0,
                  new PointF(p1.X * 2 / 3 + p0.X * 1 / 3, p1.Y * 2 / 3 + p0.Y * 1 / 3),
                  new PointF(p1.X * 2 / 3 + p2.X * 1 / 3, p1.Y * 2 / 3 + p2.Y * 1 / 3),
                  p2)
        {
        }

        public PenPathElement(PointF p0, PointF p1, PointF p2, PointF p3)
        {
            IsLine = false;
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }
    }
}
