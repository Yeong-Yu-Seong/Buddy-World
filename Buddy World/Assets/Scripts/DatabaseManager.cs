/* Author: Yeong Yu Seong
   Date: 11 November 2025
   Last Modified: 11 November 2025
   Description: This script initializes a connection to Firebase Realtime Database
                and adds a pet object for a user (for testing purposes).
*/
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;

public class DatabaseManager : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var Octopus = new Pet("Octopus", "1" /* replace with userid */, 1, 100, 100, System.DateTime.Now.ToString());
        string json = JsonUtility.ToJson(Octopus);
        db.Child("1" /* replace with userid */).Child("pets").Child("Octopus").SetRawJsonValueAsync(json);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Pet
{
    public string petName;
    public string ownerID;
    public int level;
    public int hunger;
    public int happiness;
    public string lastFed;
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
