using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace BlueBit.PhoneDatesReminder.Components
{
    public class Configurator<TOut> : IComponent<string, TOut>
        where TOut: new()
    {
        public TOut Work(string input)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(input));
            Debug.Assert(File.Exists(input));
            return JsonConvert.DeserializeObject<TOut>(File.ReadAllText(input));
        }
    }
}
