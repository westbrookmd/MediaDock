using Microsoft.Extensions.Logging;

namespace MediaDockBlazorServer.Data
{
    public class MediaControlsDataAccess : IMediaControlsDataAccess
    {
        private readonly ILogger<MediaControlsDataAccess> _log;

        public MediaControlsDataAccess(ILogger <MediaControlsDataAccess> log)
        {
            _log = log;

            //db calls to set values

        }
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
            _log.LogInformation("Getting the playing status.");
            return IsPlayingDisplay;
        }
        public void SetPlayingStatus(bool status)
        {
            _log.LogInformation("Setting the playing status to {newPlayStatus}.", status);
            isPlaying = status;
        }
        public float GetVolume()
        {
            _log.LogInformation("Getting the volume.");
            return Volume;
        }
        public void SetVolume(float volume)
        {
            _log.LogInformation("Setting the volume to {newVolume}.", volume);
            Volume = volume;
        }

        public void PreviousSong()
        {
            //TODO: db call
            _log.LogInformation("Sending previous song command.");
        }

        public void NextSong()
        {
            //TODO: db call
            _log.LogInformation("Sending next song command.");
        }
    }
}
