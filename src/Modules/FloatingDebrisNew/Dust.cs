using Watcher;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RegionKit.Modules.FloatingDebrisNew
{
	internal class Dust : FloatingDebris.Floater, IDrawable
	{
		private float rotation;
		private float lastRotation;
		private int step;
		private int totalFloaters;
		private int totalConPoints;
		private readonly bool debug;
		private Vector2 splinePos;
		private Vector2 lastOrigPos;
		private DebugSprite? debugPosSprite;
		private DebugSprite? debugTargetSprite;
		private DebugSprite? debugLineSprite;
		private Vector2 random;
		private float shaderTime;
		private float speed;
		private float intensity;
		public Vector2 smoothPos;
		public bool white;

		public new Vector2 getPos
		{
			get
			{
				preMovementPos = origPos + offset * offsetAmount;
				float num = Mathf.Sin(time + offset.x * 3.1415927f * 2f) * offset.y * 10f;
				num += Mathf.Sin(time + origPos.x / 20f) * 10f;
				num *= 1f - depth;
				return preMovementPos + Vector2.up * (num * movement);
			}
		}

		public Dust(FloatingDebris.FloaterData data, bool white) : base(data)
		{
			this.white = white;
			random = new Vector2(Random.value, Random.value);
		}

		public override void Update(bool eu)
		{
			InitDebug();
			UpdateCounts();
			UpdateRotation();
			intensity = Mathf.Clamp01((extraOffsets.x + 1f) * 0.5f * (1f + random.y * 0.3f));
			speed = Mathf.Clamp01((extraOffsets.y + 1f) * 0.5f);
			lastOrigPos = origPos;
			if (room.BeingViewed)
			{
				time += Time.fixedDeltaTime * 2f * Mathf.Clamp01(Mathf.Abs(speed * 2f - 1f) * 3f);
			}
			lastPos = pos;
			pos = getPos;
		}

		private void UpdateCounts()
		{
			step = Mathf.Max(totalFloaters / Mathf.Max(totalConPoints, 1), 2);
			totalFloaters = owner.floaters.Count;
			totalConPoints = owner.data.controlPointPosX.Count;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (debug)
			{
				debugTargetSprite!.Destroy();
				debugPosSprite!.Destroy();
				debugLineSprite!.Destroy();
			}
		}

		private void InitDebug()
		{
			if (!debug || debugPosSprite != null)
			{
				return;
			}
			debugPosSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White", true)
			{
				color = Color.red
			}, room);
			debugTargetSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White", true)
			{
				color = Color.green
			}, room);
			debugLineSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White", true)
			{
				color = Color.blue,
				anchorY = 0f,
				scaleX = 0.12f
			}, room);
			room.AddObject(debugPosSprite);
			room.AddObject(debugTargetSprite);
			room.AddObject(debugLineSprite);
		}

		private void UpdateRotation()
		{
			lastRotation = rotation;
			Vector2 offset = this.offset;
			rotation = GetAngle() + 90f;
			if (debug)
			{
				debugPosSprite!.pos = preMovementPos;
				debugTargetSprite!.pos = offset;
				debugLineSprite!.pos = preMovementPos;
				debugLineSprite!.sprite.rotation = rotation;
				debugLineSprite!.sprite.scaleY = (offset - pos).magnitude;
			}
		}

		private Vector2 ConPos(int i)
		{
			return owner.data.ConPos(i);
		}

		private float GetAngle()
		{
			if (totalConPoints <= 1)
			{
				return VecToDeg(offset);
			}
			float num = VecToDeg(Vector2.Perpendicular(ConPos(index) - ConPos(index - 1)));
			float num2 = VecToDeg(Vector2.Perpendicular(ConPos(index + 1) - ConPos(index)));
			if (index == 0 && influence <= 0.5f)
			{
				return num2;
			}
			if (index == totalConPoints - 2 && influence > 0.5f)
			{
				return num2;
			}
			float num3 = VecToDeg(Vector2.Perpendicular(ConPos(index + 2) - ConPos(index + 1)));
			if (influence > 0.5f)
			{
				return Mathf.LerpAngle(num2, num3, influence - 0.5f);
			}
			return Mathf.LerpAngle(num, num2, influence + 0.5f);
		}

		public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White", true)
			{
				shader = rainWorld.Shaders[white ? "RKWhiteDust" : "RKDust"]
			};
			AddToContainer(sLeaser, rCam, null!);
		}

		public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			sLeaser.sprites[0].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
			sLeaser.sprites[0].color = new Color(random.x, Mathf.Clamp01(_depth + depthOffset * 0.5f), speed, intensity);
			sLeaser.sprites[0].scaleX = 2f * finalScale;
			sLeaser.sprites[0].scaleY = 4f * finalScale;
			if (slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
				return;
			}
		}

		public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		}

		public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public class DustSpawner(bool white) : IFloaterSpawner
		{
			private readonly bool white = white;

			public FloatingDebris.UIText GetUIText()
			{
				return uiText;
			}

			public virtual FloatingDebris.Floater Spawn(FloatingDebris.FloaterData data)
			{
				return new Dust(data, white);
			}

			public static FloatingDebris.UIText uiText = new()
			{
				depthFar = FloatingDebris.UIText.Title("Depth Far"),
				depthNear = FloatingDebris.UIText.Title("Depth Near"),
				scaleMax = FloatingDebris.UIText.Title("Scale Max"),
				scaleMin = FloatingDebris.UIText.Title("Scale Min"),
				scaleOffset = FloatingDebris.UIText.Title("Scale Offset"),
				depthOffset = FloatingDebris.UIText.Title("Depth Offset"),
				extraSlider1 = FloatingDebris.UIText.Title("Intensity"),
				extraSlider2 = FloatingDebris.UIText.Title("Wind Speed")
			};
		}
	}
}
