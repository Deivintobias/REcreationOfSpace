using UnityEngine;
using System.Collections.Generic;
using REcreationOfSpace.Character;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Quest
{
    public class DailyLifeSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Job
        {
            public string title;
            public string description;
            public float baseSalary;
            public float experienceMultiplier;
            public int requiredLevel;
            public string[] requiredSkills;
            public float workHoursPerDay;
            public Vector3 workLocation;
            public bool requiresWorkbench;
            public string workbenchType;
        }

        [System.Serializable]
        public class Bill
        {
            public string name;
            public float amount;
            public float dueInDays;
            public bool isRecurring;
            public float recurringDays;
            public string category;
            public bool isPaid;
        }

        [Header("Jobs")]
        [SerializeField] private Job[] availableJobs;
        [SerializeField] private float overtimeMultiplier = 1.5f;
        [SerializeField] private float nightShiftBonus = 1.2f;
        [SerializeField] private float weekendBonus = 1.3f;
        [SerializeField] private int maxWorkHoursPerWeek = 40;

        [Header("Economy")]
        [SerializeField] private float startingMoney = 1000f;
        [SerializeField] private float taxRate = 0.2f;
        [SerializeField] private float inflationRate = 0.05f;
        [SerializeField] private float interestRate = 0.03f;
        [SerializeField] private float loanInterestRate = 0.08f;

        [Header("Living Expenses")]
        [SerializeField] private Bill[] recurringBills;
        [SerializeField] private float foodCostPerDay = 20f;
        [SerializeField] private float transportCostPerDay = 10f;
        [SerializeField] private float entertainmentCostPerDay = 15f;

        [Header("Skills")]
        [SerializeField] private string[] workSkills;
        [SerializeField] private float skillGainRate = 0.1f;
        [SerializeField] private float skillDecayRate = 0.01f;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;

        private Dictionary<string, float> skills = new Dictionary<string, float>();
        private List<Bill> currentBills = new List<Bill>();
        private Job currentJob;
        private float currentMoney;
        private float workExperience;
        private float hoursWorkedThisWeek;
        private bool isWorking;

        private LifeCycleSystem lifeCycleSystem;
        private DayNightSystem dayNightSystem;

        private void Start()
        {
            lifeCycleSystem = FindObjectOfType<LifeCycleSystem>();
            dayNightSystem = FindObjectOfType<DayNightSystem>();

            if (lifeCycleSystem == null || dayNightSystem == null)
            {
                Debug.LogError("Required systems not found!");
                return;
            }

            InitializeSkills();
            InitializeBills();
            currentMoney = startingMoney;
        }

        private void Update()
        {
            if (isWorking)
            {
                UpdateWork();
            }

            UpdateBills();
            UpdateSkills();
        }

        private void InitializeSkills()
        {
            foreach (string skill in workSkills)
            {
                skills[skill] = 0f;
            }
        }

        private void InitializeBills()
        {
            currentBills.Clear();
            foreach (Bill bill in recurringBills)
            {
                currentBills.Add(new Bill
                {
                    name = bill.name,
                    amount = bill.amount,
                    dueInDays = bill.dueInDays,
                    isRecurring = bill.isRecurring,
                    recurringDays = bill.recurringDays,
                    category = bill.category,
                    isPaid = false
                });
            }
        }

        public bool TryGetJob(string jobTitle)
        {
            Job job = System.Array.Find(availableJobs, j => j.title == jobTitle);
            if (job == null) return false;

            // Check requirements
            if (workExperience < job.requiredLevel) return false;
            foreach (string skill in job.requiredSkills)
            {
                if (!skills.ContainsKey(skill) || skills[skill] < 1f) return false;
            }

            currentJob = job;
            guiderMessage.ShowMessage($"Congratulations! You got the job as {job.title}!", Color.green);
            return true;
        }

        public void StartWork()
        {
            if (currentJob == null || isWorking) return;

            if (hoursWorkedThisWeek >= maxWorkHoursPerWeek)
            {
                guiderMessage.ShowMessage("You've reached the maximum work hours for this week.", Color.red);
                return;
            }

            if (currentJob.requiresWorkbench)
            {
                // Check if at workbench
                // This would integrate with your workbench system
            }

            isWorking = true;
            guiderMessage.ShowMessage($"Started working as {currentJob.title}.", Color.white);
        }

        public void StopWork()
        {
            if (!isWorking) return;

            isWorking = false;
            guiderMessage.ShowMessage("Finished work for now.", Color.white);
        }

        private void UpdateWork()
        {
            float timeWorked = Time.deltaTime / (dayNightSystem.GetDayLength() / 24f); // Convert to in-game hours
            hoursWorkedThisWeek += timeWorked;

            // Calculate pay
            float hourlyPay = currentJob.baseSalary / 8f; // Assuming 8-hour workday
            float bonusMultiplier = 1f;

            // Apply bonuses
            if (hoursWorkedThisWeek > 40) bonusMultiplier *= overtimeMultiplier;
            if (dayNightSystem.IsNight()) bonusMultiplier *= nightShiftBonus;
            if (IsWeekend()) bonusMultiplier *= weekendBonus;

            float pay = hourlyPay * timeWorked * bonusMultiplier * (1f + workExperience * currentJob.experienceMultiplier);
            currentMoney += pay * (1f - taxRate);

            // Gain experience and skills
            workExperience += timeWorked * 0.01f;
            foreach (string skill in currentJob.requiredSkills)
            {
                if (skills.ContainsKey(skill))
                {
                    skills[skill] += timeWorked * skillGainRate;
                }
            }
        }

        private void UpdateBills()
        {
            foreach (Bill bill in currentBills)
            {
                bill.dueInDays -= Time.deltaTime / dayNightSystem.GetDayLength();
                
                if (bill.dueInDays <= 0 && !bill.isPaid)
                {
                    if (currentMoney >= bill.amount)
                    {
                        PayBill(bill);
                    }
                    else
                    {
                        // Handle missed payment
                        guiderMessage.ShowMessage($"Missed payment for {bill.name}!", Color.red);
                    }

                    if (bill.isRecurring)
                    {
                        bill.dueInDays = bill.recurringDays;
                        bill.isPaid = false;
                    }
                }
            }

            // Daily expenses
            float dailyExpenses = (foodCostPerDay + transportCostPerDay + entertainmentCostPerDay) 
                                * (Time.deltaTime / dayNightSystem.GetDayLength());
            currentMoney -= dailyExpenses;
        }

        private void UpdateSkills()
        {
            foreach (var skill in skills.Keys)
            {
                if (!IsUsingSkill(skill))
                {
                    skills[skill] = Mathf.Max(0f, skills[skill] - skillDecayRate * Time.deltaTime);
                }
            }
        }

        private void PayBill(Bill bill)
        {
            currentMoney -= bill.amount;
            bill.isPaid = true;
            guiderMessage.ShowMessage($"Paid {bill.name}: ${bill.amount}", Color.yellow);
        }

        private bool IsUsingSkill(string skill)
        {
            return isWorking && currentJob != null && System.Array.Exists(currentJob.requiredSkills, s => s == skill);
        }

        private bool IsWeekend()
        {
            float totalDays = dayNightSystem.GetTotalDays();
            return (int)totalDays % 7 >= 5;
        }

        public float GetMoney() => currentMoney;
        public float GetWorkExperience() => workExperience;
        public float GetSkillLevel(string skill) => skills.ContainsKey(skill) ? skills[skill] : 0f;
        public Job GetCurrentJob() => currentJob;
        public float GetHoursWorkedThisWeek() => hoursWorkedThisWeek;
        public List<Bill> GetCurrentBills() => currentBills;

        public void ResetWorkWeek()
        {
            hoursWorkedThisWeek = 0f;
        }

        public void AddBill(Bill bill)
        {
            currentBills.Add(bill);
        }

        public void RemoveBill(string billName)
        {
            currentBills.RemoveAll(b => b.name == billName);
        }

        public void AddMoney(float amount)
        {
            currentMoney += amount;
            if (amount > 0)
                guiderMessage.ShowMessage($"Received ${amount}", Color.green);
        }

        public bool SpendMoney(float amount)
        {
            if (currentMoney >= amount)
            {
                currentMoney -= amount;
                guiderMessage.ShowMessage($"Spent ${amount}", Color.yellow);
                return true;
            }
            return false;
        }
    }
}
