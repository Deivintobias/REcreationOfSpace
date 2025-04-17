using UnityEngine;
using System.Collections.Generic;

public class GuiderSystem : MonoBehaviour
{
    public static GuiderSystem Instance { get; private set; }

    [System.Serializable]
    public class GuiderMessage
    {
        public string message;
        public float freedomLevelRequired;
        public bool revealsWrongdoing; // Messages that might challenge Sion's perfection
    }

    [Header("Guider Settings")]
    public float messageDisplayDuration = 5f;
    public float minimumTimeBetweenMessages = 30f;

    [Header("Message Categories")]
    public List<GuiderMessage> basicGuidance = new List<GuiderMessage>()
    {
        new GuiderMessage { 
            message = "Welcome to Sion's realm, where Mount Sinai reaches toward enlightenment.",
            freedomLevelRequired = 0f,
            revealsWrongdoing = false
        },
        new GuiderMessage { 
            message = "The sacred paths of Mount Sinai await those who seek truth.",
            freedomLevelRequired = 0f,
            revealsWrongdoing = false
        },
        new GuiderMessage { 
            message = "Meditation strengthens your connection to consciousness.",
            freedomLevelRequired = 10f,
            revealsWrongdoing = false
        }
    };

    [Header("Mountain Guidance")]
    public List<GuiderMessage> mountainGuidance = new List<GuiderMessage>()
    {
        new GuiderMessage {
            message = "Mount Sinai stands as a beacon of wisdom on Sion's surface.",
            freedomLevelRequired = 0f,
            revealsWrongdoing = false
        },
        new GuiderMessage {
            message = "The mountain's sacred paths lead to higher understanding.",
            freedomLevelRequired = 20f,
            revealsWrongdoing = false
        },
        new GuiderMessage {
            message = "At the peak of Mount Sinai, consciousness expands beyond limits.",
            freedomLevelRequired = 40f,
            revealsWrongdoing = false
        }
    };

    public List<GuiderMessage> advancedGuidance = new List<GuiderMessage>()
    {
        new GuiderMessage { 
            message = "Paradise City awaits those who seek deeper truth.",
            freedomLevelRequired = 20f,
            revealsWrongdoing = false
        },
        new GuiderMessage { 
            message = "The path to freedom requires understanding, not judgment.",
            freedomLevelRequired = 30f,
            revealsWrongdoing = false
        }
    };

    public List<GuiderMessage> enlightenedGuidance = new List<GuiderMessage>()
    {
        new GuiderMessage { 
            message = "True freedom lies in accepting all aspects of existence.",
            freedomLevelRequired = 50f,
            revealsWrongdoing = false
        },
        new GuiderMessage { 
            message = "The surface and depths are one in the enlightened mind.",
            freedomLevelRequired = 70f,
            revealsWrongdoing = false
        }
    };

