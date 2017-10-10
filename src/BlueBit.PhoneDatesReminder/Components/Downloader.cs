using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Downloader
    {
        public interface InputData
        {
            Cfg.DownloaderCfg DownloaderCfg { get; }
        }

        public interface OutputData
        {
            string Content { set; }
        }
    }

    public class Downloader<TIn, TOut> :
        ComponentBase<TIn, TOut>
        where TIn : class, Downloader.InputData
        where TOut : Downloader.OutputData, new()
    {
        override protected async Task OnWorkAsync(TIn input, TOut output)
        {
            Debug.Assert(input.DownloaderCfg != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(input.DownloaderCfg.Url));
            var uri = new Uri(input.DownloaderCfg.Url);
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (var client = new HttpClient())
            using (var response = await client.SendAsync(request))
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var contentReader = new StreamReader(contentStream))
                output.Content = await contentReader.ReadToEndAsync();
        }
    }
}