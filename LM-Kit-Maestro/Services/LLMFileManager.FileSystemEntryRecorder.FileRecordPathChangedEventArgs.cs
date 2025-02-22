namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    private sealed partial class FileSystemEntryRecorder
    {
        public sealed class FileRecordPathChangedEventArgs : EventArgs
        {
            public Uri OldPath { get; }

            public Uri NewPath { get; }

            public FileRecordPathChangedEventArgs(Uri oldPath, Uri newPath)
            {
                OldPath = oldPath;
                NewPath = newPath;
            }
        }
    }
}