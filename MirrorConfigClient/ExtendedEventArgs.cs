using System;

namespace MirrorConfigClient
{
    public class ExtendedEventArgs<T> : EventArgs
    {
        public ExtendedEventArgs(T Item)
            : base()
        {
            this.Item = Item;
        }

        public T Item { get; private set; }
    }
}
