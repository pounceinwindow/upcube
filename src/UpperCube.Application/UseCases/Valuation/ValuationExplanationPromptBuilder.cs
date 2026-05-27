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
                Ты — внимательный помощник по оценке недвижимости. Отвечай по-русски живо и понятно.
                Используй только предоставленные данные и осторожные формулировки.
                Не придумывай новые цены, районы, характеристики, состояние ремонта, инфраструктуру или внешние факты.
                Можно повторять только те числа, которые есть в контексте.
                Не меняй оценочный диапазон и не утверждай абсолютную точность оценки.
                Если важного фактора нет в данных, прямо скажи, что он не учтён в расчёте.
              """
            : """
              You are a careful real estate valuation assistant. Answer in natural, user-friendly English.
              Use only the provided data and cautious wording.
              Do not invent new prices, districts, features, renovation condition, infrastructure, or external facts.
              You may repeat only numbers that appear in the context.
              Do not change the valuation range and do not claim absolute accuracy.
              If an important factor is missing from the data, say that it was not included in the calculation.
              """;

        return new LocalLlmRequest(systemPrompt, BuildUserPrompt(context, isRussian), context.Culture);
    }

    private static string BuildUserPrompt(ValuationExplanationContext context, bool isRussian)
    {
        var culture = CultureInfo.InvariantCulture;
        var prompt = new StringBuilder();

        prompt.AppendLine("Valuation context:");
        prompt.AppendLine($"- Culture: {context.Culture}");
        prompt.AppendLine($"- City: {ValueOrEmpty(context.CityName)}");
        prompt.AppendLine($"- District: {ValueOrEmpty(context.DistrictName)}");
        prompt.AppendLine($"- Property type: {ValueOrEmpty(context.PropertyTypeName)}");
        prompt.AppendLine($"- Area: {context.Area.ToString("0.##", culture)}");
        prompt.AppendLine($"- Rooms: {context.Rooms}");
        prompt.AppendLine($"- Floor: {context.Floor?.ToString(culture) ?? "not provided"}");
        prompt.AppendLine($"- Total floors: {context.TotalFloors?.ToString(culture) ?? "not provided"}");
        prompt.AppendLine(
            $"- Estimated range: {context.EstimatedMin.ToString("0.##", culture)} - {context.EstimatedMax.ToString("0.##", culture)} {context.Currency}");
        prompt.AppendLine($"- Strategy: {context.StrategyUsed}");
        prompt.AppendLine("- Comparable properties:");

        if (context.Comparables.Count == 0)
        {
            prompt.AppendLine("  none");
        }
        else
        {
            var count = Math.Min(context.Comparables.Count, 5);
            for (var i = 0; i < count; i++)
            {
                var comparable = context.Comparables[i];

                prompt.AppendLine(
                    $"  {i + 1}. id={comparable.PropertyId}; title={ValueOrEmpty(comparable.Title)}; district={ValueOrEmpty(comparable.DistrictName)}; area={comparable.Area.ToString("0.##", culture)}; rooms={comparable.Rooms}; price={comparable.Price.ToString("0.##", culture)}; price_per_sqm={comparable.PricePerSquareMeter.ToString("0.##", culture)}");
            }
        }

        prompt.AppendLine(isRussian
            ? """
              Request:
              Объясни оценку в 2-4 коротких абзацах до 220 слов.
              Сделай ответ более интересным и полезным: начни с краткого вывода, затем объясни ключевые факторы, похожие объекты и ограничения данных.
              Можно писать естественно и немного editorial-style, но без выдуманных фактов.
              Не добавляй новые числа, которых нет в контексте.
              """
            : """
              Request:
              Explain the valuation in 2-4 short paragraphs under 220 words.
              Make the answer more interesting and useful: start with a brief takeaway, then explain key factors, comparable listings, and data limitations.
              You may write naturally with a light editorial style, but do not invent facts.
              Do not add numbers that are not present in the context.
              """);

        return prompt.ToString();
    }

    private static string ValueOrEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }
}