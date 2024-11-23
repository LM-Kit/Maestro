using LMKit.Maestro.Helpers;
using LMKit.Maestro.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace LMKit.Maestro.UI;

public partial class ModelsPage : ContentPage
{
    private readonly ModelsPageViewModel _modelsPageViewModel;

    public static readonly BindableProperty SelectedTabProperty = BindableProperty.Create(nameof(SelectedTab), typeof(ModelsPageTab), typeof(ModelsPage));
    public ModelsPageTab SelectedTab
    {
        get => (ModelsPageTab)GetValue(SelectedTabProperty);
        private set => SetValue(SelectedTabProperty, value);
    }

    public ModelsPage(ModelsPageViewModel modelsPageViewModel)
    {
        InitializeComponent();
        BindingContext = modelsPageViewModel;
        _modelsPageViewModel = modelsPageViewModel;
    }

    private void UserModelsTabTapped(object sender, EventArgs e)
    {
        SelectedTab = ModelsPageTab.UserModels;
    }

    private void LMKitModelsTabTapped(object sender, EventArgs e)
    {
        SelectedTab = ModelsPageTab.LMKitModels;
    }

    [RelayCommand]
    public async Task ShowUnsortedModelFilesPopup()
    {
        var fileNames = _modelsPageViewModel.FileManager.UnsortedModels.Select(uri => FileHelpers.GetModelFileRelativePath(uri.LocalPath,
            _modelsPageViewModel.AppSettingsService.ModelsFolderPath)).ToList();

        UnsortedModelFilesPopup popup = new UnsortedModelFilesPopup(_modelsPageViewModel.PopupNavigation, new UnsortedModelFilesPopupViewModel()
        {
            UnsortedModelFiles = _modelsPageViewModel.FileManager.UnsortedModels
        });

        await _modelsPageViewModel.PopupNavigation.PushAsync(popup);
    }
}
