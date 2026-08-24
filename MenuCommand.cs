using Microsoft.Win32;
using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;

namespace GIS_Viewer_CSharp {
    internal class MenuCommand {

        private readonly MainViewModel mainViewModel;
        public MapLoader? mapLoader { get; set; }

        public ICommand OpenFileCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public MenuCommand(MainViewModel mainViewModel) {
            this.mainViewModel = mainViewModel;
            OpenFileCommand = new Command(RunOpenFileCommand);
            ExitCommand = new Command(RunExitCommand);
        }

        public void CreateMap(string fileName, Func<IFeature, bool> filter) {
            mapLoader = new MapLoader(fileName, 1000, 1000);

            mapLoader.SetBackground(Color.White, filter);
            mapLoader.AddLine(Color.Black, 1, filter);
            var image = mapLoader.Build();
            var hBitmap = image.GetHbitmap();
            var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
            mainViewModel.ImageFileName = imageSource;

            StackPanel viewPanel = mainViewModel.mainWindow.DataPanel;
            viewPanel.Children.Clear();

            foreach (Feature feature in mapLoader.FeatureList) {
                var checkBox = new CheckBox();
                checkBox.Content = feature.Attributes.GetNames()[1] + ":" + feature.Attributes.GetValues()[1];
                checkBox.Tag = feature.Attributes.GetValues()[1].ToString();
                viewPanel.Children.Add(checkBox);
            }
        }

        public void RunOpenFileCommand() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "シェイプファイル(*.shp) | *.shp | すべてのファイル(*.*) | *.*";

            if (dialog.ShowDialog() == true) {
                Func<IFeature, bool> filter = feature => {
                    return true;
                };
                CreateMap(dialog.FileName, filter);
            }
        }

        public void RunExitCommand() {
            Application.Current.Shutdown(0);
        }
    }
}
