namespace LMKit.Maestro.Services;

public interface IFolderPicker
{
    Task<FolderPickerResult> PickAsync(string initialPath, CancellationToken cancellationToken = default);
}
