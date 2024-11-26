﻿namespace LMKit.Maestro.Models;

public sealed class Message
{
    public string? Text { get; set; }

    public MessageSender Sender { get; set; }
}