namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    private sealed partial class FileSystemEntryRecorder
    {
        public class DirectoryRecord : FileSystemEntryRecord
        {
            public List<FileSystemEntryRecord> Entries { get; protected set; }

            public DirectoryRecord(string name, DirectoryRecord? parent) : base(name, parent)
            {
                Entries = new List<FileSystemEntryRecord>();
            }

            public bool DeleteEntry(FileSystemEntryRecord entry)
            {
                return Entries.Remove(entry);
            }

            public void AddEntry(FileSystemEntryRecord entry)
            {
                Entries.Add(entry);
            }

            public DirectoryRecord? TryGetChildDirectory(string name)
            {
                foreach (var child in Entries)
                {
                    if (child is DirectoryRecord childDirectory && string.CompareOrdinal(child.Name, name) == 0)
                    {
                        return childDirectory;
                    }
                }

                return null;
            }

            public FileRecord? TryGetChildFile(string name)
            {
                foreach (var child in Entries)
                {
                    if (child is FileRecord childFile && string.CompareOrdinal(childFile.Name, name) == 0)
                    {
                        return childFile;
                    }
                }

                return null;
            }

            public FileSystemEntryRecord? TryGetChildEntry(string name)
            {
                foreach (var entry in Entries)
                {
                    if (string.CompareOrdinal(entry.Name, name) == 0)
                    {
                        return entry;
                    }
                }

                return null;
            }

            protected override void OnEntryRenamed()
            {
                PropagateRenamedParentEntryToChildren(1, Name, this);
            }

            private void PropagateRenamedParentEntryToChildren(int ancestorLevel, string newName, DirectoryRecord directoryRecord)
            {
                foreach (var child in directoryRecord.Entries)
                {
                    if (child is FileRecord childFile)
                    {
                        childFile.OnParentDirectoryRenamed(ancestorLevel, newName);
                    }
                    else if (child is DirectoryRecord childDirectory)
                    {
                        PropagateRenamedParentEntryToChildren(ancestorLevel + 1, newName, childDirectory);
                    }
                }
            }

#if DEBUG
            public override string ToString()
            {
                return ("(directory)" + Name);
            }
#endif
        }
    }
}