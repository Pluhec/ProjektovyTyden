using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PlayerChoice.DataSets;
using PlayerChoice.Timing;

using Newtonsoft.Json; // newtonsoft cuz using System.Text.Json did't work for half an hour

// gonna leave it for here cuz I don't wanna break anything
#if UNITY_EDITOR
using UnityEditor;
#endif

// Helper classes for JSON deserialization
[System.Serializable]
public class IndependentMessageList
{
    public List<SocialPost_JSON> IndependentZpravy;
}

public class UserMessageScript : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text Username;
    public TMP_Text UserMessage;

    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public Transform parentTransform;

    [Header("Data to Inject")]
    public string usernameInput = "Joe Doe"; // Default values ignore
    [TextArea(3, 10)]
    public string userMessageInput = "Prezident je velice dobrý"; // Default values ignore

    private static System.Random V_Random = new System.Random();
    private static HashSet<string> postedMessages = new HashSet<string>();

    void Start() // here was the issue when we left off in school
    {
        if (Username == null || UserMessage == null) return;
        
        if (string.IsNullOrEmpty(Username.text) || Username.text == "New Text" || Username.text == "Username")
        {
            setName(usernameInput);
            setMessage(userMessageInput);
        }
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

#if UNITY_EDITOR
        SocialPost_JSON messageToPost = GetRandomPost();
        if (messageToPost == null)
        {
            Debug.LogError("No message could be fetched. Check JSON files and perk conditions.");
            return;
        }

        GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
        spawned.transform.SetParent(parentTransform, false);

        UserMessageScript newScript = spawned.GetComponent<UserMessageScript>();

        if (newScript != null)
        {
            newScript.usernameInput = messageToPost.UserName;
            newScript.userMessageInput = messageToPost.Content;

            newScript.setName(messageToPost.UserName);
            newScript.setMessage(messageToPost.Content);
            
            EditorUtility.SetDirty(newScript);
            EditorUtility.SetDirty(spawned);
        }

        Undo.RegisterCreatedObjectUndo(spawned, "Spawn and Setup User");
#endif
    }

    private SocialPost_JSON GetRandomPost()
    {
        string socialBasePath = Path.Combine(Application.dataPath, "PleyerDecisions/SocialMessagesJSON/");
        
        byte V_SocialPostSpawnProb = 50; // we'll set this later automatically hopefully

        if (V_Random.Next(0, 101) <= V_SocialPostSpawnProb)
        {
            var unlockedTopics = new List<EnumStructs.E_CampaignTopic>();
            if (PerkSet.Campaign_AntiImmigrants.IsBought) { unlockedTopics.Add(EnumStructs.E_CampaignTopic.Imigrants); }
            if (PerkSet.Campaign_AntiNationalMinorities.IsBought) { unlockedTopics.Add(EnumStructs.E_CampaignTopic.Naionality); }
            if (PerkSet.Campaign_AntiSexualMinorities.IsBought) { unlockedTopics.Add(EnumStructs.E_CampaignTopic.Sexuality); }
            if (PerkSet.Campaign_AntiReligiousMinorities.IsBought) { unlockedTopics.Add(EnumStructs.E_CampaignTopic.Religion); }
            if (PerkSet.Campaign_AntiElites.IsBought) { unlockedTopics.Add(EnumStructs.E_CampaignTopic.Elites); }

            if (unlockedTopics.Count > 0)
            {
                var selectedTopic = unlockedTopics[V_Random.Next(0, unlockedTopics.Count)];
                var dependentMessageFile = Path.Combine(socialBasePath, "DependentMessages", $"T{(int)selectedTopic}_{selectedTopic}Messages.json");
                
                if (File.Exists(dependentMessageFile))
                {
                    var jsonString = File.ReadAllText(dependentMessageFile);

                    try
                    {
                        var dependentMessages = JsonConvert.DeserializeObject<Dictionary<string, List<SocialPost_JSON>>>(jsonString);
                        var stageKey = ((int)TimerBase.V_GameStage).ToString();

                        if (dependentMessages.TryGetValue(stageKey, out var messagesForStage) && messagesForStage.Count > 0)
                        {
                            var availableMessages = messagesForStage.Where(m => !postedMessages.Contains(m.Content)).ToList();
                            if (availableMessages.Count > 0)
                            {
                                var selectedMessage = availableMessages[V_Random.Next(0, availableMessages.Count)];
                                postedMessages.Add(selectedMessage.Content);
                                return selectedMessage;
                            }
                            else 
                            {
                                Debug.Log("All messages depleated !!!!!!!");
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                         Debug.LogError($"Failed to deserialize dependent messages: {ex.Message}");
                    }
                }
            }
        }
        
        var independentMessagesFile = Path.Combine(socialBasePath, "IndependentMessages.json");
        if (!File.Exists(independentMessagesFile))
        {
            Debug.LogError($"File not found: {independentMessagesFile}");
            return null;
        }

        try 
        {
            var jsonStringIndep = File.ReadAllText(independentMessagesFile);
            var independentMessages = JsonConvert.DeserializeObject<IndependentMessageList>(jsonStringIndep);
            
            if (independentMessages != null && independentMessages.IndependentZpravy != null && independentMessages.IndependentZpravy.Count > 0)
            {
                var availableMessages = independentMessages.IndependentZpravy.Where(m => !postedMessages.Contains(m.Content)).ToList();
                if (availableMessages.Count > 0)
                {
                    var selectedMessage = availableMessages[V_Random.Next(0, availableMessages.Count)];
                    postedMessages.Add(selectedMessage.Content);
                    return selectedMessage;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to deserialize independent messages: {ex.Message}");
        }

        return null;
    }
}
