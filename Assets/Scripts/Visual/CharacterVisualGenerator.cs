using UnityEngine;

namespace REcreationOfSpace.Visual
{
    public class CharacterVisualGenerator : MonoBehaviour
    {
        [SerializeField] private Material[] characterMaterials;
        [SerializeField] private GameObject[] equipmentPrefabs;

        public void ApplyVisuals(GameObject character)
        {
            if (character == null) return;

            // Apply random material to character
            var renderer = character.GetComponent<Renderer>();
            if (renderer != null && characterMaterials != null && characterMaterials.Length > 0)
            {
                int randomMaterialIndex = Random.Range(0, characterMaterials.Length);
                renderer.material = characterMaterials[randomMaterialIndex];
            }

            // Add random equipment
            if (equipmentPrefabs != null && equipmentPrefabs.Length > 0)
            {
                int numEquipment = Random.Range(1, 4); // 1-3 pieces of equipment
                for (int i = 0; i < numEquipment; i++)
                {
                    int randomEquipIndex = Random.Range(0, equipmentPrefabs.Length);
                    var equipment = Instantiate(equipmentPrefabs[randomEquipIndex], character.transform);
                    
                    // Randomize equipment position and rotation slightly
                    equipment.transform.localPosition += Random.insideUnitSphere * 0.2f;
                    equipment.transform.localRotation = Quaternion.Euler(
                        Random.Range(-15f, 15f),
                        Random.Range(0f, 360f),
                        Random.Range(-15f, 15f)
                    );
                }
            }

            // Add particle effects for special characters
            if (character.GetComponent<FirstGuide>() != null)
            {
                var effects = character.GetComponent<FirstGuideEffects>();
                if (effects != null)
                    effects.enabled = true;
            }
        }
    }
}
