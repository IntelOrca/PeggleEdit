/*
 
 2008 José Manuel Menéndez Poo
 * 
 * Please give me credit if you use this code. It's all I ask.
 * 
 * Contact me for more info: menendezpoo@gmail.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Windows.Forms.Design.Behavior;

namespace System.Windows.Forms
{
    public class RibbonTabDesigner
        : ComponentDesigner
    {
        Adorner panelAdorner;

        public override DesignerVerbCollection Verbs
        {
            get
            {
                return new DesignerVerbCollection(new DesignerVerb[] { 
                    new DesignerVerb("Add Panel", new EventHandler(AddPanel))
                });
            }
        }

        public RibbonTab Tab
        {
            get { return Component as RibbonTab; }
        }

        public void AddPanel(object sender, EventArgs e)
        {
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;

            if (host != null && Tab != null)
            {
                

                DesignerTransaction transaction = host.CreateTransaction("AddPanel" + Component.Site.Name);
                MemberDescriptor member = TypeDescriptor.GetProperties(Component)["Panels"];
                base.RaiseComponentChanging(member);

                RibbonPanel panel = host.CreateComponent(typeof(RibbonPanel)) as RibbonPanel;

                if (panel != null)
                {
                    panel.Text = panel.Site.Name;
                    Tab.Panels.Add(panel);
                    Tab.Owner.OnRegionsChanged();
                }

                base.RaiseComponentChanged(member, null, null);
                transaction.Commit();
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            panelAdorner = new Adorner();

            BehaviorService bs = RibbonDesigner.Current.GetBehaviorService();

            if (bs == null) return;

            bs.Adorners.AddRange(new Adorner[] { panelAdorner });

            panelAdorner.Glyphs.Add(new RibbonPanelGlyph(bs, this, Tab));
        }
    }
}
