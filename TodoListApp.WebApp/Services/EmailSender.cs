using Microsoft.AspNetCore.Identity.UI.Services;

namespace TodoListApp.WebApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            this._logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            this._logger.LogInformation($"Email: {email}, Subject: {subject}, Message: {htmlMessage}");

            // For development, just log the email instead of actually sending it
            // In production, you would implement actual email sending here
            return Task.CompletedTask;
        }
    }
}
