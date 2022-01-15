using MediaDock.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MediaDock
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(ref UserSettingsModel settings)
        {
            InitializeComponent();
            DataContext = settings;
            AlwaysOnTop.IsChecked = settings.WindowIsAlwaysOnTop;
            StartupLocation.SelectedIndex = ((int)settings.WindowStartupLocation);
            ResizeMode.SelectedIndex = ((int)settings.WindowResizeMode);
            VolumeUpdateInterval.Value = ((int)settings.VolumeSliderUpdateInterval);
        }
        public void Save_Settings(object sender, System.EventArgs e)
        {
            UserSettingsModel? _settings = Application.Current.Resources["Settings"] as UserSettingsModel;
            if (_settings != null)
            {
                _settings.WindowIsAlwaysOnTop  = (bool)AlwaysOnTop.IsChecked;
                _settings.WindowStartupLocation  = (WindowStartupLocation)StartupLocation.SelectedIndex;
                _settings.WindowResizeMode  = (ResizeMode)ResizeMode.SelectedIndex;
                _settings.VolumeSliderUpdateInterval  = (float)VolumeUpdateInterval.Value;
            }
            Application.Current.Resources.Remove("Settings");
            Application.Current.Resources.Add("Settings", _settings);
            DialogResult = true;
        }
        public void Load_Settings(object sender, System.EventArgs e)
        {
            //stuff here
        }
        public void Cancel_Settings(object sender, System.EventArgs e)
        {
            DialogResult = false;
        }
    }
}
