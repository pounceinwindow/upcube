using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class Message : Entity
{
    public int InquiryId { get; set; }

    public Inquiry? Inquiry { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime? ReadAt { get; set; }
}