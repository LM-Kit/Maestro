using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LMKitMaestro.ViewModels;

public partial class ModelSelectionPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    ChatPageViewModel _chatPageViewModel;

    public ModelSelectionPopupViewModel(ChatPageViewModel chatPageViewModel)
    {
        ChatPageViewModel = chatPageViewModel;
    }

    [RelayCommand]
    public async Task NavigateToModelPage()
    {
        await ChatPageViewModel.NavigationService.NavigateToAsync("//ModelsPage");
    }
}