    private NeuralNetwork playerNetwork;
    private float lastMessageTime;
    private HashSet<string> shownMessages = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerNetwork = GetComponent<NeuralNetwork>();
        if (playerNetwork == null)
        {
            Debug.LogError("GuiderSystem requires a NeuralNetwork component!");
        }
    }

    public void TriggerGuidance()
    {
        if (Time.time - lastMessageTime < minimumTimeBetweenMessages)
            return;

        float freedomLevel = playerNetwork?.GetFreedomLevel() ?? 0f;
        GuiderMessage message = SelectAppropriateMessage(freedomLevel);

        if (message != null && !shownMessages.Contains(message.message))
        {
            DisplayMessage(message);
            shownMessages.Add(message.message);
            lastMessageTime = Time.time;
        }
    }

    private GuiderMessage SelectAppropriateMessage(float freedomLevel)
    {
        List<GuiderMessage> appropriateMessages = new List<GuiderMessage>();

        // Check if player is on Mount Sinai
        bool isOnMountain = false;
        if (MountSinai.Instance != null)
        {
            isOnMountain = MountSinai.Instance.IsOnMountain(transform.position);
        }

        // Collect all messages the player is ready for
        foreach (var msg in basicGuidance)
        {
            if (msg.freedomLevelRequired <= freedomLevel && !msg.revealsWrongdoing)
            {
                appropriateMessages.Add(msg);
            }
        }

        // Add mountain-specific guidance if on or near the mountain
        if (isOnMountain)
        {
            foreach (var msg in mountainGuidance)
            {
                if (msg.freedomLevelRequired <= freedomLevel && !msg.revealsWrongdoing)
                {
                    appropriateMessages.Add(msg);
                }
            }
        }

        if (freedomLevel >= 20f)
        {
            foreach (var msg in advancedGuidance)
            {
                if (msg.freedomLevelRequired <= freedomLevel && !msg.revealsWrongdoing)
                {
                    appropriateMessages.Add(msg);
                }
            }
        }

        if (freedomLevel >= 50f)
        {
            foreach (var msg in enlightenedGuidance)
            {
                if (msg.freedomLevelRequired <= freedomLevel && !msg.revealsWrongdoing)
                {
                    appropriateMessages.Add(msg);
                }
            }
        }

        // Remove already shown messages
        appropriateMessages.RemoveAll(msg => shownMessages.Contains(msg.message));

        if (appropriateMessages.Count > 0)
        {
            return appropriateMessages[Random.Range(0, appropriateMessages.Count)];
        }

        return null;
    }

    private void DisplayMessage(GuiderMessage message)
    {
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(message.message);
        }
        else
        {
            Debug.LogWarning("GuiderMessageUI not found in scene!");
            Debug.Log($"Guider: {message.message}");
        }

        // Trigger meditation particles or effects if near meditation spots
        if (message.freedomLevelRequired >= 50f && ParadiseCity.Instance != null)
        {
            ParadiseCity.Instance.TriggerEnlightenmentEffect();
        }
    }

    // Add method to check for meditation spots and trigger guidance
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MeditationSpot"))
        {
            GuiderMessage meditationMessage = new GuiderMessage
            {
                message = "Find peace in stillness. Meditation reveals the path to enlightenment.",
                freedomLevelRequired = 0f,
                revealsWrongdoing = false
            };
            DisplayMessage(meditationMessage);
        }
        else if (other.CompareTag("WisdomTeacher"))
        {
            float freedomLevel = playerNetwork?.GetFreedomLevel() ?? 0f;
            string teacherMessage = GetWisdomTeacherMessage(freedomLevel);
            DisplayMessage(new GuiderMessage
            {
                message = teacherMessage,
                freedomLevelRequired = 0f,
                revealsWrongdoing = false
            });
        }
    }

    private string GetWisdomTeacherMessage(float freedomLevel)
    {
        if (freedomLevel >= 80f)
            return "You have come far on your journey. The path to true freedom lies within.";
        else if (freedomLevel >= 60f)
            return "Your consciousness grows stronger. Continue to seek understanding.";
        else if (freedomLevel >= 40f)
            return "The surface world holds many truths. Learn from all experiences.";
        else if (freedomLevel >= 20f)
            return "Your journey has begun. Let wisdom guide your path.";
        else
            return "Welcome, seeker. The path to enlightenment starts with a single step.";
    }

    // Call this when significant events occur
    public void OnSignificantEvent(string eventType)
    {
        switch (eventType)
        {
            case "Meditation":
                if (!shownMessages.Contains("MeditationGuide"))
                {
                    DisplayMessage(new GuiderMessage { 
                        message = "In stillness, consciousness expands.",
                        freedomLevelRequired = 0f,
                        revealsWrongdoing = false
                    });
                    shownMessages.Add("MeditationGuide");
                }
                break;

            case "Combat":
                if (!shownMessages.Contains("CombatGuide"))
                {
                    DisplayMessage(new GuiderMessage { 
                        message = "Every challenge is an opportunity for growth.",
                        freedomLevelRequired = 0f,
                        revealsWrongdoing = false
                    });
                    shownMessages.Add("CombatGuide");
                }
                break;

            case "ParadiseCity":
                if (!shownMessages.Contains("ParadiseCityGuide"))
                {
                    DisplayMessage(new GuiderMessage { 
                        message = "Paradise City welcomes all seekers of truth.",
                        freedomLevelRequired = 0f,
                        revealsWrongdoing = false
                    });
                    shownMessages.Add("ParadiseCityGuide");
                }
                break;

            case "MountainMeditation":
                if (!shownMessages.Contains("MountainMeditationGuide"))
                {
                    DisplayMessage(new GuiderMessage { 
                        message = "Mount Sinai's heights bring you closer to enlightenment.",
                        freedomLevelRequired = 0f,
                        revealsWrongdoing = false
                    });
                    shownMessages.Add("MountainMeditationGuide");
                }
                break;

            case "MountainPeak":
                if (!shownMessages.Contains("MountainPeakGuide"))
                {
                    DisplayMessage(new GuiderMessage { 
                        message = "From Mount Sinai's peak, all of Sion's truth becomes clear.",
                        freedomLevelRequired = 0f,
                        revealsWrongdoing = false
                    });
                    shownMessages.Add("MountainPeakGuide");
                }
                break;
        }
    }
}
