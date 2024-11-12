using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels
{
    public partial class AssistantsPageViewModel : PageViewModelBase
    {
        [ObservableProperty]
        private TranslationViewModel _currentTranslation;

        [ObservableProperty]
        private string? _result;

        public ModelListViewModel ModelListViewModel { get; }

        public LMKitService LMKitService { get; }

        public AssistantsPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation, ModelListViewModel modelListViewModel, LMKitService lMKitService) : base(navigationService, popupService, popupNavigation)
        {
            _currentTranslation = new TranslationViewModel(popupService, lMKitService);

            _currentTranslation.TranslationCompleted += OnTranslationCompleted;   
            ModelListViewModel = modelListViewModel;
            LMKitService = lMKitService;
        }

        private void OnTranslationCompleted(object? sender, EventArgs e)
        {
            var args = (TranslationViewModel.TranslationCompletedEventArgs)e;

            Result = args.Translation;

            if (args.Exception != null)
            {
                // error
            }
        }
    }
}
