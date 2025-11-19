/* Author: Yeong Yu Seong
   Date: 11 November 2025
   Last Modified: 12 November 2025
   Description: This script manages user accounts.
   Info: This script is written with the help of the fix code function.
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

public class AccountManager : MonoBehaviour
{
    /// <summary>
    /// Input fields for email and password.
    /// </summary>
    [SerializeField]
    private TMP_InputField EmailInput;
    [SerializeField]
    private TMP_InputField PasswordInput;
    /// <summary>
    /// UI elements for displaying errors and managing canvases.
    /// </summary>
    [SerializeField]
    private TMP_Text errorText;
    [SerializeField]
    private Canvas signInCanvas;
    [SerializeField]
    private Canvas loadingCanvas;
    [SerializeField]
    private Canvas gameplayCanvas;
    [SerializeField]
    private TMP_Text loadingText;
    /// <summary>
    /// Reference to the Firebase Realtime Database.
    /// </summary>
    private DatabaseReference db;
    
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

            Debug.Log($"Signed in successfully: {task.Result.User.UserId}");
            //retrieve pet data
            var retrieveTask = db.Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("pets").GetValueAsync();
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
                        Debug.Log("No pets found for this user. Creating a default pet.");
                        var dog = new Pet("Dog", FirebaseAuth.DefaultInstance.CurrentUser.UserId, 1, 50, 50, System.DateTime.Now.ToString());
                        string json = JsonUtility.ToJson(dog);
                        db.Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("pets").Child("Dog")
                          .SetRawJsonValueAsync(json)
                          .ContinueWithOnMainThread(t =>
                          {
                              if (t.IsFaulted || t.IsCanceled)
                                  Debug.LogError("Failed to add Dog: " + (t.Exception != null ? t.Exception.Message : "Task canceled"));
                              else
                                  Debug.Log("Dog added to database.");
                          });
                        return;
                    }

                    // If pets node exists it may contain multiple child pet entries, so iterate children
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
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SignOut threw an exception: " + ex.Message);
        }
        StartCoroutine(ShowLoadingThenSignIn(1.5f));
    }

    void Start()
    {
        // Initial canvas setup
        signInCanvas.enabled = true;
        loadingCanvas.enabled = false;
        gameplayCanvas.enabled = false;
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }
    IEnumerator CanvasTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        gameplayCanvas.enabled = true;
    }

    /// <summary>
    /// Coroutine to show loading canvas then switch to sign-in canvas.
    /// </summary>
    IEnumerator ShowLoadingThenSignIn(float delay)
    {
        // Called on sign out: show loading, hide gameplay, then show sign-in
        loadingCanvas.enabled = true;
        gameplayCanvas.enabled = false;
        loadingText.text = "Signing out...";
        yield return new WaitForSeconds(delay);
        loadingCanvas.enabled = false;
        signInCanvas.enabled = true;
    }
    void Update()
    {
        
    }
}
