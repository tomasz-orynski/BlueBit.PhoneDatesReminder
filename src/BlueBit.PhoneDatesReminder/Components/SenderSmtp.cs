using DefensiveProgrammingFramework;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class SenderSmtp
    {
        public interface InputData : Sender.InputData
        {
            Cfg.SenderSmtpCfg SenderSmtpCfg { get; }
        }
    }

    public class SenderSmtp<T> :
        Sender<T>
        where T : class, SenderSmtp.InputData
    {
        protected override string Name => "eMail";

        protected override IEnumerable<(string Code, Func<Task> Action)> GetTasks(T input)
        {
            input.SenderSmtpCfg.CannotBeNull();
            yield return (
                "ALL",
                async () => {
                    MimeMessage prepareMsg()
                    {
                        var title = $"Przypomnienie o terminie";
                        var content = string.Join(Environment.NewLine, input.Items.OrderBy(_ => (_.Date, _.PhoneNumber)).Select(GetMsg));

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
            );
        }
    }
}