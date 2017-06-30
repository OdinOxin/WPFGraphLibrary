using MirrorConfigClient.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MirrorConfigClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConfigClient : Window, ChangeTracker
    {
        #region [ctor]
        public ConfigClient()
        {
            InitializeComponent();
            DataContext = this;

            Undoables = new Stack<Transaction>();
            Redoables = new Stack<Transaction>();
            UndoCmd = new RelayCommand((x) => Undoables.Count > 0, (x) => Undo());
            RedoCmd = new RelayCommand((x) => Redoables.Count > 0, (x) => Redo());

            Graph.ChangeTracker = this;
            StoryDetails.ChangeTracker = this;
        }
        #endregion

        #region [Events]
        
        #region popUpAbout_Close
        private void popUpAbout_Close(object sender, EventArgs e)
        {
            btnAbout.IsChecked = false;
        }
        #endregion
        
        #endregion

        #region [Methods]
        
        #region Undo
        public void Undo()
        {
            if (Undoables.Count <= 0)
                return;
            Transaction Transaction = Undoables.Pop();
            Transaction.Undo?.Invoke();
            Redoables.Push(Transaction);
        }
        #endregion

        #region Redo
        private void Redo()
        {
            if (Redoables.Count <= 0)
                return;
            Transaction Transaction = Redoables.Pop();
            Transaction.Redo?.Invoke();
            Undoables.Push(Transaction);
        }
        #endregion
        
        #region AddUndoable
        public void AddUndoable(Transaction Undoable)
        {
            Redoables.Clear();
            Undoables.Push(Undoable);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region ForgetEverything
        public void ForgetEverything()
        {
            Undoables.Clear();
            Redoables.Clear();
        }
        #endregion
        
        #endregion

        #region [Properties]

        #region Undoables
        public Stack<Transaction> Undoables { get; private set; }
        #endregion

        #region Redoables
        public Stack<Transaction> Redoables { get; private set; }
        #endregion

        #region [Commands]
        
        #region UndoCmd
        public ICommand UndoCmd { get; private set; }
        #endregion

        #region RedoCmd
        public ICommand RedoCmd { get; private set; }
        #endregion

        #endregion
        
        #endregion
    }
}
