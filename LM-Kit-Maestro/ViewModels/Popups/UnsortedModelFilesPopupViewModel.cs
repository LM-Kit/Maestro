using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.ViewModels;

public partial class UnsortedModelFilesPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    ObservableCollection<Uri> _unsortedModelFiles = new ObservableCollection<Uri>();

    public void Load(ObservableCollection<Uri> unsortedModels)
    {
        UnsortedModelFiles = unsortedModels;
    }
}
