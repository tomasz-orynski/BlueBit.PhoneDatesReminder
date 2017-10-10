using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class SenderSmtp
    {
        public interface InputData
        {
            Cfg.SenderSmtpCfg SenderSmtpCfg { get; }
            DateTime Date { get; }
        }
    }

    public class SenderSmtp<T> :
        ComponentBase<T>
        where T : class, SenderSmtp.InputData
    {
        override protected async Task OnWorkAsync(T input)
        {
            Debug.Assert(input.SenderSmtpCfg != null);

            MimeMessage prepareMsg()
            {
                var dt = $"{input.Date.ToString("yyyy-MM-dd")}";
                var content = $"Dnia [{dt}] upływa termin aktywacji/zapłaty za telefon!";
                var title = $"Przypomnienie o terminie";

                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(input.SenderSmtpCfg.User, input.SenderSmtpCfg.Email));
                msg.To.Add(new MailboxAddress(input.SenderSmtpCfg.User, input.SenderSmtpCfg.Email));
                msg.Subject = title;
                msg.Body = new TextPart("plain") { Text = content };
                return msg;
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(input.SenderSmtpCfg.Host, input.SenderSmtpCfg.Port);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(input.SenderSmtpCfg.User, input.SenderSmtpCfg.Password);
                await client.SendAsync(prepareMsg());
                await client.DisconnectAsync(true);
            }
        }
    }
}