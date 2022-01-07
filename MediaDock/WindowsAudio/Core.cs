using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAudio
{
    public class Core
    {
        //https://stackoverflow.com/a/41534820
        // controlling media keys
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int KEYEVENTF_EXTENTEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 0;

        public static void PreviousSong()
        {
            //previous song
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        public static void NextSong()
        {
            //next song
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        public static void PlayPauseSong()
        {
            //pause or play music
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        public static void ToggleMasterAudioMute()
        {
            AudioManager.ToggleMasterVolumeMute();
        }

        public static void ChangeMasterVolume(float newVolume)
        {
            AudioManager.SetMasterVolume(newVolume);
        }

        public static float GetMasterVolume()
        {
            return AudioManager.GetMasterVolume();
        }


    }
}
