using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
using Mopups.Interfaces;

namespace LMKit.Maestro.ViewModels
{
    public partial class AssistantsPageViewModel : PageViewModelBase
    {
        [ObservableProperty]
        private TranslationViewModel _currentTranslation;

        public ModelListViewModel ModelListViewModel { get; }

        public LMKitService LMKitService { get; }

        public AssistantsPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation, ModelListViewModel modelListViewModel, LMKitService lMKitService) : base(navigationService, popupService, popupNavigation)
        {
            _currentTranslation = new TranslationViewModel(popupService, lMKitService);

            ModelListViewModel = modelListViewModel;
            LMKitService = lMKitService;
        }
    }
}
