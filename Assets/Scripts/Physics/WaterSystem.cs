using UnityEngine;
using System.Collections.Generic;

namespace REcreationOfSpace.Physics
{
    public class WaterSystem : MonoBehaviour
    {
        [Header("Water Properties")]
        [SerializeField] private float density = 1f;
        [SerializeField] private float viscosity = 0.5f;
        [SerializeField] private float surfaceTension = 0.1f;
        [SerializeField] private float waveHeight = 0.5f;
        [SerializeField] private float waveSpeed = 1f;
        [SerializeField] private float rippleStrength = 0.2f;
        [SerializeField] private float flowSpeed = 1f;

        [Header("Visual Settings")]
        [SerializeField] private Material waterMaterial;
        [SerializeField] private float transparency = 0.8f;
        [SerializeField] private Color shallowColor = new Color(0.3f, 0.5f, 0.7f);
        [SerializeField] private Color deepColor = new Color(0.1f, 0.2f, 0.4f);
        [SerializeField] private float colorTransitionDepth = 5f;
        [SerializeField] private float reflectivity = 0.5f;

        [Header("Interaction")]
        [SerializeField] private float splashForce = 10f;
        [SerializeField] private float buoyancyForce = 15f;
        [SerializeField] private float dragMultiplier = 1f;
        [SerializeField] private ParticleSystem splashEffect;
        [SerializeField] private ParticleSystem rippleEffect;

        private List<WaterBody> waterBodies = new List<WaterBody>();
        private Dictionary<Collider, Rigidbody> floatingObjects = new Dictionary<Collider, Rigidbody>();
        private Vector3[] waveVertices;
        private float[] waveOffsets;
        private float timeOffset;

        private void Start()
        {
            InitializeWaterBodies();
            UpdateWaterMaterial();
        }

        private void InitializeWaterBodies()
        {
            // Find all water bodies in the scene
            var bodies = FindObjectsOfType<WaterBody>();
            foreach (var body in bodies)
            {
                waterBodies.Add(body);
                InitializeWaveData(body);
            }
        }

        private void InitializeWaveData(WaterBody body)
        {
            var mesh = body.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                waveVertices = mesh.vertices;
                waveOffsets = new float[waveVertices.Length];

                // Initialize random wave offsets
                for (int i = 0; i < waveOffsets.Length; i++)
                {
                    waveOffsets[i] = Random.Range(0f, 2f * Mathf.PI);
                }
            }
        }

        private void UpdateWaterMaterial()
        {
            if (waterMaterial != null)
            {
                waterMaterial.SetFloat("_Transparency", transparency);
                waterMaterial.SetColor("_ShallowColor", shallowColor);
                waterMaterial.SetColor("_DeepColor", deepColor);
                waterMaterial.SetFloat("_ColorTransitionDepth", colorTransitionDepth);
                waterMaterial.SetFloat("_Reflectivity", reflectivity);
            }
        }

        private void Update()
        {
            timeOffset += Time.deltaTime * waveSpeed;
            UpdateWaves();
            UpdateFlow();
        }

        private void FixedUpdate()
        {
            ApplyBuoyancy();
            ApplyDrag();
        }

