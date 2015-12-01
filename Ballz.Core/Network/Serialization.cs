using System;
using Ballz.GameSession.World;
using Ballz.Utils;

namespace Ballz
{
	[Serializable]
	public class SEntity
	{
		public SEntity(Entity e)
		{
			this.ID = e.ID;
			this.Kind = e.Kind;
			this.Position = Utils.VectorExtensions.ToBallz(e.Position);
			this.Rotation = e.Rotation;
			this.Velocity = Utils.VectorExtensions.ToBallz(e.Velocity);
		}
		public int ID;
		public Entity.EntityType Kind;
		public Utils.BalllzVec2 Position, Velocity;
		public float Rotation;
	}
}

