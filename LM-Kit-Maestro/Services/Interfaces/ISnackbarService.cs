using MudBlazor;

namespace LMKit.Maestro.Services;

public interface ISnackbarService
{
    event Action<string, string, Severity>? OnShowSnackbar;

    void Show(string title, string message, Severity severity = Severity.Info);
}
