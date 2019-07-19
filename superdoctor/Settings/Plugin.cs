using Newtonsoft.Json;
using System.ComponentModel;

namespace superdoctor.Settings
{
    class Plugin
    {
        [JsonProperty(PropertyName = "hostname")]
        public string HostName { get; set; }

        [DefaultValue(@"C:\Program Files\Supermicro\SuperDoctor5\sdc.bat")]
        [JsonProperty(PropertyName = "path", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Path { get; set; }
    }
}
