using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

using WebApi.Services;
using WebApi.Models;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace WebApi.Controllers
{
    [Route("api/auth/hubspot")]
    [ApiController]
    public class AuthHubSpotController : ControllerBase
    {
        private readonly CrmService _crmService;
        private readonly HubSpotAuthService _hubSpotAuthService;

        public AuthHubSpotController(CrmService crmService, IConfiguration configuration)
        {
            _crmService = crmService;
            _hubSpotAuthService = new HubSpotAuthService(configuration);
        }

        [HttpGet("authorize")]
        public IActionResult Authorize()
        {
            var authorizationUrl = _hubSpotAuthService.GetAuthorizationUrl();
            return Redirect(authorizationUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, int crmId)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing");
            }

            try
            {
                var accessToken = await _hubSpotAuthService.GetAccessTokenAsync(code);
                var crm = _crmService.GetCrm(crmId);

                if (crm == null)
                {
                    crm = new Crm { Id = crmId, Name = "CRM " + crmId };
                }

                crm.AccessToken = accessToken;

                _crmService.SaveCrm(crm);

                return Ok(new { AccessToken = accessToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{crmId}/contacts")]
        public async Task<IActionResult> GetContacts(int crmId)
        {
            try
            {
                var crm = _crmService.GetCrm(crmId);
                
                if (crm == null)
                {
                    return NotFound("CRM not found");
                }

                var contacts = await _hubSpotAuthService.GetContactsAsync(crm.AccessToken);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
