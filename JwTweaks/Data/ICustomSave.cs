using System;

namespace JwTweaks.Data;

public interface ICustomSave
{
    public String BlockName { get; }

    public string SaveData();
    public void LoadData(string data);

}