namespace LMKit.Maestro.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync(string? initialPath = null, string? title = null);
}
