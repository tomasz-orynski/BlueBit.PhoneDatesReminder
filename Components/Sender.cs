using System;
using System.Diagnostics;
using MimeKit;
using MailKit.Net.Smtp;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Sender
    {
        public interface InputData
        {
            Cfg.SenderCfg SenderCfg {get;}
            DateTime Date {get;}
        }
    }

    public class Sender<T> : 
        ComponentBase<T>
        where T: class, Sender.InputData
    {
        override protected void OnWork(T input)
        {
            Debug.Assert(input.SenderCfg != null);

            var dt = $"{input.Date.ToString("yyyy-MM-dd")}";
            var content = $"Dnia [{dt}] upływa termin aktywacji/zapłaty za telefon!";
            var title = $"Przypomnienie o terminie - {dt}";

            Func<MimeMessage> prepareMsg = () => {
                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(input.SenderCfg.User, input.SenderCfg.Email));
                msg.To.Add(new MailboxAddress(input.SenderCfg.User, input.SenderCfg.Email));
                msg.Subject = title;
                msg.Body = new TextPart("plain") { Text = content };
                return msg;
            };

            using (var client = new SmtpClient())
            {
                client.Connect(input.SenderCfg.Host, input.SenderCfg.Port);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(input.SenderCfg.User, input.SenderCfg.Password);
                client.Send(prepareMsg());
                client.Disconnect(true);
            }
        }
    }
}
