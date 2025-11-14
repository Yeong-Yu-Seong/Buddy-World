/*
    Author: Yeong Yu Seong
    Date Created: 12 November 2025
    Date Modified: 12 November 2025
    Description: Show pet data on UI elements within a pet prefab.
*/
using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class PetDatas : MonoBehaviour
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
                Debug.Log("Pet data loaded successfully.");
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
