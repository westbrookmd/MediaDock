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
        public bool WindowIsAlwaysOnTop { get; set; }
        public WindowStartupLocation WindowStartupLocation { get; set; }
        public ResizeMode WindowResizeMode { get; set; }
        public float VolumeSliderUpdateInterval { get; set; }
    }
}
