using vki_schedule_telegram.Models;

namespace vki_schedule_telegram.Services;
public class ParserService
{
    private readonly ILogger<ParserService> _logger;
    public const string Schedule = "schedule";

    public ParserService(ILogger<ParserService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Parsed> GetSchedule()
    {
        return new Parsed()
        {
            Name = Schedule, 
            Data = await GetPDFs("https://ci.nsu.ru/education/schedule/")
        };
    }

    private async Task<List<Pdf>> GetPDFs(string _url)
    {
        var doc = await GetDocAsync(_url);
        var list = new List<Pdf>();
        if (doc == null)
        {
            _logger.LogError($"Документ is null URl: {_url}");
            return list;
        }
        foreach (var node in doc.DocumentNode.SelectNodes(".//div[@class='file-div']"))
        {
            var name = node.SelectSingleNode(".//div[@class='file-name']").InnerText.Trim();
            var url = "https://ci.nsu.ru" + node.SelectSingleNode(".//a").GetAttributeValue("href", "");
            list.Add(new Pdf() { Name = name, Url = url});
        }
        return list;
    }
    private Task<string>? GetHtmlAsync(string url)
    {
        var html = string.Empty;
        using (var hdl = new HttpClientHandler { 
                   AllowAutoRedirect = false, 
                   AutomaticDecompression = System.Net.DecompressionMethods.All 
               })
        {
            using (var client = new HttpClient(hdl))
            {
                using (var resp = client.GetAsync(url).Result)
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        html = resp.Content.ReadAsStringAsync().Result;
                        if (string.IsNullOrEmpty(html))
                        {
                            _logger.LogError("{Url}\\nHtml is Null or Empty", url);
                        }
                    }
                }
            }
        }
        return Task.FromResult(html);
    }
    private async Task<HtmlAgilityPack.HtmlDocument?> GetDocAsync(string url)
    {
        HtmlAgilityPack.HtmlDocument doc = new();
        var html = await GetHtmlAsync(url)!;
        if (string.IsNullOrEmpty(html)) return null;
        doc.LoadHtml(html);
        return doc;
    }
}