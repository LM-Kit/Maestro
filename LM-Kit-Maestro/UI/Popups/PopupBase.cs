using Mopups.Interfaces;
using Mopups.Pages;

namespace LMKitMaestro.UI
{
    public abstract class PopupBase : PopupPage
    {
        private readonly IPopupNavigation _popupNavigation;
        private readonly TaskCompletionSource<PopupResult> _taskCompletionSource = new TaskCompletionSource<PopupResult>();
        private readonly object _locker = new object();

        public Task<PopupResult> PopupTask => _taskCompletionSource.Task;

        public PopupBase(IPopupNavigation popupNavigation)
        {
            _popupNavigation = popupNavigation;
            CloseWhenBackgroundIsClicked = false;
            BackgroundClicked += OnBackgroundClicked;
        }

        protected virtual async Task Dismiss(bool cancelled = false)
        {
            await SetResult(null, cancelled);
        }

        private async void OnBackgroundClicked(object? sender, EventArgs e)
        {
            await SetResult(null, true);
        }

        protected async Task SetResult(object? value, bool cancelled = false)
        {
            await _popupNavigation.PopAsync();

            lock (_locker)
            {
                if (!_taskCompletionSource.Task.IsCompleted)
                {
                    _taskCompletionSource!.SetResult(new PopupResult() { Cancelled = cancelled, Value = value });
                }
            }

        }

        public sealed class PopupResult
        {
            public object? Value { get; set; }

            public bool Cancelled { get; set; }
        }
    }
}
