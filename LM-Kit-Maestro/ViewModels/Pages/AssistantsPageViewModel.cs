using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Maestro.Services;
using Mopups.Interfaces;

namespace LMKit.Maestro.ViewModels
{
    public partial class AssistantsPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private TranslationViewModel _translationViewModel;

        public ModelListViewModel ModelListViewModel { get; }

        public LMKitService LMKitService { get; }

        public AssistantsPageViewModel(ModelListViewModel modelListViewModel, LMKitService lMKitService)
        {
            TranslationViewModel = new TranslationViewModel(lMKitService);
            ModelListViewModel = modelListViewModel;
            LMKitService = lMKitService;
        }
    }
}
