namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    private sealed partial class FileSystemEntryRecorder
    {
        public abstract class FileSystemEntryRecord
        {
            public DirectoryRecord? Parent { get; protected set; }

            public string Name { get; private set; }

            public FileSystemEntryRecord(string name, DirectoryRecord? parent)
            {
                Name = name;
                Parent = parent;
            }

            protected abstract void OnEntryRenamed();

            public void Rename(string name)
            {
                Name = name;
                OnEntryRenamed();
            }

            public bool Delete()
            {
                if (Parent == null)
                {
                    return false;
                }

                return Parent.DeleteEntry(this);
            }
        }
    }
}