using System;
using System.Windows.Input;

namespace DemoApp.Common
{
    public  class DelegateCommand : ICommand
    {
        public  event EventHandler CanExecuteChanged;
        public Func<object, bool> _canExecute;
        public Action<object> _executeAction;

        public  DelegateCommand(Action<object> executeAction)
            : this(executeAction, null)
        {
        }

        public  DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            if (executeAction == null) throw new ArgumentNullException("executeAction");

            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        public  bool CanExecute(object parameter)
        {
            var result = true;
            var canExecuteHandler = _canExecute;
            if (canExecuteHandler != null)
                result = canExecuteHandler(parameter);

            return result;
        }

        public  void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public  void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
