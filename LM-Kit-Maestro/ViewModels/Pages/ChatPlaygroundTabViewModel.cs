using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKit.Maestro.ViewModels;

public partial class MaestroTabViewModel : ViewModelBase
{
    public string Route { get; }

    [ObservableProperty]
    string title;

    [ObservableProperty]
    bool isSelected;

    public MaestroTabViewModel(string title, string route)
    {
        Title = title;
        Route = route;
    }
}
