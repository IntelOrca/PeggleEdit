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
    /// Represents a collection of RibbonTab objects
    /// </summary>
    public sealed class RibbonTabCollection
           : List<RibbonTab>
    {
        private Ribbon _owner;

        /// <summary>
        /// Creates a new RibbonTabCollection
        /// </summary>
        /// <param name="owner">|</param>
        /// <exception cref="AgrumentNullException">owner is null</exception>
        internal RibbonTabCollection(Ribbon owner)
        {
            if (owner == null) throw new ArgumentNullException("null");

            _owner = owner;
        }

        /// <summary>
        /// Gets the Ribbon that owns this tab
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Ribbon Owner
        {
            get
            {
                return _owner;
            }
        }

        /// <summary>
        /// Adds the specified item to the collection
        /// </summary>
        /// <param name="item">Item to add to the collection</param>
        public new void Add(RibbonTab item)
        {
            item.SetOwner(Owner);
            base.Add(item);

            Owner.OnRegionsChanged();
        }

        /// <summary>
        /// Adds the specified items to the collection
        /// </summary>
        /// <param name="items">Items to add to the collection</param>
        public new void AddRange(System.Collections.Generic.IEnumerable<System.Windows.Forms.RibbonTab> items)
        {
            foreach (RibbonTab tab in items)
            {
                tab.SetOwner(Owner);
            }

            base.AddRange(items);

            Owner.OnRegionsChanged();
        }

        /// <summary>
        /// Inserts the specified item into the specified index
        /// </summary>
        /// <param name="index">Desired index of the item into the collection</param>
        /// <param name="item">Tab to be inserted</param>
        public new void Insert(int index, System.Windows.Forms.RibbonTab item)
        {
            item.SetOwner(Owner);

            base.Insert(index, item);

            Owner.OnRegionsChanged();
        }

        public new void Remove(RibbonTab context)
        {
            base.Remove(context);
            Owner.OnRegionsChanged();
        }

        public new int RemoveAll(Predicate<RibbonTab> predicate)
        {
            throw new ApplicationException("RibbonTabCollection.RemoveAll function is not supported");
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            Owner.OnRegionsChanged();
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            Owner.OnRegionsChanged();
        }

        /// <summary>
        /// Sets the value of the Owner Property
        /// </summary>
        internal void SetOwner(Ribbon owner)
        {
            _owner = owner;
        }
    }
}
