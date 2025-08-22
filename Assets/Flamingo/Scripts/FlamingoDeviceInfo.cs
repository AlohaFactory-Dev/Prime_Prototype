using System;
using System.Text;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class JsonPropertyAttribute : Attribute
{
    public JsonPropertyAttribute()
    {
    }

    public string Name;
}

public class FlamingoDeviceInfo
{
    [JsonProperty(Name = "deviceId")] public string DeviceId { get; set; }

    [JsonProperty(Name = "deviceModel")] public string DeviceModel { get; set; }

    [JsonProperty(Name = "deviceName")] public string DeviceName { get; set; }

    [JsonProperty(Name = "deviceType")] public string DeviceType { get; set; }

    [JsonProperty(Name = "deviceIdentifier")]
    public string DeviceIdentifier { get; set; }

    [JsonProperty(Name = "locale")] public string Locale { get; set; }

    [JsonProperty(Name = "timezoneOffset")]
    public string TimezoneOffset { get; set; }

    [JsonProperty(Name = "operatingSystem")]
    public string VarOperatingSystem { get; set; }

    [JsonProperty(Name = "operatingSystemFamily")]
    public string OperatingSystemFamily { get; set; }

    [JsonProperty(Name = "screenResolution")]
    public string ScreenResolution { get; set; }

    [JsonProperty(Name = "screenDPI")] public string ScreenDPI { get; set; }

    public string ToJson()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("{");
        foreach (var prop in this.GetType().GetProperties())
        {
            var attr = (JsonPropertyAttribute)Attribute.GetCustomAttribute(prop, typeof(JsonPropertyAttribute));
            sb.Append(attr != null ? $"\"{attr.Name}\": " : $"\"{prop.Name}\": ");
            sb.Append($"\"{prop.GetValue(this)}\", ");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append("}");

        return sb.ToString();
    }
}