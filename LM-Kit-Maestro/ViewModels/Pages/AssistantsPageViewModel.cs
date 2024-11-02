using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKitMaestro.Data;
using Microsoft.Extensions.Logging;
using LMKitMaestro.Helpers;
using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels
{
    public sealed class AssistantsPageViewModel : PageViewModelBase
    {
        public ModelListViewModel ModelListViewModel { get; }

        public AssistantsPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation, ModelListViewModel modelListViewModel) : base(navigationService, popupService, popupNavigation)
        {
            ModelListViewModel = modelListViewModel;
        }
    }
}
