using System.Net;
using Waybon.Application.Emails.Dtos;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Emails.Templates;

public static class VerificationCodeEmail
{
    // ===================================
    // Constants
    // ===================================

    private const string Subject = "Your Waybon verification code";


    // ===================================
    // Create
    // ===================================

    public static EmailMessage Create(string to, string code)
    {
        var encodedCode = WebUtility.HtmlEncode(code);
        var minutes = (int)VerificationCode.CodeLifetime.TotalMinutes;

        return new EmailMessage
        (
            To: to,
            Subject: Subject,
            HtmlContent: BuildHtml(encodedCode, minutes),
            TextContent: BuildText(code, minutes)
        );
    }


    // ===================================
    // Content
    // ===================================

    private static string BuildHtml(string code, int minutes)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{Subject}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f5f6f0;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f6f0;padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:480px;font-family:Arial,Helvetica,sans-serif;">
                      <tr>
                        <td style="background-color:#e4ecd0;border-radius:16px 16px 0 0;padding:24px 32px;text-align:center;">
                          <div style="font-size:20px;line-height:28px;font-weight:bold;color:#3d4a23;">Waybon</div>
                        </td>
                      </tr>
                      <tr>
                        <td style="background-color:#ffffff;padding:32px;text-align:center;">
                          <h1 style="margin:0;font-size:20px;line-height:28px;font-weight:bold;color:#2b331a;">Verify your email</h1>
                          <p style="margin:16px 0 0 0;font-size:15px;line-height:24px;color:#5b6150;">Enter this code in the app to finish setting up your account.</p>
                          <div style="margin:24px 0 0 0;">
                            <span style="display:inline-block;padding:16px 24px;background-color:#f3f6ea;border:1px solid #d3dcb8;border-radius:12px;font-family:'Courier New',Courier,monospace;font-size:32px;line-height:40px;font-weight:bold;letter-spacing:8px;color:#3d4a23;">{code}</span>
                          </div>
                          <p style="margin:24px 0 0 0;font-size:13px;line-height:20px;color:#7a8069;">This code expires in {minutes} minutes.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="background-color:#f3f6ea;border-radius:0 0 16px 16px;padding:24px 32px;text-align:center;">
                          <p style="margin:0;font-size:12px;line-height:20px;color:#7a8069;">If you didn't create a Waybon account, you can ignore this email.</p>
                          <p style="margin:8px 0 0 0;font-size:12px;line-height:20px;color:#7a8069;">This is an automated message, please do not reply.</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildText(string code, int minutes)
    {
        return $"""
            Waybon

            Verify your email

            Enter this code in the app to finish setting up your account:

            {code}

            This code expires in {minutes} minutes.

            If you didn't create a Waybon account, you can ignore this email.
            This is an automated message, please do not reply.
            """;
    }
}