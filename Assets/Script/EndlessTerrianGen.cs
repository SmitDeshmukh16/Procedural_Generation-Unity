using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrianGen : MonoBehaviour
{
    public const float maxViewDst = 420;
    public LODinfo[] lodinfos;
    public Transform viewer;
    public static Vector2 viewerPosition;
    int chunkSize;
    public Material mapMaterial;
    int chunksVisible;
    static MapGenerator mapGenerator;
    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleChunks = new List<TerrainChunk>();
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisible = Mathf.RoundToInt(maxViewDst / chunkSize);
    }
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        updateVisibleChunks();
    }
    void updateVisibleChunks()
    {
        int currentChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for (int i = 0; i < visibleChunks.Count; i++)
        {
            visibleChunks[i].SetVisible(false);
        }
        visibleChunks.Clear();
        for (int yOffset = -chunksVisible; yOffset <= chunksVisible; yOffset++)
        {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++)
            {
                Vector2 viewedChunk = new Vector2(currentChunkX + xOffset, currentChunkY + yOffset);
                if (terrainChunks.ContainsKey(viewedChunk))
                {
                    terrainChunks[viewedChunk].Update();
                    if (terrainChunks[viewedChunk].IsVisible())
                    {
                        visibleChunks.Add(terrainChunks[viewedChunk]);
                    }

                }
                else
                {
                    terrainChunks.Add(viewedChunk, new TerrainChunk(viewedChunk, chunkSize, transform, mapMaterial));
                }
            }
        }
    }
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            this.position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshObject.transform.position = positionV3;
            meshRenderer.material = material;
            meshObject.transform.parent = parent;
            SetVisible(false);
            mapGenerator.RequestMapData(OnMapDataReceived);
        }
        void OnMapDataReceived(MapData mapData)
        {
            //mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }
        public void Update()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool IsVisible() { return meshObject.activeSelf; }
    }
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        public LODMesh(int lod)
        {
            this.lod = lod;
        }
        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

        }

    }
    [System.Serializable]
    public struct LODinfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}
