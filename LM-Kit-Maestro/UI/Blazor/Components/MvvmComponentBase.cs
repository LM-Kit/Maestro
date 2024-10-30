using LMKitMaestro.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace LMKitMaestro.UI.Components
{
    public abstract class MvvmComponentBase<TViewModel> : ComponentBase where TViewModel : IViewModelBase
    {
        [Inject]
        [NotNull]
#pragma warning disable CS8618
        protected TViewModel ViewModel { get; set; }
#pragma warning restore CS8618

        protected override void OnInitialized()
        {
            ViewModel.PropertyChanged += (_, _) => InvokeAsync(() => StateHasChanged());
            base.OnInitialized();
        }

        protected override Task OnInitializedAsync()
        {
            return ViewModel.OnInitializedAsync();
        }
    }
}
