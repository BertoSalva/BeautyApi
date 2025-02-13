﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Sender")]
    public int SenderId { get; set; }
    public User Sender { get; set; }

    [ForeignKey("Receiver")]
    public int ReceiverId { get; set; }
    public User Receiver { get; set; }

    public string? Message { get; set; }
    public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
}
 