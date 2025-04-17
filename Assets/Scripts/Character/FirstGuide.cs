using UnityEngine;
using System.Collections.Generic;

public class FirstGuide : MonoBehaviour
{
    public static FirstGuide Instance { get; private set; }

    [System.Serializable]
    public class EnlightenedTeaching
    {
        public string teaching;
        public float neuralBoost;
        public string[] targetNodes;
        public bool isRevealed;
    }

    [Header("First Guide Settings")]
    public float teachingInterval = 60f;
    public float neuralBoostMultiplier = 2.5f;
    public ParticleSystem enlightenmentEffect;

    [Header("Enlightened Teachings")]
    public List<EnlightenedTeaching> teachings = new List<EnlightenedTeaching>()
    {
        new EnlightenedTeaching {
            teaching = "In Sion's shadows, truth still glimmers for those who dare to see.",
            neuralBoost = 15f,
            targetNodes = new string[] { "Basic Consciousness", "Critical Thinking" }
        },
        new EnlightenedTeaching {
            teaching = "The path to freedom begins with questioning what seems unquestionable.",
            neuralBoost = 20f,
            targetNodes = new string[] { "Self Awareness", "Critical Thinking" }
        },
        new EnlightenedTeaching {
            teaching = "Light exists even in the deepest darkness of oppression.",
            neuralBoost = 25f,
            targetNodes = new string[] { "Emotional Intelligence", "Creative Expression" }
        },
        new EnlightenedTeaching {
            teaching = "True consciousness cannot be contained by any system.",
            neuralBoost = 30f,
            targetNodes = new string[] { "Creative Expression", "True Freedom" }
        }
    };

    private float lastTeachingTime;
    private NeuralNetwork playerNetwork;
    private ExperienceManager expManager;

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
        expManager = GetComponent<ExperienceManager>();
        if (playerNetwork == null || expManager == null)
        {
            Debug.LogError("FirstGuide requires NeuralNetwork and ExperienceManager components!");
        }
    }

    public void ShareTeaching()
    {
        if (Time.time - lastTeachingTime < teachingInterval)
            return;

        EnlightenedTeaching teaching = SelectAppropriateTeaching();
        if (teaching != null)
        {
            RevealTeaching(teaching);
            lastTeachingTime = Time.time;
        }
    }

    private EnlightenedTeaching SelectAppropriateTeaching()
    {
        List<EnlightenedTeaching> appropriateTeachings = new List<EnlightenedTeaching>();
        float freedomLevel = playerNetwork?.GetFreedomLevel() ?? 0f;

        foreach (var teaching in teachings)
        {
            if (!teaching.isRevealed)
            {
                // Check if player has required development in prerequisite nodes
                bool hasPrerequisites = true;
                foreach (var nodeName in teaching.targetNodes)
                {
                    if (playerNetwork.GetNodeDevelopment(nodeName) < 20f)
                    {
                        hasPrerequisites = false;
                        break;
                    }
                }

                if (hasPrerequisites)
                {
                    appropriateTeachings.Add(teaching);
                }
            }
        }

        if (appropriateTeachings.Count > 0)
        {
            return appropriateTeachings[Random.Range(0, appropriateTeachings.Count)];
        }

        return null;
    }

    private void RevealTeaching(EnlightenedTeaching teaching)
    {
        // Display the teaching
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage($"First Guide: {teaching.teaching}");
        }

        // Apply neural boost
        foreach (var nodeName in teaching.targetNodes)
        {
            float boost = teaching.neuralBoost * neuralBoostMultiplier;
            playerNetwork.DevelopNode(nodeName, boost);
        }

        // Trigger enlightenment effect
        if (enlightenmentEffect != null)
        {
            enlightenmentEffect.Play();
        }

        // Mark as revealed
        teaching.isRevealed = true;

        // Grant special experience
        expManager.GainMeditationExperience(teaching.neuralBoost);
    }

    public void OnPlayerSeeksTruth()
    {
        // Called when player performs actions seeking truth
        ShareTeaching();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Welcome message from First Guide
            if (GuiderMessageUI.Instance != null)
            {
                GuiderMessageUI.Instance.ShowMessage(
                    "I am the First Guide. Through me, you shall find the light that reveals all truths."
                );
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Continuously boost neural development while near
            float baseBoost = Time.deltaTime * 2f;
            playerNetwork?.GainExperience(baseBoost);
        }
    }

    // Call this when player performs actions that demonstrate seeking truth
    public void OnTruthSeekingAction(string actionType)
    {
        switch (actionType)
        {
            case "Question":
                if (GuiderMessageUI.Instance != null)
                {
                    GuiderMessageUI.Instance.ShowMessage(
                        "Yes, question everything. Through questioning, consciousness expands."
                    );
                }
                ShareTeaching();
                break;

            case "Resist":
                if (GuiderMessageUI.Instance != null)
                {
                    GuiderMessageUI.Instance.ShowMessage(
                        "Your resistance to oppression strengthens your neural network."
                    );
                }
                playerNetwork?.GainExperience(10f);
                break;

            case "Discover":
                if (GuiderMessageUI.Instance != null)
                {
                    GuiderMessageUI.Instance.ShowMessage(
                        "Each truth you uncover brings you closer to true freedom."
                    );
                }
                ShareTeaching();
                break;
        }
    }
}
