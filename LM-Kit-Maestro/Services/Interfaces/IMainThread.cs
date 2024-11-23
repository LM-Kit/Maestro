namespace LMKit.Maestro.Services;


public interface IMainThread
{
    public void BeginInvokeOnMainThread(Action task);
}
