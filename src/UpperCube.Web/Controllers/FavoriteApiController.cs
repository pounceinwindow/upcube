using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/favorites")]
public sealed class FavoritesApiController(
    IFavoriteRepository favoriteRepository,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] ToggleRequest request, CancellationToken ct)
    {
        var isFavorite = false;
        var id = userManager.GetUserId(User);
        var favorite = await favoriteRepository.GetAsync(id, request.PropertyId, ct);
        if (favorite == null)
        {
            await favoriteRepository.AddAsync(new Favorite { PropertyId = request.PropertyId, UserId = id }, ct);
            isFavorite = true;
        }
        else
        {
            await favoriteRepository.DeleteAsync(favorite, ct);
            isFavorite = false;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Ok(new { IsFavorite = isFavorite });
    }

    public sealed record ToggleRequest(int PropertyId);
}