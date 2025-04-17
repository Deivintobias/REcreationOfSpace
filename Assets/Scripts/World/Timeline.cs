using UnityEngine;
using System.Collections.Generic;

namespace REcreationOfSpace.World
{
    public class Timeline : MonoBehaviour
    {
        [System.Serializable]
        public class HistoricalEvent
        {
            public string name;
            public int year; // Negative for BC, positive for AD
            public string description;
            public Vector3 location;
            public GameObject eventPrefab;
            public string[] relatedScriptures;
        }

        [System.Serializable]
        public class HistoricalEra
        {
            public string name;
            public int startYear;
            public int endYear;
            public string description;
            public Color skyColor;
            public float waterLevel;
            public GameObject[] environmentPrefabs;
        }

        [Header("Timeline Settings")]
        [SerializeField] private HistoricalEra[] historicalEras;
        [SerializeField] private HistoricalEvent[] historicalEvents;
        [SerializeField] private float timeTransitionSpeed = 1f;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material skyboxMaterial;

        [Header("Modern Era")]
        [SerializeField] private int currentYear = 2025;
        [SerializeField] private GameObject[] modernStructures;
        [SerializeField] private GameObject[] modernTechnology;

        private Dictionary<string, GameObject> historicalLocations = new Dictionary<string, GameObject>();
        private HistoricalEra currentEra;

        private void Start()
        {
            InitializeTimeline();
        }

        private void InitializeTimeline()
        {
            // Creation Era (Beginning of Time)
            historicalEras = new HistoricalEra[]
            {
                new HistoricalEra {
                    name = "Creation",
                    startYear = -4000,
                    endYear = -2348,
                    description = "The beginning when God created the heavens and earth",
                    waterLevel = 100f,
                    skyColor = new Color(0.7f, 0.9f, 1f)
                },
                new HistoricalEra {
                    name = "Great Flood",
                    startYear = -2348,
                    endYear = -2347,
                    description = "Noah's flood that covered the earth",
                    waterLevel = 200f,
                    skyColor = new Color(0.5f, 0.5f, 0.7f)
                },
                new HistoricalEra {
                    name = "Post-Flood",
                    startYear = -2347,
                    endYear = -1,
                    description = "The era after the flood until Christ",
                    waterLevel = 50f,
                    skyColor = new Color(0.6f, 0.8f, 1f)
                },
                new HistoricalEra {
                    name = "Modern Era",
                    startYear = 2000,
                    endYear = 2025,
                    description = "Current technological era",
                    waterLevel = 50f,
                    skyColor = new Color(0.5f, 0.7f, 0.9f)
                }
            };

            // Historical Events
            historicalEvents = new HistoricalEvent[]
            {
                new HistoricalEvent {
                    name = "Creation",
                    year = -4000,
                    description = "God creates the heavens and earth",
                    location = Vector3.zero,
                    relatedScriptures = new[] { "Genesis 1:1" }
                },
                new HistoricalEvent {
                    name = "Garden of Eden",
                    year = -4000,
                    description = "The first paradise and home of Adam and Eve",
                    location = new Vector3(100, 0, 100),
                    relatedScriptures = new[] { "Genesis 2:8" }
                },
                new HistoricalEvent {
                    name = "Noah's Ark",
                    year = -2348,
                    description = "The great flood and Noah's salvation",
                    location = new Vector3(-100, 0, 100),
                    relatedScriptures = new[] { "Genesis 6:14" }
                },
                new HistoricalEvent {
                    name = "Tower of Babel",
                    year = -2247,
                    description = "The tower that led to the confusion of languages",
                    location = new Vector3(-50, 0, -50),
                    relatedScriptures = new[] { "Genesis 11:4" }
                },
                new HistoricalEvent {
                    name = "Mount Sinai",
                    year = -1446,
                    description = "Where Moses received the Ten Commandments",
                    location = new Vector3(50, 0, -50),
                    relatedScriptures = new[] { "Exodus 19:20" }
                },
                new HistoricalEvent {
                    name = "Birth of Christ",
                    year = 0,
                    description = "The birth of Jesus in Bethlehem",
                    location = new Vector3(0, 0, 100),
                    relatedScriptures = new[] { "Luke 2:7" }
                }
            };

            CreateHistoricalLocations();
            SetCurrentEra(currentYear);
        }

        private void CreateHistoricalLocations()
        {
            foreach (var evt in historicalEvents)
            {
                if (evt.eventPrefab != null)
                {
                    var location = Instantiate(evt.eventPrefab, evt.location, Quaternion.identity);
                    location.name = evt.name;
                    historicalLocations[evt.name] = location;

                    // Add information display
                    var info = location.AddComponent<HistoricalInfo>();
                    info.Initialize(evt);
                }
            }
        }

        public void SetCurrentEra(int year)
        {
            foreach (var era in historicalEras)
            {
                if (year >= era.startYear && year <= era.endYear)
                {
                    currentEra = era;
                    UpdateEnvironment(era);
                    break;
                }
            }
        }

        private void UpdateEnvironment(HistoricalEra era)
        {
            // Update water level
            if (waterMaterial != null)
            {
                waterMaterial.SetFloat("_WaterLevel", era.waterLevel);
            }

            // Update sky
            if (skyboxMaterial != null)
            {
                skyboxMaterial.SetColor("_SkyTint", era.skyColor);
            }

            // Update environment objects
            foreach (var prefab in era.environmentPrefabs)
            {
                if (prefab != null)
                {
                    Instantiate(prefab, Vector3.zero, Quaternion.identity);
                }
            }

            // Handle modern structures
            if (era.name == "Modern Era")
            {
                foreach (var structure in modernStructures)
                {
                    if (structure != null)
                    {
                        structure.SetActive(true);
                    }
                }
            }
        }

        public HistoricalEvent GetEventByName(string eventName)
        {
            foreach (var evt in historicalEvents)
            {
                if (evt.name == eventName)
                    return evt;
            }
            return null;
        }

        public GameObject GetLocationByName(string locationName)
        {
            if (historicalLocations.ContainsKey(locationName))
                return historicalLocations[locationName];
            return null;
        }

        public HistoricalEra GetCurrentEra()
        {
            return currentEra;
        }

        public int GetCurrentYear()
        {
            return currentYear;
        }
    }
}
