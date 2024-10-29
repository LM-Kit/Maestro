using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKitMaestro.ViewModels;

public partial class LMKitMaestroTabViewModel : ViewModelBase
{
    public string Route { get; }

    [ObservableProperty]
    string title;

    [ObservableProperty]
    bool isSelected;

    public LMKitMaestroTabViewModel(string title, string route)
    {
        Title = title;
        Route = route;
    }
}
