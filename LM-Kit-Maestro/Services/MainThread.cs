namespace LMKit.Maestro.Services;

internal sealed class MainThread : IMainThread
{
    public void BeginInvokeOnMainThread(Action action)
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(action);
    }
}
