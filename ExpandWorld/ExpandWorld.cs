using SFMF;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld
{

    public class ExpandWorld : IMod
    {

        private const string SettingsPath = @".\SFMF\ModSettings\ExpandWorld.csv";
        private float WorldScale { get; set; }
        private float WorldDensity { get; set; }
        private bool Unlocked { get; set; }
        private int TerrainIndex { get; set; }
		private LevelBottomFX LBot { get; set; }
		private LevelBottomTrigger LTrig { get; set; }

        public void Start()
        {
            var settings = File.ReadAllLines(SettingsPath);
            foreach (var line in settings)
            {
				var parts = line.Split(',');
                if (parts[0] == "Setting")
				{
					if (parts[1] == "WorldScale")
						WorldScale = float.Parse(parts[2]);
					/* Does not seem to work correctly ATM. Needs to influence probe behavior which is a private list in Terrain presets */
					if (parts[1] == "WorldDensity")
						WorldDensity = float.Parse(parts[2]);
					if (parts[1] == "Unlocked")
						Unlocked = bool.Parse(parts[2]);
					/* Keep at -1 to use default game behavior */
					if (parts[1] == "TerrainIndex")
						TerrainIndex = int.Parse(parts[2]);
				}
			}

			LevelGenerator.Singleton.transform.localScale = new Vector3(
				WorldScale * LevelGenerator.Singleton.transform.localScale.x,
				WorldScale/2f * LevelGenerator.Singleton.transform.localScale.y,
				WorldScale * LevelGenerator.Singleton.transform.localScale.z
			);

			if (TerrainIndex >= 0)
			{
				LevelGenerator.Singleton.DEBUG_PICK_TERRAINPRESET_BY_INDEX = true;
				LevelGenerator.Singleton.DEBUG_TERRAINPRESET_INDEX = TerrainIndex;
			}

			foreach (LevelGenerator.TerrainPresetSetup preset in LevelGenerator.Singleton.terrainPresetLibrary)
			{
				preset.preset.mainShapeCountRange = new Vector2(
					WorldDensity * preset.preset.mainShapeCountRange.x,
					WorldDensity * preset.preset.mainShapeCountRange.y
				);
				preset.preset.cosmeticShapeCountRange = new Vector2(
					WorldDensity * preset.preset.cosmeticShapeCountRange.x,
					WorldDensity * preset.preset.cosmeticShapeCountRange.y
				);
			}
			List<GameObject> mainShapeLibrary = new List<GameObject>(LevelGenerator.Singleton.mainShapeLibrary);
			List<GameObject> cosmeticShapeLibrary = new List<GameObject>(LevelGenerator.Singleton.cosmeticShapeLibrary);
			for (int i=0; i<WorldDensity-1; i++)
			{
				LevelGenerator.Singleton.mainShapeLibrary.AddRange(mainShapeLibrary);
				LevelGenerator.Singleton.cosmeticShapeLibrary.AddRange(cosmeticShapeLibrary);
			}

			if (Unlocked)
			{
				LTrig = FindObjectsOfType<LevelBottomTrigger>()[0];
				Destroy(LTrig);
			}


        }

        public void Update()
        {
			if (Unlocked && !LBot)
			{
				LBot = LocalGameManager.Singleton.player.GetComponent<LevelBottomFX>();
				LBot.enabled = false;
			}
        }
    }
}
