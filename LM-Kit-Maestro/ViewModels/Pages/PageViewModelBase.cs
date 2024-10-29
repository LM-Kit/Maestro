using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels;

public abstract partial class PageViewModelBase : ViewModelBase
{
    public INavigationService NavigationService { get; }

    public IPopupService PopupService { get; }

    public IPopupNavigation PopupNavigation { get; }

    protected PageViewModelBase(INavigationService navigationService,  IPopupService popupService, IPopupNavigation popupNavigation)
    {
        NavigationService = navigationService;
        PopupService = popupService;
        PopupNavigation = popupNavigation; 
    }
}
