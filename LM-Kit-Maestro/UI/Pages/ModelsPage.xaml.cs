using LMKitMaestro.Helpers;
using LMKitMaestro.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace LMKitMaestro.UI;

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

    private void LmKitModelsTabTapped(object sender, EventArgs e)
    {
        SelectedTab = ModelsPageTab.LmKitModels;
    }

    [RelayCommand]
    public async Task ShowUnsortedModelFilesPopup()
    {
        var fileNames = _modelsPageViewModel.FileManager.UnsortedModels.Select(uri => FileHelpers.GetModelFileRelativePath(uri.LocalPath,
            _modelsPageViewModel.AppSettingsService.ModelsFolderPath)).ToList();

        UnsortedModelFilesPopup popup = new UnsortedModelFilesPopup(_modelsPageViewModel.PopupNavigation, new UnsortedModelFilesPopupViewModel()
        {
            UnsortedModelFiles = new System.Collections.ObjectModel.ObservableCollection<string>(fileNames)
        });

        await _modelsPageViewModel.PopupNavigation.PushAsync(popup);
    }
}
