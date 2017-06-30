using System;
using System.Windows;

namespace MirrorConfigClient.ChangeTracking
{
    public class Transaction
    {
        #region ctor
        public Transaction(Action Undo, Action Redo, object Source, FrameworkElement Associated)
        {
            this.Undo = Undo;
            this.Redo = Redo;
            this.Source = Source;
            this.Associated = Associated;
        }
        #endregion

        #region Properties
        public Action Undo { get; private set; }
        public Action Redo { get; private set; }
        public object Source { get; private set; }
        public FrameworkElement Associated { get; private set; }
        #endregion
    }
}
