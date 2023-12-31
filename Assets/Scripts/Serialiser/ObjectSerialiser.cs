//*****************************************************
//* Developed by Lochlan Kennedy - Outback Games for  *
//* the purpose of showing the ability to serialise   *
//* data for game development using Newtonsoft.Json   *
//*****************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using Newtonsoft.Json;


public static class ObjectSerialiser
{
    //Environment.UserName returns "SYSTEM" on windows in some cases. be aware.
    private static string _filePath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}Users\{Environment.UserName}\AppData\LocalLow\{Application.companyName}\Save.dat";
    private static string _filePathBackup = $@"{Path.GetPathRoot(Environment.SystemDirectory)}Users\{Environment.UserName}\AppData\LocalLow\{Application.companyName}\Save.dat.bak";
    private static Dictionary<string, Tuple<Type, object>> _itemsToSave = new Dictionary<string, Tuple<Type, object>>();
    private static bool _hasInitialised = false;
    
    public static void Initialise()
    {
        if (_hasInitialised) return;
        DeserialiseSaveFile();
        _hasInitialised = true;
    }
    
    
    public static void Save<T>(string Key, T Value)
    {
        Tuple<Type, object> tupleToStore = new Tuple<Type, object>(Value.GetType(), Value);
        
        if (_itemsToSave.ContainsKey(Key))
        {
            _itemsToSave[Key] = tupleToStore;
        }
        else
        {
            _itemsToSave.Add(Key, tupleToStore);
        }
        SerialiseItemsToSave(); //for later, batch this call for the next improvement cycle instead of serialising every time we want to add to the dictionary.
    }

    /// <summary>
    /// Returns the stored object in the items to save dictionary that matches the key.
    /// Casting of the returned object may be required.
    /// </summary>
    /// <param name="Key">Key of the object.</param>
    /// <param name="defaultValue">The Default Value and Type Expected.</param>
    /// <typeparam name="T">The Type You Expect.</typeparam>
    public static T Load<T>(string Key, T defaultValue)
    {
        Tuple<Type, object> returnedTuple;
        _itemsToSave.TryGetValue(Key, out returnedTuple);
        if (returnedTuple != null)
        {
            try
            {
                T convertedValue = (T)Convert.ChangeType(returnedTuple.Item2, returnedTuple.Item1,
                    CultureInfo.InvariantCulture);
                return convertedValue;
            }
            catch (Exception e)
            {
                Debug.Log($"{e}, - Now Returning The Default Value.");
                //We're getting an invalid cast exception. Json.Net must be doing something to class objects when serialising.
                //Todo - figure out how to handle more complex types that don't have IConvertible as it is inconvenient 
                return defaultValue;
            }

        }
        else
        {
            return defaultValue;
        }
        
    }

    //We'll add encryption later.
    static void SerialiseItemsToSave()
    {
        //Json utility doesn't serialise the dictionary, JsonConvert does. however, it has the casting problem to solve.
        //If we added a custom dictionary type that can then be serialised, it might be possible to use Json Utility and not have a problem.
        //string jsonString = JsonUtility.ToJson(_itemsToSave, true); //pretty print please.
        string jsonString = JsonConvert.SerializeObject(_itemsToSave, Formatting.Indented);
        if (System.IO.File.Exists(_filePath))
        {
            if (System.IO.File.Exists(_filePathBackup))
            {
                System.IO.File.Delete(_filePathBackup);
            }
            
            System.IO.File.Copy(_filePath,_filePathBackup); //make a backup.
            System.IO.File.Delete(_filePath);
        }

        if (!string.IsNullOrEmpty(jsonString))
        {
            File.WriteAllText(_filePath, jsonString);
        }
        
    }

    static void DeserialiseSaveFile()
    {
        if (!System.IO.File.Exists(_filePath))
        {
            Debug.Log("No Save File Could Be Found!");
            return;
        }

        string jsonString = File.ReadAllText(_filePath);
        
        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.Log("Save File Contains No Data");
            return;
        }

        //_itemsToSave = JsonUtility.FromJson<Dictionary<string, Tuple<Type, object>>>(jsonString);
        _itemsToSave = JsonConvert.DeserializeObject<Dictionary<string, Tuple<Type, object>>>(jsonString);
        foreach (KeyValuePair<string, Tuple<Type, object>> kvp in _itemsToSave)
        {
            Debug.Log($"Key: {kvp.Key}, Type:{kvp.Value.Item1}, Value: {kvp.Value.Item2}");
        }
    }
    
}
