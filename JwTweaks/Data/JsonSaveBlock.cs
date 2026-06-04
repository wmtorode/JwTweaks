using Newtonsoft.Json;

namespace JwTweaks.Data;

public class JsonSaveBlock<T>: ICustomSave where T : class
{
    public string BlockName => typeof(T).Name;
    
    public T Data { get; set; }
    
    public string SaveData()
    {
        return JsonConvert.SerializeObject(Data, Formatting.None);
    }

    public void LoadData(string data)
    {
        JsonConvert.PopulateObject(data, Data);
    }
}