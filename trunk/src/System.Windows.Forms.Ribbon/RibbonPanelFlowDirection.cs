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

namespace System.Windows.Forms
{
 
    /// <summary>
    /// Represents possible flow directions of items on the panels
    /// </summary>
    public enum RibbonPanelFlowDirection
    {
        /// <summary>
        /// Layout of items flows to the left, then down
        /// </summary>
        Right = 1,
        /// <summary>
        /// Layout of items flows to the bottom, then to the right
        /// </summary>
        Bottom = 0,
    }
}
