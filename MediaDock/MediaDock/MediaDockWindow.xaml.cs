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
using Microsoft.AspNetCore.SignalR.Client;

namespace MediaDock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Settings variables
        //window settings
        public UserSettingsModel settings = new UserSettingsModel();
        public string settingsFilePath = Environment.CurrentDirectory + "\\Settings.ini";

        //services settings
        public float volumeSliderInterval;

        //Service
        Timer volumeUpdater;

        HubConnection ?connection;

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
                volumeUpdater = MasterVolumeUpdater(settings.VolumeSliderUpdateInterval);
                if(settings.RemoteControl)
                {
                    connection = new HubConnectionBuilder()
                    .WithAutomaticReconnect()
                    // TODO: put in keyvault or something else
                    .WithUrl(settings.ConnectionString)
                    .Build();
                    if(settings.AutomaticallyConnect)
                    {
                        connection.StartAsync();
                    }
                    connection.On<float>("BroadcastVolume", BroadcastVolume);
                    connection.On<bool>("BroadcastIsPlaying", BroadcastIsPlaying);
                    // TODO: implement in mediacontrolshub and Blazor Server
                    //connection.On<bool>("BroadcastPreviousSong", BroadcastPreviousSong);
                    //connection.On<bool>("BroadcastNextSong", BroadcastNextSong);
                }
            }
        }

        private void BroadcastVolume(float newVolume)
        {
            Console.WriteLine("WPF" + " New volume of : " + newVolume);
            VolumeSlider.Value = newVolume;
            UpdateVolumeSlider();
        }
        private void BroadcastIsPlaying(bool newPlayingStatus)
        {
            Console.WriteLine("WPF" + " New playing status of : " + newPlayingStatus);
            // TODO: change playing to the proper state
            Core.PlayPauseSong();
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

                //signalr connection
                if(connection != null && connection.State == HubConnectionState.Disconnected)
                {
                    MenuItem menuItemConnect = new MenuItem
                    {
                        Header = "Connect"
                    };
                    menuItemConnect.Click += Start_Connection;
                    contextMenu.Items.Add(menuItemConnect);
                }
                else if(connection != null && connection.State == HubConnectionState.Connected)
                {
                    MenuItem menuItemDisconnect = new MenuItem
                    {
                        Header = "Disconnect"
                    };
                    menuItemDisconnect.Click += Stop_Connection;
                    contextMenu.Items.Add(menuItemDisconnect);
                }
                
                

                //saving
                MenuItem menuItemSettings = new MenuItem
                {
                    Header = "Settings"
                };
                menuItemSettings.Click += Show_Settings_Window;
                contextMenu.Items.Add(menuItemSettings);

                //exit
                MenuItem menuItemExit = new MenuItem
                {
                    Header = "Close MediaDock"
                };
                menuItemExit.Click += new RoutedEventHandler(Close_Window);
                contextMenu.Items.Add(menuItemExit);

                //make the context menu visible
                contextMenu.IsOpen = true;
            }
        }

        public void Close_Window(object sender, System.EventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        public void SaveSettings()
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

        public async void Start_Connection(object sender, System.EventArgs e)
        {
            if(connection != null && connection.State == HubConnectionState.Disconnected)
            {
                await connection.StartAsync();
            }
            
        }
        public async void Stop_Connection(object sender, System.EventArgs e)
        {
            if (connection != null && connection.State == HubConnectionState.Connected)
            {
                await connection.StopAsync();
            }

        }

        public void Show_Settings_Window(object sender, System.EventArgs e)
        {
            this.Hide();
            // HACK: using this gross way to pass information to other windows
            Application.Current.Resources.Add("Settings", settings);
            
            // open the settings window and give it our current settings
            SettingsWindow settingsWindow = new SettingsWindow(ref settings);
            
            bool? settingsUpdated = settingsWindow.ShowDialog();

            //update our main window to reflect the settings if the result is true
            if (settingsUpdated != null)
            {
                if(settingsUpdated.Value == true)
                {
                    // get the settings that were edited
                    UserSettingsModel? _settings = Application.Current.Resources["Settings"] as UserSettingsModel;
                    if (_settings != null)
                    {
                        settings = _settings;
                    }
                    // remove the resource to prevent accidental usage in other areas of the program
                    Application.Current.Resources.Remove("Settings");

                    // save to the default file, update the UI, and update the services
                    SaveSettings();
                    LoadWindowSettings(settings);
                    volumeUpdater.Stop();
                    volumeUpdater = MasterVolumeUpdater(settings.VolumeSliderUpdateInterval);
                }
            }
            Application.Current.Resources.Remove("Settings");
            this.Show();
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
        private async void MediaPlayPause(object sender, RoutedEventArgs e)
        {
            Core.PlayPauseSong();
            // TODO: get proper playing status
            // check if specific application is playing volume
            if(connection != null)
            {
                await connection.SendAsync("PlayingStatusChange", false);
            }
            
        }
        private async void VolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (connection != null)
            {
                await connection.SendAsync("VolumeChange", (float)VolumeSlider.Value);
                // also triggers value changed
            }
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

        private Timer MasterVolumeUpdater(float intervalInSeconds)
        {
            Timer timer = new Timer();
            timer = new Timer(intervalInSeconds*1000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += (sender, e) =>  UpdateVolumeSlider();
            timer.AutoReset = true;
            timer.Enabled = true;
            return timer;
        }


    }
}
