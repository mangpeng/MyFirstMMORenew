using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.DB
{
    // 규모가 커지먼 db 담당이 여러개 필요 할수도 있다. => 이 경우 db 담당 간의 순서 보장 로직이 필요
    public class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        // 1 GameRoom => 2 Db => 3 GameRoom
        public static void SavePlayerSatus_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // 1 GameRoom
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            // 2 Db
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if(success)
                    {
                        // 3 GameRoom
                        room.Push(() =>
                        {
                            Console.WriteLine($"Hp Saved({playerDb.Hp})");
                        });
                    }
                }
            });


        }

        // 1 GameRoom
        public static void SavePlayerSatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // 1 GameRoom
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerSatus_Step2, playerDb, room);
        }

        // 2 Db => 이쪽을 별도의 서버로 빼기도 한다. db서버에게 패킷을 날려서 결과를 통지 받는 형태
        public static void SavePlayerSatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success)
                {
                    // 3 GameRoom
                    room.Push(SavePlayerSatus_Step3, playerDb.Hp);
                }
            }
        }

        // 3 GameRoom
        public static void SavePlayerSatus_Step3(int hp)
        {
            Console.WriteLine($"Hp Saved({hp})");
        }
    }
}
