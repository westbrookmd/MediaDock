using MediaDock.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsAudio;

namespace MediaDock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Settings variables
        //window settings
        UserSettingsModel settings = new UserSettingsModel();
        string settingsFilePath = Environment.CurrentDirectory + "\\Settings.ini";

        //services settings
        public float volumeSliderInterval;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            UpdateVolumeSlider();
            try
            {
                GetSettings();
                LoadWindowSettings(settings);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load profile. Setting defaults.");
                //set default settings
                SetDefaultSettings();
                //load default settings
                LoadWindowSettings(settings);
            }
            finally
            {
                //startup necessary services from profile/default settings

                //start service to count time and refresh volume
                MasterVolumeUpdater(settings.VolumeSliderUpdateInterval);
            }
        }

        private void GetSettings()
        {
            UserSettingsModel? settingsFile = ReadFromJsonFile<UserSettingsModel>(settingsFilePath);
            if (settingsFile != null)
            {
                settings = settingsFile;
            }
            else
            {
                SetDefaultSettings();
            }
        }

        //https://stackoverflow.com/a/22425211/17573746
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

        //https://stackoverflow.com/a/20623867/17573746
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                //context menu goes here
                ContextMenu contextMenu = new ContextMenu();
                MenuItem menuItemExit = new MenuItem();
                menuItemExit.Header = "Close MediaDock";
                menuItemExit.Click += new RoutedEventHandler(Close_Window);
                contextMenu.Items.Add(menuItemExit);

                MenuItem menuItemSave = new MenuItem();
                menuItemSave.Header = "Save Settings";
                menuItemSave.Click += new RoutedEventHandler(Save_Settings);
                contextMenu.Items.Add(menuItemSave);


                contextMenu.IsOpen = true;
            }
        }

        public void Close_Window(object sender, System.EventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        public void Save_Settings(object sender, System.EventArgs e)
        {

            try
            {
                WriteToJsonFile<UserSettingsModel>(settingsFilePath, settings);
                MessageBox.Show("File Saved Successfully!", "Save Complete");
            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message, exception.Message);
            }
        }

        //https://stackoverflow.com/a/22425211/17573746
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter? writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        private void SetDefaultSettings()
        {
            UserSettingsModel defaultSettings = new UserSettingsModel();
            settings = defaultSettings;
        }
            

        private static void LoadWindowSettings(UserSettingsModel s)
        {
            Window window = (Window)Application.Current.MainWindow;
            window.Topmost = s.WindowIsAlwaysOnTop;
            window.WindowStartupLocation = s.WindowStartupLocation;
            window.ResizeMode = s.WindowResizeMode;
        }

        private void UpdateVolumeSlider()
        {
            //updating UI from thread other than the main thread
            this.Dispatcher.Invoke(() =>
            {
                VolumeSlider.Value = Core.GetMasterVolume();
            });
        }
        private void MediaPrevious(object sender, RoutedEventArgs e)
        {
            Core.PreviousSong();

        }
        private void MediaNext(object sender, RoutedEventArgs e)
        {
            Core.NextSong();
        }
        private void MediaPlayPause(object sender, RoutedEventArgs e)
        {
            Core.PlayPauseSong();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> newVolume)
        {
            try
            {
                Core.ChangeMasterVolume((float)newVolume.NewValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.StackTrace);
            }        
        }
        private void MasterVolumeUpdater(float intervalInSeconds)
        {
            Timer timer = new Timer();
            timer = new Timer(intervalInSeconds*1000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += (sender, e) =>  UpdateVolumeSlider();
            timer.AutoReset = true;
            timer.Enabled = true;

        }
    }
}
