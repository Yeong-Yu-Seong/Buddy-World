/*
    Author: Yeong Yu Seong
    Date Created: 12 November 2025
    Date Modified: 10 December 2025
    Description: Handles pet data operations such as loading, updating, and displaying pet stats from Firebase Realtime Database.
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
    private const float hungerInterval = 60f; // Interval in seconds to decrease hunger/happiness
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
    private TMP_Text cooldownMsg; // Text element to display cooldown messages
    /// <summary>
    /// Audio sources for pet sounds
    /// </summary>
    [SerializeField]
    private AudioSource petEatSound; // Sound effect for pet eating
    [SerializeField]
    private AudioSource petHappySound; // Sound effect for pet happy
    [SerializeField]
    private AudioSource petNormalSound; // Sound effect for pet normal state
    private bool isPetting = false; // Flag to track if pet is being petted
    [SerializeField]
    private Button petButton; // Button element to display petting message
    [SerializeField]
    private GameObject infoPanel; // Info panel to show petting instructions
    private DatabaseReference petRef; // Reference to the pet data in the database

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
                    StartCoroutine(StartStatChangeMessageClear(5f));
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
            cooldownMsg.text = "Actions on cooldown.";
            StartCoroutine(StartCooldownMessageClear(5f));
            return;
        }
        cooldownMsg.text = "";
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
                    StartCoroutine(CooldownCoroutine(25f)); // Set cooldown duration here (e.g., 30 seconds)
                    StartCoroutine(StartStatChangeMessageClear(5f));
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
                    StartCoroutine(StartStatChangeMessageClear(5f));
                }
            });
        });
    }

    /// <summary>
    /// Toggle petting state and update button text/color
    /// </summary>
    public void ActivatePetting()
    {
        isPetting = !isPetting;
        if (petButton != null)
        {
            var tmpText = petButton.GetComponentInChildren<TMP_Text>();
            if (!isPetting)
            {
                if (tmpText != null)
                {
                    tmpText.text = "Pet";
                }

                var img = petButton.GetComponent<Image>();
                if (img != null)
                {
                    // Set a light cream color (hex #ECE4D8) for the button background
                    img.color = new Color32(0xEC, 0xE4, 0xD8, 0xFF);
                }
                return;
            } else
            {
                if (tmpText != null)
                {
                    tmpText.text = "Petting...";
                }

                var img = petButton.GetComponent<Image>();
                if (img != null)
                {
                    // Set a light peach color (hex #FEEACC) for the button background
                    img.color = new Color32(0xFE, 0xEA, 0xCC, 0xFF);
                }
            }
        }
    }
    
    /// <summary>
    /// Press play with pet to increase happiness
    /// </summary>
    /// <param name="petName"></param>
    public void IncreaseHappiness(string petName)
    {   if (!isPetting)
        {
            return; // Skip if not petting
        }
        infoPanel.SetActive(false);
        if (isCooldownActive)
        {
            cooldownMsg.text = "Actions on cooldown.";
            StartCoroutine(StartCooldownMessageClear(5f));
            return;
        }
        cooldownMsg.text = "";
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

            pet.happiness = Mathf.Min(100, pet.happiness + 8); // Increase happiness but not above 100
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
                    statChangeMsg.text = "Happiness increased! +8";
                    loveParticles.Play(); // Play love particle effect
                    isCooldownActive = true;
                    petNormalSound.Stop();
                    petHappySound.Play(); // Play happy sound effect
                    petAnimator.SetTrigger("playTrigger");
                    StartCoroutine(CooldownCoroutine(25f)); // Set cooldown duration here (e.g., 30 seconds)
                    StartCoroutine(StartStatChangeMessageClear(5f));
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
    /// Reset hunger and happiness after leveling up
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
            hunger = 55;
            happiness = 65;
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
                    StartCoroutine(StartStatChangeMessageClear(5f));
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
        yield return new WaitForSeconds(5f); // Short delay to allow action effects to play
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
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
    }
    /// <summary>
    /// Clear cooldown message after delay
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator StartCooldownMessageClear(float delay)
    {
        yield return new WaitForSeconds(delay);
        cooldownMsg.text = "";
    }
    /// <summary>
    /// Clear stat change message after delay
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator StartStatChangeMessageClear(float delay)
    {
        yield return new WaitForSeconds(delay);
        statChangeMsg.text = "";
    }
    
    /// <summary>
    /// Event handler for pet data updates from Firebase Realtime Database
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void PetDataUpdate(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists)
        {
            string json = args.Snapshot.GetRawJsonValue();
            Pet updatedPet = JsonUtility.FromJson<Pet>(json);

            // Update local pet data
            petName = updatedPet.petName;
            level = updatedPet.level;
            hunger = updatedPet.hunger;
            happiness = updatedPet.happiness;

            // Update UI elements
            petNameText.text = "Pet Name: " + updatedPet.petName;
            petLevelText.text = "Level: " + updatedPet.level.ToString();
            petHungerText.text = "Hunger: " + updatedPet.hunger.ToString();
            petHappinessText.text = "Happiness: " + updatedPet.happiness.ToString();
            lastFedText.text = "Last Fed: " + updatedPet.lastFed;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statChangeMsg.text = "";
        cooldownMsg.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPetDataLoaded) // only attempt to load pet data once upon sign-in
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null)
            {
                LoadPetData();
                isPetDataLoaded = true;
                var PetDataRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(user.UserId).Child("pets").Child(prefabType);
                PetDataRef.ValueChanged += PetDataUpdate;
            }
        } else if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            // User signed out; reset flag to attempt reload on next Update
            isPetDataLoaded = false;
            var PetDataRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("pets").Child(prefabType);
            PetDataRef.ValueChanged -= PetDataUpdate;
            return;
        }

        // Update hunger and happiness based on time interval
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

            if (hunger >= 60) // Decrease hunger only if hunger is high
            {
                DecreaseHunger(petName, 2);
            }
            else if (hunger < 60) // Decrease happiness and hunger if hunger is low
            {
                DecreaseHappinessAndHunger(petName, 3);
            }
        }
    }
}
