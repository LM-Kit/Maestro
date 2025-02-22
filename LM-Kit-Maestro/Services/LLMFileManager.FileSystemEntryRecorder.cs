using LMKit.Maestro.Helpers;
using System.Diagnostics;

namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    private sealed partial class FileSystemEntryRecorder
    {
        private DirectoryRecord? _rootDirectoryRecord;
        private Uri _rootDirectoryUri;

        public FileSystemEntryRecorder(Uri rootDirectoryUri)
        {
            Update(rootDirectoryUri);
        }

        public void Update(Uri rootDirectoryUri)
        {
            if (rootDirectoryUri == _rootDirectoryUri)
            {
                return;
            }

            if (_rootDirectoryRecord != null)
            {
                Clear();
            }

            _rootDirectoryUri = rootDirectoryUri;
            _rootDirectoryRecord = new DirectoryRecord(rootDirectoryUri.LocalPath, null);
        }

        public void Clear()
        {
            _rootDirectoryRecord!.Entries.Clear();
        }

        public FileRecord? RecordFile(Uri fileUri)
        {
            string fileBaseName = FileHelpers.GetFileBaseName(fileUri);
            DirectoryRecord directParentDirectory = TryGetDirectParentDirectory(fileUri, true)!;
            FileRecord? file = directParentDirectory!.TryGetChildFile(fileBaseName);

            if (file != null)
            {
                // This file is already recorded
                return null;
            }
            else
            {
                FileRecord fileRecord = new FileRecord(fileUri, fileBaseName, directParentDirectory);

                directParentDirectory.AddEntry(fileRecord);

                return fileRecord;
            }
        }

        public FileRecord? DeleteFileRecord(Uri fileUri)
        {
            var entry = TryGetExistingEntry(fileUri);

            if (entry != null && entry.Delete() && entry is FileRecord fileRecord)
            {
                return fileRecord;
            }

            return null;
        }

        public FileSystemEntryRecord? TryGetExistingEntry(Uri fileUri)
        {
            int depth = fileUri.Segments.Length - _rootDirectoryUri!.Segments.Length;
            DirectoryRecord current = _rootDirectoryRecord!;

            DirectoryRecord? parentDirectory = TryGetDirectParentDirectory(fileUri);

            if (parentDirectory != null)
            {
                return parentDirectory.TryGetChildEntry(FileHelpers.GetFileBaseName(fileUri));
            }
            else
            {
                return null;
            }
        }

        private DirectoryRecord? TryGetDirectParentDirectory(Uri fileUri, bool createIfNonExisting = false)
        {
            int targetDepth = fileUri.Segments.Length - _rootDirectoryUri!.Segments.Length - 1;
            DirectoryRecord current = _rootDirectoryRecord!;

            for (int currentLevel = 0; currentLevel < targetDepth; currentLevel++)
            {
                int segmentIndex = fileUri.Segments.Length - (targetDepth - currentLevel) - 1;
                string entryName = FileHelpers.SanitizeUriSegment(fileUri.Segments[segmentIndex]);

                DirectoryRecord? childDirectory = current.TryGetChildDirectory(entryName);

                if (childDirectory == null)
                {
                    if (!createIfNonExisting)
                    {
                        return null;
                    }
                    else
                    {
                        childDirectory = new DirectoryRecord(entryName, current);
                        current.AddEntry(childDirectory);
                    }
                }

                current = childDirectory;
            }

            return current;
        }

        public static List<FileRecord> GetAllChildFiles(DirectoryRecord directoryRecord)
        {
            List<FileRecord> files = [];

            CollectChildFiles(directoryRecord, files);

            return files;
        }

        private static void CollectChildFiles(DirectoryRecord directoryRecord, List<FileRecord> files)
        {
            foreach (var entry in directoryRecord.Entries)
            {
                if (entry is DirectoryRecord subDirectory)
                {
                    CollectChildFiles(subDirectory, files);
                }
                else if (entry is FileRecord fileRecord)
                {
                    files.Add(fileRecord);
                }
            }
        }


#if DEBUG
        public void DumpRecords()
        {
            DumpRecords(_rootDirectoryRecord!);
        }

        private void DumpRecords(DirectoryRecord directory, int level = 0)
        {
            Trace.Write(new string(' ', level * 5));
            Trace.WriteLine(directory.Name);

            foreach (var entry in directory.Entries)
            {
                if (entry is DirectoryRecord childDirectory)
                {
                    DumpRecords(childDirectory, level + 1);
                }
                else if (entry is FileRecord file)
                {
                    Trace.Write(new string(' ', (level + 1) * 5));
                    Trace.WriteLine(file.Name);
                }
            }
        }

#endif
    }
}