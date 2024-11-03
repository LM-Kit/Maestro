using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels
{
    public partial class AssistantsPageViewModel : PageViewModelBase
    {
        private string _input = string.Empty;
        public string Input
        {
            get => _input;
            set
            {
                _input = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private string? _result;

        public ModelListViewModel ModelListViewModel { get; }

        public LMKitService LMKitService { get; }

        public AssistantsPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation, ModelListViewModel modelListViewModel, LMKitService lMKitService) : base(navigationService, popupService, popupNavigation)
        {
            ModelListViewModel = modelListViewModel;
            LMKitService = lMKitService;
        }

        [RelayCommand]
        public async Task RunAssistant()
        {
            Result = await LMKitService.SubmitTranslation(Input, LMKit.TextGeneration.Language.Undefined);
        }
    }
}
