#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Animations;
using UnityEngine;

namespace STS.Presentation.Editor
{
    public static class OfficeBattleArtBuilder
    {
        private const string ArtRoot = "Assets/Art/OfficeBattle";
        private const string AnimFolder = ArtRoot + "/Animations";
        private const string ArtSetPath = ArtRoot + "/OfficeBattleArtSet.asset";

        private static readonly string[] SingleSpriteTextures =
        {
            "/Background/office_battle_background.png",
            "/Characters/Player/player_programmer.png",
            "/Characters/Player/player_idle_01.png",
            "/Characters/Player/player_attack_01.png",
            "/Characters/Enemy/enemy_deadline.png",
            "/Characters/Enemy/enemy_idle_01.png",
            "/Characters/Enemy/enemy_idle_02.png",
            "/Characters/Enemy/enemy_attack_01.png"
        };

        private static readonly SpriteSheetDefinition[] SpriteSheets =
        {
            new SpriteSheetDefinition(
                "/Characters/Player/player_idle_02.png",
                columns: 4,
                rows: 3,
                idleFrameIndices: new[] { 0, 1, 2 })
        };

        [MenuItem("STS/Build Office Battle Art")]
        public static void BuildAll()
        {
            EnsureFolder(AnimFolder);
            ConfigureTextureImports();

            var background = LoadSingleSprite(ArtRoot + "/Background/office_battle_background.png");
            var playerPortrait = LoadSingleSprite(ArtRoot + "/Characters/Player/player_programmer.png");
            var playerIdleBase = LoadSingleSprite(ArtRoot + "/Characters/Player/player_idle_01.png");
            var playerIdleSheetFrames = LoadSpriteSheetFrames(
                ArtRoot + "/Characters/Player/player_idle_02.png",
                columns: 4,
                rows: 3,
                frameIndices: new[] { 0, 1, 2 });
            var playerIdleFrames = BuildPlayerIdleFrames(playerIdleBase, playerIdleSheetFrames);
            var playerAttack = LoadSingleSprite(ArtRoot + "/Characters/Player/player_attack_01.png");

            var enemyPortrait = LoadSingleSprite(ArtRoot + "/Characters/Enemy/enemy_deadline.png");
            var enemyIdle1 = LoadSingleSprite(ArtRoot + "/Characters/Enemy/enemy_idle_01.png");
            var enemyIdle2 = LoadSingleSprite(ArtRoot + "/Characters/Enemy/enemy_idle_02.png");
            var enemyIdleFrames = new[] { enemyIdle1, enemyIdle2 };
            var enemyAttack = LoadSingleSprite(ArtRoot + "/Characters/Enemy/enemy_attack_01.png");

            var playerIdleClip = CreateSpriteClip(
                AnimFolder + "/Player_Idle.anim",
                playerIdleFrames,
                loop: true,
                fps: 6f);
            var playerAttackClip = CreateSpriteClip(
                AnimFolder + "/Player_Attack.anim",
                new[] { playerAttack },
                loop: false,
                fps: 8f);
            var enemyIdleClip = CreateSpriteClip(
                AnimFolder + "/Enemy_Idle.anim",
                enemyIdleFrames,
                loop: true,
                fps: 4f);
            var enemyAttackClip = CreateSpriteClip(
                AnimFolder + "/Enemy_Attack.anim",
                new[] { enemyAttack },
                loop: false,
                fps: 8f);

            var playerController = CreateAnimatorController(
                AnimFolder + "/Player.controller",
                playerIdleClip,
                playerAttackClip);
            var enemyController = CreateAnimatorController(
                AnimFolder + "/Enemy.controller",
                enemyIdleClip,
                enemyAttackClip);

            var artSet = AssetDatabase.LoadAssetAtPath<OfficeBattleArtSet>(ArtSetPath);
            if (artSet == null)
            {
                artSet = ScriptableObject.CreateInstance<OfficeBattleArtSet>();
                AssetDatabase.CreateAsset(artSet, ArtSetPath);
            }

            artSet.background = background;
            artSet.playerPortrait = playerPortrait;
            artSet.enemyPortrait = enemyPortrait;
            artSet.playerIdleFrames = playerIdleFrames;
            artSet.playerAttackFrame = playerAttack;
            artSet.enemyIdleFrames = enemyIdleFrames;
            artSet.enemyAttackFrame = enemyAttack;
            artSet.playerAnimator = playerController;
            artSet.enemyAnimator = enemyController;

            EditorUtility.SetDirty(artSet);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[OfficeBattleArtBuilder] Ready. Player idle frames={playerIdleFrames.Length}, enemy idle frames={enemyIdleFrames.Length}.");
        }

        public static Sprite[] LoadPlayerIdleFramesForRuntime()
        {
            var playerIdleBase = LoadSingleSprite(ArtRoot + "/Characters/Player/player_idle_01.png");
            var sheetFrames = LoadSpriteSheetFrames(
                ArtRoot + "/Characters/Player/player_idle_02.png",
                columns: 4,
                rows: 3,
                frameIndices: new[] { 0, 1, 2 });
            return BuildPlayerIdleFrames(playerIdleBase, sheetFrames);
        }

