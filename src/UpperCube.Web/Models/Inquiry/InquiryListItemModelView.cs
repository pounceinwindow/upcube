namespace UpperCube.Web.Models.Inquiry;

public sealed record InquiryListItemModelView(
    int Id,
    int PropertyId,
    string PropertyTitle,
    string PropertyAddress,
    string InitialMessage,
    string StatusText,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessagesCount,
    string? LastMessageText,
    DateTime? LastMessageAt);