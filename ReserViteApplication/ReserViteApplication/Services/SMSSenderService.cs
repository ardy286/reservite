using Microsoft.Extensions.Options;
using ReserViteApplication.Services;
using ReserViteApplication.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class SMSSenderService : ISMSSenderService
{
    private readonly TwilioSettings _twilioSettings;

    public SMSSenderService(IOptions<TwilioSettings> twilioSettings)
    {
        _twilioSettings = twilioSettings.Value;
    }

    public async Task SendSmsAsync(string number, string message)
    {
        TwilioClient.Init(_twilioSettings.AccountSId, _twilioSettings.AuthToken);

        var messageResource = await MessageResource.CreateAsync(
            to: number,
            from: _twilioSettings.FromPhoneNumber,
            body: message
        );

        Console.WriteLine($"Message envoyer avec SID: {messageResource.Sid}");
    }
}
