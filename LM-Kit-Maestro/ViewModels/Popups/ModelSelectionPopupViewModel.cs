using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LMKitMaestro.ViewModels;

public partial class ModelSelectionPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    ModelListViewModel _modelListViewModel;

    public ModelSelectionPopupViewModel(ModelListViewModel modelListViewModel)
    {
        ModelListViewModel = modelListViewModel;
    }

    [RelayCommand]
    public async Task NavigateToModelPage()
    {
        await ModelListViewModel.NavigationService.NavigateToAsync("//ModelsPage");
    }
}
