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
            string Content {get;}
            string Title {get;}
        }
    }

    public class Sender<TIn> : 
        ComponentBase<TIn, Void>
        where TIn: class, Sender.InputData
    {
        override protected void OnWork(TIn input, Void output)
        {
            Debug.Assert(input.SenderCfg != null);

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(input.SenderCfg.User, input.SenderCfg.Email));
            emailMessage.To.Add(new MailboxAddress(input.SenderCfg.User, input.SenderCfg.Email));
            emailMessage.Subject = input.Title;
            emailMessage.Body = new TextPart("plain") { Text = input.Content };

            using (var client = new SmtpClient())
            {
                client.Connect(input.SenderCfg.Host, input.SenderCfg.Port);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(input.SenderCfg.User, input.SenderCfg.Password);
                client.Send(emailMessage);
                client.Disconnect(true);
            }
        }
    }
}
