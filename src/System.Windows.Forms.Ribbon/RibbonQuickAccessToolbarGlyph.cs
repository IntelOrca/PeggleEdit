
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design.Behavior;
using System.Drawing;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
    public class RibbonQuickAccessToolbarGlyph
        : Glyph

    {
        BehaviorService _behaviorService;
        Ribbon _ribbon;
        RibbonDesigner _componentDesigner;

        public RibbonQuickAccessToolbarGlyph(BehaviorService behaviorService, RibbonDesigner designer, Ribbon ribbon)
            : base(new RibbonQuickAccessGlyphBehavior(designer, ribbon))
        {
            _behaviorService = behaviorService;
            _componentDesigner = designer;
            _ribbon = ribbon;
        }

        public override Rectangle Bounds
        {
            get
            {
                Point edge = _behaviorService.ControlToAdornerWindow(_ribbon);
                return new Rectangle(
                    edge.X + _ribbon.QuickAcessToolbar.Bounds.Right + _ribbon.QuickAcessToolbar.Bounds.Height / 2 + 4 + _ribbon.QuickAcessToolbar.DropDownButton.Bounds.Width,
                    edge.Y + _ribbon.QuickAcessToolbar.Bounds.Top, _ribbon.QuickAcessToolbar.Bounds.Height, _ribbon.QuickAcessToolbar.Bounds.Height);
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
            using (SolidBrush b = new SolidBrush(Color.FromArgb(50,Color.Blue)))
            {
                pe.Graphics.FillEllipse(b, Bounds); 
            }
            StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            pe.Graphics.DrawString("+", SystemFonts.DefaultFont, Brushes.White, Bounds, sf);
            pe.Graphics.SmoothingMode = smbuff;
        }
    }

    public class RibbonQuickAccessGlyphBehavior
        : Behavior
    {
        Ribbon _ribbon;
        RibbonDesigner _designer;

        public RibbonQuickAccessGlyphBehavior(RibbonDesigner designer, Ribbon ribbon)
        {
            _designer = designer;
            _ribbon = ribbon;
        }

        

        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            _designer.CreateItem(_ribbon, _ribbon.QuickAcessToolbar.Items, typeof(RibbonButton));
            return base.OnMouseUp(g, button);
        }
    }
}
