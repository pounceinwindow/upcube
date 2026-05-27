using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Web.Models.Inquiry;

namespace UpperCube.Web.Mapping;

public static class InquiryMapping
{
    public static InquiryListItemModelView ToListItemModelView(this Inquiry inquiry)
    {
        var lastMessage = inquiry.Messages
            .OrderByDescending(x => x.SentAt)
            .FirstOrDefault();

        return new InquiryListItemModelView(
            inquiry.Id,
            inquiry.PropertyId,
            GetPropertyTitle(inquiry),
            inquiry.Property?.Address ?? string.Empty,
            inquiry.InitialMessage,
            ToDisplayText(inquiry.Status),
            inquiry.CreatedAt,
            inquiry.UpdatedAt,
            inquiry.Messages.Count,
            lastMessage?.Text,
            lastMessage?.SentAt);
    }

    public static InquiryDetailsModelView ToDetailsModelView(this Inquiry inquiry, string currentUserId)
    {
        var messages = inquiry.Messages
            .OrderBy(x => x.SentAt)
            .Select(x => new InquiryMessageModelView(
                x.Id,
                x.Text,
                x.SentAt,
                string.Equals(x.SenderId, currentUserId, StringComparison.Ordinal)))
            .ToList();

        return new InquiryDetailsModelView(
            inquiry.Id,
            inquiry.PropertyId,
            GetPropertyTitle(inquiry),
            inquiry.Property?.Address ?? string.Empty,
            inquiry.InitialMessage,
            ToDisplayText(inquiry.Status),
            inquiry.CreatedAt,
            inquiry.UpdatedAt,
            currentUserId,
            messages);
    }

    private static string GetPropertyTitle(Inquiry inquiry)
    {
        return inquiry.Property?.Title ?? $"Объект #{inquiry.PropertyId}";
    }

    private static string ToDisplayText(InquiryStatus status)
    {
        return status switch
        {
            InquiryStatus.Open => "Открыта",
            InquiryStatus.InProgress => "В работе",
            InquiryStatus.Closed => "Закрыта",
            _ => status.ToString()
        };
    }
}