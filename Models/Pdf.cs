using System.ComponentModel.DataAnnotations;

namespace vki_schedule_telegram.Models;

public class Pdf
{
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
}