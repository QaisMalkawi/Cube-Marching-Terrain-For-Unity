using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk : MonoBehaviour
{
    public float[,,] terrainMap;

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;

	Vector3Int Offset;

	Mesh mesh;
	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> uvs = new List<Vector2>();
	List<int> triangles = new List<int>();

	int width, height;

	public void PlaceTerrain(Vector3 pos)
	{
		vertices.Clear();
		uvs.Clear();
		triangles.Clear();

		mesh = new Mesh();

		Vector3Int v3Int = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
		v3Int -= Offset * new Vector3Int(width, height, width);

		terrainMap[v3Int.x, v3Int.y, v3Int.z] = 0;
		CreateMeshData();
		UpdateMesh();
	}
	public void RemoveTerrain(Vector3 pos)
	{
		vertices.Clear();
		uvs.Clear();
		triangles.Clear();

		mesh = new Mesh();

		Vector3Int v3Int = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		v3Int -= Offset * new Vector3Int(width, height, width);

		terrainMap[v3Int.x, v3Int.y, v3Int.z] = 1;
		CreateMeshData();
		UpdateMesh();
	}

    public void SetHeights(float scale, int width, int height, Wave[] waves, int seed)
	{
		this.width = width;
		this.height = height;

		float[,,] heightMap = NoiseMapGeneration.Generate3DPerlinNoiseMap(width + 1, height + 1, width + 1, 10f / scale, (Offset.x * width), (Offset.y * height), (Offset.z * width), waves, seed);

		terrainMap = new float[width + 1, height + 1, width + 1];

		// The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger
		// than the width/height of our mesh.
		for (int x = 0; x < width + 1; x++)
		{
			for (int z = 0; z < width + 1; z++)
			{
				for (int y = 0; y < height + 1; y++)
				{
					// Get a terrain height using regular old Perlin noise.
					float thisHeight = (float)height * heightMap[x, y, z];


					// Set the value of this point in the terrainMap.
					terrainMap[x, y, z] = (float)y - thisHeight;
				}
			}
		}
	}

	public void CreateMeshData()
	{

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					// Pass the value into our MarchCube function.
					MarchCube(new Vector3Int(x, y, z), width);
				}
			}
		}
	}

	void MarchCube(Vector3Int position, int width)
	{

		float[] cube = new float[8];
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + MarchingTables.CornerTable[i]);
		}

		// Get the configuration index of this cube.
		int configIndex = GetCubeConfiguration(cube);

		// If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
		if (configIndex == 0 || configIndex == 255)
			return;

		// Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
		int edgeIndex = 0;
		for (int i = 0; i < 5; i++)
		{
			for (int p = 0; p < 3; p++)
			{

				// Get the current indice. We increment triangleIndex through each loop.
				int indice = MarchingTables.TriangleTable[configIndex, edgeIndex];

				// If the current edgeIndex is -1, there are no more indices and we can exit the function.
				if (indice == -1)
					return;

				// Get the vertices for the start and end of this edge.
				Vector3 vert1 = position + MarchingTables.CornerTable[MarchingTables.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + MarchingTables.CornerTable[MarchingTables.EdgeIndexes[indice, 1]];

				Vector3 vertPosition;
				if (WorldGenerator.instance.SmoothTerrain)
				{
					float vert1Sample = cube[MarchingTables.EdgeIndexes[indice, 0]];
					float vert2Sample = cube[MarchingTables.EdgeIndexes[indice, 1]];

					float difference = vert2Sample - vert1Sample;
					if (difference == 0)
						difference = 0.5f;
					else
						difference = (0.5f - vert1Sample) / difference;

					vertPosition = vert1 + ((vert2 - vert1) * difference);
				}
				else
				{
					// Get the midpoint of this edge.
					vertPosition = (vert1 + vert2) / 2f;
				}

				// Add to our vertices and triangles list and incremement the edgeIndex.
				if (!WorldGenerator.instance.SmoothTerrain)
				{
					vertices.Add(vertPosition);
					uvs.Add(new Vector2(vertPosition.x / width, vertPosition.z / width));
					triangles.Add(vertices.Count - 1);
				}
				else
				{
					triangles.Add(VertForIndice(vertPosition));
				}

				edgeIndex++;

			}
		}
	}
	int GetCubeConfiguration(float[] cube)
	{

		// Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
		int configurationIndex = 0;
		for (int i = 0; i < 8; i++)
		{

			// If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
			// the surface, the bit would look like 00100000, which represents the integer value 32.
			if (cube[i] > 0.5f)
				configurationIndex |= 1 << i;

		}

		return configurationIndex;

	}
	public void UpdateMesh()
    {
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		meshFilter.sharedMesh = mesh;
		meshCollider.sharedMesh = mesh;
    }

	public void AssignReferences(Vector3Int position)
	{
		Offset = position;
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
		mesh = new Mesh();
	}


	float SampleTerrain(Vector3Int point)
	{
		return terrainMap[point.x, point.y, point.z];
	}
	int VertForIndice(Vector3 vert)
	{
		for (int i = 0; i < vertices.Count; i++)
		{
			if (vertices[i] == vert)
				return i;
		}
		vertices.Add(vert);
		uvs.Add(new Vector2(vert.x / width, vert.z / width));
		return vertices.Count - 1;
	}
	void Update()
	{

		Vector2 centerPosition = new Vector2(transform.position.x + (width / 2f),
			                                 transform.position.z + (width / 2f) );
		Vector2 viewerPosition = new Vector2(WorldGenerator.instance.viewer.transform.position.x,															   WorldGenerator.instance.viewer.transform.position.z);

		meshRenderer.enabled = 
			meshCollider.enabled = 
				Vector2.Distance(centerPosition, viewerPosition) <
				(WorldGenerator.instance.renderDistance * width);
	}
}
