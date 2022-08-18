using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct Pos
{
	public Pos(int y, int x) { Y = y; X = x; }
	public int Y;
	public int X;
}

public struct PQNode : IComparable<PQNode>
{
	public int F;
	public int G;
	public int Y;
	public int X;

	public int CompareTo(PQNode other)
	{
		if (F == other.F)
			return 0;
		return F < other.F ? 1 : -1;
	}
}

public class MapManager
{
	public int Id { get;  set; } = 0;
	public Grid CurrentGrid { get; private set; }

	public int MinX { get; set; }
	public int MaxX { get; set; }
	public int MinY { get; set; }
	public int MaxY { get; set; }

	public int SizeX { get { return MaxX - MinX + 1; } }
	public int SizeY { get { return MaxY - MinY + 1; } }

	bool[,] _collision;

	public int DivideCount { get; private set; }

	public int SlicedCellWidthSize { get; private set; }
	public int SlicedCellHeightSize { get; private set; }

	public Vector3Int BaseTileMin { get; private set; }
	public Vector3Int BaseTileMax { get; private set; }

	Tilemap[,] slicedBaseTile;
	Tilemap[,] slicedEnvTile;

	List<Tilemap> activeBaseTileList = new List<Tilemap>();
	List<Tilemap> activeEnvTileList = new List<Tilemap>();


	public bool CanGo(Vector3Int cellPos)
	{
		if (cellPos.x < MinX || cellPos.x > MaxX - 1)
			return false;
		if (cellPos.y < MinY || cellPos.y > MaxY - 1)
			return false;

		int x = cellPos.x - MinX;
		int y = MaxY - cellPos.y;	
		return !_collision[y, x];
	}

    public void LoadMap(int mapId, int divideCount = 1)
    {
		// 보스 맵이 아닌 경우 Boss UI 비활성화
		if(mapId != 3)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            if (gameSceneUI != null && gameSceneUI.bossUI.gameObject.activeSelf)
                gameSceneUI.bossUI.gameObject.SetActive(false);
        }

		Id = mapId;
		DivideCount = divideCount;

        DestroyMap();

        string mapName = "Map_" + Id.ToString("000");
        GameObject root = Managers.Resource.Instantiate($"Map/{mapName}");

		if (root == null)
			return;

        root.name = "Map";

        GameObject collision = Util.FindChild(root, "Tilemap_Collision", true);
        if (collision != null) collision.SetActive(false);

        GameObject portal_Prev = Util.FindChild(root, "Tilemap_Portal_Prev", true);
        if (portal_Prev != null) portal_Prev.SetActive(false);

        GameObject portal_Nextr = Util.FindChild(root, "Tilemap_Portal_Next", true);
        if (portal_Nextr != null) portal_Nextr.SetActive(false);

        GameObject gen = Util.FindChild(root, "Tilemap_Gen", true);
        if (gen != null) gen.SetActive(false);



        CurrentGrid = root.GetComponent<Grid>();

		//// 타일맵 렌더링 요소를 분할 한다.
		//// Divide Base TileMap
		//{

		//	Tilemap originTile = root.transform.GetChild(0).GetComponent<Tilemap>();
		//	originTile.CompressBounds();
		//	BaseTileMin = originTile.cellBounds.min;
		//	BaseTileMax = originTile.cellBounds.max;

  //          SlicedCellWidthSize = (int)((BaseTileMax.x - BaseTileMin.x) / (float)divideCount);
  //          SlicedCellHeightSize = (int)((BaseTileMax.y - BaseTileMin.y) / (float)divideCount);

  //          string groupName = originTile.name;
  //          GameObject group = new GameObject(groupName);
		//	group.transform.parent = root.transform;
		//	group.transform.SetSiblingIndex(0);

		//	slicedBaseTile = new Tilemap[divideCount, divideCount];
  //          for (int dy = 0; dy < divideCount; dy++)
  //          {
  //              for (int dx = 0; dx < divideCount; dx++)
  //              {
  //                  Tilemap slice = Tilemap.Instantiate(originTile);
  //                  slice.ClearAllTiles();

  //                  for (int y = 0; y < SlicedCellHeightSize; y++)
  //                  {
  //                      for (int x = 0; x < SlicedCellWidthSize; x++)
  //                      {
		//					// -22 ~ -5
		//					// -5 ~ -10
							
							
  //                          int cellYPos = BaseTileMin.y + dy * SlicedCellHeightSize + y;
  //                          int cellXPos = BaseTileMin.x + dx * SlicedCellWidthSize + x;

  //                          TileBase tile = originTile.GetTile(new Vector3Int(cellXPos, cellYPos, 0));
  //                          slice.SetTile(new Vector3Int(cellXPos, cellYPos, 0), tile);
  //                      }
  //                  }

