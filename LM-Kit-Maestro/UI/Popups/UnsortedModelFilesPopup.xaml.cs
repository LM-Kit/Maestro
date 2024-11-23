using LMKit.Maestro.ViewModels;
using Mopups.Interfaces;

namespace LMKit.Maestro.UI;

public partial class UnsortedModelFilesPopup : PopupView
{
    public UnsortedModelFilesPopup(IPopupNavigation popupNavigation, UnsortedModelFilesPopupViewModel unsortedModelFilesPopupViewModel) : base(popupNavigation)
    {
        BindingContext = unsortedModelFilesPopupViewModel;
        InitializeComponent();
    }
}