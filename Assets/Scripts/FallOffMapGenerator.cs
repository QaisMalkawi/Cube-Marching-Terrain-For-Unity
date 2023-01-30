using UnityEngine;

public static class FallOffMapGenerator
{
	public static float[,] Generate(int resolution, FalloffShape shape, float edge)
	{
		float[,] map = new float[resolution, resolution];

		if (shape == FalloffShape.Circle)
		{
			for (int y = 0; y < map.GetLength(1); y++)
			{
				for (int x = 0; x < map.GetLength(0); x++)
				{

					Vector2 pos = new Vector2(x, y);
					Vector2 center = new Vector2(map.GetLength(1) / 2, map.GetLength(0) / 2);

					float distance = Vector2.Distance(pos, center);
					distance /= (map.GetLength(1) / 2);

					map[x, y] = edge - distance;

				}
			}
		}

		else if (shape == FalloffShape.Square)
		{
			for (int y = 0; y < map.GetLength(1); y++)
			{
				for (int x = 0; x < map.GetLength(0); x++)
				{
					float xdist = 0;
					float ydist = 0;

					if (x > map.GetLength(1))
						xdist = 1 - (map.GetLength(1) - x);
					else
						xdist = x;

					xdist /= map.GetLength(1);

					if (y > map.GetLength(0))
						ydist = 1 - (map.GetLength(0) - y);
					else
						ydist = y;

					ydist /= map.GetLength(0);


					float distance = (xdist + ydist) / 2;

					map[x, y] = (edge - distance) * (edge / 2);
				}
			}
		}
		return map;
	}

	public static float[,] CreateCircularFalloffMap(int resolution, float edge, float strength)
	{
		float[,] falloffMap = new float[resolution, resolution];

		float center = resolution / 2f;
		float radius = edge * center;
		float radiusSqr = radius * radius;

		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				float distSqr = (x - center) * (x - center) + (y - center) * (y - center);
				falloffMap[x, y] = Mathf.Lerp(1, 0, distSqr / radiusSqr) * strength;
			}
		}

		return falloffMap;
	}
}
public enum FalloffShape { Square, Circle }
