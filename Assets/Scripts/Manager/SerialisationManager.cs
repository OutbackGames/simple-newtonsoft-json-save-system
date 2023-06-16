using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[System.Serializable, JsonObject(MemberSerialization.OptIn)]
public class SimpleData
{
    [SerializeField, JsonProperty] public int age;
    [SerializeField, JsonProperty] public string name;
    [SerializeField, JsonProperty] public string birthdate;
    
    public SimpleData(int Age, string Name, string Birthdate)
    {
        age = Age;
        name = Name;
        birthdate = Birthdate;
    }

}

public class SerialisationManager : MonoBehaviour
{
    public string levelKey = "Level";
    public string weightKey = "Weight";
    public string simpleDataKey = "SimpleData";
    
    [SerializeField] private int _level = 100;
    [SerializeField] private float _weight = 505.2f;
    [SerializeField] private SimpleData _simpleData = new SimpleData(10,"Shane", "10/11/2023");
    
    void Start()
    {
        ObjectSerialiser.Initialise(); // initialise the Serialiser.
        LoadData();
    }

    void LoadData()
    {
        //problems to solve - figure out dynamic way of returning the data using generics saving api users from having to cast
        //or convert based on things such as Int64
        //currently if we box it up using generics, then the value will be serialised as Type Int64 on x86_64 systems or Int32 on 32bit systems and not as "int")
        //floats are being serialised as doubles.
        //all of this was not an issue when serialising with XML. - nice to know.
        //Finally, figure out why the data in the struct isn't serialising, even though the fields are exposed for serialisation.

        _level = ObjectSerialiser.Load(levelKey, _level);
        _weight = ObjectSerialiser.Load(weightKey, _weight);
        _simpleData = ObjectSerialiser.Load<SimpleData>(simpleDataKey, _simpleData);
    }

    void SaveData()
    {
        ObjectSerialiser.Save<int>(levelKey, _level);
        ObjectSerialiser.Save<float>(weightKey,_weight);
        ObjectSerialiser.Save<SimpleData>(simpleDataKey,_simpleData);
    }

    private void OnDisable()
    {
        SaveData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
}
