using System.Text.Json;
using System.Text.Json.Nodes;

namespace deeplynx.helpers.json;

public static class JsonSerialization
{
    /// <summary>
    /// Deserialize input json into list of generic object type
    /// </summary>
    /// <param name="jsonArray">The input json to be serialized to an object</param>
    /// <returns>List of serialized objects parsed from json</returns>
    /// <note>Due to possible null reference return, returns an empty list of generic type on failure.</note>
    public static List<T> DeserializeJsonArray<T>(JsonArray jsonArray)
    {
        string jsonString = jsonArray.ToString();
        var result = JsonSerializer.Deserialize<List<T>>(jsonString);
        return result ?? new List<T>();
    }

    /// <summary>
    /// Serialize input list of generic object type into json. Reverse of above.
    /// </summary>
    /// <param name="list">The input list of generic type to be serialized to json</param>
    /// <returns>A json array of the serialized objects parsed from the list</returns>
    public static JsonArray SerializeToJsonArray<T>(List<T> list)
    {
        string jsonString = JsonSerializer.Serialize(list);
        var jsonArray = JsonDocument.Parse(jsonString).RootElement.EnumerateArray();
        JsonArray result = new JsonArray();

        foreach (var element in jsonArray)
        {
            result.Add(element.Clone());
        }

        return result;
    }
}