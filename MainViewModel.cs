using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GIS_Viewer_CSharp {
    internal class MainViewModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        public MenuCommand MenuCommand {
            get; private set;
        }

        public MainViewModel() {
            MenuCommand = new MenuCommand(this);
        }

    }
}
