using UglyToad.PdfPig;

namespace SIRU.Infraestructure.Ranking.Helpers;

public class PdfTextExtractor
{
    public async Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var text = string.Join(" ", document.GetPages().SelectMany(p => p.GetWords()));
            return text;
        }
        catch
        {
            return string.Empty;
        }
    }
}