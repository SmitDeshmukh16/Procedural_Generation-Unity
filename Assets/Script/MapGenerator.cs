using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode{NoiseMap, ColorMap,DrawMesh};
    public DrawMode drawMode;
    public const int mapChunkSize = 241;
    [Range(0,6) ]
    public int levelofdetail;
    public float scale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public float heightMultipiler;
    public AnimationCurve meshHeightCurve;
    public bool autoUpdate;
    public TerrainType[] regions;
    Queue<MapThreadinfo<MapData>> mapThreadInfoQueue = new Queue<MapThreadinfo<MapData>>();
    Queue<MapThreadinfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadinfo<MeshData>>();
    public void RequestMapData(Action<MapData> callback){
        ThreadStart threadStart = delegate {
            MapDataThread(callback);
        };
        new Thread(threadStart).Start();
        
    }
    void MapDataThread(Action<MapData> callback){
        MapData mapData = GenerateMapData();
        lock (mapThreadInfoQueue){
            mapThreadInfoQueue.Enqueue(new MapThreadinfo<MapData>(callback, mapData));   
        }
        
    }
    public void RequestMeshData(MapData mapData,int lod,Action<MeshData> callback){
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod,callback);
        };
        new Thread(threadStart).Start();
    }
    void MeshDataThread(MapData mapData,int lod ,Action<MeshData> callback){
        MeshData meshData = MeshGenerator.GenerateTerrianMesh(mapData.heightMap,heightMultipiler,meshHeightCurve,lod);
        lock (meshDataThreadInfoQueue){
            meshDataThreadInfoQueue.Enqueue(new MapThreadinfo<MeshData>(callback, meshData));
        }
    }
    void Update(){
        if (mapThreadInfoQueue.Count > 0){
            for (int i =0; i<mapThreadInfoQueue.Count; i++){
                MapThreadinfo<MapData> threadinfo = mapThreadInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0){
            for (int i =0; i<meshDataThreadInfoQueue.Count; i++){
                MapThreadinfo<MeshData> threadinfo = meshDataThreadInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }
    }
    public void DrawMapInEditor(){
        MapData mapData = GenerateMapData();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.DrawMesh){
            display.DrawMesh(MeshGenerator.GenerateTerrianMesh(mapData.heightMap,heightMultipiler,meshHeightCurve,levelofdetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }
    public MapData GenerateMapData(){
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize,seed, scale,octaves,persistance,lacunarity,offset);
        Color[] colormap = new Color[mapChunkSize*mapChunkSize]; 
        for (int y =0; y<mapChunkSize; y++){
            for (int x =0; x<mapChunkSize; x++){
                float currentHeight = noiseMap[x,y];
                for (int i =0 ;i<regions.Length; i++){
                    if (currentHeight <= regions[i].height){
                        colormap[y*mapChunkSize + x]= regions[i].color;
                        break;
                    }
                }
            } 
        }
        return new MapData(noiseMap,colormap);
       
    }
    void OnValidate(){
        if (lacunarity<1){
            lacunarity =1;
        }
        if (octaves<1){
            octaves =1;
        }
    }
    struct MapThreadinfo<T>{
        public readonly Action<T> callback;
        public readonly T parameter;
        public MapThreadinfo(Action<T> callback, T parameter){
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public Color color;
}
public struct MapData{
    public float[,] heightMap;
    public Color[] colorMap;
    public MapData(float[,] heightMap, Color[] colorMap){
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
    
}