/*
    Author: Yeong Yu Seong
    Date Created: 12 November 2025
    Date Modified: 18 November 2025
    Description: This script loads pet data from Firebase Realtime Database
                 and updates the UI accordingly. It also manages hunger and happiness
                 levels of the pet over time and through user interactions.
                 [Currently does not work when the user is not signed in when game starts.]
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class PetDataLoader : MonoBehaviour
{
    [SerializeField]
    private TMP_Text petNameText;
    [SerializeField]
    private TMP_Text ownerIDText;
    [SerializeField]
    private TMP_Text petLevelText;
    [SerializeField]
    private TMP_Text petHungerText;
    [SerializeField]
    private TMP_Text petHappinessText;
    [SerializeField]
    private TMP_Text lastFedText;
    [SerializeField]
    private GameObject petDataObject;
    private string petName;
    private int hunger;
    private int happiness;
    
    /// <summary>
    /// Decrease hunger when time passes
    /// </summary>
    /// <param name="petName"></param>
    public void DecreaseHunger(string petName, int amount)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        db.Child(user.UserId).Child("pets").Child(petName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            Pet pet = JsonUtility.FromJson<Pet>(json);

            pet.hunger = Mathf.Max(0, pet.hunger - amount); // Decrease hunger but not below 0
            petHungerText.text = "Hunger: " + pet.hunger.ToString();
            hunger = pet.hunger;

            string updatedJson = JsonUtility.ToJson(pet);
            db.Child(user.UserId).Child("pets").Child(petName).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    Debug.Log("Pet hunger decreased successfully.");
                }
            });
        });
    }
    /// <summary>
    /// Press feed button to increase hunger
    /// </summary>
    /// <param name="petName"></param>
    public void IncreaseHunger(string petName)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        petName = this.petName;

        db.Child(user.UserId).Child("pets").Child(petName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            Pet pet = JsonUtility.FromJson<Pet>(json);

            pet.hunger = Mathf.Min(100, pet.hunger + 10); // Increase hunger but not above 100
            pet.lastFed = System.DateTime.Now.ToString();
            petHungerText.text = "Hunger: " + pet.hunger.ToString();
            lastFedText.text = "Last Fed: " + pet.lastFed;
            hunger = pet.hunger;

            string updatedJson = JsonUtility.ToJson(pet);
            db.Child(user.UserId).Child("pets").Child(petName).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    Debug.Log("Pet hunger increased successfully.");
                }
            });
        });
    }
    /// <summary>
    /// Decrease happiness when hunger is low
    /// </summary>
    /// <param name="petName"></param>
    /// <param name="amount"></param>
    public void DecreaseHappiness(string petName, int amount)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        db.Child(user.UserId).Child("pets").Child(petName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            Pet pet = JsonUtility.FromJson<Pet>(json);

            pet.happiness = Mathf.Max(0, pet.happiness - amount); // Decrease happiness but not below 0
            happiness = pet.happiness;

            string updatedJson = JsonUtility.ToJson(pet);
            db.Child(user.UserId).Child("pets").Child(petName).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                    Debug.Log("Pet happiness decreased successfully.");
                }
            });
        });
    }
    /// <summary>
    /// Press play with pet to increase happiness
    /// </summary>
    /// <param name="petName"></param>
    public void IncreaseHappiness(string petName)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        petName = this.petName;

        db.Child(user.UserId).Child("pets").Child(petName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            Pet pet = JsonUtility.FromJson<Pet>(json);

            pet.happiness = Mathf.Min(100, pet.happiness + 10); // Increase happiness but not above 100
            happiness = pet.happiness;

            string updatedJson = JsonUtility.ToJson(pet);
            db.Child(user.UserId).Child("pets").Child(petName).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                    Debug.Log("Pet happiness increased successfully.");
                }
            });
        });
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        // get current pet data from database and display on UI
        var petDataTask = db.Child(user.UserId).Child("pets").GetValueAsync();
        petDataTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            foreach (DataSnapshot petSnapshot in snapshot.Children)
            {
                string json = petSnapshot.GetRawJsonValue();
                Pet pet = JsonUtility.FromJson<Pet>(json);

                // Update UI elements with pet data
                petNameText.text = "Name: " + pet.petName;
                ownerIDText.text = "Owner ID: " + pet.ownerID;
                petLevelText.text = "Level: " + pet.level.ToString();
                petHungerText.text = "Hunger: " + pet.hunger.ToString();
                petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                lastFedText.text = "Last Fed: " + pet.lastFed;
                petName = pet.petName;
                Debug.Log("Pet data loaded successfully.");
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        // Decrease hunger every minute if pet is currently active (For testing, decrease every 5 seconds)
        // [Bug: When user sign in and happiness is already below a certain threshold, hunger does not decrease as intended.]
        // [Bug 2: Database do not update hunger after a certain threshold is met.]
        if (Time.frameCount % (60 * 5) == 0) // Assuming 60 FPS, adjust as necessary
        {
            if (hunger > 0)
            {
                DecreaseHunger(petName, 1);
            }
            // Decrease happiness every minute if hunger is below a certain threshold (For testing, decrease every 5 seconds)
            if (hunger < 95)
            {
                DecreaseHappiness(petName, 1);
            }
        }
    }
}
