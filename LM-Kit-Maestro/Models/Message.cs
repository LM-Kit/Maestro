
namespace LMKitMaestro.Models;

public sealed class Message
{
    public string? Text { get; set; }

    public MessageSender Sender { get; set; }
}

public enum MessageSender
{
    Undefined,
    User,
    Assistant
}
