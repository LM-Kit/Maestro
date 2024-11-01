using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKitMaestro.Data;
using Microsoft.Extensions.Logging;
using LMKitMaestro.Helpers;
using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels
{
    internal class AssistantsPageViewModel : PageViewModelBase
    {
        public AssistantsPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation) : base(navigationService, popupService, popupNavigation)
        {

        }
    }
}
