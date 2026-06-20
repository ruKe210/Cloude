#if UNITY_EDITOR
using System.Linq;
#endif
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace STS.Presentation
{
    public static class OfficeBattleArtLoader
    {
        private const string ArtRoot = "Assets/Art/OfficeBattle";
        private const string AnimFolder = ArtRoot + "/Animations";

        public static OfficeBattleArtSet Load()
        {
#if UNITY_EDITOR
            var artSet = AssetDatabase.LoadAssetAtPath<OfficeBattleArtSet>(ArtRoot + "/OfficeBattleArtSet.asset");
            if (artSet == null)
            {
                artSet = BuildFromProjectAssets();
            }
            else
            {
                RefreshRuntimeSprites(artSet);
            }

            return artSet;
#else
            return Resources.Load<OfficeBattleArtSet>("OfficeBattle/OfficeBattleArtSet");
#endif
        }

#if UNITY_EDITOR
        private static void RefreshRuntimeSprites(OfficeBattleArtSet artSet)
        {
            artSet.background = LoadSprite(ArtRoot + "/Background/office_battle_background.png");
            artSet.playerPortrait = LoadSprite(ArtRoot + "/Characters/Player/player_programmer.png");
            artSet.enemyPortrait = LoadSprite(ArtRoot + "/Characters/Enemy/enemy_deadline.png");
            artSet.playerIdleFrames = LoadPlayerIdleFrames();
            artSet.playerAttackFrame = LoadSprite(ArtRoot + "/Characters/Player/player_attack_01.png");
            artSet.enemyIdleFrames = new[]
            {
                LoadSprite(ArtRoot + "/Characters/Enemy/enemy_idle_01.png"),
                LoadSprite(ArtRoot + "/Characters/Enemy/enemy_idle_02.png")
            };
            artSet.enemyAttackFrame = LoadSprite(ArtRoot + "/Characters/Enemy/enemy_attack_01.png");
            artSet.playerAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimFolder + "/Player.controller");
            artSet.enemyAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimFolder + "/Enemy.controller");
        }

        private static OfficeBattleArtSet BuildFromProjectAssets()
        {
            var artSet = ScriptableObject.CreateInstance<OfficeBattleArtSet>();
            artSet.background = LoadSprite(ArtRoot + "/Background/office_battle_background.png");
            artSet.playerPortrait = LoadSprite(ArtRoot + "/Characters/Player/player_programmer.png");
            artSet.enemyPortrait = LoadSprite(ArtRoot + "/Characters/Enemy/enemy_deadline.png");
            artSet.playerIdleFrames = LoadPlayerIdleFrames();
            artSet.playerAttackFrame = LoadSprite(ArtRoot + "/Characters/Player/player_attack_01.png");
            artSet.enemyIdleFrames = new[]
            {
                LoadSprite(ArtRoot + "/Characters/Enemy/enemy_idle_01.png"),
                LoadSprite(ArtRoot + "/Characters/Enemy/enemy_idle_02.png")
            };
            artSet.enemyAttackFrame = LoadSprite(ArtRoot + "/Characters/Enemy/enemy_attack_01.png");
            artSet.playerAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimFolder + "/Player.controller");
            artSet.enemyAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimFolder + "/Enemy.controller");
            return artSet;
        }

        private static Sprite[] LoadPlayerIdleFrames()
        {
            const string sheetPath = ArtRoot + "/Characters/Player/player_idle_02.png";
            var sheetSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
                .OfType<Sprite>()
                .OrderBy(sprite => ParseSheetIndex(sprite.name))
                .ToArray();

            if (sheetSprites.Length >= 3)
            {
                return new[] { sheetSprites[0], sheetSprites[1], sheetSprites[2] };
            }

            var fallback = LoadSprite(ArtRoot + "/Characters/Player/player_idle_01.png");
            return fallback != null ? new[] { fallback } : System.Array.Empty<Sprite>();
        }

        private static int ParseSheetIndex(string spriteName)
        {
            var parts = spriteName.Split('_');
            return parts.Length > 0 && int.TryParse(parts[^1], out var index) ? index : 0;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
#endif
    }

    public static class OfficeBattleRuntimeSetup
    {
        public static void ApplyToScene(OfficeBattleArtSet artSet)
        {
            if (artSet == null || artSet.background == null)
            {
                Debug.LogWarning("[OfficeBattle] Art set missing. Run STS/Build Office Battle Art or MCP build first.");
                return;
            }

            HidePrimitive("Floor");
            HidePrimitive("BackWall");
            SetupBackground(artSet.background);
        }

        public static void SetupBackground(Sprite background)
        {
            var backgroundObject = GameObject.Find("OfficeBackground");
            if (backgroundObject == null)
            {
                backgroundObject = new GameObject("OfficeBackground");
            }

            backgroundObject.transform.position = new Vector3(0f, 2.2f, 8f);
            var spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = background;
            spriteRenderer.sortingOrder = -20;

            var spriteSize = background.bounds.size;
            if (spriteSize.y > 0.01f)
            {
                var targetHeight = 10f;
                var scale = targetHeight / spriteSize.y;
                backgroundObject.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static void HidePrimitive(string objectName)
        {
            var target = GameObject.Find(objectName);
            if (target == null)
            {
                return;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}
