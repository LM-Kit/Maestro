using LMKit.Maestro.Models;

namespace LMKit.Maestro.Services;

/// <summary>
/// Represents a prompt submission with optional attachments.
/// </summary>
public class PromptSubmission
{
    /// <summary>
    /// The text content of the prompt.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Optional attachments (images, PDFs) to include with the prompt.
    /// </summary>
    public IReadOnlyList<ChatAttachment> Attachments { get; }

    /// <summary>
    /// Whether this submission has any attachments.
    /// </summary>
    public bool HasAttachments => Attachments != null && Attachments.Count > 0;

    public PromptSubmission(string text, IEnumerable<ChatAttachment>? attachments = null)
    {
        Text = text ?? string.Empty;
        Attachments = attachments?.ToList() ?? new List<ChatAttachment>();
    }
}
