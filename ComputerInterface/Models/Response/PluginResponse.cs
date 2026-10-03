using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComputerInterface.Models.Response;

[Serializable]
internal class PluginResponse {
    [JsonProperty("plugin_id")]
    public string PluginId { get; set; }

    [JsonProperty("plugin_guid")]
    public string PluginGuid { get; set; }

    [JsonProperty("plugin_name")]
    public string PluginName { get; set; }

    [JsonProperty("plugin_version")]
    public string PluginVersion { get; set; }

    [JsonProperty("extra_data")]
    public JObject ExtraData { get; set; }
}