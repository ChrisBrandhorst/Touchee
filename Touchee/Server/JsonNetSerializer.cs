using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Touchee.Server {

    public class ToucheeJsonSerializer : JsonSerializer {
        public ToucheeJsonSerializer() {
            this.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.DefaultValueHandling = DefaultValueHandling.Ignore;
            this.Formatting = Formatting.None;
        }
    }

}
