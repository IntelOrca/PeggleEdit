using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;

namespace System.Windows.Forms
{
    internal class RibbonOrbMenuItemDesigner
        : RibbonElementWithItemCollectionDesigner
    {
        public override Ribbon Ribbon
        {
            get
            {
                if (Component is RibbonButton)
                {
                    return (Component as RibbonButton).Owner;
                }
                return null;
            }
        }

        public override RibbonItemCollection Collection
        {
            get
            {
                if (Component is RibbonButton)
                {
                    return (Component as RibbonButton).DropDownItems;
                }
                return null;
            }
        }

        protected override DesignerVerbCollection OnGetVerbs()
        {
            return new DesignerVerbCollection(new DesignerVerb[] { 
                new DesignerVerb("Add DescriptionMenuItem", new EventHandler(AddDescriptionMenuItem)),
                new DesignerVerb("Add Separator", new EventHandler(AddSeparator))
            });
        }
    }
}
