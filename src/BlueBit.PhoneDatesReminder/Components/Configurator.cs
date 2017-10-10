using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public class Configurator<TOut> : IComponent<string, TOut>
        where TOut : new()
    {
        public async Task<TOut> WorkAsync(string input)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(input));
            Debug.Assert(File.Exists(input));
            return JsonConvert.DeserializeObject<TOut>(await File.ReadAllTextAsync(input));
        }
    }
}