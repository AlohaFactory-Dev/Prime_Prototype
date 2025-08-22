using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Aloha.Coconut
{
    public interface ISaveDataSaver
    {
        Task<JObject> LoadAsync();
        void Save(JObject jObject);
        void Delete();
    }
}
