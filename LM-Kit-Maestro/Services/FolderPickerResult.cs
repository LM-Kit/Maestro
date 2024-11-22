namespace LMKitMaestro.Services;

public sealed class FolderPickerResult
{
    public bool Success { get; }

    public string? FolderPath { get; }

    public Exception? Exception { get; }

    public FolderPickerResult(bool success, string? folderPath, Exception? exception)
    {
        Success = success;
        FolderPath = folderPath;
        Exception = exception;
    }
}
