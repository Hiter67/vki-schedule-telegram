namespace vki_schedule_telegram.Services;

public class ParserService
{
    private readonly ILogger<ParserService> _logger;
    public ParserService(ILogger<ParserService> logger)
    {
        _logger = logger;
    }
    public async Task<List<List<string>>> GetPDFs(string url)
    {
        var list = new List<List<string>>();
        var doc = await GetDoc(url);
        if (doc == null) 
        {
            _logger.LogWarning("Документ is null URl: {Url}", url);
            return list;
        }
        foreach (HtmlAgilityPack.HtmlNode i in doc.DocumentNode.SelectNodes(".//div[@class='file-div']"))
        {
            var name = i.SelectSingleNode(".//div[@class='file-name']").InnerText.Trim();
            var link = "https://ci.nsu.ru" + i.SelectSingleNode(".//a").GetAttributeValue("href", "");
            list.Add(new List<string> { name, link });
        }
        return list;
    }
    public async Task<List<List<string>>> GetSchedule(string url)
    {
        var list = new List<List<string>>();
        var doc = await GetDoc(url);
        if (doc == null)
        {
            _logger.LogWarning("Документ is null URl: {Url}", url);
            return list;
        }
        var ctr = 0;
        foreach (var i in doc.DocumentNode.SelectSingleNode(".//table[@class='table']").SelectNodes(".//tr"))
        {
            list.Add(new List<string>());
            foreach (var j in i.SelectNodes(".//p"))
            {
                list[ctr].Add(j.InnerText.Trim());
            }
            ctr++;
        }
        list = list
            .SelectMany(inner => inner.Select((item, index) => new { item, index }))
            .GroupBy(i => i.index, i => i.item)
            .Select(g => g.ToList())
            .ToList();
        return list;
    }
    private Task<string>? GetHtml(string url)
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
                            _logger.LogWarning("{Url}:\\nHtml is Null or Empty", url);
                        }
                    }
                }
            }
        }
        return Task.FromResult(html);
    }
    private async Task<HtmlAgilityPack.HtmlDocument?> GetDoc(string url)
    {
        HtmlAgilityPack.HtmlDocument doc = new();
        var html = await GetHtml(url)!;
        if (string.IsNullOrEmpty(html)) return null;
        doc.LoadHtml(html);
        return doc;
    }
}