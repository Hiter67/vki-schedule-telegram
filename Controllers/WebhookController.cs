using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using vki_schedule_telegram.Services;

namespace vki_schedule_telegram.Controllers;

public class WebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
    {
        await handleUpdateService.EchoAsync(update);
        return Ok();
    }
}
