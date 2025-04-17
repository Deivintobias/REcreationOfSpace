using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Quest;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class DailyLifeUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Job Info")]
        [SerializeField] private TextMeshProUGUI jobTitleText;
        [SerializeField] private TextMeshProUGUI salaryText;
        [SerializeField] private TextMeshProUGUI hoursWorkedText;
        [SerializeField] private TextMeshProUGUI experienceText;
        [SerializeField] private Button startWorkButton;
        [SerializeField] private Button stopWorkButton;

        [Header("Money Info")]
        [SerializeField] private TextMeshProUGUI currentMoneyText;
        [SerializeField] private TextMeshProUGUI dailyExpensesText;
        [SerializeField] private TextMeshProUGUI weeklyIncomeText;
        [SerializeField] private Image moneyTrendArrow;
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;

        [Header("Bills Panel")]
        [SerializeField] private RectTransform billsContainer;
        [SerializeField] private GameObject billPrefab;
        [SerializeField] private TextMeshProUGUI totalBillsText;
        [SerializeField] private Button payAllBillsButton;

        [Header("Skills Panel")]
        [SerializeField] private RectTransform skillsContainer;
        [SerializeField] private GameObject skillBarPrefab;
        [SerializeField] private float skillBarUpdateSpeed = 5f;

        [Header("Job Market")]
        [SerializeField] private GameObject jobListingPrefab;
        [SerializeField] private RectTransform jobListingsContainer;
        [SerializeField] private Button refreshJobsButton;
        [SerializeField] private TextMeshProUGUI jobSearchStatus;

        private DailyLifeSystem dailyLifeSystem;
        private Dictionary<string, Slider> skillBars = new Dictionary<string, Slider>();
        private Dictionary<string, GameObject> billItems = new Dictionary<string, GameObject>();
        private float lastMoneyAmount;
        private float updateTimer;

        private void Start()
        {
            dailyLifeSystem = FindObjectOfType<DailyLifeSystem>();
            if (dailyLifeSystem == null)
            {
                Debug.LogError("No DailyLifeSystem found!");
                return;
            }

            InitializeUI();
            UpdateUI();
            mainPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }

            if (mainPanel.activeSelf)
            {
                updateTimer += Time.deltaTime;
                if (updateTimer >= 0.5f)
                {
                    UpdateUI();
                    updateTimer = 0f;
                }
            }
        }

        private void InitializeUI()
        {
            if (startWorkButton != null)
                startWorkButton.onClick.AddListener(OnStartWorkClicked);
            
            if (stopWorkButton != null)
                stopWorkButton.onClick.AddListener(OnStopWorkClicked);
            
            if (payAllBillsButton != null)
                payAllBillsButton.onClick.AddListener(OnPayAllBillsClicked);
            
            if (refreshJobsButton != null)
                refreshJobsButton.onClick.AddListener(RefreshJobListings);

            lastMoneyAmount = dailyLifeSystem.GetMoney();
        }

        private void UpdateUI()
        {
            UpdateJobInfo();
            UpdateMoneyInfo();
            UpdateBills();
            UpdateSkills();
        }

        private void UpdateJobInfo()
        {
            var currentJob = dailyLifeSystem.GetCurrentJob();
            if (currentJob != null)
            {
                jobTitleText.text = currentJob.title;
                salaryText.text = $"${currentJob.baseSalary}/day";
                hoursWorkedText.text = $"Hours: {dailyLifeSystem.GetHoursWorkedThisWeek():F1}/40";
                experienceText.text = $"Experience: {dailyLifeSystem.GetWorkExperience():F1}";

                startWorkButton.gameObject.SetActive(!dailyLifeSystem.IsWorking());
                stopWorkButton.gameObject.SetActive(dailyLifeSystem.IsWorking());
            }
            else
            {
                jobTitleText.text = "Unemployed";
                salaryText.text = "--";
                hoursWorkedText.text = "--";
                experienceText.text = "--";

                startWorkButton.gameObject.SetActive(false);
                stopWorkButton.gameObject.SetActive(false);
            }
        }

        private void UpdateMoneyInfo()
        {
            float currentMoney = dailyLifeSystem.GetMoney();
            currentMoneyText.text = $"${currentMoney:F2}";

            // Update money trend arrow
            if (moneyTrendArrow != null)
            {
                float moneyDelta = currentMoney - lastMoneyAmount;
                moneyTrendArrow.color = moneyDelta >= 0 ? positiveColor : negativeColor;
                moneyTrendArrow.transform.rotation = Quaternion.Euler(0, 0, moneyDelta >= 0 ? 0 : 180);
            }

            lastMoneyAmount = currentMoney;
        }

        private void UpdateBills()
        {
            var bills = dailyLifeSystem.GetCurrentBills();
            float totalBills = 0f;

            // Remove old bill items
            foreach (var billItem in billItems.Values)
            {
                Destroy(billItem);
            }
            billItems.Clear();

            // Create new bill items
            foreach (var bill in bills)
            {
                GameObject billItem = Instantiate(billPrefab, billsContainer);
                billItems[bill.name] = billItem;

                var nameText = billItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var amountText = billItem.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
                var dueText = billItem.transform.Find("DueText")?.GetComponent<TextMeshProUGUI>();
                var payButton = billItem.transform.Find("PayButton")?.GetComponent<Button>();

                if (nameText != null) nameText.text = bill.name;
                if (amountText != null) amountText.text = $"${bill.amount:F2}";
                if (dueText != null) dueText.text = $"Due in {Mathf.Ceil(bill.dueInDays)} days";
                if (payButton != null)
                {
                    payButton.interactable = !bill.isPaid && dailyLifeSystem.GetMoney() >= bill.amount;
                    payButton.onClick.AddListener(() => OnPayBillClicked(bill));
                }

                totalBills += bill.amount;
            }

            if (totalBillsText != null)
            {
                totalBillsText.text = $"Total Bills: ${totalBills:F2}";
            }
        }

        private void UpdateSkills()
        {
            foreach (var skill in dailyLifeSystem.GetWorkSkills())
            {
                if (!skillBars.ContainsKey(skill))
                {
                    // Create new skill bar
                    GameObject skillBarObj = Instantiate(skillBarPrefab, skillsContainer);
                    var nameText = skillBarObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                    var levelText = skillBarObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
                    var slider = skillBarObj.GetComponentInChildren<Slider>();

                    if (nameText != null) nameText.text = skill;
                    skillBars[skill] = slider;
                }

                // Update skill bar
                float currentLevel = dailyLifeSystem.GetSkillLevel(skill);
                Slider skillBar = skillBars[skill];
                skillBar.value = Mathf.Lerp(skillBar.value, currentLevel, Time.deltaTime * skillBarUpdateSpeed);

                var levelText = skillBar.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
                if (levelText != null) levelText.text = $"Level {currentLevel:F1}";
            }
        }

        private void RefreshJobListings()
        {
            // Clear existing listings
            foreach (Transform child in jobListingsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new listings
            foreach (var job in dailyLifeSystem.GetAvailableJobs())
            {
                GameObject listing = Instantiate(jobListingPrefab, jobListingsContainer);
                
                var titleText = listing.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
                var descText = listing.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
                var salaryText = listing.transform.Find("SalaryText")?.GetComponent<TextMeshProUGUI>();
                var requirementsText = listing.transform.Find("RequirementsText")?.GetComponent<TextMeshProUGUI>();
                var applyButton = listing.GetComponentInChildren<Button>();

                if (titleText != null) titleText.text = job.title;
                if (descText != null) descText.text = job.description;
                if (salaryText != null) salaryText.text = $"${job.baseSalary}/day";
                if (requirementsText != null)
                {
                    string reqs = $"Level {job.requiredLevel}+\n";
                    reqs += string.Join(", ", job.requiredSkills);
                    requirementsText.text = reqs;
                }

                if (applyButton != null)
                {
                    applyButton.onClick.AddListener(() => OnApplyForJob(job.title));
                }
            }
        }

        private void OnStartWorkClicked()
        {
            dailyLifeSystem.StartWork();
            UpdateUI();
        }

        private void OnStopWorkClicked()
        {
            dailyLifeSystem.StopWork();
            UpdateUI();
        }

        private void OnPayBillClicked(DailyLifeSystem.Bill bill)
        {
            if (dailyLifeSystem.SpendMoney(bill.amount))
            {
                bill.isPaid = true;
                UpdateUI();
            }
        }

        private void OnPayAllBillsClicked()
        {
            float totalUnpaid = 0f;
            foreach (var bill in dailyLifeSystem.GetCurrentBills())
            {
                if (!bill.isPaid)
                {
                    totalUnpaid += bill.amount;
                }
            }

            if (dailyLifeSystem.SpendMoney(totalUnpaid))
            {
                foreach (var bill in dailyLifeSystem.GetCurrentBills())
                {
                    bill.isPaid = true;
                }
                UpdateUI();
            }
        }

        private void OnApplyForJob(string jobTitle)
        {
            if (dailyLifeSystem.TryGetJob(jobTitle))
            {
                jobSearchStatus.text = "Successfully got the job!";
                jobSearchStatus.color = positiveColor;
            }
            else
            {
                jobSearchStatus.text = "Did not meet job requirements.";
                jobSearchStatus.color = negativeColor;
            }

            UpdateUI();
        }

        public void TogglePanel()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            if (mainPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (startWorkButton != null)
                startWorkButton.onClick.RemoveListener(OnStartWorkClicked);
            
            if (stopWorkButton != null)
                stopWorkButton.onClick.RemoveListener(OnStopWorkClicked);
            
            if (payAllBillsButton != null)
                payAllBillsButton.onClick.RemoveListener(OnPayAllBillsClicked);
            
            if (refreshJobsButton != null)
                refreshJobsButton.onClick.RemoveListener(RefreshJobListings);
        }
    }
}
