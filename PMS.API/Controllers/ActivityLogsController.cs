using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;

[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _service;

    public ActivityLogsController(IActivityLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] string entityName, [FromQuery] string entityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(entityId))
        {
            return BadRequest("Entity name and ID are required.");
        }

        var history = await _service.GetHistoryAsync(entityName, entityId, cancellationToken);

        return Ok(history);
    }
}