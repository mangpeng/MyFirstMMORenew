using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Projectile : GameObject
	{
		public Data.Skill Data { get; set; }

		public Projectile()
		{
			
		}

        public override void SetType()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public override void Update()
		{

		}
	}
}
