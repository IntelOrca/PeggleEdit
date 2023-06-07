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
        private PegKind _pegKind;
        private CurveGenerator _entry;

        public BezierPath Path => _entry?.BezierPath;

        public PenEditorTool(PegKind pegKind)
        {
            _pegKind = pegKind;
        }

        private PenEditorTool(PenEditorTool original)
        {
            _pegKind = original._pegKind;
            _entry = original._entry;
        }

        public override void Activate()
        {
            base.Activate();

            Editor.ClearSelection();
            Editor.UpdateRedraw();
        }

        public override void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
        {
            if (button != MouseButtons.Left)
                return;

            var virtualLocation = GetVirtualLocation(location, modifierKeys);
            switch (Path?.State)
            {
                case null:
                case PenState.Initial:
                    _entry = _pegKind == PegKind.Circle ?
                        (CurveGenerator)new PegCurveGenerator(Editor.Level) :
                        (CurveGenerator)new BrickCurveGenerator(Editor.Level);
                    Editor.Level.Entries.Add(_entry);
                    Editor.ClearSelection();
                    Editor.AddToSelection(_entry);

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
                case PenState.LineUp:
                case PenState.LineDown:
                    Path.PopPoint();
                    break;
                case PenState.CurveDown:
                    Path.PopPoint();
                    Path.PopPoint();
                    Path.PopPoint();
                    Path.PopPoint();
                    break;
                case PenState.CurveUp:
                case PenState.CurveDown2:
                    Path.PopPoint();
                    Path.PopPoint();
                    break;
            }
            Path.State = PenState.Initial;

            if (Path.GetElements().Length == 0)
            {
                Editor.Level.Entries.Remove(_entry);
            }
            else
            {
                _entry.RecalculateOrigin();
            }

            _entry = null;
        }

        public override object Clone()
        {
            return new PenEditorTool(this);
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
}
