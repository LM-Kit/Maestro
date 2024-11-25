namespace LMKit.Maestro.Services;

public sealed class ModelInfo
{
    public string Publisher { get; }

    public string Repository { get; }

    public string FileName { get; }

    public long? FileSize { get; }

    public Uri FileUri { get; }

    public override bool Equals(object? obj)
    {
        ModelInfo? modelObj = obj as ModelInfo;

        if (modelObj != null)
        {
            if (modelObj.Publisher == this.Publisher &&
                modelObj.Repository == this.Repository &&
                modelObj.FileName == this.FileName &&
                modelObj.FileSize == this.FileSize)
            {
                return true;
            }
        }

        return false;
    }

    public ModelInfo(string publisher, string repository, string fileName, Uri fileUri, long? fileSize = 0)
    {
        Publisher = publisher;
        Repository = repository;
        FileName = fileName;
        FileUri = fileUri;
        FileSize = fileSize;
    }
}
