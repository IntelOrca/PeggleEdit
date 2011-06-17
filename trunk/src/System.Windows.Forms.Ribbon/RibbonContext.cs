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
using System.ComponentModel;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents a context on the Ribbon
    /// </summary>
    /// <remarks>Contexts are useful when some tabs are volatile, depending on some selection. A RibbonTabContext can be added to the ribbon by calling Ribbon.Contexts.Add</remarks>
    [ToolboxItem(false)]
    public class RibbonContext
        : Component
    {
        private string _text;
        private System.Drawing.Color _glowColor;
        private Ribbon _owner;
        private RibbonTabCollection _tabs;

        /// <summary>
        /// Creates a new RibbonTabContext
        /// </summary>
        /// <param name="Ribbon">Ribbon that owns the context</param>
        public RibbonContext(Ribbon owner)
        {
            _tabs = new RibbonTabCollection(owner);
        }
        /// <summary>
        /// Gets or sets the text of the Context
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the glow that indicates a context
        /// </summary>
        public System.Drawing.Color GlowColor
        {
            get
            {
                return _glowColor;
            }
            set
            {
                _glowColor = value;
            }
        }

        /// <summary>
        /// Gets the Ribbon that owns this context
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Ribbon Owner
        {
            get
            {
                return _owner;
            }
        }

        public RibbonTabCollection Tabs
        {
            get
            {
                return _tabs;
            }
        }

        /// <summary>
        /// Sets the value of the Owner Property
        /// </summary>
        internal void SetOwner(Ribbon owner)
        {
            _owner = owner;
            _tabs.SetOwner(owner);
        }
    }
}
