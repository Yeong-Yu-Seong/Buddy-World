/* Author: Yeong Yu Seong
   Date: 11 November 2025
   Last Modified: 12 November 2025
   Description: This script initializes a connection to Firebase Realtime Database
                and adds a pet object for a user (for testing purposes).
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

    // Parameterless constructor required/safer for JsonUtility deserialization
    public Pet() { }

    public Pet(string petName, string ownerID, int level, int hunger, int happiness, string lastFed)
    {
        this.petName = petName;
        this.ownerID = ownerID;
        this.level = level;
        this.hunger = hunger;
        this.happiness = happiness;
        this.lastFed = lastFed;
    }
}
