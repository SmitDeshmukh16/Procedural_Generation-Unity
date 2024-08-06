using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public  static class MeshGenerator{
    public static MeshData GenerateTerrianMesh(float[,] heightMap,float heightMultipiler,AnimationCurve _heightCurve, int levelofdetail){
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;
        int meshincrement = (levelofdetail==0)?1:levelofdetail*2;
        int vetricesPerLine = (width-1)/meshincrement +1;
        MeshData meshdata = new MeshData(vetricesPerLine, vetricesPerLine);
        int vertexIndex = 0;
        for (int y=0 ; y<height; y+=meshincrement){
            for (int x =0; x<width; x+=meshincrement){
                meshdata.vertices[vertexIndex] = new Vector3(x+topLeftX, heightCurve.Evaluate(heightMap[x,y])* heightMultipiler, topLeftZ-y);
                meshdata.uv[vertexIndex] = new Vector2(x/(float)width, y/(float)height);
                if (x<width-1 && y<height-1){
                    meshdata.addTriangle(vertexIndex, vertexIndex+vetricesPerLine+1, vertexIndex+vetricesPerLine);
                    meshdata.addTriangle(vertexIndex +vetricesPerLine +1, vertexIndex, vertexIndex+1);
                }
                vertexIndex++;
            }
        }
        return meshdata;
    }
  
}

public class MeshData{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;
    int triangleIndex ;
    public MeshData(int width, int height){
        vertices = new Vector3[width * height];
        triangles = new int[(width-1)*(height-1)*6];
        uv = new Vector2[width * height];
    }
    public void addTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        return mesh;
    }
}
