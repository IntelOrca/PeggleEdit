using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design.Behavior;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
    public class RibbonTabGlyph
        : Glyph
    {
        BehaviorService _behaviorService;
        Ribbon _ribbon;
        RibbonDesigner _componentDesigner;
        Size size;

        public RibbonTabGlyph(BehaviorService behaviorService, RibbonDesigner designer, Ribbon ribbon)
            : base(new RibbonTabGlyphBehavior(designer, ribbon))
        {
            _behaviorService = behaviorService;
            _componentDesigner = designer;
            _ribbon = ribbon;
            size = new Size(60, 16);
        }

        public override Rectangle Bounds
        {
            get
            {
                Point edge = _behaviorService.ControlToAdornerWindow(_ribbon);
                Point tab = new Point(5,_ribbon.OrbBounds.Bottom + 5 );

                //If has tabs
                if (_ribbon.Tabs.Count > 0)
                {
                    //Place glyph next to the last tab
                    RibbonTab t = _ribbon.Tabs[_ribbon.Tabs.Count - 1];
                    tab.X = t.Bounds.Right + 5;
                    tab.Y = t.Bounds.Top + 2;
                }

                return new Rectangle(
                    edge.X + tab.X,
                    edge.Y + tab.Y, 
                    size.Width , size.Height);
            }
        }

        public override Cursor GetHitTest(System.Drawing.Point p)
        {
            if (Bounds.Contains(p))
            {
                return Cursors.Hand;
            }

            return null;
        }


        public override void Paint(PaintEventArgs pe)
        {
            SmoothingMode smbuff = pe.Graphics.SmoothingMode;
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = RibbonProfessionalRenderer.RoundRectangle(Bounds, 2))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(50, Color.Blue)))
                {
                    pe.Graphics.FillPath(b, p);
                } 
            }
            StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            pe.Graphics.DrawString("Add Tab", SystemFonts.DefaultFont, Brushes.White, Bounds, sf);
            pe.Graphics.SmoothingMode = smbuff;
        }
    }

    public class RibbonTabGlyphBehavior
        : Behavior
    {
        Ribbon _ribbon;
        RibbonDesigner _designer;

        public RibbonTabGlyphBehavior(RibbonDesigner designer, Ribbon ribbon)
        {
            _designer = designer;
            _ribbon = ribbon;
        }



        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            _designer.AddTabVerb(this, EventArgs.Empty);
            return base.OnMouseUp(g, button);
        }
    }
}
