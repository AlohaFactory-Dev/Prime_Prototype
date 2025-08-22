using System;
using Newtonsoft.Json;
using UniRx;

public class ReactiveBoolConverter : JsonConverter<ReactiveProperty<bool>>
{
    public override void WriteJson(JsonWriter writer, ReactiveProperty<bool> value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Value ? 1 : 0);
    }

    public override ReactiveProperty<bool> ReadJson(JsonReader reader, Type objectType, ReactiveProperty<bool> existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var value = (long)(reader.Value) == 1;
        if (hasExistingValue)
        {
            existingValue.Value = value;
            return existingValue;
        }
        else
        {
            return new ReactiveProperty<bool>(value);
        }
    }
}

public class ReactiveIntConverter : JsonConverter<ReactiveProperty<int>>
{
    public override void WriteJson(JsonWriter writer, ReactiveProperty<int> value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Value);
    }

    public override ReactiveProperty<int> ReadJson(JsonReader reader, Type objectType, ReactiveProperty<int> existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var value = (long)(reader.Value);
        if (hasExistingValue)
        {
            existingValue.Value = (int)value;
            return existingValue;
        }
        else
        {
            return new ReactiveProperty<int>((int)value);
        }
    }
}
