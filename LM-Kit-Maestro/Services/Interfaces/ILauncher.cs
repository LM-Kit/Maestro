namespace LMKit.Maestro.Services;

public interface ILauncher
{
    public Task<bool> OpenAsync(Uri uri);
}
