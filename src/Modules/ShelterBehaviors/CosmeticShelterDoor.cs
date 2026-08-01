namespace RegionKit.Modules.ShelterBehaviors
{
    /// <summary>
    /// Cosmetic version of shelter door, requiring an existing shelter door to be present in the room to mirror and add data to.
    /// Used in place of summoning more than one shelter door because the game only expects there to be one shelter door, but we want more than one.
    /// </summary>
	public class CosmeticShelterDoor : CosmeticSprite
    {
        public ShelterDoor ParentDoor => room.shelterDoor;
        public IntVector2 IntDir => dir.ToIntVector2();

        public IntVector2 origPos;
        public Vector2 dir;
        public Vector2 pZero;
        public Vector2 perp;

        private readonly float[,] segmentPairs, pistons, covers, pumps;
        public float closedFac, closeSpeed;

        private bool initializedCloseTiles = false;

        public float Closed => ParentDoor.Closed;
        public float PistonsClosed => ParentDoor.PistonsClosed;
        public float FlapsOpen => ParentDoor.FlapsOpen;
        public float PumpsExit => ParentDoor.PumpsExit;
        public float Cylinders => ParentDoor.Cylinders;

        public CosmeticShelterDoor(Room room, IntVector2 pZero, IntVector2 dir)
        {
            origPos = pZero;
            this.room = room;
            this.pZero = room.MiddleOfTile(pZero);
            this.dir = dir.ToVector2();

            perp = Custom.PerpendicularVector(this.dir);
            segmentPairs = new float[5, 3];
            for (int i = 0; i < 5; i++)
            {
                segmentPairs[i, 0] = closedFac;
                segmentPairs[i, 1] = closedFac;
            }
            pistons = new float[2, 3];
            for (int i = 0; i < 2; i++)
            {
                pistons[i, 0] = closedFac;
                pistons[i, 1] = closedFac;
            }
            covers = new float[4, 3];
            for (int i = 0; i < 4; i++)
            {
                covers[i, 0] = closedFac;
                covers[i, 1] = closedFac;
            }
            pumps = new float[8, 3];
            for (int i = 0; i < 8; i++)
            {
                pumps[i, 0] = closedFac;
                pumps[i, 1] = closedFac;
            }

            closeSpeed = 0.003125f;
        }

		public override void Update(bool eu)
		{
			base.Update(eu);
            if (ParentDoor == null) return;

            if (!initializedCloseTiles)
            {
                initializedCloseTiles = true;
                Array.Resize(ref ParentDoor.closeTiles, ParentDoor.closeTiles.Length + 4);
                for (int i = 0; i < 4; i++)
                {
                    ParentDoor.closeTiles[ParentDoor.closeTiles.Length - 1 - i] = room.GetTilePosition(pZero) + dir.ToIntVector2() * (i + 2);
                }
                pZero += dir * 60f;
            }

            closedFac = ParentDoor.closedFac;
            closeSpeed = ParentDoor.closeSpeed;

            for (int i = 0; i < segmentPairs.GetLength(0); i++)
            {
                for (int j = 0; j < segmentPairs.GetLength(1); j++)
                {
                    segmentPairs[i, j] = ParentDoor.segmentPairs[i, j];
                }
            }
            for (int i = 0; i < pistons.GetLength(0); i++)
            {
                for (int j = 0; j < pistons.GetLength(1); j++)
                {
                    pistons[i, j] = ParentDoor.pistons[i, j];
                }
            }
            for (int i = 0; i < covers.GetLength(0); i++)
            {
                for (int j = 0; j < covers.GetLength(1); j++)
                {
                    covers[i, j] = ParentDoor.covers[i, j];
                }
            }
            for (int i = 0; i < pumps.GetLength(0); i++)
            {
                for (int j = 0; j < pumps.GetLength(1); j++)
                {
                    pumps[i, j] = ParentDoor.pumps[i, j];
                }
            }
		}

		private int CogSprite(int cog) => cog;
		private int PistonSprite(int piston) => 4 + piston;
		private int PlugSprite(int plug) => 6 + plug;
		private int SegmentSprite(int segment) => 14 + segment;
		private int CylinderSprite(int cylinder) => 24 + cylinder;
		private int CoverSprite(int cover) => 28 + cover;
		private int PumpSprite(int pump) => 32 + pump;
		private int FlapSprite(int flap) => 40 + flap;

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[42];
            for (int j = 0; j < 4; j++)
            {
				sLeaser.sprites[CogSprite(j)] = new FSprite("ShelterGate_cog", true)
				{
					alpha = 1f - ((j < 2) ? 6f : 5f) / 30f
				};
			}
            for (int k = 0; k < 2; k++)
            {
				sLeaser.sprites[PistonSprite(k)] = new FSprite("ShelterGate_piston" + (k + 1).ToString(), true)
				{
					alpha = 0.9f
				};
			}
            for (int l = 0; l < 8; l++)
            {
                sLeaser.sprites[PlugSprite(l)] = new FSprite("ShelterGate_plug" + (l + 1).ToString(), true);
            }
            for (int m = 0; m < 10; m++)
            {
                sLeaser.sprites[SegmentSprite(m)] = new FSprite("ShelterGate_segment" + (m + 1).ToString(), true);
            }
            for (int n = 0; n < 4; n++)
            {
                sLeaser.sprites[CylinderSprite(n)] = new FSprite("ShelterGate_cylinder" + (n + 1).ToString(), true);
            }
            for (int num = 0; num < 4; num++)
            {
                sLeaser.sprites[CoverSprite(num)] = new FSprite("ShelterGate_cover" + (num + 1).ToString(), true);
            }
            for (int num2 = 0; num2 < 8; num2++)
            {
                sLeaser.sprites[PumpSprite(num2)] = new FSprite("ShelterGate_pump" + (num2 + 1).ToString(), true);
            }
            for (int num3 = 0; num3 < 2; num3++)
            {
				sLeaser.sprites[FlapSprite(num3)] = new FSprite("ShelterGate_Hatch", true)
				{
					anchorX = 0.2f,
					anchorY = 0.43f
				};
				if (num3 == 1)
                {
                    sLeaser.sprites[FlapSprite(num3)].scaleX = -1f;
                }
            }
            float num4 = Custom.AimFromOneVectorToAnother(dir, new Vector2(0f, 0f));
            for (int num5 = 0; num5 < sLeaser.sprites.Length; num5++)
            {
                sLeaser.sprites[num5].rotation = num4;
                sLeaser.sprites[num5].shader = room.game.rainWorld.Shaders["ColoredSprite2"];
            }
            AddToContainer(sLeaser, rCam, null!);
        }

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (ParentDoor == null)
            {
                foreach (FSprite sprite in sLeaser.sprites)
                {
                    sprite.isVisible = false;
                }
                return;
            }
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.isVisible = true;
            }

            camPos.x += 0.25f;
            camPos.y += 0.25f;
            for (int j = 0; j < 4; j++)
            {
                Vector2 vector = perp * (((j % 2 == 0) ? (-1f) : 1f) * ((j >= 2) ? 35f : 55f));
                vector -= dir * ((j >= 2) ? 50f : 15f);
                sLeaser.sprites[CogSprite(j)].x = pZero.x - camPos.x + vector.x;
                sLeaser.sprites[CogSprite(j)].y = pZero.y - camPos.y + vector.y;
                sLeaser.sprites[CogSprite(j)].rotation = Closed * ((j >= 2) ? 400f : (-150f)) * ((j % 2 == 0) ? (-1f) : 1f);
            }
            for (int k = 0; k < 10; k++)
            {
                int num = k / 2;
                Vector2 vector2 = perp * (Mathf.Pow(1f - Mathf.Lerp(segmentPairs[num, 1], segmentPairs[num, 0], timeStacker), 0.75f) * 55f * ((k % 2 == 0) ? (-1f) : 1f));
                sLeaser.sprites[SegmentSprite(k)].x = pZero.x - camPos.x + vector2.x;
                sLeaser.sprites[SegmentSprite(k)].y = pZero.y - camPos.y + vector2.y;
                sLeaser.sprites[SegmentSprite(k)].alpha = 1f - 4f * Mathf.InverseLerp(0.78f, 0.61f, Closed) / 30f;
            }
            for (int l = 0; l < 2; l++)
            {
                Vector2 vector3 = dir * ((1f - Mathf.Lerp(pistons[l, 1], pistons[l, 0], timeStacker)) * -120f);
                if (PistonsClosed > 0f)
                {
                    vector3 += perp * (5f * ((l == 0) ? (-1f) : 1f));
                }
                sLeaser.sprites[PistonSprite(l)].x = pZero.x - camPos.x + vector3.x;
                sLeaser.sprites[PistonSprite(l)].y = pZero.y - camPos.y + vector3.y;
            }
            for (int m = 0; m < 4; m++)
            {
                Vector2 vector4 = perp * (Mathf.Pow(1f - Mathf.Lerp(covers[m, 1], covers[m, 0], timeStacker), 2.5f) * 65f * ((m >= 2) ? (-1f) : 1f));
                sLeaser.sprites[CoverSprite(m)].x = pZero.x - camPos.x + vector4.x;
                sLeaser.sprites[CoverSprite(m)].y = pZero.y - camPos.y + vector4.y;
            }
            for (int n = 0; n < 4; n++)
            {
                if ((float)n / 4f < Cylinders)
                {
                    sLeaser.sprites[CylinderSprite(n)].x = pZero.x - camPos.x;
                    sLeaser.sprites[CylinderSprite(n)].y = pZero.y - camPos.y;
                    sLeaser.sprites[CylinderSprite(n)].isVisible = true;
                }
                else
                {
                    sLeaser.sprites[CylinderSprite(n)].isVisible = false;
                }
            }
            for (int num2 = 0; num2 < 8; num2++)
            {
                Vector2 vector5 = perp * (Mathf.Lerp(pumps[num2, 1], pumps[num2, 0], timeStacker) * -42f * ((num2 % 2 == 0) ? (-1f) : 1f));
                vector5 += -dir * (PumpsExit * 80f);
                sLeaser.sprites[PumpSprite(num2)].x = pZero.x - camPos.x + vector5.x;
                sLeaser.sprites[PumpSprite(num2)].y = pZero.y - camPos.y + vector5.y;
                vector5 = perp * (Mathf.Clamp(1f - Mathf.Lerp(pumps[num2, 1], pumps[num2, 0], timeStacker) - 0.35f, 0f, 1f) * 60f * ((num2 % 2 == 0) ? (-1f) : 1f));
                sLeaser.sprites[PlugSprite(num2)].x = pZero.x - camPos.x + vector5.x;
                sLeaser.sprites[PlugSprite(num2)].y = pZero.y - camPos.y + vector5.y;
            }
            for (int num3 = 0; num3 < 2; num3++)
            {
                Vector2 vector6 = pZero + dir * 46f + perp * (Mathf.Lerp(15f, 25f, FlapsOpen) * ((num3 == 0) ? 1f : (-1f)));
                sLeaser.sprites[FlapSprite(num3)].x = vector6.x - camPos.x;
                sLeaser.sprites[FlapSprite(num3)].y = vector6.y - camPos.y;
                sLeaser.sprites[FlapSprite(num3)].rotation = Custom.AimFromOneVectorToAnother(-dir, dir) - 90f * ((num3 == 0) ? (-1f) : 1f) * FlapsOpen;
            }
        }

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
	}
}
