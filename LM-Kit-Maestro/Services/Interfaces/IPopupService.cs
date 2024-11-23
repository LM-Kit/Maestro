namespace LMKit.Maestro.Services;

public interface IPopupService
{
    Task DisplayAlert(string title, string message, string okText);
}