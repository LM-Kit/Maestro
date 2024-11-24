using LMKit.Maestro.Services;

namespace LMKit.Maestro;

public class MacFolderPicker : IFolderPicker
{
    public Task<FolderPickerResult> PickAsync(string initialPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FolderPickerResult(false, null, null));
    }
}