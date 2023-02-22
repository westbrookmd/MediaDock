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

using WindowsAudio;
using Microsoft.AspNetCore.SignalR.Client;

namespace MediaDock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsPlaying
        {
            get { return IsPlaying; }
            set
            {
                IsPlaying = value;
                SetPlayPauseButton();
            }
        }

        //Settings variables
        public UserSettingsModel _settings = new UserSettingsModel();
        public string settingsFilePath = Environment.CurrentDirectory + "\\Settings.ini";

        // service settings
        public float volumeSliderInterval;

        // Services
        Timer volumeUpdater;

        // SignalR
        readonly HubConnection ?connection;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            UpdateVolumeSlider();
            try
            {
                _settings = GetSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load profile. Setting defaults.");
                //set default settings
                _settings = SetDefaultSettings();
            }
            finally
            {
                LoadWindowSettings(_settings);
                //startup necessary services from profile/default settings

                //start service to count time and refresh volume
                volumeUpdater = MasterVolumeUpdater(_settings.VolumeSliderUpdateInterval);
                if(_settings.RemoteControl)
                {
                    connection = new HubConnectionBuilder()
                    .WithAutomaticReconnect()
                    // TODO: put in keyvault or something else
                    .WithUrl(_settings.ConnectionString)
                    .Build();
                    if(_settings.AutomaticallyConnect)
                    {
                        connection.StartAsync();
                    }
                    connection.On<float>("BroadcastVolume", BroadcastVolume);
                    connection.On<bool>("BroadcastIsPlaying", BroadcastIsPlaying);
                    // TODO: implement in mediacontrolshub and Blazor Server
                    connection.On("BroadcastPreviousSong", BroadcastPreviousSong);
                    connection.On("BroadcastNextSong", BroadcastNextSong);
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
            IsPlaying = newPlayingStatus;
            Core.PlayPauseSong(newPlayingStatus);
        }
        private static void BroadcastPreviousSong()
        {
            Core.PreviousSong();
        }
        private static void BroadcastNextSong()
        {
            Core.NextSong();
        }
        private UserSettingsModel GetSettings()
        {
            UserSettingsModel settingsToReturn = new();
            UserSettingsModel? settingsFile = ReadFromJsonFile<UserSettingsModel>(settingsFilePath);
            if (settingsFile != null)
            {
                settingsToReturn = settingsFile;
            }
            else
            {
                settingsToReturn = SetDefaultSettings();
            }
            return settingsToReturn;
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
                var contextMenu = CreateContextMenu();
                //make the context menu visible
                contextMenu.IsOpen = true;
            }
        }

        private ContextMenu CreateContextMenu()
        {
            var contextMenu = new ContextMenu();

            //signalr connection connect/disconnect
            if (BuildConnectDisconnectMenuItem(out MenuItem menuItemToReturn))
            {
                contextMenu.Items.Add(menuItemToReturn);
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

            return contextMenu;
        }

        private bool BuildConnectDisconnectMenuItem(out MenuItem menuItemToReturn)
        {
            menuItemToReturn = new();
            if (connection != null)
            {
                switch (connection.State)
                {
                    case HubConnectionState.Disconnected:
                        menuItemToReturn = new MenuItem
                        {
                            Header = "Connect"
                        };
                        menuItemToReturn.Click += Start_Connection;
                        menuItemToReturn.Items.Add(menuItemToReturn);
                        break;
                    case HubConnectionState.Connected:
                    case HubConnectionState.Connecting:
                    case HubConnectionState.Reconnecting:
                        menuItemToReturn = new MenuItem
                        {
                            Header = "Disconnect"
                        };
                        menuItemToReturn.Click += Stop_Connection;
                        
                        break;
                    default:
                        break;
                }
                return true;
                
            }
            return false;
        }

        public void Close_Window(object sender, System.EventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
        public void SaveSettings()
        {

            try
            {
                WriteToJsonFile<UserSettingsModel>(settingsFilePath, _settings);
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
            // open the settings window and give it our current settings
            SettingsWindow settingsWindow = new SettingsWindow(ref _settings);
            
            bool? settingsUpdated = settingsWindow.ShowDialog();

            //update our main window to reflect the settings if the result is true
            if (settingsUpdated != null)
            {
                if(settingsUpdated.Value == true)
                {
                    // save to the default file, update the UI, and update the services
                    SaveSettings();
                    LoadWindowSettings(_settings);
                    volumeUpdater.Stop();
                    volumeUpdater = MasterVolumeUpdater(_settings.VolumeSliderUpdateInterval);
                }
            }
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
        private UserSettingsModel SetDefaultSettings()
        {
            return new UserSettingsModel();
        }
        private static void LoadWindowSettings(UserSettingsModel settings)
        {
            Window window = (Window)Application.Current.MainWindow;
            window.Topmost = settings.WindowIsAlwaysOnTop;
            window.WindowStartupLocation = settings.WindowStartupLocation;
            window.ResizeMode = settings.WindowResizeMode;
        }
        private void UpdateVolumeSlider()
        {
            //updating UI from thread other than the main thread
            this.Dispatcher.Invoke(() =>
            {
                VolumeSlider.Value =  Math.Round(Core.GetMasterVolume());
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
            IsPlaying = !IsPlaying;
            Core.PlayPauseSong(IsPlaying);
            // TODO: get proper playing status
            // check if specific application is playing volume
            if (connection != null)
            {
                await connection.SendAsync("PlayingStatusChange", IsPlaying);
            }

        }

        private void SetPlayPauseButton()
        {
            if (IsPlaying)
            {
                PlayPause.Content = "Pause";
            }
            else
            {
                PlayPause.Content = "Play";
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
