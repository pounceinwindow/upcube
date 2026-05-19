using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UpperCube.Web.Services.Inquiries;

namespace UpperCube.Web.Hubs;

[Authorize]
public sealed class InquiryHub(IInquiryChatService inquiryChatService) : Hub
{
    public async Task JoinInquiry(int inquiryId)
    {
        var access = await inquiryChatService.GetAccessibleInquiryAsync(
            inquiryId,
            Context.User,
            Context.ConnectionAborted);
        if (!access.Succeeded) throw new HubException(access.ErrorMessage ?? "Заявка недоступна.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            inquiryChatService.GetGroupName(inquiryId),
            Context.ConnectionAborted);
    }

    public async Task LeaveInquiry(int inquiryId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            inquiryChatService.GetGroupName(inquiryId),
            Context.ConnectionAborted);
    }

    public async Task SendMessage(int inquiryId, string? text)
    {
        var result = await inquiryChatService.SendMessageAsync(
            inquiryId,
            text,
            Context.User,
            Context.ConnectionAborted);
        if (!result.Succeeded) throw new HubException(result.ErrorMessage ?? "Не удалось отправить сообщение.");
    }
}
