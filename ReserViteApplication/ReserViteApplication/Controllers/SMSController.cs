using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using ReserViteApplication.Services;

namespace ReserViteApplication.Controllers
{
    public class SMSController : Controller
    {
        private readonly ISMSSenderService _smsSender;

        public SMSController(ISMSSenderService smsSender)
        {
            _smsSender = smsSender;
        }

        [HttpPost]
        [Route("send-sms")]
        public async Task<IActionResult> SendSms(string to, string message)
        {
            try
            {
                await _smsSender.SendSmsAsync(to, message);
                return Ok("SMS envoyer avec succe!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Echec de l'envoie du SMS: {ex.Message}");
            }
        }
    }

}
