using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.CommandLineUtils;
using MimeKit;
using MailKit.Net.Smtp;

namespace BlueBit.PhoneDatesReminder
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var cmdApp = new CommandLineApplication();
            cmdApp.OnExecute(async () => await OnExecuteAsync(
                "https://docs.google.com/spreadsheets/d/1YElTTn9M5IvX6kVK3_fVK3p1sVYTrPvgJUrgseUgaHQ/pub?output=csv",
                "orynscy@gmail.com"));
            cmdApp.Execute(args);
        }

        private static async Task<int> OnExecuteAsync(string url, string address)
        {
            var uri = new Uri(url);
            var contentReader = await GetContentFromUrlAsync(uri);
            return 0;
        }

        private static async Task<string> GetContentFromUrlAsync(Uri requestUri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (var contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync())
            using (var contentReader = new StreamReader(contentStream))
            {
                return await contentReader.ReadToEndAsync();
            }
        }

        private static async Task SendEmailAsync(string address)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Joe Bloggs", "jbloggs@example.com"));
            emailMessage.To.Add(new MailboxAddress("", address));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = "some.domain.com";                
                await client.ConnectAsync("smtp.relay.uri", 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
