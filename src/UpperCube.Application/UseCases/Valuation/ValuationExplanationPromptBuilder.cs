using System.Globalization;
using System.Text;
using UpperCube.Application.Abstractions.AI;
using UpperCube.Application.Abstractions.Valuation;

namespace UpperCube.Application.UseCases.Valuation;

public sealed class ValuationExplanationPromptBuilder
{
    public LocalLlmRequest Build(ValuationExplanationContext context)
    {
        var isRussian = context.Culture.StartsWith("ru", StringComparison.OrdinalIgnoreCase);
        var systemPrompt = isRussian
            ? """
              Ты - помощник по оценке недвижимости.
              Отвечай на русском языке.
              Используй только предоставленные данные.
              Не придумывай новые цены.
              Не изменяй оценочный диапазон.
              Не ссылайся на внешние источники.
              Не утверждай абсолютную точность оценки.
              Дай короткое и понятное объяснение для пользователя.
              """
            : """
              You are a real estate valuation assistant.
              Answer in English.
              Use only the provided data.
              Do not invent prices.
              Do not change the valuation range.
              Do not refer to external sources.
              Do not claim absolute accuracy.
              Give a concise and user-friendly explanation.
              """;

        return new LocalLlmRequest(systemPrompt, BuildUserPrompt(context, isRussian), context.Culture);
    }

    private static string BuildUserPrompt(ValuationExplanationContext context, bool isRussian)
    {
        var culture = CultureInfo.InvariantCulture;
        var prompt = new StringBuilder();

        prompt.AppendLine($"Culture: {context.Culture}");
        prompt.AppendLine($"City: {ValueOrEmpty(context.CityName)}");
        prompt.AppendLine($"District: {ValueOrEmpty(context.DistrictName)}");
        prompt.AppendLine($"Property type: {ValueOrEmpty(context.PropertyTypeName)}");
        prompt.AppendLine($"Area: {context.Area.ToString("0.##", culture)}");
        prompt.AppendLine($"Rooms: {context.Rooms}");
        prompt.AppendLine($"Floor: {context.Floor?.ToString(culture) ?? string.Empty}");
        prompt.AppendLine($"Total floors: {context.TotalFloors?.ToString(culture) ?? string.Empty}");
        prompt.AppendLine(
            $"Estimated range: {context.EstimatedMin.ToString("0.##", culture)} - {context.EstimatedMax.ToString("0.##", culture)}");
        prompt.AppendLine($"Currency: {context.Currency}");
        prompt.AppendLine($"Strategy: {context.StrategyUsed}");
        prompt.AppendLine("Comparable properties:");

        if (context.Comparables.Count == 0)
        {
            prompt.AppendLine("- none provided");
        }
        else
        {
            for (var i = 0; i < context.Comparables.Count; i++)
            {
                var comparable = context.Comparables[i];
                prompt.AppendLine(
                    $"{i + 1}. Id: {comparable.PropertyId}; Title: {comparable.Title}; District: {ValueOrEmpty(comparable.DistrictName)}; Area: {comparable.Area.ToString("0.##", culture)}; Price: {comparable.Price.ToString("0.##", culture)}; PricePerSquareMeter: {comparable.PricePerSquareMeter.ToString("0.##", culture)}; Rooms: {comparable.Rooms}");
            }
        }

        prompt.AppendLine();
        prompt.AppendLine("Ask:");
        prompt.AppendLine(isRussian
            ? "Объясни на русском, почему этот диапазон оценки может быть разумным. Упомяни ключевые факторы. Ответ до 1200 символов. Не добавляй числа, которых нет в контексте."
            : "Explain in English why this valuation range may be reasonable. Mention key factors. Keep the answer concise and under 180 words. Do not add new numbers that were not provided.");

        return prompt.ToString();
    }

    private static string ValueOrEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }
}
