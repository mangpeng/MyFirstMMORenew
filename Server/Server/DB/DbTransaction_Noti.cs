using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.DB
{
    // 규모가 커지먼 db 담당이 여러개 필요 할수도 있다. => 이 경우 db 담당 간의 순서 보장 로직이 필요
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if(success)
                    {
                        // 실패 했으면 kick
                    }
                }
            });
        }
    }
}
