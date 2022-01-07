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
        public MainWindow()
        {
            InitializeComponent();
            VolumeSlider.Value = Core.GetMasterVolume();
        }
        public void MediaPrevious(object sender, RoutedEventArgs e)
        {
            Core.PreviousSong();

        }
        public void MediaNext(object sender, RoutedEventArgs e)
        {
            Core.NextSong();
        }
        public void MediaPlayPause(object sender, RoutedEventArgs e)
        {
            Core.PlayPauseSong();
        }
        public void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> newVolume)
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
    }
}
