using Google.Protobuf.Protocol;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Player : GameObject
	{
		public int PlayerDbId { get; set; }
		public ClientSession Session { get; set; }
		public Inventory Inven { get; private set; } = new Inventory();

		public Player()
		{
			ObjectType = GameObjectType.Player;
		}

		public override void OnDamaged(GameObject attacker, int damage)
		{
			base.OnDamaged(attacker, damage);


		}

		public override void OnDead(GameObject attacker)
		{
			base.OnDead(attacker);
		}

	    public void OnLeaveGame()
        {
			// DB연동
			// 피가 깍일 때마다 DB 접근할 필요가 있을까?
			// 1) 서버가 다운되면 아직 저장되지 않은 정보 날아감
			// 2) 코드 흐름을 다 막아 버린다. (실행하고 있는 스레드)
			// - 비동기(Async) 방법 사용?
			// - 다른 스레드로 DB 읾감을 던지면?
			// -- 결과를 받아서 이어서 처리 해야 하는 경우가 많음..
			//        using (AppDbContext db = new AppDbContext())
			//        {
			//            // Find -> Save는 읽고->쓰고 두번의 db 접근
			//            // 이와 같은 방식은 한번만 접근해서 원하는 속성 수정
			//            PlayerDb playerDb = new PlayerDb();
			//playerDb.PlayerDbId = PlayerDbId;
			//            playerDb.Hp = Stat.Hp;

			//db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
			//db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
			//            db.SaveChangesEx();

			//            Console.WriteLine($"Hp Saved({playerDb.Hp})");
			//        }

			//DbTransaction.SavePlayerSatus_AllInOne(this, Room);
			DbTransaction.SavePlayerSatus_Step1(this, Room);
		}
	}
}
