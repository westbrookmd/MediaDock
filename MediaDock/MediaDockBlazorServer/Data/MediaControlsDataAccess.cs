namespace MediaDockBlazorServer.Data
{
    public class MediaControlsDataAccess : IMediaControlsDataAccess
    {
        public bool isPlaying { get; set; }

        public string IsPlayingDisplay
        {
            get 
            {
                if(isPlaying)
                {
                    return "Playing";
                }
                else
                {
                    return "Paused";
                }
                
            }
        }

        public float Volume { get; set; }

        public string GetPlayingStatus()
        {
            return IsPlayingDisplay;
        }
        public void SetPlayingStatus(bool status)
        {
            isPlaying = status;
        }
        public float GetVolume()
        {
            return Volume;
        }
        public void SetVolume(float volume)
        {
            Volume = volume;
        }
    }
}
