/* Author: Yeong Yu Seong
   Date: 11 November 2025
   Last Modified: 12 November 2025
   Description: This script manages user accounts.
*/
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Collections;

public class AccountManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField EmailInput;
    [SerializeField]
    private TMP_InputField PasswordInput;
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
            signInCanvas.enabled = false;
            loadingCanvas.enabled = true;
            loadingText.text = "Signing in...";
            StartCoroutine(CanvasTimer(2f));
            loadingCanvas.enabled = false;
            gameplayCanvas.enabled = true;
            
        });
    }
    public void SignOut()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("User signed out.");
        loadingCanvas.enabled = true;
        gameplayCanvas.enabled = false;
        loadingText.text = "Signing out...";
        StartCoroutine(CanvasTimer(2f));
        loadingCanvas.enabled = false;
        signInCanvas.enabled = true;
    }
    IEnumerator CanvasTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        signInCanvas.enabled = true;
        loadingCanvas.enabled = false;
        gameplayCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
