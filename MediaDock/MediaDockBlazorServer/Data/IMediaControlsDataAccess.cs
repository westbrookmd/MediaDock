namespace MediaDockBlazorServer.Data
{
    public interface IMediaControlsDataAccess
    {
        bool isPlaying { get;}
        float Volume { get; set; }
        string GetPlayingStatus();
        float GetVolume();
        void SetPlayingStatus(bool status);
        void SetVolume(float volume);
    }
}