using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Hubs;

namespace UpperCube.Web.Services.Inquiries;

public sealed class InquiryChatService(
    IInquiryRepository inquiryRepository,
    IMessageRepository messageRepository,
    IUnitOfWork unitOfWork,
    IHubContext<InquiryHub> hubContext,
    UserManager<ApplicationUser> userManager) : IInquiryChatService
{
    private const int MessageMaxLength = 4000;

    public string GetGroupName(int inquiryId)
    {
        return $"inquiry-{inquiryId}";
    }

    public async Task<InquiryAccessResult> GetAccessibleInquiryAsync(
        int inquiryId,
        ClaimsPrincipal? user,
        CancellationToken ct = default)
    {
        if (user is null)
            return AccessFailure(InquiryChatFailure.Unauthorized, "Требуется авторизация.");

        var userId = userManager.GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return AccessFailure(InquiryChatFailure.Unauthorized, "Требуется авторизация.");

        var inquiry = await inquiryRepository.GetByIdAsync(inquiryId, ct);
        if (inquiry is null)
            return AccessFailure(InquiryChatFailure.NotFound, "Заявка не найдена.", userId);

        if (!CanAccessInquiry(inquiry, user, userId))
            return AccessFailure(InquiryChatFailure.Forbidden, "Заявка недоступна.", userId);

        return new InquiryAccessResult(InquiryChatFailure.None, null, userId, inquiry);
    }

    public async Task<InquirySendMessageResult> SendMessageAsync(
        int inquiryId,
        string? text,
        ClaimsPrincipal? user,
        CancellationToken ct = default)
    {
        var access = await GetAccessibleInquiryAsync(inquiryId, user, ct);
        if (!access.Succeeded)
            return SendFailure(access.Failure, access.ErrorMessage);

        var normalizedText = text?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedText))
            return SendFailure(InquiryChatFailure.EmptyMessage, "Введите сообщение перед отправкой.");

        if (normalizedText.Length > MessageMaxLength)
            return SendFailure(InquiryChatFailure.MessageTooLong,
                $"Сообщение слишком длинное. Максимум {MessageMaxLength} символов.");

        var now = DateTime.UtcNow;
        var inquiry = access.Inquiry!;
        var userId = access.UserId!;
        var message = new Message
        {
            InquiryId = inquiry.Id,
            SenderId = userId,
            Text = normalizedText,
            SentAt = now
        };

        inquiry.UpdatedAt = now;

        await messageRepository.AddAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var sender = await userManager.GetUserAsync(user!);
        var payload = new InquiryChatMessagePayload(
            inquiry.Id,
            message.Id,
            userId,
            GetDisplayName(sender),
            message.Text,
            message.SentAt);

        await hubContext.Clients.Group(GetGroupName(inquiry.Id)).SendAsync("ReceiveMessage", payload, ct);

        return new InquirySendMessageResult(InquiryChatFailure.None, null, payload);
    }

    private static bool CanAccessInquiry(Inquiry inquiry, ClaimsPrincipal user, string userId)
    {
        if (user.IsInRole("Admin")) return true;
        if (string.Equals(inquiry.FromUserId, userId, StringComparison.Ordinal)) return true;

        return user.IsInRole("Agent")
               && string.Equals(inquiry.Property?.AgentId, userId, StringComparison.Ordinal);
    }

    private static InquiryAccessResult AccessFailure(
        InquiryChatFailure failure,
        string errorMessage,
        string? userId = null)
    {
        return new InquiryAccessResult(failure, errorMessage, userId, null);
    }

    private static InquirySendMessageResult SendFailure(InquiryChatFailure failure, string? errorMessage)
    {
        return new InquirySendMessageResult(failure, errorMessage, null);
    }

    private static string GetDisplayName(ApplicationUser? user)
    {
        if (user is null) return "Пользователь";

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName)
            ? user.Email ?? user.UserName ?? "Пользователь"
            : fullName;
    }
}
