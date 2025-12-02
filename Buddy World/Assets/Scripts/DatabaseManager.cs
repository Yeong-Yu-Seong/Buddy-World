/* Author: Yeong Yu Seong
   Date: 11 November 2025
   Last Modified: 2 December 2025
   Description: Database manager script to handle pet data structure.
*/
using UnityEngine;
using Firebase.Database;
using System.Collections.Generic;
using Firebase.Extensions;
using TMPro;
using Firebase.Auth;
using System;

public class DatabaseManager : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class Pet
{
    public string petName;
    public string ownerID;
    public int level;
    public int hunger;
    public int happiness;
    public string lastFed;
    public string prefabType;

    // Parameterless constructor required/safer for JsonUtility deserialization
    public Pet() { }

    public Pet(string petName, string ownerID, int level, int hunger, int happiness, string lastFed, string prefabType)
    {
        this.petName = petName;
        this.ownerID = ownerID;
        this.level = level;
        this.hunger = hunger;
        this.happiness = happiness;
        this.lastFed = lastFed;
        this.prefabType = prefabType;
    }
}