  //                  slice.RefreshAllTiles();
  //                  slice.ResizeBounds();
  //                  int cellMinYPos = BaseTileMin.y + dy * SlicedCellHeightSize + 0;
  //                  int cellMinXPos = BaseTileMin.x + dx * SlicedCellWidthSize + 0;
  //                  int cellMaxYPos = BaseTileMin.y + dy * SlicedCellHeightSize + SlicedCellHeightSize;
  //                  int cellMaxXPos = BaseTileMin.x + dx * SlicedCellWidthSize + SlicedCellWidthSize;
  //                  slice.gameObject.name = $"{dx},{dy} ({cellMinXPos},{cellMinYPos})~,({cellMaxXPos},{cellMaxYPos}))";
  //                  slice.transform.parent = group.transform;
		//			slice.gameObject.SetActive(false);
		//			slicedBaseTile[dy, dx] = slice;
		//		}
  //          }
			
		//	Managers.Resource.DestroyImmediate(originTile.gameObject);
  //      }

  //      // Divide Env TileMap
  //      {
  //          Tilemap originTile = root.transform.GetChild(1).GetComponent<Tilemap>();

  //          string groupName = originTile.name;
  //          GameObject group = new GameObject(groupName);
  //          group.transform.parent = root.transform;
  //          group.transform.SetSiblingIndex(1);

  //          slicedEnvTile = new Tilemap[divideCount, divideCount];
  //          for (int dy = 0; dy < divideCount; dy++)
  //          {
  //              for (int dx = 0; dx < divideCount; dx++)
  //              {
  //                  Tilemap slice = Tilemap.Instantiate(originTile);
  //                  slice.ClearAllTiles();

  //                  for (int y = 0; y < SlicedCellHeightSize; y++)
  //                  {
  //                      for (int x = 0; x < SlicedCellWidthSize; x++)
  //                      {
  //                          int cellYPos = BaseTileMin.y + dy * SlicedCellHeightSize + y;
  //                          int cellXPos = BaseTileMin.x + dx * SlicedCellWidthSize + x;

  //                          TileBase tile = originTile.GetTile(new Vector3Int(cellXPos, cellYPos, 0));
  //                          slice.SetTile(new Vector3Int(cellXPos, cellYPos, 0), tile);
  //                      }
  //                  }

  //                  slice.RefreshAllTiles();
  //                  slice.ResizeBounds();
  //                  int cellMinYPos = BaseTileMin.y + dy * SlicedCellHeightSize + 0;
  //                  int cellMinXPos = BaseTileMin.x + dx * SlicedCellWidthSize + 0;
  //                  int cellMaxYPos = BaseTileMin.y + dy * SlicedCellHeightSize + SlicedCellHeightSize;
  //                  int cellMaxXPos = BaseTileMin.x + dx * SlicedCellWidthSize + SlicedCellWidthSize;
  //                  slice.gameObject.name = $"{dx},{dy} ({cellMinXPos},{cellMinYPos})~,({cellMaxXPos},{cellMaxYPos}))";
  //                  slice.transform.parent = group.transform;
  //                  slice.gameObject.SetActive(false);
  //                  slicedEnvTile[dy, dx] = slice;
  //              }
  //          }

  //          Managers.Resource.Destroy(originTile.gameObject);
  //      }

