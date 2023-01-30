using UnityEngine;

public static class NoiseMapGeneration
{
	public static float[,] GeneratePerlinNoiseMap(int mapSize, float scale, float offsetX, float offsetZ, Wave[] waves, int Seed)
	{
		for (int i = 0; i < waves.Length; i++)
		{
			waves[i].setSeed(Seed);
		}

		// create an empty noise map with the mapDepth and mapWidth coordinates
		float[,] noiseMap = new float[mapSize, mapSize];

		for (int zIndex = 0; zIndex < mapSize; zIndex++)
		{
			for (int xIndex = 0; xIndex < mapSize; xIndex++)
			{
				// calculate sample indices based on the coordinates, the scale and the offset
				float sampleX = (xIndex + offsetX) / scale;
				float sampleZ = (zIndex + offsetZ) / scale;

				float noise = 0f;
				float normalization = 0f;
				foreach (Wave wave in waves)
				{
					// generate noise value using PerlinNoise for a given Wave
					noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
					normalization += wave.amplitude;
				}
				// normalize the noise value so that it is within 0 and 1
				noise /= normalization;

				noiseMap[zIndex, xIndex] = noise;
			}
		}

		return noiseMap;
	}

	public static float[,,] Generate3DPerlinNoiseMap(int mapSizeX, int mapSizeY, int mapSizeZ, float scale, float offsetX, float offsetY, float offsetZ, Wave[] waves, int Seed)
	{
		for (int i = 0; i < waves.Length; i++)
		{
			waves[i].setSeed(Seed);
		}

		// create an empty noise map with the mapDepth and mapWidth coordinates
		float[,,] noiseMap = new float[mapSizeX, mapSizeY, mapSizeZ];

		for (int zIndex = 0; zIndex < mapSizeZ; zIndex++)
		{
			for (int yIndex = 0; yIndex < mapSizeY; yIndex++)
			{
				for (int xIndex = 0; xIndex < mapSizeX; xIndex++)
				{
					// calculate sample indices based on the coordinates, the scale and the offset
					float sampleX = (xIndex + offsetX) / scale;
					float sampleY = (yIndex + offsetY) / scale;
					float sampleZ = (zIndex + offsetZ) / scale;

					float noise = 0f;
					float normalization = 0f;
					foreach (Wave wave in waves)
					{
						float perlinValueX = Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleY * wave.frequency + wave.seed);
						float perlinValueY = Mathf.PerlinNoise(sampleY * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
						float perlinValueZ = Mathf.PerlinNoise(sampleZ * wave.frequency + wave.seed, sampleX * wave.frequency + wave.seed);

						noise += wave.amplitude * (perlinValueX + perlinValueY + perlinValueZ) / 3f;
						normalization += wave.amplitude;
					}
					// normalize the noise value so that it is within 0 and 1
					noise /= normalization;

					noiseMap[xIndex, yIndex, zIndex] = noise;
				}
			}
		}

		return noiseMap;
	}

}

[System.Serializable]
public class Wave
{
	[HideInInspector] public float seed;
	public float frequency;
	public float amplitude;

	public void setSeed(int mainSeed)
	{
		seed = Mathf.Floor((mainSeed / amplitude) * frequency);
	}
}
