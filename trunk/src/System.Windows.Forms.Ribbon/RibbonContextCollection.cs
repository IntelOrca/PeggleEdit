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
    /// Represents a collection of RibbonTabContext
    /// </summary>
    public sealed class RibbonContextCollection
        : List<RibbonContext>
    {
        private Ribbon _owner;

        /// <summary>
        /// Creates a new RibbonTabContext Collection
        /// </summary>
        /// <param name="owner">Ribbon that owns this collection</param>
        /// <exception cref="ArgumentNullException">owner is null</exception>
        internal RibbonContextCollection(Ribbon owner)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            _owner = owner;
        }

        /// <summary>
        /// Gets the Ribbon that owns this collection
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
        /// Adds the specified context to the collection
        /// </summary>
        /// <param name="item">Item to add to the collection</param>
        public new void Add(RibbonContext item)
        {
            item.SetOwner(Owner);
            Owner.Tabs.AddRange(item.Tabs);
            base.Add(item);
        }

        /// <summary>
        /// Adds the specified contexts to the collection
        /// </summary>
        /// <param name="items">Items to add to the collection</param>
        public new void AddRange(System.Collections.Generic.IEnumerable<System.Windows.Forms.RibbonContext> items)
        {
            foreach (RibbonContext c in items)
            {
                c.SetOwner(Owner);

                Owner.Tabs.AddRange(c.Tabs);
            }

            base.AddRange(items);
        }

        /// <summary>
        /// Inserts the specified context into the specified index
        /// </summary>
        /// <param name="index">Desired index of the item into the collection</param>
        /// <param name="item">Tab to be inserted</param>
        public new void Insert(int index, System.Windows.Forms.RibbonContext item)
        {
            item.SetOwner(Owner);

            Owner.Tabs.InsertRange(index, item.Tabs);

            base.Insert(index, item);
        }

        public new void Remove(RibbonContext context)
        {
            base.Remove(context);

            foreach (RibbonTab tab in context.Tabs)
            {
                Owner.Tabs.Remove(tab);
            }
        }

        public new int RemoveAll(Predicate<RibbonContext> predicate)
        {
            throw new ApplicationException("RibbonContextCollectin.RemoveAll function is not supported");
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);

            RibbonContext ctx = this[index];

            foreach (RibbonTab tab in ctx.Tabs)
            {
                Owner.Tabs.Remove(tab);
            }
        }

        public new void RemoveRange(int index, int count)
        {
            throw new ApplicationException("RibbonContextCollection.RemoveRange function is not supported");
        }

        /// <summary>
        /// Sets the value of the Owner Property
        /// </summary>
        internal void SetOwner(Ribbon owner)
        {
            if (owner == null) throw new ArgumentNullException("owner");

            _owner = owner;
        }
    }
}
