using CommunityToolkit.Maui.Storage;

namespace LMKit.Maestro.Services;

public class DefaultFolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync(string? initialPath = null, string? title = null)
    {
        try
        {
            var result = await FolderPicker.Default.PickAsync(initialPath ?? string.Empty, CancellationToken.None);
            return result.IsSuccessful ? result.Folder?.Path : null;
        }
        catch
        {
            return null;
        }
    }
}
