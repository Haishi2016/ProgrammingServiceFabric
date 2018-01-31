using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.ArchiGraph
{
    public class TypeTree
    {
        public static TypeTree LoadFromJson(string jsonFile)
        {
            if (string.IsNullOrEmpty(jsonFile) || !File.Exists(jsonFile))
                throw new FileNotFoundException();
            return JsonConvert.DeserializeObject<TypeTree>(File.ReadAllText(jsonFile));
        }
        [JsonProperty("types")]
        public IList<EntityType> Types { get; set; }
        
    }
}
