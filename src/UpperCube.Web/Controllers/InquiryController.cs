using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Web.Mapping;
using UpperCube.Web.Services.Inquiries;

namespace UpperCube.Web.Controllers;

[Authorize]
[Route("inquiries")]
public sealed class InquiryController(IInquiryChatService inquiryChatService) : Controller
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var access = await inquiryChatService.GetAccessibleInquiryAsync(id, User, ct);
        if (!access.Succeeded) return ToActionResult(access.Failure);

        return View(access.Inquiry!.ToDetailsModelView(access.UserId!));
    }

    [HttpPost("{id:int}/reply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(
        int id,
        [FromForm] string? message,
        CancellationToken ct)
    {
        var result = await inquiryChatService.SendMessageAsync(id, message, User, ct);
        if (!result.Succeeded)
        {
            if (IsAccessFailure(result.Failure)) return ToActionResult(result.Failure);

            TempData["Error"] = result.ErrorMessage ?? "Не удалось отправить сообщение.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Сообщение отправлено.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private IActionResult ToActionResult(InquiryChatFailure failure)
    {
        return failure switch
        {
            InquiryChatFailure.Unauthorized => Challenge(),
            InquiryChatFailure.NotFound => NotFound(),
            InquiryChatFailure.Forbidden => Forbid(),
            _ => BadRequest()
        };
    }

    private static bool IsAccessFailure(InquiryChatFailure failure)
    {
        return failure is InquiryChatFailure.Unauthorized
            or InquiryChatFailure.NotFound
            or InquiryChatFailure.Forbidden;
    }
}
