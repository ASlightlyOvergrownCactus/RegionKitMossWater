//RainSong by HelloThere

using System.IO;
using Music;

///<summary>
///Allows specifying a song file per region that will play when rain is coming
///</summary>
internal static class RainSong
{
    internal static void Enable()
    {
        On.RainWorldGame.ctor += GameCtorPatch;
        On.RainCycle.Update += RainUpdatePatch;
        On.Music.MusicPlayer.RainRequestStopSong += RainStopPatch;
    }

    internal static void Disable()
    {
        On.RainWorldGame.ctor -= GameCtorPatch;
        On.RainCycle.Update -= RainUpdatePatch;
        On.Music.MusicPlayer.RainRequestStopSong -= RainStopPatch;
    }

    static bool filesChecked = false;

    static readonly Dictionary<string, string> rainSongDict = new();

    static void GameCtorPatch(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig.Invoke(self, manager);

        if (!filesChecked)
        {
            string[] songArray = AssetManager.ListDirectory("music" + Path.DirectorySeparatorChar + "songs").Where(s => s.Contains("rainsong")).ToArray();
            if (songArray.Length != 0)
            {
                foreach (string song in songArray)
                {
                    string songName = Path.GetFileNameWithoutExtension(song);
                    string key = songName.Remove(2);
                    if (!rainSongDict.Keys.Contains(key))
                    {
                        rainSongDict.Add(key, songName);
                        Debug.Log("RainSong:  Registered Rain Song named " + song);
                    }
                }
            }

            filesChecked = true;
        }
        if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.name.Contains("rainsong")) manager.musicPlayer.song.FadeOut(100f);
    }

    static void RainUpdatePatch(On.RainCycle.orig_Update orig, RainCycle self)
    {
        orig.Invoke(self);
        RainWorldGame game = self.world.game;
        MusicPlayer player = game.manager.musicPlayer;
        if (self.RainApproaching < 0.5f && player != null && game.cameras[0].room.roomSettings.DangerType != RoomRain.DangerType.None && player.song == null && !game.cameras[0].room.abstractRoom.name.Contains("GATE") && !game.Players[0].realizedCreature.dead && rainSongDict.TryGetValue(self.world.region.name.ToLower(), out string songName))
        {
            Debug.Log("RainSong:  Playing end of cycle song");
			Song song = new(game.manager.musicPlayer, songName, MusicPlayer.MusicContext.StoryMode)
			{
                playWhenReady = true,
				Loop = true,
				priority = 100f,
				baseVolume = 0.33f,
				stopAtDeath = true,
				stopAtGate = true
			};
			player.song = song;
        }
    }

    static void RainStopPatch(On.Music.MusicPlayer.orig_RainRequestStopSong orig, MusicPlayer self)
    {
        if (self.song != null && self.song.name.Contains("rainsong")) return;
        orig.Invoke(self);
    }
}