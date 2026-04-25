using UpperCube.Domain.Common;
using UpperCube.Domain.Enums;

namespace UpperCube.Domain.Entities;

public sealed class Inquiry : AuditableEntity
{
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public string FromUserId { get; set; } = string.Empty;

    public string InitialMessage { get; set; } = string.Empty;

    public InquiryStatus Status { get; set; } = InquiryStatus.Open;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
