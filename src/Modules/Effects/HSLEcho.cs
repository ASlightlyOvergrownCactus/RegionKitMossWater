using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EffExt;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.CoralBrain;
using On.MoreSlugcats;
using RegionKit.Modules.Objects;

namespace RegionKit.Modules.Effects
{
	// By ASlightlyOvergrownCactus
	// Called before MossWaterRGB's Load Resources
	internal static class HSLEchoBuilder
	{
		internal static void __RegisterBuilder()
		{
			try
			{
				EffectDefinitionBuilder builder = new EffectDefinitionBuilder("HSLEcho");
				builder
					.AddFloatField("SkinLum", 0, 100, 1, 11)
					.AddFloatField("SkinSat", 0, 100, 1, 82)
					.AddFloatField("SkinHue", 0, 360, 1, 225)
	
					.AddFloatField("Luminosity", 0, 100, 1, 59)
					.AddFloatField("Saturation", 0, 100, 1, 95)
					.AddFloatField("Hue", 0, 360, 1, 252)
					
					.AddBoolField("AffectGoldFlakes", true)
					.AddBoolField("AffectPalette", true)
					.AddBoolField("AffectEchoSkin", true)
					.AddStringField("EchoPalette", "32")
					.SetUADFactory((room, data, firstTimeRealized) => new HSLEchoUAD(data))
					.SetCategory("RegionKit")
					.Register();
			}
			catch (Exception ex)
			{
				LogWarning($"Error on eff HSLEcho init {ex}");
			}
		}
	}
	
	internal class HSLEchoUAD : UpdatableAndDeletable
	{
		public EffectExtraData EffectData { get; }
		public HSLColor color;
		public HSLColor color2;
		public HSLEcho HslEcho;
		public bool affectGoldFlakes;
		public bool affectPalette;
		public bool affectEchoSkin;
		public string palette;
		

		public HSLEchoUAD(EffectExtraData effectData)
		{
			EffectData = effectData;
			Vector3 temp = RGB2HSL(Color.white);
			Vector3 endTemp = RGB2HSL(Color.black);
			color = new HSLColor(temp.x, temp.y, temp.z);
			color2 = new HSLColor(endTemp.x, endTemp.y, endTemp.z);
			affectGoldFlakes = true;
			affectPalette = true;
			affectEchoSkin = true;
			palette = "32";
			HslEcho = new HSLEcho();
		}

		public override void Update(bool eu)
		{
			color.hue = EffectData.GetFloat("Hue") / 360f;
			color.saturation = EffectData.GetFloat("Saturation") / 100f;
			color.lightness = EffectData.GetFloat("Luminosity") / 100f;
			affectGoldFlakes = EffectData.GetBool("AffectGoldFlakes");
			affectPalette = EffectData.GetBool("AffectPalette");
			affectEchoSkin = EffectData.GetBool("AffectEchoSkin");
			color2.hue = EffectData.GetFloat("SkinHue") / 360f;
			color2.saturation = EffectData.GetFloat("SkinSat") / 100f;
			color2.lightness = EffectData.GetFloat("SkinLum") / 100f;
			palette = EffectData.GetString("EchoPalette");

			if (HslEcho != null && room.BeingViewed)
			{
				Shader.SetGlobalColor("_InputHSLEchoColor2", HSL2RGB(color.hue, color.saturation, color.lightness));
				Shader.SetGlobalColor("_InputHSLEchoColor1", HSL2RGB(color2.hue, color2.saturation, color2.lightness));
			}
		}
	}

	public class HSLEcho
	{
		static bool loaded = false;
		private static int lastPal = 32;
		private static int lastPal2 = 32;
		
		public static void HEDLoadResources(RainWorld rw)
		{
			if (!loaded)
			{
				loaded = true;
				if (MossWaterUnlit.mossBundle != null)
				{
					rw.Shaders["HSLEcho"] = FShader.CreateShader("HSLEcho", MossWaterUnlit.mossBundle.LoadAsset<Shader>("Assets/shaders 1.9.03/RGBEchoSkin.shader"));
					rw.Shaders["HSLEchoTentacle"] = FShader.CreateShader("HSLEchoTentacle", MossWaterUnlit.mossBundle.LoadAsset<Shader>("Assets/shaders 1.9.03/RGBEchoTentacle.shader"));
				}
				else
				{
					LogMessage("HSLEcho must be loaded after MossWaterUnlit!");
				}
			}
		}

		internal static void Apply()
		{
			On.Ghost.ctor += GhostOnctor;
			On.Ghost.DrawSprites += GhostOnDrawSprites;
			On.GoldFlakes.GoldFlake.DrawSprites += GoldFlakeOnDrawSprites;
			On.Ghost.Rags.DrawSprites += RagsOnDrawSprites;
			On.RoomCamera.Update += RoomCameraOnUpdate;
		}
		internal static void Undo()
		{
			On.Ghost.ctor -= GhostOnctor;
			On.Ghost.DrawSprites -= GhostOnDrawSprites;
			On.GoldFlakes.GoldFlake.DrawSprites -= GoldFlakeOnDrawSprites;
			On.Ghost.Rags.DrawSprites -= RagsOnDrawSprites;
			On.RoomCamera.Update -= RoomCameraOnUpdate;
		}
		
