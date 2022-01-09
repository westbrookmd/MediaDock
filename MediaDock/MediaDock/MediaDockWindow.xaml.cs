﻿using System;
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
        public MainWindow()
        {
            InitializeComponent();
            UpdateVolumeSlider();
            //start service to count time and refresh volume
            MasterVolumeUpdater(4);
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
