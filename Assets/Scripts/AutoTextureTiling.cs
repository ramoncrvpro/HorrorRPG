namespace HorrorRPG.Presentation
{



using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class AutoTextureTiling : MonoBehaviour
{
    [Header("Tiling Settings")]
    [Tooltip("Tamanho em metros que corresponde a 1 repetição da textura")]
    public float textureSize = 1f;
    
    [Tooltip("Aplicar tiling automaticamente no Start")]
    public bool applyOnStart = true;
    
    [Tooltip("Índice do material a ser ajustado (caso o objeto tenha múltiplos materiais)")]
    public int materialIndex = 0;

    private MeshRenderer meshRenderer;
    private Material instanceMaterial;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyTiling();
        }
    }

    [ContextMenu("Aplicar Tiling Agora")]
    public void ApplyTiling()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer == null)
        {
            Debug.LogError($"MeshRenderer não encontrado em {gameObject.name}");
            return;
        }

        if (materialIndex >= meshRenderer.materials.Length)
        {
            Debug.LogError($"Material index {materialIndex} inválido em {gameObject.name}");
            return;
        }

        Material[] materials = meshRenderer.materials;
        instanceMaterial = new Material(materials[materialIndex]);
        materials[materialIndex] = instanceMaterial;
        meshRenderer.materials = materials;

        Vector3 objectSize = CalculateObjectSize();
        Vector2 tiling = new Vector2(
            objectSize.x / textureSize,
            objectSize.y / textureSize
        );

        instanceMaterial.mainTextureScale = tiling;
    }

    private Vector3 CalculateObjectSize()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning($"MeshFilter não encontrado em {gameObject.name}, usando escala do transform");
            return transform.lossyScale;
        }

        Bounds bounds = meshFilter.sharedMesh.bounds;
        Vector3 size = bounds.size;
        
        Vector3 worldSize = new Vector3(
            size.x * transform.lossyScale.x,
            size.y * transform.lossyScale.y,
            size.z * transform.lossyScale.z
        );

        return worldSize;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && instanceMaterial != null)
        {
            ApplyTiling();
        }
    }

    private void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}


}
