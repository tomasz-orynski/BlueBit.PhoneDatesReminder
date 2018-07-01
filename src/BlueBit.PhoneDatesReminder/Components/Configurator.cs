using DefensiveProgrammingFramework;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public class Configurator<TOut> : IComponent<string, TOut>
        where TOut : new()
    {
        public async Task<TOut> WorkAsync(string input)
        {
            input.CannotBeEmpty();
            input.DoesFileExist();
            return JsonConvert.DeserializeObject<TOut>(await File.ReadAllTextAsync(input));
        }
    }
}