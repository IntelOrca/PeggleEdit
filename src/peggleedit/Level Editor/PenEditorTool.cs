using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Crom.Controls.Docking;
using static System.Windows.Forms.AxHost;

namespace IntelOrca.PeggleEdit.Designer.Level_Editor
{
    internal class PenEditorTool : EditorTool
    {
        private List<PointF> _points = new List<PointF>();
        private List<PointKind> _pointKinds = new List<PointKind>();
        private PointF _p0;
        private PointF _p1;
        private PointF _p2;
        private PointF _p1other;
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
                case PenState.CurveUp:
                    _pointKinds[_pointKinds.Count - 1] = PointKind.CurveVia;
                    _points.Add(virtualLocation);
                    _pointKinds.Add(PointKind.CurveTo);
                    _points.Add(virtualLocation);
                    _pointKinds.Add(PointKind.CurveVia);
                    _state = PenState.Down;
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
                case PenState.Down:
                    var toPoint = _points[_points.Count - 2];
                    _points[_points.Count - 1] = virtualLocation;
                    _points[_points.Count - 3] = new PointF(
                        toPoint.X - (virtualLocation.X - toPoint.X),
                        toPoint.Y - (virtualLocation.Y - toPoint.Y));
                    break;
                case PenState.CurveUp:
                    _points[_points.Count - 1] = virtualLocation;
                    break;
            }
            Editor.UpdateRedraw();
        }

        public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
        {
            MouseMove(button, location, modifierKeys);

            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (_state)
            {
                case PenState.MoveDown:
                    _points[_points.Count - 1] = virtualLocation;
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
                case PenState.Down:
                    if (_points[_points.Count - 1] == _points[_points.Count - 2])
                    {
                        // Line
                        _pointKinds[_points.Count - 1] = PointKind.LineTo;
                        _state = PenState.LineUp;
                    }
                    else
                    {
                        // Curve
                        _points.Add(virtualLocation);
                        _pointKinds.Add(PointKind.CurveTo);
                        _state = PenState.CurveUp;
                    }
                    break;
            }
            Editor.UpdateRedraw();
        }

        public override void Draw(Graphics g)
        {
            if (_points.Count == 0)
                return;

            var drawingIndex = _pointKinds.FindLastIndex(x => x == PointKind.LineTo || x == PointKind.CurveTo);

            var path = new GraphicsPath();
            var lastPosition = new PointF();
            var bezier1 = default(PointF?);
            var bezier2 = default(PointF?);
            for (int i = 0; i < _points.Count; i++)
            {
                var pen = Pens.White;
                if (i == drawingIndex)
                {
                    pen = Pens.Red;
                }

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
                        g.DrawLine(pen,
                            GetVisualLocation(lastPosition),
                            GetVisualLocation(pos));
                        lastPosition = pos;
                        break;
                    case PointKind.CurveTo:
                        if (bezier2 == null)
                        {
                            g.DrawBezier(pen,
                                GetVisualLocation(lastPosition),
                                GetVisualLocation(bezier1.Value),
                                GetVisualLocation(pos));
                        }
                        else
                        {
                            g.DrawBezier(pen,
                                GetVisualLocation(lastPosition),
                                GetVisualLocation(bezier1.Value),
                                GetVisualLocation(bezier2.Value),
                                GetVisualLocation(pos));
                        }
                        lastPosition = pos;
                        bezier1 = null;
                        bezier2 = null;
                        break;
                }
            }
            // g.DrawPath(Pens.White, path);

            if (_state == PenState.Down)
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
            return result;
        }

        private enum PenState
        {
            Initial,    //
            MoveDown,   //       M  Cv
            LineUp,     // ?? ?? ?? Lt
            Down,       // ?? Cv Ct Cv
            CurveUp,    // Cv Ct Cv Ct
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
}