        private static Sprite[] BuildPlayerIdleFrames(Sprite baseFrame, Sprite[] sheetFrames)
        {
            if (sheetFrames != null && sheetFrames.Length >= 3)
            {
                return sheetFrames.Where(sprite => sprite != null).ToArray();
            }

            if (baseFrame != null)
            {
                return new[] { baseFrame };
            }

            return System.Array.Empty<Sprite>();
        }

        private static void ConfigureTextureImports()
        {
            foreach (var relativePath in SingleSpriteTextures)
            {
                ConfigureSingleSprite(ArtRoot + relativePath);
            }

            foreach (var sheet in SpriteSheets)
            {
                SliceSpriteSheet(ArtRoot + sheet.RelativePath, sheet.Columns, sheet.Rows);
            }
        }

        private static void ConfigureSingleSprite(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }

        private static void SliceSpriteSheet(string assetPath, int columns, int rows)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100f;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            var cellWidth = texture.width / columns;
            var cellHeight = texture.height / rows;
            var baseName = Path.GetFileNameWithoutExtension(assetPath);
            var spriteRects = new List<SpriteRect>();

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    var index = row * columns + col;
                    spriteRects.Add(new SpriteRect
                    {
                        name = $"{baseName}_{index}",
                        spriteID = GUID.Generate(),
                        rect = new Rect(
                            col * cellWidth,
                            texture.height - (row + 1) * cellHeight,
                            cellWidth,
                            cellHeight),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        border = Vector4.zero
                    });
                }
            }

            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();
            importer.SaveAndReimport();
        }

        public static Sprite[] LoadSpriteSheetFrames(
            string assetPath,
            int columns,
            int rows,
            int[] frameIndices)
        {
            SliceSpriteSheet(assetPath, columns, rows);

            var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .OrderBy(sprite => ParseSheetIndex(sprite.name))
                .ToArray();

            if (frameIndices == null || frameIndices.Length == 0)
            {
                return sprites;
            }

            return frameIndices
                .Where(index => index >= 0 && index < sprites.Length)
                .Select(index => sprites[index])
                .ToArray();
        }

        private static int ParseSheetIndex(string spriteName)
        {
            var parts = spriteName.Split('_');
            if (parts.Length == 0)
            {
                return 0;
            }

            return int.TryParse(parts[^1], out var index) ? index : 0;
        }

        private static Sprite LoadSingleSprite(string assetPath)
        {
            ConfigureSingleSprite(assetPath);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static AnimationClip CreateSpriteClip(string assetPath, Sprite[] sprites, bool loop, float fps)
        {
            var validSprites = sprites.Where(sprite => sprite != null).ToArray();
            if (validSprites.Length == 0)
            {
                Debug.LogWarning($"[OfficeBattleArtBuilder] No sprites for clip: {assetPath}");
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            }

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, assetPath);
            }

            clip.frameRate = fps;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            var keys = new ObjectReferenceKeyframe[validSprites.Length];
            var step = 1f / Mathf.Max(fps, 1f);
            for (var i = 0; i < validSprites.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i * step,
                    value = validSprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController CreateAnimatorController(
            string assetPath,
            AnimationClip idleClip,
            AnimationClip attackClip)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            }

            while (controller.parameters.Length > 0)
            {
                controller.RemoveParameter(0);
            }

            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;
            foreach (var child in root.states)
            {
                root.RemoveState(child.state);
            }

            var idleState = root.AddState("Idle");
            idleState.motion = idleClip;
            var attackState = root.AddState("Attack");
            attackState.motion = attackClip;
            root.defaultState = idleState;

            var toAttack = idleState.AddTransition(attackState);
            toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            toAttack.hasExitTime = false;
            toAttack.duration = 0.05f;

            var toIdle = attackState.AddTransition(idleState);
            toIdle.hasExitTime = true;
            toIdle.exitTime = 0.9f;
            toIdle.duration = 0.05f;

            EditorUtility.SetDirty(controller);

            var resourcesFolder = "Assets/Resources/OfficeBattle";
            EnsureFolder(resourcesFolder);
            var resourcesPath = resourcesFolder + "/OfficeBattleArtSet.asset";
            var existing = AssetDatabase.LoadAssetAtPath<OfficeBattleArtSet>(resourcesPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(resourcesPath);
            }

            AssetDatabase.CopyAsset(ArtSetPath, resourcesPath);
            return controller;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            var leaf = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }

        private readonly struct SpriteSheetDefinition
        {
            public string RelativePath { get; }
            public int Columns { get; }
            public int Rows { get; }
            public int[] IdleFrameIndices { get; }

            public SpriteSheetDefinition(string relativePath, int columns, int rows, int[] idleFrameIndices)
            {
                RelativePath = relativePath;
                Columns = columns;
                Rows = rows;
                IdleFrameIndices = idleFrameIndices;
            }
        }
    }
}
#endif