		private static void RoomCameraOnUpdate(On.RoomCamera.orig_Update orig, RoomCamera self)
		{
			orig(self);
			if (self.room != null && self.room.roomSettings.GetEffect(_Enums.HSLEcho) != null)
			{
				if (self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.affectPalette == true && int.TryParse(self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault().palette, out int num))
				{
					if (num != lastPal)
					{
						self.LoadGhostPalette(num);
						self.ApplyFade();
						lastPal = num;
					}
				}
				else
				{
					if (lastPal != 32)
					{
						self.LoadGhostPalette(32);
						self.ApplyFade();
						lastPal = 32;
					}
				}
			}
		}

		
		private static void GhostOnctor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost)
		{
			orig(self, room, placedobject, worldghost);
			if (self.room != null && self.room.roomSettings.GetEffect(_Enums.HSLEcho) != null && self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.affectEchoSkin == true)
			{
				self.goldColor = self.room.updateList.OfType<RGBElectricDeathUAD>().FirstOrDefault()?.color ?? new Color(0.5294118f, 0.3647059f, 0.18431373f);
			}
		}
		
		private static void GhostOnDrawSprites(On.Ghost.orig_DrawSprites orig, Ghost self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
		{
			orig(self, sleaser, rcam, timestacker, campos);
			if (self.room != null && self.room.roomSettings.GetEffect(_Enums.HSLEcho) != null)
			{
				if (self.room.game.devToolsActive)
					self.fadeOut = 0f;
				// Controls echo skin
				if (self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.affectEchoSkin == true)
				{
					self.goldColor = self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.color.rgb ?? self.goldColor;
					sleaser.sprites[self.HeadMeshSprite].shader = rcam.game.rainWorld.Shaders["HSLEcho"];
					for (int i = 0; i < self.legs.GetLength(0); i++)
					{
						sleaser.sprites[self.ThightSprite(i)].shader = rcam.game.rainWorld.Shaders["HSLEcho"];
						sleaser.sprites[self.LowerLegSprite(i)].shader = rcam.game.rainWorld.Shaders["HSLEcho"];
						sleaser.sprites[self.ButtockSprite(i)].color = self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.color2.rgb ?? sleaser.sprites[self.ButtockSprite(i)].color;
					}
					sleaser.sprites[self.NeckConnectorSprite].color = self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.color2.rgb ?? sleaser.sprites[self.NeckConnectorSprite].color;
					
					for (int i = 0; i < (sleaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors.Length; i++)
					{
						(sleaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors[i] = self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.color2.rgb ?? (sleaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors[i];
					}
					
				}
				else
				{
					self.goldColor = new Color(0.5294118f, 0.3647059f, 0.18431373f);
					sleaser.sprites[self.HeadMeshSprite].shader = rcam.game.rainWorld.Shaders["GhostSkin"];
					for (int i = 0; i < self.legs.GetLength(0); i++)
					{
						sleaser.sprites[self.ThightSprite(i)].shader = rcam.game.rainWorld.Shaders["GhostSkin"];
						sleaser.sprites[self.LowerLegSprite(i)].shader = rcam.game.rainWorld.Shaders["GhostSkin"];
						sleaser.sprites[self.ButtockSprite(i)].color = self.blackColor;
					}
					sleaser.sprites[self.NeckConnectorSprite].color = self.blackColor;
					
					for (int i = 0; i < (sleaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors.Length; i++)
					{
						(sleaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors[i] = self.blackColor;
					}
				}
				
				if (self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.affectPalette == true && int.TryParse(self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault().palette, out int num))
				{
					if (num != lastPal2)
					{
						self.ApplyPalette(sleaser, rcam, rcam.currentPalette);
						lastPal2 = num;
					}
				}
				else
				{
					if (lastPal2 != 32)
					{
						self.ApplyPalette(sleaser, rcam, rcam.currentPalette);
						lastPal2 = 32;
					}
				}
			}
		}
		
		private static void RagsOnDrawSprites(On.Ghost.Rags.orig_DrawSprites orig, Ghost.Rags self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
		{
			orig(self, sleaser, rcam, timestacker, campos);
			if (self.ghost.room != null && self.ghost.room.roomSettings.GetEffect(_Enums.HSLEcho) != null)
			{
				if (self.ghost.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.affectEchoSkin == true)
				{
					for (int i = 0; i < self.segments.Length; i++)
					{
						sleaser.sprites[self.firstSprite + i].shader = rcam.room.game.rainWorld.Shaders["HSLEchoTentacle"];
					}
				}
				else
				{
					for (int i = 0; i < self.segments.Length; i++)
					{
						sleaser.sprites[self.firstSprite + i].shader = rcam.room.game.rainWorld.Shaders["TentaclePlant"];
					}
				}
			}
		}

		private static void GoldFlakeOnDrawSprites(On.GoldFlakes.GoldFlake.orig_DrawSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
		{
			orig(self, sleaser, rcam, timestacker, campos);
			if (self.room != null && self.room.roomSettings.GetEffect(_Enums.HSLEcho) != null && self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.affectGoldFlakes == true)
			{
				float f = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(RWCustom.Custom.DegToVec(45f), RWCustom.Custom.DegToVec(Mathf.Lerp(self.lastYRot, self.yRot, timestacker) * 57.29578f + Mathf.Lerp(self.lastRot, self.rot, timestacker))));
				float ghostMode = rcam.ghostMode;
				HSLColor tempColor = self.room.updateList.OfType<HSLEchoUAD>().FirstOrDefault()?.color ??
				                      new HSLColor(0.08611111f, 0.65f, 0.53f);
				
				Color c = RWCustom.Custom.HSL2RGB(tempColor.hue, tempColor.saturation, Mathf.Lerp(tempColor.lightness, 0f, ghostMode));
				Color d = RWCustom.Custom.HSL2RGB(tempColor.hue, Mathf.Lerp(1f, tempColor.saturation, ghostMode), Mathf.Lerp(1f, tempColor.lightness, ghostMode));
				sleaser.sprites[0].color = Color.Lerp(c, d, f);
			}
		}
	}
}
