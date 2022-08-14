using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{

#if UNITY_EDITOR

	// % (Ctrl), # (Shift), & (Alt)

	[MenuItem("Tools/GenerateMap %#g")]
	private static void GenerateMap()
	{
		GenerateByPath("Assets/Resources/Map");
		GenerateByPath("../Common/MapData");
	}

	private static void GenerateByPath(string pathPrefix)
	{
		GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");
	
		foreach (GameObject go in gameObjects)
		{
			Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
			Tilemap tmCollision = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
			Tilemap tmPortalPrev = Util.FindChild<Tilemap>(go, "Tilemap_Portal_Prev", true);
			Tilemap tmPortalNext = Util.FindChild<Tilemap>(go, "Tilemap_Portal_Next", true);
			Tilemap tmGen = Util.FindChild<Tilemap>(go, "Tilemap_Gen", true);

			tmBase.CompressBounds();
			tmCollision.size = tmBase.size;


			using (var writer = File.CreateText($"{pathPrefix }/{go.name}.txt"))
			{
				writer.WriteLine(tmBase.cellBounds.xMin);
				writer.WriteLine(tmBase.cellBounds.xMax);
				writer.WriteLine(tmBase.cellBounds.yMin);
				writer.WriteLine(tmBase.cellBounds.yMax);

				for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
				{
					for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
					{
						string data = ((int)Define.MAP.EMPTY).ToString();

						TileBase tile = tmCollision.GetTile(new Vector3Int(x, y, 0));
						if (tile != null)
							data = ((int)Define.MAP.OBSTACLE).ToString();

						tile = tmPortalPrev.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
							data = ((int)Define.MAP.PORTAL_PREV).ToString();

                        tile = tmPortalNext.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            data = ((int)Define.MAP.PORTAL_NEXT).ToString();

                        tile = tmGen.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
							data = ((int)Define.MAP.RESPAWN).ToString();

						writer.Write(data);
					}
					writer.WriteLine();
				}
			}
		}
	}

#endif

}
