using MudBlazor;

namespace LMKit.Maestro.Services;

public class SnackbarService
{
    public event Action<string, string, Severity>? OnShowSnackbar;

    public void Show(string title, string message, Severity severity = Severity.Info)
    {
        OnShowSnackbar?.Invoke(title, message, severity);
    }
}