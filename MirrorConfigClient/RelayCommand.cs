using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MirrorConfigClient
{
    public class RelayCommand : ICommand, INotifyPropertyChanged
    {
        private Predicate<object> _canExecute;
        private Action<object> _execute;

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
            CanExecuteChanged += delegate (object sender, EventArgs e)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuteable)));
            };
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool IsExecuteable
        {
            get
            {
                return CanExecute(null);
            }
        }
    }
}
