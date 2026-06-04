using System;
using System.Collections.Generic;
using IRBTModUtils.Logging;

namespace CustomBlockTest.Data;

public class TestData
{
    public EmbeddedData EmbeddedData = new EmbeddedData();
    public TestEnum TestEnum = TestEnum.Test1;
    public List<String> ListTest = new List<String>();
    public Dictionary<String, String> TestDict = new Dictionary<String, String>();


    public void RandomizeData()
    {
        
        Random random = new Random();
        
        EmbeddedData.TestBool = !EmbeddedData.TestBool;
        EmbeddedData.TestInt = random.Next(0, 100);
        EmbeddedData.TestFloat = random.Next(0, 1000) / 100f;
        EmbeddedData.TestString = Guid.NewGuid().ToString();
        EmbeddedData.TestEnum = (TestEnum) random.Next(0, 3);
        TestEnum = (TestEnum) random.Next(0, 3);
        
        var listLength = random.Next(0, 15);
        ListTest.Clear();
        TestDict.Clear();
        for (int i = 0; i < listLength; i++)
        {
            ListTest.Add(Guid.NewGuid().ToString());
            TestDict.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }
    }

    public void Log(DeferringLogger logger)
    {
        logger.Info?.Write("EmbeddedData: " + EmbeddedData.TestBool + " " + EmbeddedData.TestInt + " " + EmbeddedData.TestFloat + " " + EmbeddedData.TestString + " " + EmbeddedData.TestEnum);
        logger.Info?.Write("TestEnum: " + TestEnum);
        logger.Info?.Write("ListTest Len: " + ListTest.Count);
        foreach (var item in ListTest)
        {
            logger.Info?.Write("ListTest: " + item);
        }
        logger.Info?.Write("TestDict Len: " + TestDict.Count);
        foreach (var item in TestDict)
        {
            logger.Info?.Write("TestDict: " + item.Key + " " + item.Value);
        }
    }
    
}