using LMKit.Maestro.ViewModels;
using Mopups.Interfaces;

namespace LMKit.Maestro.UI;

public partial class AlertPopup : PopupView
{
    public AlertPopup(IPopupNavigation popupNavigation, AlertPopupViewModel informationPopupViewModel) : base(popupNavigation)
    {
        InitializeComponent();
        BindingContext = informationPopupViewModel;
    }
}