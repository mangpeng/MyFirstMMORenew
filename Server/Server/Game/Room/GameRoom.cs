using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Game
{
    public enum RoomType
    {
		Stage1,
		Stage2,
		Boss
    }

	public partial class GameRoom : JobSerializer
	{
		public int VisionCells = 7;

		public int RoomId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
		Dictionary<int, Boss> _bosses = new Dictionary<int, Boss>();
		Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

		public Zone[,] Zones { get; private set; }
		public int ZoneCells { get; private set; }

		public Map Map { get; private set; } = new Map();

		// ㅁㅁㅁ
		// ㅁㅁㅁ
		// ㅁㅁㅁ
		public Zone GetZone(Vector2Int cellPos)
		{
			int x = (cellPos.x - Map.MinX) / ZoneCells;
			int y = (Map.MaxY - cellPos.y) / ZoneCells;
			return GetZone(y, x);
		}

		public Zone GetZone(int indexY, int indexX)
		{
			if (indexX < 0 || indexX >= Zones.GetLength(1))
				return null;
			if (indexY < 0 || indexY >= Zones.GetLength(0))
				return null;

			return Zones[indexY, indexX];
		}

		public void Init(int mapId, int zoneCells, RoomType type, int visionCells)
		{
			VisionCells = visionCells;

			Map.LoadMap(mapId);

			// Zone
			ZoneCells = zoneCells; // 10
			// 1~10 칸 = 1존
			// 11~20칸 = 2존
			// 21~30칸 = 3존
			int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
			int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
			Zones = new Zone[countY, countX];
			for (int y = 0; y < countY; y++)
			{
				for (int x = 0; x < countX; x++)
				{
					Zones[y, x] = new Zone(y, x);
				}
			}

            if (type == RoomType.Stage1)
            {
                for (int i = 0; i < (int)(Map.RespawnList.Count()); i++)
                {
                    Monster monster = ObjectManager.Instance.Add<Monster>();
                    monster.Init(1);
                    EnterGame(monster, randomPos: true);
                }
            }
            else if (type == RoomType.Stage2)
            {
                for (int i = 0; i < (int)(Map.RespawnList.Count()); i++)
                {
                    Monster monster = ObjectManager.Instance.Add<Monster>();
                    monster.Init(2);
                    EnterGame(monster, randomPos: true);
                }
            }
            else if (type == RoomType.Boss)
            {

                Boss boss = ObjectManager.Instance.Add<Boss>();
                boss.Init(0);

                // TODO 보스 시작 위치 하드 코딩되어 있음
                boss.CellPos = new Vector2Int(6, 12);
                EnterGame(boss, false);
            }
        }

        // 누군가 주기적으로 호출해줘야 한다
        public void Update()
		{
			Flush();
		}

		Random _rand = new Random();
		public void EnterGame(GameObject gameObject, bool randomPos)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

			if (randomPos)
			{
				if(type == GameObjectType.Monster)
                {
                    Vector2Int? respanPos = Map.GetAvailiableGenSpot();
                    if (respanPos != null)
                        gameObject.CellPos = (Vector2Int)respanPos;
                }
				else
                {
					Vector2Int respawnPos;
					while (true)
					{
						respawnPos.x = _rand.Next(Map.MinX, Map.MaxX + 1);
						respawnPos.y = _rand.Next(Map.MinY, Map.MaxY + 1);
						if (Map.CanGo(respawnPos) == true)
						{
							gameObject.CellPos = respawnPos;
							break;
						}
					}
                }
            }


			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				_players.Add(gameObject.Id, player);
				player.Room = this;

				player.RefreshAdditionalStat();

				Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
				GetZone(player.CellPos).Players.Add(player);

				// 본인한테 정보 전송
				{
					S_EnterGame enterPacket = new S_EnterGame();
					enterPacket.Player = player.Info;
					enterPacket.MapId = RoomId;
					player.Session.Send(enterPacket);

					player.Vision.Update();
				}
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = gameObject as Monster;
				_monsters.Add(gameObject.Id, monster);
				monster.Room = this;

				gameObject.Info.MonsterTemplateId = monster.TemplateId;

				GetZone(monster.CellPos).Monsters.Add(monster);
				Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

				monster.Update();
			}
            else if (type == GameObjectType.Boss)
            {
                Boss boss = gameObject as Boss;
                _bosses.Add(gameObject.Id, boss);
				boss.Room = this;

                GetZone(boss.CellPos).Bosses.Add(boss);
                Map.ApplyMove(boss, new Vector2Int(boss.CellPos.x, boss.CellPos.y));

				boss.Update();
            }
            else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = gameObject as Projectile;
				_projectiles.Add(gameObject.Id, projectile);
				projectile.Room = this;

				GetZone(projectile.CellPos).Projectiles.Add(projectile);
				projectile.Update();
			}

			// 타인한테 정보 전송
			{
				S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				BroadCastVision(gameObject.CellPos, spawnPacket);
			}
		}

		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			Vector2Int cellPos;

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;

				cellPos = player.CellPos;

				player.OnLeaveGame();
				Map.ApplyLeave(player);
				player.Room = null;

				// 본인한테 정보 전송
				{
					S_LeaveGame leavePacket = new S_LeaveGame();
					player.Session.Send(leavePacket);
				}
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = null;
				if (_monsters.Remove(objectId, out monster) == false)
					return;

				cellPos = monster.CellPos;
				Map.ApplyLeave(monster);
				monster.Room = null;
			}
            else if (type == GameObjectType.Boss)
            {
                Boss boss = null;
                if (_bosses.Remove(objectId, out boss) == false)
                    return;

                cellPos = boss.CellPos;
                Map.ApplyLeave(boss);
                boss.Room = null;
            }
            else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = null;
				if (_projectiles.Remove(objectId, out projectile) == false)
					return;

				cellPos = projectile.CellPos;
				Map.ApplyLeave(projectile);
				projectile.Room = null;
			}
			else
			{
				return;
			}

			// 타인한테 정보 전송
			{
				S_Despawn despawnPacket = new S_Despawn();
				despawnPacket.ObjectIds.Add(objectId);
				BroadCastVision(cellPos, despawnPacket);
			}
		}

		public Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		// 살짝 부담스러운 함수
		public Player FindClosestPlayer(Vector2Int pos, int range)
		{
			List<Player> players = GetAdjacentPlayers(pos, range);

			players.Sort((left, right) =>
			{
				int leftDist = (left.CellPos - pos).cellDistFromZero;
				int rightDist = (right.CellPos - pos).cellDistFromZero;
				return leftDist - rightDist;
			});

			foreach (Player player in players)
			{
				List<Vector2Int> path = Map.FindPath(pos, player.CellPos, checkObjects: true);
				if (path.Count < 2 || path.Count > range)
					continue;

				return player;
			}

			return null;
		}



        public int BroadCastVision(Vector2Int pos, IMessage packet)
		{
			List<Zone> zones = GetAdjacentZones(pos, VisionCells);

			for(int i =0; i< zones.Count; i++)
            {
				foreach(Player p in zones[i].Players)
                {
                    int dx = p.CellPos.x - pos.x;
                    int dy = p.CellPos.y - pos.y;
                    if (Math.Abs(dx) > VisionCells)
                        continue;
                    if (Math.Abs(dy) > VisionCells)
                        continue;

                    p.Session.Send(packet);
                }
            }

			return zones.Select(z => z.Players).Count();
		}

        public int BroadCastRoom(IMessage packet)
        {
            foreach (var pair in _players)
            {
				Player p = pair.Value;
				p.Session.Send(packet);
			}

			return _players.Count();
        }

        public List<Player> GetAdjacentPlayers(Vector2Int pos, int range)
		{
			List<Zone> zones = GetAdjacentZones(pos, range);
			return zones.SelectMany(z => z.Players).ToList();
		}

		// ㅁㅁㅁㅁㅁㅁ
		// ㅁㅁㅁㅁㅁㅁ
		// ㅁㅁㅁㅁㅁㅁ
		// ㅁㅁㅁㅁㅁㅁ
		public List<Zone> GetAdjacentZones(Vector2Int cellPos, int range)
		{
			HashSet<Zone> zones = new HashSet<Zone>();

			int maxY = cellPos.y + range;
			int minY = cellPos.y - range;
			int maxX = cellPos.x + range;
			int minX = cellPos.x - range;

			// 좌측 상단
			Vector2Int leftTop = new Vector2Int(minX, maxY);
			int minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;
			int minIndexX = (leftTop.x - Map.MinX) / ZoneCells;
			
			// 우측 하단
			Vector2Int rightBot = new Vector2Int(maxX, minY);
			int maxIndexY = (Map.MaxY - rightBot.y) / ZoneCells;
			int maxIndexX = (rightBot.x - Map.MinX) / ZoneCells;

			for (int x = minIndexX; x <= maxIndexX; x++)
			{
				for (int y = minIndexY; y <= maxIndexY; y++)
				{
					Zone zone = GetZone(y, x);
					if (zone == null)
						continue;

					zones.Add(zone);
				}
			}

			return zones.ToList();
		}
	}
}
