using LMKitMaestro.Services;

namespace LMKitMaestro.WinUI;

internal class FolderPicker : IFolderPicker
{
    public async Task<FolderPickerResult> PickAsync(string initialPath, CancellationToken cancellationToken = default)
    {
        var result = await CommunityToolkit.Maui.Storage.FolderPicker.PickAsync(initialPath, cancellationToken);

        return new LMKitMaestro.Services.FolderPickerResult(result.IsSuccessful, 
            result.Folder?.Path, result.Exception);
    }
}