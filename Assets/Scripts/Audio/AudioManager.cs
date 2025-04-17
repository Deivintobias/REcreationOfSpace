using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

namespace REcreationOfSpace.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            public AudioMixerGroup mixerGroup;
            
            [Range(0f, 1f)]
            public float volume = 1f;
            [Range(0.1f, 3f)]
            public float pitch = 1f;
            [Range(0f, 1f)]
            public float spatialBlend = 0f;
            public float minDistance = 1f;
            public float maxDistance = 500f;
            
            public bool loop = false;
            public bool playOnAwake = false;
            public bool randomizePitch = false;
            [Range(0f, 0.5f)]
            public float pitchRandomRange = 0.1f;

            [HideInInspector]
            public AudioSource source;
        }

        [Header("Audio Settings")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private float crossFadeTime = 2f;
        [SerializeField] private float fadeInTime = 1f;
        [SerializeField] private float fadeOutTime = 1f;

        [Header("Sound Categories")]
        [SerializeField] private Sound[] musicTracks;
        [SerializeField] private Sound[] ambientSounds;
        [SerializeField] private Sound[] effectSounds;
        [SerializeField] private Sound[] uiSounds;
        [SerializeField] private Sound[] characterSounds;
        [SerializeField] private Sound[] divineSounds;

        [Header("Environment Settings")]
        [SerializeField] private float dayAmbienceVolume = 1f;
        [SerializeField] private float nightAmbienceVolume = 0.7f;
        [SerializeField] private float indoorReverb = 0.5f;
        [SerializeField] private float outdoorReverb = 0.1f;

        private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
        private Sound currentMusic;
        private Sound currentAmbience;
        private List<Sound> activeLoopingSounds = new List<Sound>();

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudio()
        {
            // Initialize all sound categories
            InitializeSoundCategory(musicTracks);
            InitializeSoundCategory(ambientSounds);
            InitializeSoundCategory(effectSounds);
            InitializeSoundCategory(uiSounds);
            InitializeSoundCategory(characterSounds);
            InitializeSoundCategory(divineSounds);
        }

        private void InitializeSoundCategory(Sound[] sounds)
        {
            foreach (var sound in sounds)
            {
                if (soundDictionary.ContainsKey(sound.name))
                {
                    Debug.LogWarning($"Duplicate sound name found: {sound.name}");
                    continue;
                }

                GameObject soundObject = new GameObject($"Sound_{sound.name}");
                soundObject.transform.SetParent(transform);
                
                AudioSource source = soundObject.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.outputAudioMixerGroup = sound.mixerGroup;
                source.volume = sound.volume;
                source.pitch = sound.pitch;
                source.spatialBlend = sound.spatialBlend;
                source.minDistance = sound.minDistance;
                source.maxDistance = sound.maxDistance;
                source.loop = sound.loop;
                source.playOnAwake = sound.playOnAwake;

                sound.source = source;
                soundDictionary.Add(sound.name, sound);

                if (sound.playOnAwake)
                {
                    if (sound.loop)
                    {
                        activeLoopingSounds.Add(sound);
                    }
                    source.Play();
                }
            }
        }

        public void PlayMusic(string name, bool crossFade = true)
        {
            if (!soundDictionary.TryGetValue(name, out Sound newMusic))
            {
                Debug.LogWarning($"Music track not found: {name}");
                return;
            }

            if (currentMusic != null && crossFade)
            {
                StartCoroutine(CrossFadeMusic(currentMusic, newMusic));
            }
            else
            {
                if (currentMusic != null)
                {
                    currentMusic.source.Stop();
                }
                newMusic.source.Play();
                currentMusic = newMusic;
            }
        }

        public void PlayAmbience(string name, bool crossFade = true)
        {
            if (!soundDictionary.TryGetValue(name, out Sound newAmbience))
            {
                Debug.LogWarning($"Ambience track not found: {name}");
                return;
            }

            if (currentAmbience != null && crossFade)
            {
                StartCoroutine(CrossFadeAmbience(currentAmbience, newAmbience));
            }
            else
            {
                if (currentAmbience != null)
                {
                    currentAmbience.source.Stop();
                }
                newAmbience.source.Play();
                currentAmbience = newAmbience;
            }
        }

        public void PlaySound(string name, Vector3? position = null)
        {
            if (!soundDictionary.TryGetValue(name, out Sound sound))
            {
                Debug.LogWarning($"Sound not found: {name}");
                return;
            }

            if (position.HasValue)
            {
                sound.source.transform.position = position.Value;
            }

            if (sound.randomizePitch)
            {
                sound.source.pitch = sound.pitch + UnityEngine.Random.Range(-sound.pitchRandomRange, sound.pitchRandomRange);
            }

            sound.source.Play();
        }

        public void StopSound(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Stop();
            }
        }

        public void PauseSound(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Pause();
            }
        }

        public void ResumeSound(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.UnPause();
            }
        }

        public void SetVolume(string mixerParameter, float volume)
        {
            mainMixer.SetFloat(mixerParameter, Mathf.Log10(volume) * 20);
        }

        public void SetDayNightAmbience(bool isNight)
        {
            float targetVolume = isNight ? nightAmbienceVolume : dayAmbienceVolume;
            if (currentAmbience != null)
            {
                StartCoroutine(FadeVolume(currentAmbience.source, targetVolume, crossFadeTime));
            }
        }

        public void SetEnvironmentReverb(bool isIndoor)
        {
            float reverbAmount = isIndoor ? indoorReverb : outdoorReverb;
            mainMixer.SetFloat("ReverbAmount", reverbAmount);
        }

        private System.Collections.IEnumerator CrossFadeMusic(Sound oldMusic, Sound newMusic)
        {
            float timeElapsed = 0;
            newMusic.source.volume = 0;
            newMusic.source.Play();

            while (timeElapsed < crossFadeTime)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / crossFadeTime;

                oldMusic.source.volume = Mathf.Lerp(oldMusic.volume, 0, t);
                newMusic.source.volume = Mathf.Lerp(0, newMusic.volume, t);

                yield return null;
            }

            oldMusic.source.Stop();
            oldMusic.source.volume = oldMusic.volume;
            currentMusic = newMusic;
        }

        private System.Collections.IEnumerator CrossFadeAmbience(Sound oldAmbience, Sound newAmbience)
        {
            float timeElapsed = 0;
            newAmbience.source.volume = 0;
            newAmbience.source.Play();

            while (timeElapsed < crossFadeTime)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / crossFadeTime;

                oldAmbience.source.volume = Mathf.Lerp(oldAmbience.volume, 0, t);
                newAmbience.source.volume = Mathf.Lerp(0, newAmbience.volume, t);

                yield return null;
            }

            oldAmbience.source.Stop();
            oldAmbience.source.volume = oldAmbience.volume;
            currentAmbience = newAmbience;
        }

        private System.Collections.IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
        {
            float startVolume = source.volume;
            float timeElapsed = 0;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / duration;
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            source.volume = targetVolume;
        }

        private void OnDestroy()
        {
            foreach (var sound in activeLoopingSounds)
            {
                if (sound.source != null)
                {
                    sound.source.Stop();
                }
            }
        }
    }
}
