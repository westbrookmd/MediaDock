using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaDock.Models
{
    public class UserSettingsModel
    {
        public bool WindowIsAlwaysOnTop { get; set; } = true;
        public WindowStartupLocation WindowStartupLocation { get; set; } = WindowStartupLocation.Manual;
        public ResizeMode WindowResizeMode { get; set; } = ResizeMode.NoResize;
        public float VolumeSliderUpdateInterval { get; set; } = 4;

    }
}
