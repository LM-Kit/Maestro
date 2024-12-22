using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Model;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.ViewModels;

public partial class UnsortedModelFilesPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    ObservableCollection<ModelCard> _unsortedModelFiles = new ObservableCollection<ModelCard>();

    public void Load(ObservableCollection<ModelCard> unsortedModels)
    {
        UnsortedModelFiles = unsortedModels;
    }
}
