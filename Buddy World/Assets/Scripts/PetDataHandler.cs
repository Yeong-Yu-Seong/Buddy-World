/*
    Author: Yeong Yu Seong
    Date Created: 12 November 2025
    Date Modified: 3 December 2025
    Description: Script to handle pet data operations such as loading, updating hunger/happiness, leveling up, and renaming.
    Info: This script is written with the help of the fix code function.
    Note to reviewers: Write cooldown logic so that feeding and playing cannot be spammed.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;

public class PetDataHandler : MonoBehaviour
{
    [SerializeField]
    private string prefabType; // Type of the pet prefab (e.g., "Dog", "Cat")
    /// <summary>
    /// UI Elements to display pet data
    /// </summary>
    [SerializeField]
    private TMP_Text petNameText; // Text element to display pet name
    [SerializeField]
    private TMP_Text ownerIDText; // Text element to display owner ID
    [SerializeField]
    private TMP_Text petLevelText; // Text element to display pet level
    [SerializeField]
    private TMP_Text petHungerText; // Text element to display pet hunger
    [SerializeField]
    private TMP_Text petHappinessText; // Text element to display pet happiness
    [SerializeField]
    private TMP_Text lastFedText; // Text element to display last fed time
    /// <summary>
    /// Reference to the pet data UI object
    /// </summary>
    [SerializeField]
    private GameObject petDataObject; // Reference to the pet data UI object
    /// <summary>
    /// Pet data fields
    /// </summary>
    private string petName; // Current pet name
    private int hunger; // hunger level
    private int happiness; // happiness level
    private int level; // pet level
    /// <summary>
    /// Timer to run hunger/happiness updates on a time interval instead of frame count
    /// </summary>
    private float hungerTimer = 0f; // Timer to track hunger updates
    private const float hungerInterval = 5f; // seconds (for testing, adjust as needed)
    /// <summary>
    /// Flag to track if pet data has been loaded
    /// </summary>
    private bool isPetDataLoaded = false; // Flag to track if pet data has been loaded
    /// <summary>
    /// UI Elements for renaming pet
    /// </summary>
    [SerializeField]
    private TMP_InputField renameInputField; // Input field for renaming pet
    [SerializeField]
    private TMP_Text petNames; // Text element to display pet name in rename section
    private string newPetName; // New pet name for renaming
    /// <summary>
    /// GameObjects for dog eating animation
    /// </summary>
    [SerializeField]
    private GameObject petFood; // Pet food object to show when eating
    [SerializeField]
    private GameObject pet; // Pet object to animate when eating
    private Animator petAnimator; // Animator component for the pet
    [SerializeField]
    private ParticleSystem loveParticles; // Particle system for playing effect
    private bool isCooldownActive = false; // Flag to track if cooldown is active
    [SerializeField]
    private TMP_Text statChangeMsg; // Text element to display stat change messages
    [SerializeField]
    private AudioSource petEatSound; // Sound effect for pet eating
    [SerializeField]
    private AudioSource petHappySound; // Sound effect for pet happy
    [SerializeField]
    private AudioSource petNormalSound; // Sound effect for pet normal state

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

        db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).GetValueAsync().ContinueWithOnMainThread(task =>
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
            db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    // successfully updated hunger in DB; update UI already done above
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
                    statChangeMsg.text = "Hunger decreased! -" + amount;
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
        if (isCooldownActive)
            {
                Debug.Log("Actions are on cooldown. Please wait before performing another action.");
            return;
        }
        isCooldownActive = true;
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot increase hunger.");
            return;
        }
        petName = this.petName;

        db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).GetValueAsync().ContinueWithOnMainThread(task =>
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
            db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
                    lastFedText.text = "Last Fed: " + pet.lastFed;
                    statChangeMsg.text = "Hunger increased! +10";
                    petFood.SetActive(true);
                    petAnimator.SetTrigger("eatTrigger");
                    isCooldownActive = true;
                    petNormalSound.Stop();
                    petEatSound.Play(); // Play eating sound effect
                    StartCoroutine(CooldownCoroutine(5f)); // Set cooldown duration here (e.g., 5 seconds)
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

        db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).GetValueAsync().ContinueWithOnMainThread(task =>
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
            db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
                    statChangeMsg.text = "Happiness and Hunger decreased! -" + amount;
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
        if (isCooldownActive)
        {
            Debug.Log("Actions are on cooldown. Please wait before performing another action.");
            return;
        }
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot increase happiness.");
            return;
        }
        petName = this.petName;

        db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).GetValueAsync().ContinueWithOnMainThread(task =>
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
            db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                    statChangeMsg.text = "Happiness increased! +10";
                    loveParticles.Play(); // Play love particle effect
                    isCooldownActive = true;
                    petNormalSound.Stop();
                    petHappySound.Play(); // Play happy sound effect
                    petAnimator.SetTrigger("playTrigger");
                    StartCoroutine(CooldownCoroutine(5f)); // Set cooldown duration here (e.g., 5 seconds)
                }
            });
        });
    }

    /// <summary>
    /// Load pet data from Firebase Realtime Database
    /// </summary>
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
        var petDataTask = db.Child("users").Child(user.UserId).Child("pets").GetValueAsync();
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
                Pet currentPet = JsonUtility.FromJson<Pet>(json);
                if (currentPet.prefabType != prefabType)
                {
                    continue; // Skip pets that don't match the prefab type
                }
                // Get correct pet data from all the pets under this user
                petName = currentPet.petName;
                petNameText.text = "Pet Name: " + currentPet.petName;
                ownerIDText.text = "Owner ID: " + currentPet.ownerID;
                petLevelText.text = "Level: " + currentPet.level.ToString();
                petHungerText.text = "Hunger: " + currentPet.hunger.ToString();
                petHappinessText.text = "Happiness: " + currentPet.happiness.ToString();
                lastFedText.text = "Last Fed: " + currentPet.lastFed;
                hunger = currentPet.hunger;
                happiness = currentPet.happiness;
                level = currentPet.level;
                petFood.SetActive(false);
                petAnimator = pet.GetComponent<Animator>();
                Debug.Log("Loaded pet data for pet: " + currentPet.petName);
                petNormalSound.Play(); // Play normal sound effect
                break; // Exit loop after finding the matching pet
            }
        });
    }

    /// <summary>
    /// Increase pet level when happiness reaches 100
    /// Reset hunger and happiness to 10 upon leveling up
    /// </summary>
    public void IncreaseLevel(string petName)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot increase level.");
            return;
        }
        petName = this.petName;

        db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            Pet pet = JsonUtility.FromJson<Pet>(json);

            pet.level += 1; // Increase level by 1
            level = pet.level;
            hunger = 10;
            happiness = 10;
            pet.hunger = hunger;
            pet.happiness = happiness;
            string updatedJson = JsonUtility.ToJson(pet);
            db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petLevelText.text = "Level: " + pet.level.ToString();
                    petHungerText.text = "Hunger: " + pet.hunger.ToString();
                    petHappinessText.text = "Happiness: " + pet.happiness.ToString();
                    statChangeMsg.text = "Level increased! +1";
                }
            });
        });
    }

    /// <summary>
    /// Display current and new pet name in rename section
    /// </summary>
    public void DisplayPetNameInRenameSection()
    {
        petNames.text = $"Current: {petName}\nNew: {renameInputField.text}";
    }

    /// <summary>
    /// Rename pet in the database and update UI
    /// </summary>
    public void RenamePet(string newName)
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No signed-in user; cannot rename pet.");
            return;
        }

        newName = renameInputField.text;

        db.Child("users").Child(user.UserId).Child("pets").Child(prefabType).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                return;
            }

            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            Pet pet = JsonUtility.FromJson<Pet>(json);

            pet.petName = newName; // Update pet name

            string updatedJson = JsonUtility.ToJson(pet);
            db.Child("users").Child(user.UserId).Child("pets").Child(prefabType)
            .SetRawJsonValueAsync(updatedJson)
            .ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update pet data: " + (updateTask.Exception != null ? updateTask.Exception.Message : "Task canceled"));
                }
                else
                {
                    petNameText.text = "Pet Name: " + newName;
                    petName = newName;
                    statChangeMsg.text = "Pet renamed to " + newName;
                }
            });
        });
    }

    /// <summary>
    /// Cooldown coroutine to reset cooldown flag after specified time
    /// </summary>
    /// <param name="cooldownTime">Duration of the cooldown in seconds</param>
    /// <returns></returns>
    IEnumerator CooldownCoroutine(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
        if (loveParticles.isPlaying)
        {
            loveParticles.Stop();
            petHappySound.Stop();
        } else if (petFood.activeSelf)
        {
            petFood.SetActive(false);
            petEatSound.Stop();
        }
        petNormalSound.Play(); // Resume normal sound effect after cooldown
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statChangeMsg.text = "";
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
            if (hunger <= 95 && hunger > 0)
            {
                DecreaseHappinessAndHunger(petName, 1);
            }
            else if (hunger == 0)
            {
                DecreaseHappinessAndHunger(petName, 1);
            }
            // Increase level if happiness reaches 100
            if (happiness == 100)
            {
                IncreaseLevel(petName);
            }
        }
    }
}