        // Collision 관련 파일
        TextAsset txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader reader = new StringReader(txt.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                _collision[y, x] = (line[x] == '1' ? true : false);
            }
        }
    }

    public void DestroyMap()
	{
		GameObject map = GameObject.Find("Map");
		if (map != null)
		{
			GameObject.Destroy(map);
			CurrentGrid = null;
		}
	}

	public Vector2Int GetCollapsedTilesIndex(Vector3Int cell)
    {
		int curDivisionX = (cell.x + (BaseTileMax.x - BaseTileMin.x) - BaseTileMax.x) / SlicedCellWidthSize;
		int curDivisionY = (cell.y + (BaseTileMax.y - BaseTileMin.y) - BaseTileMax.y) / SlicedCellHeightSize;

		return new Vector2Int(curDivisionX, curDivisionY);
    }

    public Vector2Int GetCurTileMap(Pos pos)
    {
        return GetCollapsedTilesIndex(Pos2Cell(pos));
    }

    public void ToggleDivision(Vector3Int pos, int range = 10)
	{
		//HashSet<Tilemap> baseTileset = new HashSet<Tilemap>();
		//HashSet<Tilemap> EnvTileset = new HashSet<Tilemap>();

		//for(int dy = pos.y - range; dy <= pos.y + range; dy++)
  //      {
		//	for(int dx = pos.x - range; dx <= pos.x + range; dx++)
  //          {
		//		Vector2Int tileIdx = GetCollapsedTilesIndex(new Vector3Int(dx, dy, 0));
		//		if(tileIdx.x >= 0 && tileIdx.x < slicedEnvTile.GetLength(1) &&
		//			tileIdx.y >= 0 && tileIdx.y < slicedEnvTile.GetLength(0))
  //              {
		//			baseTileset.Add(slicedBaseTile[tileIdx.y, tileIdx.x]);
		//			EnvTileset.Add(slicedEnvTile[tileIdx.y, tileIdx.x]);
		//		}
  //          }
  //      }

		//List<Tilemap> prevBaseTiles = activeBaseTileList.Except(baseTileset).ToList();
		//List<Tilemap> prevEnvTiles = activeEnvTileList.Except(EnvTileset).ToList();
		//List<Tilemap> newBaseTiles = baseTileset.Except(activeBaseTileList).ToList();
		//List<Tilemap> newEnvTiles = EnvTileset.Except(activeEnvTileList).ToList();

  //      foreach (Tilemap tm in prevBaseTiles) tm.gameObject.SetActive(false);
  //      foreach (Tilemap tm in prevEnvTiles) tm.gameObject.SetActive(false);
  //      foreach (Tilemap tm in newBaseTiles) tm.gameObject.SetActive(true);
  //      foreach (Tilemap tm in newEnvTiles) tm.gameObject.SetActive(true);

		//activeBaseTileList = baseTileset.ToList();
		//activeEnvTileList = EnvTileset.ToList();
	}


	#region A* PathFinding
	// 서버에서 길찾기를 하기 때문에 클라에서 사용하진 않는다.

	// U D L R
	int[] _deltaY = new int[] { 1, -1, 0, 0 };
	int[] _deltaX = new int[] { 0, 0, -1, 1 };
	int[] _cost = new int[] { 10, 10, 10, 10 };

	public List<Vector3Int> FindPath(Vector3Int startCellPos, Vector3Int destCellPos, bool ignoreDestCollision = false)
	{
		List<Pos> path = new List<Pos>();

		// 점수 매기기
		// F = G + H
		// F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
		// G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
		// H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

		// (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
		bool[,] closed = new bool[SizeY, SizeX]; // CloseList

		// (y, x) 가는 길을 한 번이라도 발견했는지
		// 발견X => MaxValue
		// 발견O => F = G + H
		int[,] open = new int[SizeY, SizeX]; // OpenList
		for (int y = 0; y < SizeY; y++)
			for (int x = 0; x < SizeX; x++)
				open[y, x] = Int32.MaxValue;

		Pos[,] parent = new Pos[SizeY, SizeX];

		// 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
		PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

		// CellPos -> ArrayPos
		Pos pos = Cell2Pos(startCellPos);
		Pos dest = Cell2Pos(destCellPos);

		// 시작점 발견 (예약 진행)
		open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
		pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
		parent[pos.Y, pos.X] = new Pos(pos.Y, pos.X);

		while (pq.Count > 0)
		{
			// 제일 좋은 후보를 찾는다
			PQNode node = pq.Pop();
			// 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
			if (closed[node.Y, node.X])
				continue;

			// 방문한다
			closed[node.Y, node.X] = true;
			// 목적지 도착했으면 바로 종료
			if (node.Y == dest.Y && node.X == dest.X)
				break;

			// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
			for (int i = 0; i < _deltaY.Length; i++)
			{
				Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

				// 유효 범위를 벗어났으면 스킵
				// 벽으로 막혀서 갈 수 없으면 스킵
				if (!ignoreDestCollision || next.Y != dest.Y || next.X != dest.X)
				{
					if (CanGo(Pos2Cell(next)) == false) // CellPos
						continue;
				}
				
				// 이미 방문한 곳이면 스킵
				if (closed[next.Y, next.X])
					continue;

				// 비용 계산
				int g = 0;// node.G + _cost[i];
				int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
				// 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
				if (open[next.Y, next.X] < g + h)
					continue;

				// 예약 진행
				open[dest.Y, dest.X] = g + h;
				pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
				parent[next.Y, next.X] = new Pos(node.Y, node.X);
			}
		}

		return CalcCellPathFromParent(parent, dest);
	}

	List<Vector3Int> CalcCellPathFromParent(Pos[,] parent, Pos dest)
	{
		List<Vector3Int> cells = new List<Vector3Int>();

		int y = dest.Y;
		int x = dest.X;
		while (parent[y, x].Y != y || parent[y, x].X != x)
		{
			cells.Add(Pos2Cell(new Pos(y, x)));
			Pos pos = parent[y, x];
			y = pos.Y;
			x = pos.X;
		}
		cells.Add(Pos2Cell(new Pos(y, x)));
		cells.Reverse();

		return cells;
	}

	public Pos Cell2Pos(Vector3Int cell)
	{
		// CellPos -> ArrayPos
		return new Pos(MaxY - cell.y, cell.x - MinX);
	}

	public Vector3Int Pos2Cell(Pos pos)
	{
		// ArrayPos -> CellPos
		return new Vector3Int(pos.X + MinX, MaxY - pos.Y, 0);
	}


	public Vector3 Pos2World(Pos pos)
    {
		Vector3 temp = CurrentGrid.CellToWorld(Pos2Cell(pos));
		temp.x += 0.5f;
		temp.y += 0.5f;

		return temp;
	}

    public Vector3 Cell2World(Vector3Int cell)
    {
        Vector3 temp = CurrentGrid.CellToWorld(cell);
		temp.x += 0.5f;
        temp.y += 0.5f;

        return temp;
    }

    #endregion
}
