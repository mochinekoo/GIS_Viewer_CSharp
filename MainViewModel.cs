using GIS_Viewer_CSharp;
using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Orientation = System.Windows.Controls.Orientation;

namespace GIS_Viewer_CSharp {
    internal class MainViewModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        public MenuCommand MenuCommand {
            get; private set;
        }
        public ICommand CreateImageButton => createImageButton ??= new Command(RunCreateImageButton);
        public ICommand ChangeBackgroundButton => changeBackgroundColor ??= new Command(RunChangeBackground);
        public ICommand ChangeLineButton => changeLineColor ??= new Command(RunChangeLine);

        private Command createImageButton;
        private Command changeBackgroundColor;
        private Command changeLineColor;
        private System.Windows.Media.ImageSource imageFileName;
        public MainWindow mainWindow { get; private set; }

        public Dictionary<string, Color> backgroundDictionary = new Dictionary<string, Color>();
        public Dictionary<string, Color> lineDictionary = new Dictionary<string, Color>();

        public MainViewModel(MainWindow main) {
            MenuCommand = new MenuCommand(this);
            this.mainWindow = main;
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
            if (!Equals(field, newValue)) {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        public System.Windows.Media.ImageSource ImageFileName {
            get => imageFileName;
            set => SetProperty(ref imageFileName, value);
        }

        private void RunChangeBackground() {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK) {

            }
        }

        private void RunChangeLine() {
        }

        private void RunCreateImageButton() {
            bool isInit = false;
            foreach(UIElement child in mainWindow.DataPanel.Children) {
                if (child is CheckBox checkBox) {
                    var tag = checkBox.Tag;
                    var fileName = MenuCommand.mapLoader.FileName;

                    if (checkBox.IsChecked == false)
                        continue;
                    if (!isInit) {
                        MenuCommand.mapLoader = new MapLoader(fileName, 1000, 1000);
                        isInit = true;
                    }

                    bool filter(IFeature feature) {
                        var key = feature.Attributes
                                            .GetNames().FirstOrDefault(a =>
                                             a.Equals("name", StringComparison.OrdinalIgnoreCase));
                        if (key == null)
                            return false;
                        var val = feature.Attributes[key]?.ToString();
                        return string.Equals(val, tag.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                   
                    MenuCommand.mapLoader.SetBackground(Color.White, filter);
                    MenuCommand.mapLoader.AddLine(Color.Black, 1, filter);
                }
            }

            var image = MenuCommand.mapLoader.Build();
            var hBitmap = image.GetHbitmap();
            var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
            ImageFileName = imageSource;

            StackPanel viewPanel = mainWindow.DataPanel;
            viewPanel.Children.Clear();

            foreach (Feature feature in MenuCommand.mapLoader.FeatureList) {
                var stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                var checkBox1 = new CheckBox();
                checkBox1.Content = feature.Attributes.GetNames()[1] + ":" + feature.Attributes.GetValues()[1];
                checkBox1.Tag = feature.Attributes.GetValues()[1].ToString();

                var button = new Button();
                button.Content = "背景";
                button.Command = ChangeBackgroundButton;

                var button1 = new Button();
                button1.Content = "縁取り";
                button1.Command = ChangeLineButton;

                stackPanel.Children.Add(checkBox1);
                stackPanel.Children.Add(button);
                viewPanel.Children.Add(stackPanel);
            }
        }


    }
}
