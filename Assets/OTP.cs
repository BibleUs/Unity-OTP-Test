using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;

public class OTP : MonoBehaviour
{
    FirebaseAuth auth;

    // Set the phone authentication timeout to a minute.
    private uint phoneAuthTimeoutMs = 60 * 1000;
    // The verification id needed along with the sent code for phone authentication.
    private string phoneAuthVerificationId;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;


    // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    protected void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        GetOTP();
    }

    void GetOTP()
    {
        Debug.Log("Getting OTP");
        PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(auth);
        provider.VerifyPhoneNumber("+917907136126", phoneAuthTimeoutMs, null,
          verificationCompleted: (credential) =>
          {
              // Auto-sms-retrieval or instant validation has succeeded (Android only).
              // There is no need to input the verification code.
              // `credential` can be used instead of calling GetCredential().
          },
          verificationFailed: (error) =>
          {
              // The verification code was not sent.
              // `error` contains a human readable explanation of the problem.
          },
          codeSent: (id, token) =>
          {
              // Verification code was successfully sent via SMS.
              // `id` contains the verification id that will need to passed in with
              // the code from the user when calling GetCredential().
              // `token` can be used if the user requests the code be sent again, to
              // tie the two requests together.
          },
          codeAutoRetrievalTimeOut: (id) =>
          {
              // Called when the auto-sms-retrieval has timed out, based on the given
              // timeout parameter.
              // `id` contains the verification id of the request that timed out.
          });
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = senderAuth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => Debug.Log(string.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }
}
