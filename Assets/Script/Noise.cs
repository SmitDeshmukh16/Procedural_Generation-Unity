
using UnityEngine;

public static class Noise 
{
   public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale,int octaves, float persistance,float lacunarity,Vector2 offest){
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i =0; i<octaves; i++){
            float offsetX = prng.Next(-1000000, 1000000) + offest.x;
            float offsetY = prng.Next(-1000000, 1000000) + offest.y;
            octaveOffsets[i] = new Vector2(offsetX,offsetY);
        }
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float[,] noiseMap = new float[mapWidth, mapHeight];
        if (scale <= 0){
            scale = 0.0001f;
        }
        for (int y =0; y<mapHeight; y++){
            for (int x =0; x<mapWidth; x++){
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int o =0; o<octaves; o++){
                    float sampleX = x/scale * frequency + octaveOffsets[o].x;
                    float sampleY = y/scale * frequency + octaveOffsets[o].y;
                    float perlinvalue = Mathf.PerlinNoise(sampleX, sampleY)*2 -1;
                    noiseHeight += perlinvalue*amplitude; 
                    amplitude *= persistance;
                    frequency *= lacunarity;

                }
                if (noiseHeight > maxNoiseHeight){
                    maxNoiseHeight = noiseHeight;
                }else if (noiseHeight < minNoiseHeight){
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x,y] = noiseHeight;
            }
       }
       for (int y =0; y<mapHeight; y++){
           for(int x =0;x<mapWidth; x++){
               noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight,maxNoiseHeight,noiseMap[x,y]);
           }
       }
       return noiseMap;
   }
}
