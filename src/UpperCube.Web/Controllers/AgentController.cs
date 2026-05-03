using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

[Authorize(Roles = "Agent")]
[Route("agent")]
public sealed class AgentController(
    IPropertyRepository repository,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("properties")]
    public async Task<IActionResult> MyProperties(CancellationToken ct)
    {
        var userId = await userManager.GetUserAsync(HttpContext.User);
        if (userId == null) return Challenge();
        var property = await repository.GetByAgentIdAsync(userId.Id, ct);
        var propertyListItem = property
            .Select(x => x.ToListItemDto());
        return View(propertyListItem);
    }
}