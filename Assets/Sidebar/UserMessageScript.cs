using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UserMessageScript : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text Username;
    public TMP_Text UserMessage;

    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public Transform parentTransform;

    [Header("Data to Inject")]
    public string usernameInput = "Joe Doe"; // Deafult values ignore
    [TextArea(3, 10)]
    public string userMessageInput = "Prezident je velice dobrý"; // Deafult values ignore

    void Start() 
    {
        if (Username == null || UserMessage == null) return;
        setName(usernameInput);
        setMessage(userMessageInput);
    }

    public void setName(string name) 
    {
        if(Username != null) Username.text = name;
    }

    public void setMessage(string message) 
    {
        if(UserMessage != null) UserMessage.text = message;
    }

    [ContextMenu("Spawn and Setup Prefab")]
    public void SpawnPrefab()
    {
        if (prefabToSpawn == null || parentTransform == null)
        {
            Debug.LogError("UserMessageScript: Assign Prefab and Parent in Inspector!");
            return;
        }

// TO SPAWN MESSAGE USES CONTEXT MENU IN EDITOR. RUNTIME OR NOT IT DOESNT MATTER

#if UNITY_EDITOR
        GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
        spawned.transform.SetParent(parentTransform, false);

        UserMessageScript newScript = spawned.GetComponent<UserMessageScript>();

        if (newScript != null)
        {
            newScript.usernameInput = this.usernameInput;
            newScript.userMessageInput = this.userMessageInput;

            newScript.setName(this.usernameInput);
            newScript.setMessage(this.userMessageInput);
            
            EditorUtility.SetDirty(newScript);
        }

        Undo.RegisterCreatedObjectUndo(spawned, "Spawn and Setup User");
#endif
    }
}