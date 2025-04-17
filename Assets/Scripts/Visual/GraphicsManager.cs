using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace REcreationOfSpace.Visual
{
    public class GraphicsManager : MonoBehaviour
    {
        [System.Serializable]
        public class EraVisualSettings
        {
            public string eraName;
            public Color skyColor;
            public Color sunColor;
            public float sunIntensity;
            public float fogDensity;
            public Color fogColor;
            public float bloomIntensity;
            public float vignetteIntensity;
            public Color ambientColor;
        }

        [System.Serializable]
        public class BiomeVisualSettings
        {
            public string biomeName;
            public Color groundTint;
            public float vegetationDensity;
            public float windStrength;
            public ParticleSystem[] atmosphericEffects;
            public AudioClip[] ambientSounds;
        }

        [Header("Visual Settings")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private Light mainLight;
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private EraVisualSettings[] eraSettings;
        [SerializeField] private BiomeVisualSettings[] biomeSettings;

        [Header("Effects")]
        [SerializeField] private ParticleSystem rainSystem;
        [SerializeField] private ParticleSystem snowSystem;
        [SerializeField] private ParticleSystem dustSystem;
        [SerializeField] private ParticleSystem[] divineEffects;

        private Timeline timeline;
        private BiomeManager biomeManager;
        private AudioSource ambientAudioSource;

        private void Start()
        {
            timeline = FindObjectOfType<Timeline>();
            biomeManager = FindObjectOfType<BiomeManager>();
            
            SetupComponents();
            InitializeDefaultSettings();
        }

        private void SetupComponents()
        {
            // Create post-processing volume if not assigned
            if (postProcessVolume == null)
            {
                var volumeObject = new GameObject("Post Process Volume");
                postProcessVolume = volumeObject.AddComponent<Volume>();
                volumeObject.transform.SetParent(transform);

                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                postProcessVolume.profile = profile;

                // Add post-processing effects
                profile.Add<Bloom>();
                profile.Add<Vignette>();
                profile.Add<ColorAdjustments>();
                profile.Add<DepthOfField>();
            }

            // Create main light if not assigned
            if (mainLight == null)
            {
                var lightObject = new GameObject("Main Directional Light");
                mainLight = lightObject.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                lightObject.transform.SetParent(transform);
            }

            // Setup ambient audio
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.spatialBlend = 0f; // 2D sound
                ambientAudioSource.priority = 0; // Highest priority
            }
        }

        private void InitializeDefaultSettings()
        {
            if (eraSettings == null || eraSettings.Length == 0)
            {
                eraSettings = new EraVisualSettings[]
                {
                    // Creation Era
                    new EraVisualSettings {
                        eraName = "Creation",
                        skyColor = new Color(0.7f, 0.9f, 1f),
                        sunColor = Color.white,
                        sunIntensity = 2f,
                        fogDensity = 0.1f,
                        fogColor = new Color(0.8f, 0.9f, 1f),
                        bloomIntensity = 1.5f,
                        vignetteIntensity = 0.2f,
                        ambientColor = new Color(0.6f, 0.7f, 1f)
                    },

                    // Great Flood Era
                    new EraVisualSettings {
                        eraName = "Great Flood",
                        skyColor = new Color(0.4f, 0.4f, 0.6f),
                        sunColor = new Color(0.8f, 0.8f, 0.9f),
                        sunIntensity = 0.5f,
                        fogDensity = 0.8f,
                        fogColor = new Color(0.5f, 0.5f, 0.7f),
                        bloomIntensity = 0.5f,
                        vignetteIntensity = 0.4f,
                        ambientColor = new Color(0.4f, 0.4f, 0.6f)
                    },

                    // Modern Era
                    new EraVisualSettings {
                        eraName = "Modern Era",
                        skyColor = new Color(0.6f, 0.8f, 1f),
                        sunColor = Color.white,
                        sunIntensity = 1f,
                        fogDensity = 0.2f,
                        fogColor = new Color(0.7f, 0.8f, 0.9f),
                        bloomIntensity = 0.8f,
                        vignetteIntensity = 0.3f,
                        ambientColor = new Color(0.5f, 0.6f, 0.7f)
                    }
                };
            }

            if (biomeSettings == null || biomeSettings.Length == 0)
            {
                biomeSettings = new BiomeVisualSettings[]
                {
                    new BiomeVisualSettings {
                        biomeName = "Desert",
                        groundTint = new Color(0.95f, 0.9f, 0.7f),
                        vegetationDensity = 0.1f,
                        windStrength = 1.5f
                    },
                    new BiomeVisualSettings {
                        biomeName = "Forest",
                        groundTint = new Color(0.3f, 0.5f, 0.2f),
                        vegetationDensity = 0.9f,
                        windStrength = 0.8f
                    },
                    // Add more biome settings...
                };
            }
        }

        public void UpdateVisuals(string eraName, string biomeName)
        {
            var era = System.Array.Find(eraSettings, e => e.eraName == eraName);
            var biome = System.Array.Find(biomeSettings, b => b.biomeName == biomeName);

            if (era != null)
            {
                ApplyEraSettings(era);
            }

            if (biome != null)
            {
                ApplyBiomeSettings(biome);
            }
        }

        private void ApplyEraSettings(EraVisualSettings settings)
        {
            // Update skybox
            if (skyboxMaterial != null)
            {
                skyboxMaterial.SetColor("_SkyTint", settings.skyColor);
            }

            // Update sun
            if (mainLight != null)
            {
                mainLight.color = settings.sunColor;
                mainLight.intensity = settings.sunIntensity;
            }

            // Update post-processing
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                if (postProcessVolume.profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.intensity.value = settings.bloomIntensity;
                }

                if (postProcessVolume.profile.TryGet<Vignette>(out var vignette))
                {
                    vignette.intensity.value = settings.vignetteIntensity;
                }

                if (postProcessVolume.profile.TryGet<ColorAdjustments>(out var colorAdjust))
                {
                    colorAdjust.colorFilter.value = settings.ambientColor;
                }
            }

            // Update fog
            RenderSettings.fogDensity = settings.fogDensity;
            RenderSettings.fogColor = settings.fogColor;
        }

        private void ApplyBiomeSettings(BiomeVisualSettings settings)
        {
            // Update terrain material
            Shader.SetGlobalColor("_GroundTint", settings.groundTint);
            Shader.SetGlobalFloat("_VegetationDensity", settings.vegetationDensity);
            Shader.SetGlobalFloat("_WindStrength", settings.windStrength);

            // Update atmospheric effects
            UpdateAtmosphericEffects(settings);

            // Update ambient sounds
            UpdateAmbientSounds(settings);
        }

        private void UpdateAtmosphericEffects(BiomeVisualSettings settings)
        {
            if (settings.atmosphericEffects != null)
            {
                foreach (var effect in settings.atmosphericEffects)
                {
                    if (effect != null)
                    {
                        var instance = Instantiate(effect, transform);
                        instance.Play();
                    }
                }
            }
        }

        private void UpdateAmbientSounds(BiomeVisualSettings settings)
        {
            if (settings.ambientSounds != null && settings.ambientSounds.Length > 0 && ambientAudioSource != null)
            {
                var randomSound = settings.ambientSounds[Random.Range(0, settings.ambientSounds.Length)];
                if (randomSound != null)
                {
                    ambientAudioSource.clip = randomSound;
                    ambientAudioSource.Play();
                }
            }
        }

        public void PlayDivineEffect(Vector3 position)
        {
            if (divineEffects != null && divineEffects.Length > 0)
            {
                var effect = divineEffects[Random.Range(0, divineEffects.Length)];
                if (effect != null)
                {
                    var instance = Instantiate(effect, position, Quaternion.identity);
                    instance.Play();
                    Destroy(instance.gameObject, effect.main.duration);
                }
            }
        }

        public void SetWeatherEffect(string type, bool enable)
        {
            switch (type.ToLower())
            {
                case "rain":
                    if (rainSystem != null)
                    {
                        if (enable) rainSystem.Play();
                        else rainSystem.Stop();
                    }
                    break;
                case "snow":
                    if (snowSystem != null)
                    {
                        if (enable) snowSystem.Play();
                        else snowSystem.Stop();
                    }
                    break;
                case "dust":
                    if (dustSystem != null)
                    {
                        if (enable) dustSystem.Play();
                        else dustSystem.Stop();
                    }
                    break;
            }
        }
    }
}
