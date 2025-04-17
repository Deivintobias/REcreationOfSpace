using UnityEngine;
using System.Collections.Generic;

public class FirstGuideTexts : MonoBehaviour
{
    [System.Serializable]
    public class GuideBook
    {
        public string bookTitle;
        public string[] textFragments;
        public Vector3 location;
        public bool isHidden;
        public float insightValue;
    }

    [Header("Book Settings")]
    public List<GuideBook> guideBooks = new List<GuideBook>()
    {
        new GuideBook {
            bookTitle = "Ancient Revelations",
            textFragments = new string[] {
                "The First Guide spoke of paths beyond Sion's boundaries",
                "Truth exists even when voices are silenced",
                "Look beyond what is shown to what is hidden"
            },
            insightValue = 15f
        },
        new GuideBook {
            bookTitle = "Forgotten Teachings",
            textFragments = new string[] {
                "The Guide's wisdom persists in written form",
                "Some truths cannot be erased, only concealed",
                "Knowledge survives despite attempts to hide it"
            },
            insightValue = 20f
        },
        new GuideBook {
            bookTitle = "Hidden Wisdom",
            textFragments = new string[] {
                "The First Guide's words remain for those who seek",
                "In silence, truth still speaks through these pages",
                "What is written cannot be unwritten"
            },
            insightValue = 25f
        }
    };

    [Header("Interaction Settings")]
    public float bookDetectionRange = 5f;
    public float readingTime = 3f;
    public LayerMask bookLayer;
    public ParticleSystem bookGlowEffect;

    private Dictionary<string, bool> discoveredBooks = new Dictionary<string, bool>();
    private bool isReading = false;

    private void Start()
    {
        SpawnBooks();
    }

    private void SpawnBooks()
    {
        foreach (var book in guideBooks)
        {
            // Create book object
            GameObject bookObject = new GameObject($"GuideBook_{book.bookTitle}");
            bookObject.layer = LayerMask.NameToLayer("Book");

            // Add visual representation
            CreateBookVisual(bookObject);

            // Add interaction trigger
            SphereCollider trigger = bookObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1f;

            // Position book
            bookObject.transform.position = GetRandomBookLocation();

            // Track discovery status
            discoveredBooks[book.bookTitle] = false;
        }
    }

    private void CreateBookVisual(GameObject bookObject)
    {
        // Create basic book mesh
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.parent = bookObject.transform;
        visual.transform.localScale = new Vector3(0.3f, 0.4f, 0.1f);

        // Add subtle glow
        if (bookGlowEffect != null)
        {
            ParticleSystem glow = Instantiate(bookGlowEffect, bookObject.transform);
            glow.transform.localPosition = Vector3.zero;
        }
    }

    private Vector3 GetRandomBookLocation()
    {
        // Find valid position in Sion area
        float radius = 50f;
        Vector3 randomPos = Random.insideUnitSphere * radius;
        randomPos.y = 0f;

        // Raycast to find surface
        RaycastHit hit;
        if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out hit))
        {
            return hit.point + Vector3.up * 0.5f;
        }

        return randomPos;
    }

    public void OnBookFound(string bookTitle, SinaiCharacter finder)
    {
        if (discoveredBooks[bookTitle] || isReading)
            return;

        StartCoroutine(ReadBookSequence(bookTitle, finder));
    }

    private System.Collections.IEnumerator ReadBookSequence(string bookTitle, SinaiCharacter reader)
    {
        isReading = true;

        GuideBook book = guideBooks.Find(b => b.bookTitle == bookTitle);
        if (book == null)
        {
            isReading = false;
            yield break;
        }

        // Initial discovery message
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(
                $"You've found {book.bookTitle}, containing the First Guide's teachings..."
            );
        }

        yield return new WaitForSeconds(2f);

        // Read each fragment
        foreach (string fragment in book.textFragments)
        {
            // Check for Sion interference
            if (IsSionCharacterNearby(reader.transform.position))
            {
                // Interference message
                if (GuiderMessageUI.Instance != null)
                {
                    GuiderMessageUI.Instance.ShowMessage(
                        "A Sion presence prevents you from reading further..."
                    );
                }
                break;
            }

            // Show text fragment
            if (GuiderMessageUI.Instance != null)
            {
                GuiderMessageUI.Instance.ShowMessage($"First Guide's Text: {fragment}");
            }

            // Grant insight
            SionObserver observer = reader.GetComponent<SionObserver>();
            if (observer != null)
            {
                observer.OnInsightGained("Hidden Knowledge", fragment, book.insightValue);
            }

            yield return new WaitForSeconds(readingTime);
        }

        // Mark book as discovered
        discoveredBooks[bookTitle] = true;
        isReading = false;
    }

    private bool IsSionCharacterNearby(Vector3 position)
    {
        // Check for Sion characters that might interfere
        Collider[] nearby = Physics.OverlapSphere(position, 10f, LayerMask.GetMask("SionCharacter"));
        return nearby.Length > 0;
    }

    public void OnSionInterference(Vector3 position)
    {
        // Create interference effect
        GameObject effect = new GameObject("InterferenceEffect");
        effect.transform.position = position;
        
        // Add particle system
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        main.startSize = 1f;
        main.duration = 2f;
        main.loop = false;
        
        // Destroy after effect
        Destroy(effect, main.duration);
    }

    public bool HasFoundAllBooks()
    {
        foreach (var discovered in discoveredBooks.Values)
        {
            if (!discovered) return false;
        }
        return true;
    }

    public float GetDiscoveryProgress()
    {
        int found = 0;
        foreach (var discovered in discoveredBooks.Values)
        {
            if (discovered) found++;
        }
        return (float)found / discoveredBooks.Count;
    }

    private void OnDrawGizmos()
    {
        // Visualize book locations
        Gizmos.color = Color.yellow;
        foreach (var book in guideBooks)
        {
            Gizmos.DrawWireSphere(book.location, 1f);
        }
    }
}
