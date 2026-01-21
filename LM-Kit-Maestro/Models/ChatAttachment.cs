using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LMKit.Maestro.Models;

/// <summary>
/// Represents a file attachment for chat messages (images or PDFs).
/// </summary>
public class ChatAttachment : INotifyPropertyChanged
{
    private string _fileName;
    private string _mimeType;
    private byte[] _content;
    private string _thumbnailBase64;
    private bool _isImage;
    private bool _isPdf;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The original file name.
    /// </summary>
    public string FileName
    {
        get => _fileName;
        set { _fileName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// The MIME type of the attachment (e.g., "image/png", "application/pdf").
    /// </summary>
    public string MimeType
    {
        get => _mimeType;
        set
        {
            _mimeType = value;
            IsImage = value?.StartsWith("image/") == true;
            IsPdf = value == "application/pdf";
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The raw file content as bytes.
    /// </summary>
    public byte[] Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Base64-encoded thumbnail for display in the UI.
    /// For images, this is the image itself (possibly resized).
    /// For PDFs, this could be a PDF icon or first page preview.
    /// </summary>
    public string ThumbnailBase64
    {
        get => _thumbnailBase64;
        set { _thumbnailBase64 = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether this attachment is an image.
    /// </summary>
    public bool IsImage
    {
        get => _isImage;
        private set { _isImage = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether this attachment is a PDF.
    /// </summary>
    public bool IsPdf
    {
        get => _isPdf;
        private set { _isPdf = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize => Content?.Length ?? 0;

    /// <summary>
    /// Human-readable file size.
    /// </summary>
    public string FileSizeDisplay
    {
        get
        {
            var size = FileSize;
            if (size < 1024)
            {
                return $"{size} B";
            }

            if (size < 1024 * 1024)
            {
                return $"{size / 1024.0:F1} KB";
            }

            return $"{size / (1024.0 * 1024.0):F1} MB";
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Creates a ChatAttachment from file bytes.
    /// </summary>
    public static ChatAttachment FromBytes(string fileName, string mimeType, byte[] content)
    {
        var attachment = new ChatAttachment
        {
            FileName = fileName,
            MimeType = mimeType,
            Content = content
        };

        // Generate thumbnail
        if (attachment.IsImage)
        {
            // For images, use the image itself as thumbnail (base64)
            attachment.ThumbnailBase64 = Convert.ToBase64String(content);
        }
        else if (attachment.IsPdf)
        {
            // For PDFs, we'll show a PDF icon (handled in UI)
            attachment.ThumbnailBase64 = null;
        }

        return attachment;
    }

    /// <summary>
    /// Gets the accepted file types for vision models.
    /// </summary>
    public static string AcceptedFileTypes => ".png,.jpg,.jpeg,.gif,.webp,.bmp,.tiff,.pdf";

    /// <summary>
    /// Validates if the given MIME type is supported for vision attachments.
    /// </summary>
    public static bool IsSupportedMimeType(string mimeType)
    {
        if (string.IsNullOrEmpty(mimeType))
        {
            return false;
        }

        var supportedTypes = new[]
        {
            "image/png",
            "image/jpeg",
            "image/jpg",
            "image/gif",
            "image/webp",
            "image/bmp",
            "image/tiff",
            "application/pdf"
        };

        return supportedTypes.Contains(mimeType.ToLowerInvariant());
    }
}
