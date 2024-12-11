namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public class NotifyModelStateChangedEventArgs : EventArgs
    {
        public Uri FileUri { get; }

        public NotifyModelStateChangedEventArgs(Uri fileUri)
        {
            FileUri = fileUri;
        }
    }
}
