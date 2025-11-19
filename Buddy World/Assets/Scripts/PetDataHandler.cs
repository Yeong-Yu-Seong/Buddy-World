/*
    Author: Yeong Yu Seong
    Date Created: 12 November 2025
    Date Modified: 19 November 2025
    Description: This script loads pet data from Firebase Realtime Database
                 and updates the UI accordingly. It also manages hunger and happiness
                 levels of the pet over time and through user interactions.
    Info: This script is written with the help of the fix code function.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class PetDataHandler : MonoBehaviour
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
    // Timer to run hunger/happiness updates on a time interval instead of frame count
    private float hungerTimer = 0f;
    private const float hungerInterval = 5f; // seconds; adjust as needed for testing
    private bool isPetDataLoaded = false;
    
    /// <summary>
    /// Decrease hunger when time passes
    /// </summary>
    /// <param name="petName"></param>
    public void DecreaseHunger(string petName, int amount)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot decrease hunger.");
            return;
        }

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
                    // successfully updated hunger in DB; update UI already done above
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
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
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot increase hunger.");
            return;
        }
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
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
                    lastFedText.text = "Last Fed: " + pet.lastFed;
                }
            });
        });
    }

    /// <summary>
    /// Decrease happiness and hunger when hunger is low
    /// </summary>
    /// <param name="petName"></param>
    /// <param name="amount"></param>
    public void DecreaseHappinessAndHunger(string petName, int amount)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot decrease happiness.");
            return;
        }

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
            pet.hunger = Mathf.Max(0, pet.hunger - amount); // Decrease hunger but not below 0
            happiness = pet.happiness;
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
                    petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
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
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot increase happiness.");
            return;
        }
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
                }
            });
        });
    }

    public void LoadPetData()
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        // If user not signed in yet, skip initial load; Update() will handle periodic updates when signed in
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; skipping initial pet load.");
            return;
        }

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadPetData();
    }

    // Decrease hunger and happiness on a reliable time interval (For testing, every 5 seconds)
    // Guard against calling Firebase code when the user is not signed in or petName is not set.
    void Update()
    {
        if (!isPetDataLoaded)
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null)
            {
                LoadPetData();
                isPetDataLoaded = true;
            }
        } else if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            // User signed out; reset flag to attempt reload on next Update
            isPetDataLoaded = false;
            return;
        }
        // advance timer based on real time
        hungerTimer += Time.deltaTime;
        if (hungerTimer >= hungerInterval)
        {
            hungerTimer = 0f;

            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null)
            {
                // User not signed in yet; skip updates until sign-in completes.
                return;
            }

            if (string.IsNullOrEmpty(petName))
            {
                // No active pet selected yet.
                return;
            }
            // Decrease hunger every interval (For testing, decrease every 5 seconds)
            if (hunger > 95)
            {
                DecreaseHunger(petName, 1);
            }
            // Decrease happiness and hunger every interval if hunger is below a certain threshold (For testing, decrease every 5 seconds)
            if (hunger <= 95)
            {
                DecreaseHappinessAndHunger(petName, 1);
            }

        }
    }
}
