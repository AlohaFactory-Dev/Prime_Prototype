using Firebase.Auth;
using UnityEditor;
using UnityEngine;

public static class DurianEditorUtility
{
    [MenuItem("Durian/Sign Out Firebase", priority = 0)]
    public static void SignOutFirebase()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("Firebase sign out");
        }
    }
}
