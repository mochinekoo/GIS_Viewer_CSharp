using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace GIS_Viewer_CSharp {
    internal class MenuCommand {

        private readonly MainViewModel mainViewModel;

        public ICommand OpenFileCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public MenuCommand(MainViewModel mainViewModel) {
            this.mainViewModel = mainViewModel;
            OpenFileCommand = new Command(RunOpenFileCommand);
            ExitCommand = new Command(RunExitCommand);
        }

        public void RunOpenFileCommand() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "シェイプファイル(*.shp) | *.shp | すべてのファイル(*.*) | *.*";

            if (dialog.ShowDialog() == true) {

            }
        }

        public void RunExitCommand() {
            Application.Current.Shutdown(0);
        }
    }
}
