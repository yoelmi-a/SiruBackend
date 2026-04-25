using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Infraestructure.Ranking.Services;

public class RankingService : IRankingService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<RankingService> _logger;

    public RankingService(MLContext mlContext, ILogger<RankingService> logger)
    {
        _mlContext = mlContext;
        _logger = logger;
    }

    public async Task<float> ComputeScoreAsync(string cvText, string vacancyText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cvText) || string.IsNullOrWhiteSpace(vacancyText))
                return 0.0f;

            var data = new[]
            {
                new TextPair { Text1 = cvText, Text2 = vacancyText }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Text
                .FeaturizeText("Features1", nameof(TextPair.Text1))
                .Append(_mlContext.Transforms.Text
                    .FeaturizeText("Features2", nameof(TextPair.Text2)));

            var transformer = pipeline.Fit(dataView);
            var transformed = transformer.Transform(dataView);

            var features1 = transformed.GetColumn<float[]>("Features1").First();
            var features2 = transformed.GetColumn<float[]>("Features2").First();

            var maxLength = Math.Max(features1.Length, features2.Length);
            if (maxLength > 0 && Math.Abs(features1.Length - features2.Length) > maxLength * 0.1)
                _logger.LogWarning("Vector length mismatch: CV={Len1} vacancy={Len2} ({Diff:F1}% apart)",
                    features1.Length, features2.Length,
                    Math.Abs(features1.Length - features2.Length) * 100.0 / maxLength);

            var score = ComputeCosineSimilarity(features1, features2);
            return Math.Clamp(score, 0.0f, 1.0f);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing score for CV or vacancy text");
            return 0.0f;
        }
    }

    private static float ComputeCosineSimilarity(float[] vector1, float[] vector2)
    {
        var length = Math.Min(vector1.Length, vector2.Length);
        var dotProduct = 0.0f;
        var norm1 = 0.0f;
        var norm2 = 0.0f;

        for (var i = 0; i < length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            norm1 += vector1[i] * vector1[i];
            norm2 += vector2[i] * vector2[i];
        }

        var denominator = (float)(Math.Sqrt(norm1) * Math.Sqrt(norm2));
        return denominator == 0 ? 0.0f : dotProduct / denominator;
    }

    private class TextPair
    {
        public string Text1 { get; set; }
        public string Text2 { get; set; }
    }
}