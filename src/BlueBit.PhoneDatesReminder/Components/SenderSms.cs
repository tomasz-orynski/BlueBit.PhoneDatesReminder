using BlueBit.PhoneDatesReminder.Commons.Extensions;
using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class SenderSms
    {
        public interface InputData : Sender.InputData
        {
            Cfg.SenderSmsCfg SenderSmsCfg { get; }
        }
    }

    public class SenderSms<T> :
        Sender<T>
        where T : class, SenderSms.InputData
    {
        protected override string Name => "SMS";

        protected override IEnumerable<(string Code, Func<Task> Action)> GetTasks(T input)
        {
            input.SenderSmsCfg.CannotBeNull();
            input.SenderSmsCfg.Phones.CannotBeEmpty();
            input.SenderSmsCfg.Urls.CannotBeEmpty();

            var phoneNumbers = input.SenderSmsCfg.Phones;
            var random = new Random();
            string GetUrl() => input.SenderSmsCfg.Urls[random.Next(input.SenderSmsCfg.Urls.Count)];

            StringContent prepareMsg((string Cookie, string Token) session, (string PhoneNumber, DateTime Date, Reason Reason) item)
            {
                var msg = GetMsg(item);
                var numbers = string.Join("", phoneNumbers.Append(item.PhoneNumber).Distinct().Select(_ => $"<Phone>{_}</Phone>"));
                var content = new StringContent(
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><request><Index>-1</Index><Phones>{numbers}</Phones><Sca></Sca><Content>{msg}</Content><Length>{msg.Length}</Length><Reserved>1</Reserved><Date>{Now.ToString("yyyy-MM-dd hh:mm:ss")}</Date></request>",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");
                content.Headers.Add("Cookie", session.Cookie);
                content.Headers.Add("__RequestVerificationToken", session.Token);
                return content;
            };
            (string Cookie, string Token) GetSession(string content)
            {
                var root = XDocument.Parse(content).Root;
                var token = root.Element("TokInfo").Value;
                var cookie = root.Element("SesInfo").Value;
                return (cookie, token);
            }

            foreach (var item in input.Items.OrderBy(_ => _.Date))
            {
                var url = GetUrl();
                yield return (
                    $"[{item.PhoneNumber}][{item.Reason.AsDescription()}]",
                    async () =>
                    {
                        using (var client = new HttpClient())
                        using (var tokenResult = await client.GetAsync($"{url}/api/webserver/SesTokInfo"))
                        {
                            var session = GetSession(await tokenResult.Content.ReadAsStringAsync());
                            using (var sendSmsResult = await client.PostAsync($"{url}/api/sms/send-sms", prepareMsg(session, item)))
                            {
                            }
                        }
                    }
                );
            }
        }
    }
}