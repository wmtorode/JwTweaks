using Newtonsoft.Json;

namespace JwTweaks.Data;

public class JsonSaveBlock<T>: ICustomSave where T : class
{
    public string BlockName => typeof(T).Name;
    
    public T Data { get; set; }
    
    public string SaveData()
    {
        JTCore.modLog.Debug?.Write($"Writing data block for: {typeof(T)} with data: \n{Data?.ToString()}");
        return JsonConvert.SerializeObject(Data, Formatting.None);
    }

    public void LoadData(string data)
    {
        JTCore.modLog.Debug?.Write($"Data block for type: {typeof(T)} is: \n'{data}'");
        if (string.IsNullOrEmpty(data) || string.Equals(data, "null", System.StringComparison.InvariantCultureIgnoreCase))
        {
            JTCore.modLog.Warn?.Write($"Data block is null or empty for type: {typeof(T)}, cannot load its data!");
            return;
        }

        var settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
        JsonConvert.PopulateObject(data, Data, settings);
    }
}