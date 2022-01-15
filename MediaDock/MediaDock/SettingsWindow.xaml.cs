using MediaDock.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        UserSettingsModel? _settings = Application.Current.Resources["Settings"] as UserSettingsModel;
        public SettingsWindow(ref UserSettingsModel settings)
        {
            InitializeComponent();
            DataContext = settings;
            UpdateUI(settings);
        }

        private void UpdateUI(UserSettingsModel settings)
        {
            AlwaysOnTop.IsChecked = settings.WindowIsAlwaysOnTop;
            StartupLocation.SelectedIndex = ((int)settings.WindowStartupLocation);
            ResizeMode.SelectedIndex = ((int)settings.WindowResizeMode);
            VolumeUpdateInterval.Value = ((int)settings.VolumeSliderUpdateInterval);
        }

        public void Save_Settings(object sender, System.EventArgs e)
        {
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Load a Media Dock Settings File";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if(openFileDialog.ShowDialog() == true)
            {
                string settingsFilePath = openFileDialog.FileName;
                UserSettingsModel? settingsFile = ReadFromJsonFile<UserSettingsModel>(settingsFilePath);
                if (settingsFile != null)
                {
                    _settings = settingsFile;
                    UpdateUI(_settings);
                }
                else
                {
                    //something here
                    MessageBox.Show("File did not load as a settings model properly. Has the format changed?", "Unable to Load");
                }
            }
            else
            {
                MessageBox.Show("Unable to load selected file.", "Unable to load");
            }
        }
        public void Cancel_Settings(object sender, System.EventArgs e)
        {
            DialogResult = false;
        }

        public static T? ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader? reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
