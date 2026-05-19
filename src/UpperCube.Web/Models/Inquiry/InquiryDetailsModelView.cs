namespace UpperCube.Web.Models.Inquiry;

public sealed record InquiryDetailsModelView(
    int Id,
    int PropertyId,
    string PropertyTitle,
    string PropertyAddress,
    string InitialMessage,
    string StatusText,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CurrentUserId,
    IReadOnlyList<InquiryMessageModelView> Messages);

public sealed record InquiryMessageModelView(
    int Id,
    string Text,
    DateTime SentAt,
    bool IsOwnMessage);
