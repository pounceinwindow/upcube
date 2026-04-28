using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using UpperCube.Application.Abstractions.Notifications;

namespace UpperCube.Infrastructure.Services.Email;

public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = configuration["Email:Host"] ?? "localhost";
        var port = configuration.GetValue("Email:Port", 1025);
        var useSsl = configuration.GetValue("Email:UseSsl", false);
        var fromAddress = configuration["Email:FromAddress"] ?? "noreply@uppercube.local";
        var fromName = configuration["Email:FromName"] ?? "UpperCube";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, useSsl, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipient}", to);
        }
    }
}