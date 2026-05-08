using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Domain.ValueObjects;
using UpperCube.Infrastructure.Identity;
using UpperCube.Infrastructure.Persistence;

namespace UpperCube.Infrastructure.Seeding;

public static class DataSeeder
{
    private static readonly DateTime SeedTimestamp = new(2026, 4, 25, 0, 0, 0, DateTimeKind.Utc);
    private const string BannerImagePrefix = "/homelengo/images/banner/";
    private const string LegacyBannerImagePrefix = "/images/banner/";
    private const string OldDemoImagePrefix = "/images/demo/properties/";
    private const int BannerPropertyImageCount = 18;
    private const int SecondaryImageOffset = 6;
    private const int TertiaryImageOffset = 12;

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedDictionariesAsync(context);
        await SeedPropertiesAsync(context, userManager);
        await SeedFeatureCatalogAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Agent", "User" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await EnsureUserAsync(
            userManager,
            "admin@uppercube.local",
            "Admin123!",
            "Admin",
            "Админ",
            "Системный",
            false);

        var agent = await EnsureUserAsync(
            userManager,
            "agent@uppercube.local",
            "Agent123!",
            "Agent",
            "Иван",
            "Агентов",
            true);

        if ((await userManager.GetClaimsAsync(agent)).All(x => x.Type != "IsVerified"))
            await userManager.AddClaimAsync(agent, new Claim("IsVerified", "true"));

        await EnsureUserAsync(
            userManager,
            "user@uppercube.local",
            "User1234!",
            "User",
            "Мария",
            "Пользователева",
            false);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string firstName,
        string lastName,
        bool isVerified)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsVerified = isVerified,
                PreferredLanguage = "ru",
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed user {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role)) await userManager.AddToRoleAsync(user, role);

        return user;
    }

    private static async Task SeedDictionariesAsync(AppDbContext context)
    {
        if (await context.Cities.AnyAsync()) return;

        var moscow = new City { Name = "Москва", Slug = "moscow" };
        var spb = new City { Name = "Санкт-Петербург", Slug = "saint-petersburg" };
        var krasnodar = new City { Name = "Краснодар", Slug = "krasnodar" };

        context.Cities.AddRange(moscow, spb, krasnodar);
        await context.SaveChangesAsync();

        context.Districts.AddRange(
            new District { CityId = moscow.Id, Name = "Центральный", Slug = "centralny" },
            new District { CityId = moscow.Id, Name = "Пресненский", Slug = "presnensky" },
            new District { CityId = moscow.Id, Name = "Хамовники", Slug = "khamovniki" },
            new District { CityId = moscow.Id, Name = "Тверской", Slug = "tverskoy" },
            new District { CityId = spb.Id, Name = "Петроградский", Slug = "petrogradsky" },
            new District { CityId = spb.Id, Name = "Василеостровский", Slug = "vasileostrovsky" },
            new District { CityId = spb.Id, Name = "Центральный", Slug = "centralny-spb" },
            new District { CityId = krasnodar.Id, Name = "Центральный", Slug = "centralny-krd" },
            new District { CityId = krasnodar.Id, Name = "Западный", Slug = "zapadny" },
            new District { CityId = krasnodar.Id, Name = "Юбилейный", Slug = "yubileyny" });

        context.PropertyTypes.AddRange(
            new PropertyType { Name = "Квартира", Slug = "apartment", IconClass = "flaticon-apartment" },
            new PropertyType { Name = "Дом", Slug = "house", IconClass = "flaticon-house" },
            new PropertyType { Name = "Коммерческая", Slug = "commercial", IconClass = "flaticon-building" },
            new PropertyType { Name = "Участок", Slug = "land", IconClass = "flaticon-land" });

        context.Categories.AddRange(
            new Category { Name = "Жилая", Slug = "residential" },
            new Category { Name = "Коммерческая", Slug = "commercial" },
            new Category { Name = "Элитная", Slug = "luxury" });

        context.Amenities.AddRange(
            new Amenity { Name = "Парковка", IconClass = "flaticon-parking" },
            new Amenity { Name = "Балкон", IconClass = "flaticon-balcony" },
            new Amenity { Name = "Лифт", IconClass = "flaticon-elevator" },
            new Amenity { Name = "Кондиционер", IconClass = "flaticon-air-conditioner" },
            new Amenity { Name = "Мебель", IconClass = "flaticon-furniture" },
            new Amenity { Name = "Охрана", IconClass = "flaticon-security" },
            new Amenity { Name = "Бассейн", IconClass = "flaticon-pool" },
            new Amenity { Name = "Спортзал", IconClass = "flaticon-gym" });

        await context.SaveChangesAsync();
    }

    private static async Task SeedPropertiesAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        var agent = await userManager.FindByEmailAsync("agent@uppercube.local")
                    ?? throw new InvalidOperationException("Seed agent was not found.");

        var cityIds = await context.Cities.ToDictionaryAsync(x => x.Slug, x => x.Id);
        var districtIds = await context.Districts.ToDictionaryAsync(x => x.Slug, x => x.Id);
        var typeIds = await context.PropertyTypes.ToDictionaryAsync(x => x.Slug, x => x.Id);
        var categoryIds = await context.Categories.ToDictionaryAsync(x => x.Slug, x => x.Id);
        var amenityIds = await context.Amenities.OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync();
        var existingProperties = await context.Properties
            .Include(x => x.Images)
            .ToDictionaryAsync(x => x.Title);

        var specs = new[]
        {
            new PropertySeed("Просторная 3-комн квартира в центре Москвы", 18500000m, 95m, 3, 7, 16, "moscow",
                "centralny", "apartment", "residential"),
            new PropertySeed("Современный дом с бассейном в Хамовниках", 85000000m, 280m, 5, 1, 2, "moscow",
                "khamovniki", "house", "luxury"),
            new PropertySeed("Студия у метро Пресня", 8200000m, 38m, 1, 3, 9, "moscow", "presnensky", "apartment",
                "residential"),
            new PropertySeed("Семейная квартира на Тверской", 22500000m, 72m, 2, 5, 12, "moscow", "tverskoy",
                "apartment", "residential"),
            new PropertySeed("Пентхаус на Петроградской стороне", 65000000m, 140m, 4, 18, 20, "saint-petersburg",
                "petrogradsky", "apartment", "luxury"),
            new PropertySeed("Уютный дом на Васильевском", 42000000m, 180m, 4, 1, 2, "saint-petersburg",
                "vasileostrovsky", "house", "residential"),
            new PropertySeed("Офис у Невского проспекта", 28000000m, 110m, 4, 2, 8, "saint-petersburg", "centralny-spb",
                "commercial", "commercial"),
            new PropertySeed("Апартаменты с готовым ремонтом", 15800000m, 82m, 3, 10, 14, "saint-petersburg",
                "petrogradsky", "apartment", "residential"),
            new PropertySeed("Большой участок под строительство", 6500000m, 900m, 1, 0, 0, "krasnodar", "zapadny",
                "land", "residential"),
            new PropertySeed("Коммерческое помещение на первой линии", 19500000m, 135m, 3, 1, 5, "krasnodar",
                "centralny-krd", "commercial", "commercial"),
            new PropertySeed("Светлая 2-комн квартира в новом ЖК", 7800000m, 61m, 2, 8, 12, "krasnodar", "yubileyny",
                "apartment", "residential"),
            new PropertySeed("Коттедж для большой семьи", 28000000m, 240m, 6, 1, 2, "krasnodar", "zapadny", "house",
                "luxury")
        };

        for (var index = 0; index < specs.Length; index++)
        {
            var spec = specs[index];
            var number = index + 1;
            if (!existingProperties.TryGetValue(spec.Title, out var property))
            {
                property = new Property
                {
                    Title = spec.Title,
                    Description =
                        $"{spec.Title}. Отличное состояние, удобная транспортная доступность и развитая инфраструктура рядом.",
                    Price = new Money(spec.Price, "RUB"),
                    Area = new Area(spec.Area),
                    Rooms = spec.Rooms,
                    Floor = spec.Floor,
                    TotalFloors = spec.TotalFloors,
                    TransactionType = TransactionType.Sale,
                    AgentId = agent.Id,
                    Address = $"Большая улица, {number}",
                    CityId = cityIds[spec.CitySlug],
                    DistrictId = districtIds[spec.DistrictSlug],
                    PropertyTypeId = typeIds[spec.TypeSlug],
                    CategoryId = categoryIds[spec.CategorySlug],
                    ViewsCount = number * 17
                };

                property.Approve(SeedTimestamp);

                foreach (var amenityId in PickAmenities(amenityIds, index))
                    property.Amenities.Add(new PropertyAmenity { AmenityId = amenityId });

                context.Properties.Add(property);
            }

            EnsureBannerImages(property, number);
        }

        await context.SaveChangesAsync();
    }

    private static void EnsureBannerImages(Property property, int number)
    {
        var primaryPath = BannerPropertyImagePath(number);
        var secondaryPath = BannerPropertyImagePath(number + SecondaryImageOffset);
        var tertiaryPath = BannerPropertyImagePath(number + TertiaryImageOffset);
        var desiredPaths = new[] { primaryPath, secondaryPath, tertiaryPath };
        var keptSeedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var managedSeedImages = property.Images.Where(IsManagedSeedImage).ToList();
        foreach (var image in managedSeedImages)
        {
            var desiredOrder = Array.FindIndex(desiredPaths,
                path => string.Equals(path, image.Path, StringComparison.OrdinalIgnoreCase));

            if (desiredOrder >= 0 && keptSeedPaths.Add(image.Path))
            {
                image.MediaType = MediaType.Photo;
                image.Order = desiredOrder;
                image.UploadedAt = SeedTimestamp;
                continue;
            }

            property.Images.Remove(image);
        }

        for (var order = 0; order < desiredPaths.Length; order++)
        {
            var path = desiredPaths[order];
            if (property.Images.Any(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase))) continue;

            property.Images.Add(new PropertyImage
            {
                Path = path,
                MediaType = MediaType.Photo,
                IsPrimary = false,
                Order = order,
                UploadedAt = SeedTimestamp
            });
        }

        foreach (var image in property.Images)
        {
            image.IsPrimary = string.Equals(image.Path, primaryPath, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool IsManagedSeedImage(PropertyImage image)
    {
        return image.Path.StartsWith(BannerImagePrefix, StringComparison.OrdinalIgnoreCase) ||
               image.Path.StartsWith(LegacyBannerImagePrefix, StringComparison.OrdinalIgnoreCase) ||
               image.Path.StartsWith(OldDemoImagePrefix, StringComparison.OrdinalIgnoreCase) ||
               (image.Path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) &&
                image.Path.Contains("/seed/", StringComparison.OrdinalIgnoreCase));
    }

    private static string BannerPropertyImagePath(int number)
    {
        var normalized = ((number - 1) % BannerPropertyImageCount) + 1;
        return $"{BannerImagePrefix}banner-property-{normalized}.jpg";
    }

    private static IEnumerable<int> PickAmenities(IReadOnlyList<int> amenityIds, int offset)
    {
        if (amenityIds.Count == 0) yield break;

        yield return amenityIds[offset % amenityIds.Count];
        yield return amenityIds[(offset + 2) % amenityIds.Count];
        yield return amenityIds[(offset + 4) % amenityIds.Count];
    }

    private static async Task SeedFeatureCatalogAsync(AppDbContext context)
    {
        if (await context.FeatureCatalog.AnyAsync()) return;

        context.FeatureCatalog.AddRange(
            new FeatureCatalogEntry
            {
                Code = "property.valuation",
                DisplayName = "Оценка стоимости",
                Description = "AI-оценщик рыночной стоимости недвижимости",
                IsEnabled = true
            },
            new FeatureCatalogEntry
            {
                Code = "property.tour360.upload",
                DisplayName = "Загрузка 360° панорам",
                Description = "Загрузка и отображение виртуальных 360° туров",
                IsEnabled = true
            },
            new FeatureCatalogEntry
            {
                Code = "property.compare",
                DisplayName = "Сравнение объектов",
                Description = "Side-by-side сравнение объявлений",
                IsEnabled = true
            });

        await context.SaveChangesAsync();
    }

    private sealed record PropertySeed(
        string Title,
        decimal Price,
        decimal Area,
        int Rooms,
        int Floor,
        int TotalFloors,
        string CitySlug,
        string DistrictSlug,
        string TypeSlug,
        string CategorySlug);
}
