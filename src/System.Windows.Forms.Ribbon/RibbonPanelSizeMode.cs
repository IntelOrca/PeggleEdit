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
    /// Represents the size modes that a RibbonElement can be
    /// </summary>
    public enum RibbonElementSizeMode
    {
        /// <summary>
        /// The item is being shown on a dropdown
        /// </summary>
        DropDown = 5,
        
        /// <summary>
        /// Maximum size the element can reach
        /// </summary>
        Large = 4,

        /// <summary>
        /// A medium size for the element when not much space is available
        /// </summary>
        Medium = 3,

        /// <summary>
        /// The minimum size the element can be
        /// </summary>
        Compact = 2,

        /// <summary>
        /// The item doesn't fit as compact, so it must be shown as a single button
        /// </summary>
        Overflow = 1,

        /// <summary>
        /// No size mode specified
        /// </summary>
        None = 0,
    }
}
