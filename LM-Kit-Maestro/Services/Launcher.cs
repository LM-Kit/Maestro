namespace LMKitMaestro.Services;

internal class Launcher : ILauncher
{
    public async Task<bool> OpenAsync(Uri uri)
    {
        return await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(uri);
    }
}