using LMKit.Maestro.Services;
using LMKit.Maestro.ViewModels;
using System.Diagnostics;

namespace Maestro.Tests.Services;

internal sealed class ConversationViewModelWrapper
{
    public ConversationViewModel ConversationViewModel { get; }

    public TaskCompletionSource<bool> PromptResultTask { get; } = new TaskCompletionSource<bool>();
    public TaskCompletionSource<bool> DatabaseSyncTask { get; } = new TaskCompletionSource<bool>();
    public TaskCompletionSource<string> TitleGenerationTask { get; } = new TaskCompletionSource<string>();

    public ConversationViewModelWrapper(ConversationViewModel conversationViewModel)
    {
        ConversationViewModel = conversationViewModel;
        ConversationViewModel.TextGenerationCompleted += OnTextGenerationCompleted;
        ConversationViewModel.DatabaseSaveOperationCompleted += OnDatabaseSynchronizationCompleted;
        ConversationViewModel.PropertyChanged += ConversationViewModel_PropertyChanged;
    }

    private void ConversationViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConversationViewModel.Title))
        {
            TitleGenerationTask.SetResult(ConversationViewModel.Title);
        }
    }

    private void OnTextGenerationCompleted(object? sender, ConversationViewModel.TextGenerationCompletedEventArgs e)
    {
        var conversationViewModel = (ConversationViewModel)sender!;

        if (e.Exception != null || e.Status != LMKitRequestStatus.OK)
        {
            Trace.Write("Text generation failed: " + e.Exception?.Message);
            PromptResultTask.SetResult(false);
        }
        else
        {
            Trace.Write("Text generation completed successfully");
            PromptResultTask.SetResult(true);
        }
    }

    private void OnDatabaseSynchronizationCompleted(object? sender, EventArgs e)
    {
        var conversationViewModel = (ConversationViewModel)sender!;

        if (!DatabaseSyncTask.Task.IsCompleted)
        {
            DatabaseSyncTask.SetResult(true);
        }
    }
}
