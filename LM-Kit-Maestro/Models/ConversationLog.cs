using SQLite;

namespace LMKit.Maestro.Models;

public sealed class ConversationLog
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string? Title { get; set; }

    public DateTime Date { get; set; }

    public byte[]? ChatHistoryData { get; set; }

    public Uri? LastUsedModel { get; set; }

    public bool IsStarred { get; set; }

    public ConversationLog()
    {
    }

    public ConversationLog(string title)
    {
        Title = title;
        Date = DateTime.Now;
    }
}
