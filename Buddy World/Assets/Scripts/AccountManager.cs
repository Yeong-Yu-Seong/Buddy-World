/* Author: Yeong Yu Seong
   Date: 11 November 2025
   Last Modified: 10 December 2025
   Description: Account manager script to handle user authentication and account management.
   Info: This script is written with the help of the fix code function and ChatGPT.
*/
using UnityEngine;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using TMPro;
using Firebase.Auth;
using Firebase;
using System;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{
    /// <summary>
    /// Input fields for email and password.
    /// </summary>
    [SerializeField]
    private TMP_InputField EmailInput; // Input field for email
    [SerializeField]
    private TMP_InputField PasswordInput; // Input field for password
    /// <summary>
    /// UI elements for displaying errors and managing canvases.
    /// </summary>
    [SerializeField]
    private TMP_Text errorText; // To display error messages
    [SerializeField]
    private Canvas signInCanvas; // Canvas for sign-in and sign-up
    [SerializeField]
    private Canvas loadingCanvas; // Canvas for loading screen
    [SerializeField]
    private Canvas gameStartCanvas; // Canvas for game start screen
    [SerializeField]
    private Canvas gameplayCanvas; // Canvas for main gameplay
    [SerializeField]
    private TMP_Text loadingText; // Text element to show loading status
    [SerializeField]
    public TMP_Text accountDetailsText; // Text element to show account details
    [SerializeField]
    private Image accountDetailsPage; // Panel for account details page
    /// <summary>
    /// Reference to the Firebase Realtime Database.
    /// </summary>
    private DatabaseReference db;
    private DatabaseReference petRef;
    private FirebaseAuth auth;
    private FirebaseUser lastUser;
    private string currentUserId;
    
    /// <summary>
    /// Creates a new user account with the provided email and password.
    /// </summary>
    public void SignUp()
    {
        errorText.text = "";

        var createTask = FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(EmailInput.text, PasswordInput.text);
        createTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                var baseException = task.Exception.GetBaseException();

                if (baseException is FirebaseException)
                {
                    var firebaseException = baseException as FirebaseException;
                    var errorCode = (AuthError)firebaseException.ErrorCode;

                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            errorText.text = "Please enter an e-mail address!";
                            break;
                        case AuthError.MissingPassword:
                            errorText.text = "Please enter a password!";
                            break;
                        case AuthError.WeakPassword:
                            errorText.text = "Please enter a password 6 characters or longer!";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            errorText.text = "The email you have entered is already in use";
                            break;
                        case AuthError.InvalidEmail:
                            errorText.text = "The email address is invalid!";
                            break;
                        case AuthError.NetworkRequestFailed:
                            errorText.text = "Network error, please try again later!";
                            break;
                        default:
                            errorText.text = $"Unknown Firebase exception: {errorCode}";
                            break;
                    }

                    return;
                }

                errorText.text = $"Unknown exception when signing up: {baseException.Message}";
                return;
            }

            if (task.IsCanceled)
            {
                errorText.text = "User creation cancelled!";
                return;
            }

            if (task.IsCompletedSuccessfully)
            {
                errorText.text = "User created successfully, please sign in!";

                var uid = task.Result.User.UserId;
                db.Child("users").Child(uid).Child("email").SetValueAsync(EmailInput.text);
                Debug.Log($"Created user UID: {uid}");
            }
        });
    }

    /// <summary>
    /// Signs in an existing user with the provided email and password.
    /// </summary>
    public void SignIn()
    {
        var signInTask = FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(EmailInput.text, PasswordInput.text);
        signInTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                var baseException = task.Exception.GetBaseException();

                if (baseException is FirebaseException)
                {
                    var firebaseException = baseException as FirebaseException;
                    var errorCode = (AuthError)firebaseException.ErrorCode;

                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            errorText.text = "Please enter an e-mail address!";
                            break;
                        case AuthError.MissingPassword:
                            errorText.text = "Please enter a password!";
                            break;
                        case AuthError.WrongPassword:
                            errorText.text = "The password you have entered is incorrect!";
                            break;
                        case AuthError.UserNotFound:
                            errorText.text = "No user found with this email!";
                            break;
                        case AuthError.InvalidEmail:
                            errorText.text = "The email address is invalid!";
                            break;
                        case AuthError.NetworkRequestFailed:
                            errorText.text = "Network error, please try again later!";
                            break;
                        default:
                            errorText.text = $"Unknown Firebase exception: {errorCode}";
                            break;
                    }
                    return;
                }
                errorText.text = $"Unknown exception when signing in: {baseException.Message}";
                return;
            }
            if (task.IsCanceled)
            {
                errorText.text = "Sign-in cancelled!";
                return;
            }
            EmailInput.text = "";
            PasswordInput.text = "";
            errorText.text = "";
            Debug.Log($"Signed in successfully: {task.Result.User.UserId}");
            //retrieve pet data
            var retrieveTask = db.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("pets").GetValueAsync();
            retrieveTask.ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error loading pets: " + (task.Exception != null ? task.Exception.Message : "Task canceled"));
                    return;
                }

                if (task.IsCompleted)
                {
                    // If no pets node exists, create a default pet
                    if (!task.Result.Exists)
                    {
                        Debug.Log("No pets found for this user. Creating default pets.");
                        var dog = new Pet("Dog", FirebaseAuth.DefaultInstance.CurrentUser.UserId, 1, 50, 60, System.DateTime.Now.ToString(), "Dog");
                        var fish = new Pet("Fish", FirebaseAuth.DefaultInstance.CurrentUser.UserId, 1, 50, 60, System.DateTime.Now.ToString(), "Fish");
                        
                        string jsonDog = JsonUtility.ToJson(dog);
                        string jsonFish = JsonUtility.ToJson(fish);
                        // Save the default pet to the database
                        db.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("pets").Child("Dog")
                          .SetRawJsonValueAsync(jsonDog)
                          .ContinueWithOnMainThread(t =>
                          {
                              if (t.IsFaulted || t.IsCanceled)
                                  Debug.LogError("Failed to add Dog: " + (t.Exception != null ? t.Exception.Message : "Task canceled"));
                              else
                                  Debug.Log("Dog added to database.");
                          });
                        db.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("pets").Child("Fish")
                          .SetRawJsonValueAsync(jsonFish)
                          .ContinueWithOnMainThread(t =>
                          {
                              if (t.IsFaulted || t.IsCanceled)
                                  Debug.LogError("Failed to add Fish: " + (t.Exception != null ? t.Exception.Message : "Task canceled"));
                              else
                                  Debug.Log("Fish added to database.");
                          });
                    }
                    // If pets node exists, load and display pet data
                    foreach (var child in task.Result.Children)
                    {
                        try
                        {
                            string childJson = child.GetRawJsonValue();
                            if (string.IsNullOrEmpty(childJson))
                            {
                                Debug.LogWarning($"Empty JSON for pet key '{child.Key}'");
                                continue;
                            }

                            Pet p = JsonUtility.FromJson<Pet>(childJson);
                            if (p != null)
                            {   
                                Debug.Log($"Pet loaded: {p.petName} (key: {child.Key})");
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse pet JSON for key '{child.Key}': {childJson}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Exception while parsing pet '{child.Key}': {ex.Message}");
                        }
                    }
                }
            });
            // Start a coroutine that shows the loading canvas and then switches to gameplay after a delay
            StartCoroutine(ShowLoadingThenSwitch(2f));
        });
    }

    /// <summary>
    /// Signs out the current user. 
    /// </summary>
    public void SignOut()
    {
        // Sign out from Firebase and show sign-in after a short loading delay
        try
        {
            FirebaseAuth.DefaultInstance.SignOut();
            if (accountDetailsText != null)
                accountDetailsText.text = "";
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SignOut threw an exception: " + ex.Message);
        }
        StartCoroutine(ShowLoadingThenSignIn(1.5f));
    }

    /// <summary>
    /// Coroutine to show loading canvas then switch to gameplay canvas.
    /// </summary>
    IEnumerator ShowLoadingThenSwitch(float delay)
    {
        // Called after successful sign-in: hide sign-in, show loading, then switch to gameplay
        signInCanvas.enabled = false;
        loadingCanvas.enabled = true;
        loadingText.text = "Signing in...";
        yield return new WaitForSeconds(delay);
        loadingCanvas.enabled = false;
        gameStartCanvas.enabled = true;
        gameplayCanvas.enabled = true;
        gameplayCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine to show loading canvas then return to sign-in canvas.
    /// </summary>
    IEnumerator ShowLoadingThenSignIn(float delay)
    {
        // Called on sign out: show loading, hide gameplay, then show sign-in
        loadingCanvas.enabled = true;
        gameStartCanvas.enabled = false;
        gameplayCanvas.enabled = false;
        accountDetailsPage.gameObject.SetActive(false);
        loadingText.text = "Signing out...";
        yield return new WaitForSeconds(delay);
        loadingCanvas.enabled = false;
        signInCanvas.enabled = true;
    }

    /// <summary>
    /// Event handler for pet data changes in the database
    /// </summary>
    public void OnPetDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists) return;

        var dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        var petDataTask = dbRef.Child("users").Child(currentUserId).Child("pets").GetValueAsync();
        petDataTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve pet data.");
                return;
            }

            DataSnapshot snapshot = task.Result;

            int petNum = 1;
            if (accountDetailsText != null) accountDetailsText.text = "";

            foreach (DataSnapshot petSnapshot in snapshot.Children)
            {
                string json = petSnapshot.GetRawJsonValue();
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning($"Empty JSON for pet key '{petSnapshot.Key}'");
                    continue;
                }

                Pet currentPet = JsonUtility.FromJson<Pet>(json);
                if (currentPet == null)
                {
                    Debug.LogWarning($"Failed to parse pet JSON for key '{petSnapshot.Key}': {json}");
                    continue;
                }

                // Level-up logic...
                if (currentPet.happiness == 100)
                {
                    currentPet.happiness = 65;
                    currentPet.level++;
                    currentPet.hunger = 55;

                    dbRef.Child("users").Child(currentUserId).Child("pets")
                        .Child(currentPet.prefabType)
                        .SetRawJsonValueAsync(JsonUtility.ToJson(currentPet));
                }

                if (accountDetailsText != null)
                {
                    accountDetailsText.text +=
                        $"Pet {petNum}:\n{currentPet.petName}\nLevel: {currentPet.level}\nHunger: {currentPet.hunger}\nHappiness: {currentPet.happiness}\nLast Fed: {currentPet.lastFed}\n";
                }

                petNum++;
            }
        });
    }

    /// <summary>
    /// Event handler for authentication state changes.
    /// </summary>
    public void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser != lastUser)
        {
            if (currentUser == null)
            {
                // User signed out
                CleanupOldListeners();
                lastUser = null;
                currentUserId = null;
                Debug.Log("User is null.");
            }
            else
            {
                // User signed in
                lastUser = currentUser;
                currentUserId = currentUser.UserId;
                AttachListener(currentUser.UserId);
            }
        }
    }

    /// <summary>
    /// Attaches a listener to the current user's pet data in the database.
    /// </summary>
    public void AttachListener(string userId)
    {
        // If there is an existing listener (possibly for a previous user), detach it first
        if (petRef != null)
        {
            try
            {
                petRef.ValueChanged -= OnPetDataChanged;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to detach previous pet listener: " + ex.Message);
            }
            petRef = null;
        }

        currentUserId = userId;

        petRef = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(userId)
            .Child("pets");

        petRef.ValueChanged += OnPetDataChanged;
        Debug.Log("Current User ID: " + currentUserId);
    }

    /// <summary>
    /// Cleans up old database listeners to prevent memory leaks.
    /// </summary>
    public void CleanupOldListeners()
    {
        if (petRef != null)
        {
            try
            {
                petRef.ValueChanged -= OnPetDataChanged;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to detach pet listener during cleanup: " + ex.Message);
            }
            petRef = null;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
        }
        lastUser = null;
        CleanupOldListeners();
        // Initial canvas setup
        signInCanvas.enabled = true;
        loadingCanvas.enabled = false;
        gameStartCanvas.enabled = false;
        gameplayCanvas.enabled = false;
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
