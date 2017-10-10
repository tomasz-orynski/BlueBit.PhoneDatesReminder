using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

            StringContent prepareMsg(string cookie, string token)
            {

                var dt = $"{input.Date.ToString("yyyy-MM-dd")}";
                var msg = $"Dnia [{dt}] uplywa termin aktywacji lub zaplaty za telefon!";
                var numbers = string.Join("", input.SenderSmsCfg.Phones.Split(";").Select(_ => $"<Phone>{_}</Phone>"));
                var content = new StringContent(
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><request><Index>-1</Index><Phones>{numbers}</Phones><Sca></Sca><Content>{msg}</Content><Length>{msg.Length}</Length><Reserved>1</Reserved><Date>{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}</Date></request>",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");
                content.Headers.Add("__RequestVerificationToken", token);
                return content;
            };

            using (var client = new HttpClient())
            {
                var tokenResult = await client.GetAsync($"{input.SenderSmsCfg.Url}/api/webserver/SesTokInfo");
                var tokenContent = await tokenResult.Content.ReadAsStringAsync();
                //<SesInfo> => Cookie
                //<TokInfo> => _RequestVerificationToken
                var sendSmsResult = await client.PostAsync($"{input.SenderSmsCfg.Url}/api/sms/send-sms", prepareMsg("cookie", "token"));
            }
        }
    }
}