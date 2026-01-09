using System.Net;
using System.Net.Mail;

namespace NehaSurgicalAPI.Services;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otp);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otp)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Pass"];
            var smtpFrom = _configuration["Smtp:From"];
            var smtpFromName = _configuration["Smtp:FromName"] ?? "Neha Surgical";

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpFrom!, smtpFromName),
                Subject = "Your OTP for Neha Surgical Login",
                Body = GenerateOtpEmailTemplate(toName, otp),
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP email to {Email}", toEmail);
            return false;
        }
    }

    private string GenerateOtpEmailTemplate(string userName, string otp)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>OTP Verification</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td align='center' style='padding: 40px 0;'>
                <table role='presentation' style='width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                    <!-- Header with Logo -->
                    <tr>
                        <td style='padding: 30px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 8px 8px 0 0; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: bold;'>Neha Surgical</h1>
                            <p style='margin: 10px 0 0 0; color: #ffffff; font-size: 14px;'>Medical Equipment & Supplies</p>
                        </td>
                    </tr>
                    
                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            <h2 style='margin: 0 0 20px 0; color: #333333; font-size: 24px;'>Hello {userName},</h2>
                            <p style='margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 1.6;'>
                                You have requested to log in to your Neha Surgical account. Please use the following One-Time Password (OTP) to complete your authentication:
                            </p>
                            
                            <!-- OTP Box -->
                            <table role='presentation' style='width: 100%; border-collapse: collapse; margin: 30px 0;'>
                                <tr>
                                    <td align='center' style='padding: 20px; background-color: #f8f9fa; border-radius: 8px; border: 2px dashed #667eea;'>
                                        <p style='margin: 0 0 10px 0; color: #666666; font-size: 14px; text-transform: uppercase; letter-spacing: 1px;'>Your OTP Code</p>
                                        <p style='margin: 0; color: #667eea; font-size: 36px; font-weight: bold; letter-spacing: 8px;'>{otp}</p>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style='margin: 0 0 15px 0; color: #666666; font-size: 14px; line-height: 1.6;'>
                                <strong>Important:</strong>
                            </p>
                            <ul style='margin: 0 0 20px 0; padding-left: 20px; color: #666666; font-size: 14px; line-height: 1.8;'>
                                <li>This OTP is valid for <strong>10 minutes</strong></li>
                                <li>Do not share this code with anyone</li>
                                <li>If you didn't request this OTP, please ignore this email</li>
                            </ul>
                            
                            <div style='margin: 30px 0 0 0; padding: 20px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;'>
                                <p style='margin: 0; color: #856404; font-size: 13px; line-height: 1.6;'>
                                    <strong>Security Notice:</strong> For your account security, never share your OTP with anyone. Neha Surgical will never ask for your OTP via phone or email.
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 30px 40px; background-color: #f8f9fa; border-radius: 0 0 8px 8px; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0 0 10px 0; color: #666666; font-size: 13px; text-align: center;'>
                                Need help? Contact us at <a href='mailto:support@nehasurgical.com' style='color: #667eea; text-decoration: none;'>support@nehasurgical.com</a>
                            </p>
                            <p style='margin: 0; color: #999999; font-size: 12px; text-align: center;'>
                                &copy; {DateTime.Now.Year} Neha Surgical. All rights reserved.
                            </p>
                            <p style='margin: 10px 0 0 0; color: #999999; font-size: 11px; text-align: center;'>
                                This is an automated email. Please do not reply to this message.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
