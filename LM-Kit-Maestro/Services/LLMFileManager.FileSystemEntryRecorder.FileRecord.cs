using LMKit.Maestro.Helpers;

namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    private sealed partial class FileSystemEntryRecorder
    {
        public class FileRecord : FileSystemEntryRecord
        {
            public Uri FileUri { get; set; }

            public event EventHandler? FilePathChanged;

            public FileRecord(Uri fileUri, string name, DirectoryRecord parent) : base(name, parent)
            {
                FileUri = fileUri;
            }

            public void OnParentDirectoryRenamed(int ancestorLevel, string newName)
            {
                Uri oldUri = FileUri;
                FileUri = FileHelpers.GetRenamedFileUri(FileUri, newName, ancestorLevel);
                FilePathChanged?.Invoke(this, new FileRecordPathChangedEventArgs(oldUri, FileUri));
            }

            protected override void OnEntryRenamed()
            {
                Uri oldUri = FileUri;
                FileUri = FileHelpers.GetRenamedFileUri(FileUri, Name);
                FilePathChanged?.Invoke(this, new FileRecordPathChangedEventArgs(oldUri, FileUri));
            }

#if DEBUG
            public override string ToString()
            {
                return ("(file)" + Name);
            }
#endif
        }
    }
}