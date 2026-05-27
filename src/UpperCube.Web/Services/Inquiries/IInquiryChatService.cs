using System.Security.Claims;
using UpperCube.Domain.Entities;

namespace UpperCube.Web.Services.Inquiries;

public interface IInquiryChatService
{
    string GetGroupName(int inquiryId);

    Task<InquiryAccessResult> GetAccessibleInquiryAsync(
        int inquiryId,
        ClaimsPrincipal? user,
        CancellationToken ct = default);

    Task<InquirySendMessageResult> SendMessageAsync(
        int inquiryId,
        string? text,
        ClaimsPrincipal? user,
        CancellationToken ct = default);
}

public enum InquiryChatFailure
{
    None,
    Unauthorized,
    NotFound,
    Forbidden,
    EmptyMessage,
    MessageTooLong
}

public sealed record InquiryAccessResult(
    InquiryChatFailure Failure,
    string? ErrorMessage,
    string? UserId,
    Inquiry? Inquiry)
{
    public bool Succeeded => Failure == InquiryChatFailure.None;
}

public sealed record InquirySendMessageResult(
    InquiryChatFailure Failure,
    string? ErrorMessage,
    InquiryChatMessagePayload? Message)
{
    public bool Succeeded => Failure == InquiryChatFailure.None;
}

public sealed record InquiryChatMessagePayload(
    int InquiryId,
    int MessageId,
    string SenderId,
    string SenderName,
    string Text,
    DateTime SentAt);