        private void UpdateWaves()
        {
            foreach (var body in waterBodies)
            {
                var mesh = body.GetComponent<MeshFilter>()?.mesh;
                if (mesh == null) continue;

                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    float x = vertices[i].x;
                    float z = vertices[i].z;
                    
                    // Calculate wave height using multiple sine waves
                    float height = 0f;
                    height += Mathf.Sin(timeOffset + x * 0.2f + waveOffsets[i]) * waveHeight;
                    height += Mathf.Sin(timeOffset * 0.8f + z * 0.3f + waveOffsets[i]) * waveHeight * 0.5f;
                    
                    vertices[i].y = height;
                }

                mesh.vertices = vertices;
                mesh.RecalculateNormals();
            }
        }

        private void UpdateFlow()
        {
            foreach (var body in waterBodies)
            {
                if (body.HasFlow)
                {
                    // Apply flow forces to floating objects
                    foreach (var obj in floatingObjects)
                    {
                        if (IsInWater(obj.Key, body))
                        {
                            Vector3 flowForce = body.FlowDirection * flowSpeed;
                            obj.Value.AddForce(flowForce, ForceMode.Acceleration);
                        }
                    }
                }
            }
        }

        private void ApplyBuoyancy()
        {
            foreach (var obj in floatingObjects)
            {
                foreach (var body in waterBodies)
                {
                    if (IsInWater(obj.Key, body))
                    {
                        float submergedVolume = CalculateSubmergedVolume(obj.Key, body);
                        Vector3 buoyancy = Vector3.up * buoyancyForce * submergedVolume;
                        obj.Value.AddForce(buoyancy, ForceMode.Acceleration);
                    }
                }
            }
        }

        private void ApplyDrag()
        {
            foreach (var obj in floatingObjects)
            {
                foreach (var body in waterBodies)
                {
                    if (IsInWater(obj.Key, body))
                    {
                        // Apply drag force
                        Vector3 velocity = obj.Value.velocity;
                        float dragMagnitude = velocity.magnitude * dragMultiplier;
                        Vector3 drag = -velocity.normalized * dragMagnitude;
                        obj.Value.AddForce(drag, ForceMode.Acceleration);

                        // Apply angular drag
                        obj.Value.angularVelocity *= 0.95f;
                    }
                }
            }
        }

        private bool IsInWater(Collider collider, WaterBody waterBody)
        {
            Bounds waterBounds = waterBody.GetComponent<Collider>().bounds;
            return waterBounds.Intersects(collider.bounds);
        }

        private float CalculateSubmergedVolume(Collider collider, WaterBody waterBody)
        {
            Bounds bounds = collider.bounds;
            float waterHeight = waterBody.transform.position.y;
            float bottomHeight = bounds.min.y;
            float topHeight = bounds.max.y;

            if (bottomHeight > waterHeight)
                return 0f;

            if (topHeight < waterHeight)
                return 1f;

            return (waterHeight - bottomHeight) / (topHeight - bottomHeight);
        }

        public void RegisterFloatingObject(Collider collider)
        {
            var rb = collider.GetComponent<Rigidbody>();
            if (rb != null && !floatingObjects.ContainsKey(collider))
            {
                floatingObjects.Add(collider, rb);
            }
        }

        public void UnregisterFloatingObject(Collider collider)
        {
            if (floatingObjects.ContainsKey(collider))
            {
                floatingObjects.Remove(collider);
            }
        }

        public void CreateSplash(Vector3 position, float force)
        {
            if (splashEffect != null)
            {
                var splash = Instantiate(splashEffect, position, Quaternion.identity);
                var main = splash.main;
                main.startSpeed = force * splashForce;
                splash.Play();
                Destroy(splash.gameObject, main.duration);
            }

            if (rippleEffect != null)
            {
                var ripple = Instantiate(rippleEffect, position, Quaternion.Euler(90f, 0f, 0f));
                var main = ripple.main;
                main.startSize = force * rippleStrength;
                ripple.Play();
                Destroy(ripple.gameObject, main.duration);
            }
        }
    }

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider))]
    public class WaterBody : MonoBehaviour
    {
        public bool HasFlow => flowDirection != Vector3.zero;
        public Vector3 FlowDirection => flowDirection;

        [SerializeField] private Vector3 flowDirection = Vector3.zero;
        [SerializeField] private float depth = 10f;

        private void OnTriggerEnter(Collider other)
        {
            var waterSystem = FindObjectOfType<WaterSystem>();
            if (waterSystem != null)
            {
                waterSystem.RegisterFloatingObject(other);
                waterSystem.CreateSplash(other.bounds.center, other.attachedRigidbody?.velocity.magnitude ?? 1f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var waterSystem = FindObjectOfType<WaterSystem>();
            if (waterSystem != null)
            {
                waterSystem.UnregisterFloatingObject(other);
                waterSystem.CreateSplash(other.bounds.center, other.attachedRigidbody?.velocity.magnitude ?? 1f);
            }
        }
    }
}
