using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ListItemViewModel<T> : ObservableObject where T : ObservableObject
    {
        [ObservableProperty] private bool _isHovered;

        [ObservableProperty] private bool _isShowingActionSheet;

        public T ViewModel { get; }

        public ListItemViewModel(T viewModel)
        {
            ViewModel = viewModel;
        }
    }
}