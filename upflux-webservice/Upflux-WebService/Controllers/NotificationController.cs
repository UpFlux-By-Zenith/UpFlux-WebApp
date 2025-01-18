using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("create-group")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupRequest request)
        {
            await _notificationService.CreateGroupAsync(request.GroupId);
            return Ok(new { Message = $"Group '{request.GroupId}' created successfully." });
        }

        [HttpPost("add-uri")]
        public async Task<IActionResult> AddUri([FromBody] UriRequest request)
        {
            await _notificationService.AddUriToGroupAsync(request.GroupId, request.Uri);
            return Ok(new { Message = $"URI '{request.Uri}' added to group '{request.GroupId}'." });
        }

        [HttpPost("remove-uri")]
        public async Task<IActionResult> RemoveUri([FromBody] UriRequest request)
        {
            await _notificationService.RemoveUriFromGroupAsync(request.GroupId, request.Uri);
            return Ok(new { Message = $"URI '{request.Uri}' removed from group '{request.GroupId}'." });
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] UriMessageRequest request)
        {
            await _notificationService.SendMessageToUriAsync(request.GroupId, request.Uri, request.Message);
            return Ok(new { Message = "Message sent successfully." });
        }
    }

    public class GroupRequest
    {
        public string GroupId { get; set; }
    }

    public class UriRequest
    {
        public string GroupId { get; set; }
        public string Uri { get; set; }
    }

    public class UriMessageRequest
    {
        public string GroupId { get; set; }
        public string Uri { get; set; }
        public string Message { get; set; }
    }

}
