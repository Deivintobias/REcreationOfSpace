using UnityEngine;

public class WorldStructure : MonoBehaviour
{
    public static WorldStructure Instance { get; private set; }

    [System.Serializable]
    public class WorldLayer
    {
        public string name;
        public float height;
        public bool isSionControlled;
        public string description;
    }

    [Header("World Structure")]
    public WorldLayer[] layers = new WorldLayer[]
    {
        new WorldLayer {
            name = "First Earth Crust",
            height = 0f,
            isSionControlled = true,
            description = "The surface layer of the first planet, eternally controlled by Sion"
        },
        new WorldLayer {
            name = "Paradise City Layer",
            height = -100f,
            isSionControlled = false,
            description = "The epicenter of the first planet, a sacred Sinai sanctuary"
        },
        new WorldLayer {
            name = "Deep Space",
            height = float.PositiveInfinity,
            isSionControlled = false,
            description = "The vast expanse of space, domain of Sinai"
        }
    };

    [Header("Resource Distribution")]
    public float sionResourceDensity = 1f;
    public float sinaiResourceDensity = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsSionTerritory(Vector3 position)
    {
        // Only the crust (surface) of the first planet is Sion territory
        // Everything else, including Paradise City in the epicenter, belongs to Sinai
        return position.y >= 0f && position.y <= 10f; // Assuming crust is 0-10 units high
    }

    public bool IsParadiseCity(Vector3 position)
    {
        // Paradise City is at the epicenter, a Sinai sanctuary
        return position.magnitude < 50f && position.y < 0f;
    }

    public bool IsSinaiTerritory(Vector3 position)
    {
        // All space belongs to Sinai except the crust
        return !IsSionTerritory(position);
    }

    public WorldLayer GetCurrentLayer(Vector3 position)
    {
        if (position.y >= 0f)
        {
            return layers[0]; // First Earth Crust (Sion)
        }
        else if (IsParadiseCity(position))
        {
            return layers[1]; // Paradise City Layer (Sinai)
        }
        else
        {
            return layers[2]; // Deep Space (Sinai)
        }
    }

    public float GetResourceDensity(Vector3 position)
    {
        WorldLayer layer = GetCurrentLayer(position);
        return layer.isSionControlled ? sionResourceDensity : sinaiResourceDensity;
    }

    public string GetLocationDescription(Vector3 position)
    {
        WorldLayer layer = GetCurrentLayer(position);
        if (IsParadiseCity(position))
        {
            return "Paradise City - Sinai's Sanctuary at the Epicenter";
        }
        else if (IsSionTerritory(position))
        {
            return "Sion's Crust - First Earth Surface";
        }
        else
        {
            return "Sinai Space - Beyond the Crust";
        }
    }

    public Color GetLayerColor(Vector3 position)
    {
        if (IsSionTerritory(position))
        {
            return new Color(0.8f, 0.4f, 0.4f); // Reddish for Sion's crust
        }
        else if (IsParadiseCity(position))
        {
            return new Color(0.4f, 0.6f, 1f) * 1.5f; // Bright blue for Sinai's Paradise City
        }
        else
        {
            return new Color(0.4f, 0.6f, 1f); // Blue for Sinai space
        }
    }

    void OnDrawGizmos()
    {
        // Draw world layer boundaries in editor
        Gizmos.color = Color.blue; // Blue for Sinai's Paradise City
        Gizmos.DrawWireSphere(Vector3.zero, 50f);

        Gizmos.color = Color.red; // Red for Sion's crust
        float crustHeight = 10f;
        Vector3 crustSize = new Vector3(1000f, crustHeight, 1000f);
        Gizmos.DrawWireCube(Vector3.up * (crustHeight * 0.5f), crustSize);

        // Draw Sinai space boundary
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Vector3.zero, 200f);
    }
}
