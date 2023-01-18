namespace RegionKit.Modules.AridBarrens;

public class SandPart : CosmeticSprite
	{
		public SandPart(Vector2 pos)
		{
			this.pos = pos;
			this.lastLastPos = pos;
			this.lastPos = pos;
			this.vel = new Vector2(0f, 0f);
			this.life = 1f;
			this.lifeTime = Mathf.Lerp(600f, 1200f, UnityEngine.Random.value);
			this.col = new Color(0.690f / 2f, 0.525f / 2f, 0.478f / 2f);
			if (UnityEngine.Random.value < 0.8f)
			{
				this.depth = 0f;
			}
			else if (UnityEngine.Random.value < 0.3f)
			{
				this.depth = -0.5f * UnityEngine.Random.value;
			}
			else
			{
				this.depth = Mathf.Pow(UnityEngine.Random.value, 1.5f) * 3f;
			}
		}
		public bool InPlayLayer
		{
			get
			{
				return this.depth == 0f;
			}
		}

		public override void Update(bool eu)
		{
			this.vel *= 0.99f;
			this.vel += new Vector2(0.11f, Custom.LerpMap(this.life, 0f, 0.5f, -0.1f, 0.05f));
			this.vel += this.dir * 0.8f;
			this.dir = (this.dir + Custom.RNV() * 0.6f).normalized;
			this.life -= 1f / this.lifeTime;
			this.lastLastPos = this.lastPos;
			this.lastPos = this.pos;
			this.pos += this.vel / (this.depth + 1f);
			if (this.InPlayLayer)
			{
				if (this.room.GetTile(this.pos).Solid)
				{
					this.life -= 0.025f;
					if (!this.room.GetTile(this.lastPos).Solid)
					{
						IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, this.room.GetTilePosition(this.lastPos), this.room.GetTilePosition(this.pos));
						FloatRect floatRect = Custom.RectCollision(this.pos, this.lastPos, this.room.TileRect(intVector ?? this.pos.ToIntVector2()).Grow(2f));
						this.pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
						float num = 0.3f;
						if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
						{
							this.vel.x = Mathf.Abs(this.vel.x) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
						{
							this.vel.x = -Mathf.Abs(this.vel.x) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
						{
							this.vel.y = Mathf.Abs(this.vel.y) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
						{
							this.vel.y = -Mathf.Abs(this.vel.y) * num;
						}
					}
					else
					{
						this.pos.y = this.room.MiddleOfTile(this.pos).y + 10f;
					}
				}
				if (this.room.PointSubmerged(this.pos))
				{
					this.pos.y = this.room.FloatWaterLevel(this.pos.x);
					this.life -= 0.025f;
				}
			}
			if (this.life < 0f || (Custom.VectorRectDistance(this.pos, this.room.RoomRect) > 100f && !this.room.ViewedByAnyCamera(this.pos, 400f)))
			{
				this.Destroy();
			}
			if (!this.room.BeingViewed)
			{
				this.Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel", true);
			if (this.depth < 0f)
			{
				sLeaser.sprites[0].scaleX = Custom.LerpMap(this.depth, 0f, -0.5f, 1.5f, 2f);
			}
			else if (this.depth > 0f)
			{
				sLeaser.sprites[0].scaleX = Custom.LerpMap(this.depth, 0f, 5f, 1.5f, 0.1f);
			}
			else
			{
				sLeaser.sprites[0].scaleX = 1.5f;
			}
			sLeaser.sprites[0].anchorY = 0f;
			if (this.depth > 0f)
			{
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
				sLeaser.sprites[0].alpha = 0f;
			}
			this.AddToContainer(sLeaser, rCam, null!);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker));
			sLeaser.sprites[0].scaleY = Mathf.Max(2f, 2f + 1.1f * Vector2.Distance(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker)));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			//this.col = palette.blackColor;
			if (this.depth <= 0f)
			{
				sLeaser.sprites[0].color = this.col;
			}
			else
			{
				sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, this.col, Mathf.InverseLerp(0f, 5f, this.depth));
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer((!this.InPlayLayer) ? "Foreground" : "Items");
			sLeaser.sprites[0].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[0]);
		}

		private Vector2 dir;

		private Vector2 lastLastPos;

		public Color col;

		public float life;

		public float lifeTime;

		public float depth;
	}
