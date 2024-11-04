using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels
{
    internal partial class AssistantsPageViewModel : PageViewModelBase
    {
        public AssistantsPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation) : base(navigationService, popupService, popupNavigation)
        {

        }
    }
}
