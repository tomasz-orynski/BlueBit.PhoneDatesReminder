using System;
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
        public interface InputData
        {
            Cfg.SenderSmsCfg SenderSmsCfg { get; }
            DateTime Date { get; }
        }
    }

    public class SenderSms<T> :
        ComponentBase<T>
        where T : class, SenderSms.InputData
    {
        override protected async Task OnWorkAsync(T input)
        {
            Debug.Assert(input.SenderSmsCfg != null);

            StringContent prepareMsg((string Cookie, string Token) session)
            {
                var dt = $"{input.Date.ToString("yyyy-MM-dd")}";
                var msg = $"Dnia [{dt}] uplywa termin aktywacji lub zaplaty za telefon!";
                var numbers = string.Join("", input.SenderSmsCfg.Phones.Split(";").Select(_ => $"<Phone>{_}</Phone>"));
                var content = new StringContent(
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><request><Index>-1</Index><Phones>{numbers}</Phones><Sca></Sca><Content>{msg}</Content><Length>{msg.Length}</Length><Reserved>1</Reserved><Date>{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}</Date></request>",
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

            using (var client = new HttpClient())
            using(var tokenResult = await client.GetAsync($"{input.SenderSmsCfg.Url}/api/webserver/SesTokInfo"))
            {
                var session = GetSession(await tokenResult.Content.ReadAsStringAsync());
                using (var sendSmsResult = await client.PostAsync($"{input.SenderSmsCfg.Url}/api/sms/send-sms", prepareMsg(session)))
                {
                }
            }
        }
    }
}