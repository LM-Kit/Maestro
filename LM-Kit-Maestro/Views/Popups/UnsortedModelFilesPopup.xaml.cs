using LMKitMaestro.ViewModels;
using Mopups.Interfaces;

namespace LMKitMaestro.Views;

public partial class UnsortedModelFilesPopup : PopupView
{
    public UnsortedModelFilesPopup(IPopupNavigation popupNavigation, UnsortedModelFilesPopupViewModel unsortedModelFilesPopupViewModel) : base(popupNavigation)
    {
        BindingContext = unsortedModelFilesPopupViewModel;
        InitializeComponent();
    }
}