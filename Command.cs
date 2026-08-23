using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace GIS_Viewer_CSharp {
    internal class Command : ICommand {

        private readonly Action action;
        public event EventHandler? CanExecuteChanged;

        public Command(Action action) {
            this.action = action;
        }

        public bool CanExecute(object? parameter) {
            return true;
        }

        public void Execute(object? parameter) {
            action();
        }
    }
}
