using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKitMaestro.ViewModels
{
    public partial class AlertPopupViewModel : ViewModelBase
    {
        [ObservableProperty]
        string? _title = null;

        [ObservableProperty]
        string message = string.Empty;

        [ObservableProperty]
        string okText = string.Empty;

        public AlertPopupViewModel()
        {

        }

        public void Load(string? title, string message, string okText)
        {
            Title = title;
            Message = message;
            OkText = okText;
        }
    }
}
