using BlueBit.PhoneDatesReminder.Commons.Extensions;
using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
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

        protected override IEnumerable<(IEnumerable<string> To, string Content, Func<Task> Action)> GetTasks(T input)
        {
            input.SenderSmsCfg.CannotBeNull();
            input.SenderSmsCfg.Phones.CannotBeEmpty();
            input.SenderSmsCfg.Urls.CannotBeEmpty();

            var phoneNumbers = input.SenderSmsCfg.Phones;
            StringContent prepareMsg((string Cookie, string Token) session, string content, IEnumerable<string> to)
            {
                var numbers = string.Join(string.Empty, to.Select(_ => $"<Phone>{_}</Phone>"));
                var msg = new StringContent(
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><request><Index>-1</Index><Phones>{numbers}</Phones><Sca></Sca><Content>{content}</Content><Length>{content.Length}</Length><Reserved>1</Reserved><Date>{Now.ToString("yyyy-MM-dd hh:mm:ss")}</Date></request>",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");
                msg.Headers.Add("Cookie", session.Cookie);
                msg.Headers.Add("__RequestVerificationToken", session.Token);
                return msg;
            };
            (string Cookie, string Token) GetSession(string content)
            {
                var root = XDocument.Parse(content).Root;
                var token = root.Element("TokInfo").Value;
                var cookie = root.Element("SesInfo").Value;
                return (cookie, token);
            }

            using (var urlsEnum = input.SenderSmsCfg.Urls.AsRandomAndCyclic().GetEnumerator())
            {
                var items = input.Items
                    .SelectMany(item => {
                        var msg = GetMsg(item);
                        return phoneNumbers.Append(item.PhoneNumber)
                            .Distinct()
                            .Select(phoneNumber => new { PhoneNumber = phoneNumber, Msg = msg });
                    })
                    .GroupBy(
                        item => item.PhoneNumber,
                        item => item.Msg)
                    .Select(grp => new
                    {
                        To = grp.Key,
                        Content = string.Join(Environment.NewLine, grp.OrderBy(msg => msg)),
                    })
                    .GroupBy(
                        item => item.Content,
                        item => item.To);

                foreach (var item in items)
                {
                    urlsEnum.MoveNext().CannotBeEqualTo(false);
                    var url = urlsEnum.Current;
                    yield return (
                        item,
                        item.Key,
                        async () =>
                        {
                            using (var client = new HttpClient())
                            using (var tokenResult = await client.GetAsync($"{url}/api/webserver/SesTokInfo"))
                            {
                                var session = GetSession(await tokenResult.Content.ReadAsStringAsync());
                                using (var sendSmsResult = await client.PostAsync($"{url}/api/sms/send-sms", prepareMsg(session, item.Key, item)))
                                {
                                }
                            }
                        }
                    );
                }
            }
        }
    }
}