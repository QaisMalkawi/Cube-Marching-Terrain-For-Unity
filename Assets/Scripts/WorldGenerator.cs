using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] Chunk chunkPrefab;

	[SerializeField] int worldSize, worldHeight, tileSize, seed;
    [SerializeField] float tileScale;
    [SerializeField] Wave[] waves;

    List<Chunk> chunks;

	public GameObject viewer;
    public int renderDistance;
    public bool SmoothTerrain;

    public static WorldGenerator instance;

	void Start()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        instance = this;
        chunks = new List<Chunk>();

		for (int z = 0; z < worldSize; z++)
        {
            for (int x = 0; x < worldSize; x++)
            {
                Chunk chunk = Instantiate(chunkPrefab, new Vector3(x * tileSize, 0, z * tileSize), Quaternion.identity);
                chunk.transform.parent = transform;
                chunk.transform.name = $"Chunk ({x}, {z})";
                chunk.AssignReferences(new Vector3Int(x, 0, z));


				chunks.Add(chunk);
            }
        }
		Parallel.For(0, chunks.Count, i =>
		{
			chunks[i].SetHeights(tileScale, tileSize, worldHeight, waves, seed);
		});
        Parallel.For(0, chunks.Count, i =>
		{
			chunks[i].CreateMeshData();
		});

		for (int i = 0; i < chunks.Count; i++)
        {
			chunks[i].UpdateMesh();
		}

        sw.Stop();
		UnityEngine.Debug.Log("World Generation Took " + sw.Elapsed);
	}
}
