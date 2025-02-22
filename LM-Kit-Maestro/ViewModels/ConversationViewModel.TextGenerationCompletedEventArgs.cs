using LMKit.Maestro.Services;
using LMKit.TextGeneration;

namespace LMKit.Maestro.ViewModels;

public partial class ConversationViewModel
{
    public sealed class TextGenerationCompletedEventArgs : EventArgs
    {
        public Exception? Exception { get; }

        public LMKitRequestStatus? Status { get; }

        public TextGenerationResult? Result { get; }

        public TextGenerationCompletedEventArgs(TextGenerationResult? result = null, Exception? exception = null,
            LMKitRequestStatus? status = null)
        {
            Result = result;
            Exception = exception;
            Status = status;
        }
    }
}