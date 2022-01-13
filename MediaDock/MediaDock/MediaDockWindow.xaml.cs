using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool topmost;
        public WindowStartupLocation startupLocation;
        public ResizeMode resizeMode;

        //services settings
        public float volumeSliderInterval;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            UpdateVolumeSlider();
            SetDefaultSettingsVariables();
            try
            {
                //TODO:get profile
                //TODO:set settings variables

                //load settings
                LoadWindowSettings(topmost, startupLocation, resizeMode);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load profile. Setting defaults.");
                //set default settings
                SetDefaultSettingsVariables();
                //load default settings
                LoadWindowSettings(topmost, startupLocation, resizeMode);
            }
            finally
            {
                //startup necessary services from profile/default settings

                //start service to count time and refresh volume
                MasterVolumeUpdater(volumeSliderInterval);
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
                menuItemExit.Click += new RoutedEventHandler(this.Close_Window);
                contextMenu.Items.Add(menuItemExit);
                contextMenu.IsOpen = true;
            }
        }

        public void Close_Window(object sender, System.EventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void SetDefaultSettingsVariables()
        {
            topmost = true;
            startupLocation = WindowStartupLocation.Manual;
            resizeMode = ResizeMode.NoResize;
            volumeSliderInterval = 4;
        }

        private static void LoadWindowSettings(bool topmost, WindowStartupLocation windowStartupLocation, ResizeMode resizeMode)
        {
            Window window = (Window)Application.Current.MainWindow;
            window.Topmost = topmost;
            window.WindowStartupLocation = windowStartupLocation;
            window.ResizeMode = resizeMode;
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
