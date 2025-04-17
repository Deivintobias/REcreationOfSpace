using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class TimelineUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject timelinePanel;
        [SerializeField] private RectTransform timelineContainer;
        [SerializeField] private GameObject eventPrefab;
        [SerializeField] private TextMeshProUGUI currentEraText;
        [SerializeField] private TextMeshProUGUI currentYearText;
        [SerializeField] private Button nextEraButton;
        [SerializeField] private Button previousEraButton;

        [Header("Timeline Settings")]
        [SerializeField] private float eventSpacing = 100f;
        [SerializeField] private Color selectedEventColor = Color.yellow;
        [SerializeField] private Color normalEventColor = Color.white;

        private Timeline timeline;
        private Dictionary<string, GameObject> eventMarkers = new Dictionary<string, GameObject>();
        private string selectedEvent = "";

        private void Start()
        {
            timeline = FindObjectOfType<Timeline>();
            if (timeline == null)
            {
                Debug.LogError("Timeline component not found!");
                return;
            }

            InitializeTimeline();
            UpdateTimelineUI();

            // Add button listeners
            if (nextEraButton != null)
                nextEraButton.onClick.AddListener(GoToNextEra);
            if (previousEraButton != null)
                previousEraButton.onClick.AddListener(GoToPreviousEra);

            // Hide panel initially
            if (timelinePanel != null)
                timelinePanel.SetActive(false);
        }

        private void InitializeTimeline()
        {
            if (timelineContainer == null || eventPrefab == null)
                return;

            // Clear existing markers
            foreach (var marker in eventMarkers.Values)
            {
                Destroy(marker);
            }
            eventMarkers.Clear();

            // Get historical events through reflection since they're private
            var eventsField = timeline.GetType().GetField("historicalEvents",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (eventsField != null)
            {
                var events = (Timeline.HistoricalEvent[])eventsField.GetValue(timeline);
                float currentPosition = 0f;

                foreach (var evt in events)
                {
                    // Create event marker
                    GameObject marker = Instantiate(eventPrefab, timelineContainer);
                    marker.name = evt.name + "Marker";

                    // Position marker
                    RectTransform rectTransform = marker.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(currentPosition, 0);
                    }

                    // Set up marker UI
                    TextMeshProUGUI nameText = marker.GetComponentInChildren<TextMeshProUGUI>();
                    if (nameText != null)
                    {
                        nameText.text = $"{evt.name}\n{(evt.year < 0 ? -evt.year + " BC" : evt.year + " AD")}";
                    }

                    // Add click handler
                    Button button = marker.GetComponent<Button>();
                    if (button != null)
                    {
                        string eventName = evt.name; // Create local copy for closure
                        button.onClick.AddListener(() => SelectEvent(eventName));
                    }

                    eventMarkers[evt.name] = marker;
                    currentPosition += eventSpacing;
                }

                // Set content width
                timelineContainer.sizeDelta = new Vector2(currentPosition, timelineContainer.sizeDelta.y);
            }
        }

        private void SelectEvent(string eventName)
        {
            // Deselect previous event
            if (eventMarkers.ContainsKey(selectedEvent))
            {
                var prevButton = eventMarkers[selectedEvent].GetComponent<Button>();
                if (prevButton != null)
                {
                    var colors = prevButton.colors;
                    colors.normalColor = normalEventColor;
                    prevButton.colors = colors;
                }
            }

            selectedEvent = eventName;

            // Highlight selected event
            if (eventMarkers.ContainsKey(eventName))
            {
                var button = eventMarkers[eventName].GetComponent<Button>();
                if (button != null)
                {
                    var colors = button.colors;
                    colors.normalColor = selectedEventColor;
                    button.colors = colors;
                }

                // Center on selected event
                RectTransform markerTransform = eventMarkers[eventName].GetComponent<RectTransform>();
                if (markerTransform != null)
                {
                    timelineContainer.anchoredPosition = new Vector2(
                        -markerTransform.anchoredPosition.x + timelineContainer.rect.width * 0.5f,
                        timelineContainer.anchoredPosition.y
                    );
                }

                // Set timeline to this era
                var evt = timeline.GetEventByName(eventName);
                if (evt != null)
                {
                    timeline.SetCurrentEra(evt.year);
                    UpdateTimelineUI();
                }
            }
        }

        private void UpdateTimelineUI()
        {
            var currentEra = timeline.GetCurrentEra();
            if (currentEra != null)
            {
                if (currentEraText != null)
                    currentEraText.text = currentEra.name;

                if (currentYearText != null)
                    currentYearText.text = currentEra.startYear < 0 ? 
                        $"{-currentEra.startYear} BC - {-currentEra.endYear} BC" :
                        $"{currentEra.startYear} AD - {currentEra.endYear} AD";
            }
        }

        private void GoToNextEra()
        {
            var currentEra = timeline.GetCurrentEra();
            if (currentEra != null)
            {
                // Find next era
                var erasField = timeline.GetType().GetField("historicalEras",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (erasField != null)
                {
                    var eras = (Timeline.HistoricalEra[])erasField.GetValue(timeline);
                    for (int i = 0; i < eras.Length - 1; i++)
                    {
                        if (eras[i] == currentEra)
                        {
                            timeline.SetCurrentEra(eras[i + 1].startYear);
                            UpdateTimelineUI();
                            break;
                        }
                    }
                }
            }
        }

        private void GoToPreviousEra()
        {
            var currentEra = timeline.GetCurrentEra();
            if (currentEra != null)
            {
                // Find previous era
                var erasField = timeline.GetType().GetField("historicalEras",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (erasField != null)
                {
                    var eras = (Timeline.HistoricalEra[])erasField.GetValue(timeline);
                    for (int i = 1; i < eras.Length; i++)
                    {
                        if (eras[i] == currentEra)
                        {
                            timeline.SetCurrentEra(eras[i - 1].startYear);
                            UpdateTimelineUI();
                            break;
                        }
                    }
                }
            }
        }

        public void Show()
        {
            if (timelinePanel != null)
            {
                timelinePanel.SetActive(true);
                UpdateTimelineUI();
            }
        }

        public void Hide()
        {
            if (timelinePanel != null)
            {
                timelinePanel.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (timelinePanel != null)
            {
                timelinePanel.SetActive(!timelinePanel.activeSelf);
                if (timelinePanel.activeSelf)
                {
                    UpdateTimelineUI();
                }
            }
        }
    }
}
