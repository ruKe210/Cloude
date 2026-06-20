using STS.Battle;
using STS.Cards;
using STS.Core;
using STS.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace STS.Presentation
{
    /// <summary>
    /// Demo0 专用：白模场景 + 仅玩家攻击敌人 + 受击震屏。
    /// </summary>
    public class Demo0BattleController : MonoBehaviour
    {
        [Header("World Views")]
        [SerializeField] private WorldEntityView playerWorldView;
        [SerializeField] private WorldEntityView enemyWorldView;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private OfficeBattleArtSet officeBattleArt;

        [Header("UI")]
        [SerializeField] private HandController handController;
        [SerializeField] private Text energyText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text outcomeText;

        private BattleManager _battleManager;
        private CombatActionManager _actionManager;

        private void Start()
        {
            SetupSceneCamera();
            ResolveWorldReferences();
            EnsureUi();
            SetupOfficeBattleVisuals();
            EnsureCameraShake();

            var rng = new System.Random();
            var context = new BattleContext(new PlayerEntity(rng), new EnemyEntity(rng), rng);
            _actionManager = new CombatActionManager();
            _battleManager = new BattleManager();
            _battleManager.Initialize(context, _actionManager);

            _actionManager.StateChanged += RefreshAllViews;
            _actionManager.DamageDealt += OnDamageDealt;
            _actionManager.BattleEnded += OnBattleEnded;

            playerWorldView?.Bind(context.Player);
            enemyWorldView?.Bind(context.Enemy);
            handController?.Initialize(_battleManager, _actionManager, attackOnlyMode: true);

            StartDemoBattle();
            RefreshAllViews();
        }

        private void Update()
        {
            _actionManager?.ProcessTick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_actionManager == null)
            {
                return;
            }

            _actionManager.StateChanged -= RefreshAllViews;
            _actionManager.DamageDealt -= OnDamageDealt;
            _actionManager.BattleEnded -= OnBattleEnded;
        }

        private void StartDemoBattle()
        {
            BuildAttackDeck(_battleManager.Context.Player);
            _battleManager.Context.Player.DrawPile.Shuffle();
            _battleManager.TurnManager.StartFirstPlayerTurn(_battleManager.Context);
            _actionManager.NotifyStateChanged();
        }

        private static void BuildAttackDeck(PlayerEntity player)
        {
            for (var i = 0; i < 4; i++)
            {
                player.DrawPile.Add(new StrikeCard());
            }

            player.DrawPile.Add(new BashCard());
        }

        private void RefreshAllViews()
        {
            playerWorldView?.Refresh();
            enemyWorldView?.Refresh();
            handController?.Refresh();

            if (energyText != null && _battleManager != null)
            {
                var player = _battleManager.Context.Player;
                energyText.text = $"能量 {player.Energy}/{player.MaxEnergy}";
            }

            if (hintText != null && _battleManager != null)
            {
                hintText.text = _battleManager.Context.IsBattleOver
                    ? string.Empty
                    : "点击手牌攻击敌人（Demo0：仅玩家出牌）";
            }

            if (outcomeText != null && _battleManager != null)
            {
                outcomeText.text = _battleManager.Context.Outcome switch
                {
                    BattleOutcome.Victory => "胜利！敌人被击败",
                    BattleOutcome.Defeat => "失败...",
                    _ => string.Empty
                };
            }
        }

        private void OnDamageDealt(CombatEntity target, int damage, CombatEntity source)
        {
            if (damage <= 0)
            {
                return;
            }

            if (source == _battleManager.Context.Player)
            {
                playerWorldView?.PlayAttack();
            }

            if (target == _battleManager.Context.Enemy)
            {
                enemyWorldView?.PlayHitFeedback();
                cameraShake?.Shake();
            }

            Debug.Log($"[Demo0] {source.DisplayName} -> {target.DisplayName} : {damage}");
        }

        private void OnBattleEnded(BattleOutcome outcome)
        {
            RefreshAllViews();
        }

        private void SetupOfficeBattleVisuals()
        {
            officeBattleArt ??= OfficeBattleArtLoader.Load();
            OfficeBattleRuntimeSetup.ApplyToScene(officeBattleArt);
            playerWorldView?.ConfigureVisuals(officeBattleArt, true);
            enemyWorldView?.ConfigureVisuals(officeBattleArt, false);
        }

        private void SetupSceneCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 2f, -10f);
            cam.transform.rotation = Quaternion.identity;

            if (cameraShake == null)
            {
                cameraShake = cam.GetComponent<CameraShake>();
            }
        }

        private void ResolveWorldReferences()
        {
            if (playerWorldView == null)
            {
                var player = GameObject.Find("PlayerWhiteBox");
                if (player != null)
                {
                    playerWorldView = player.GetComponent<WorldEntityView>();
                    if (playerWorldView == null)
                    {
                        playerWorldView = player.AddComponent<WorldEntityView>();
                    }
                }
            }

            if (enemyWorldView == null)
            {
                var enemy = GameObject.Find("EnemyWhiteBox");
                if (enemy != null)
                {
                    enemyWorldView = enemy.GetComponent<WorldEntityView>();
                    if (enemyWorldView == null)
                    {
                        enemyWorldView = enemy.AddComponent<WorldEntityView>();
                    }
                }
            }

            playerWorldView?.Setup(null, true);
            enemyWorldView?.Setup(null, false);
        }

        private void EnsureCameraShake()
        {
            if (cameraShake != null)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cameraShake = cam.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = cam.gameObject.AddComponent<CameraShake>();
            }
        }

        private void EnsureUi()
        {
            var oldCanvas = GameObject.Find("Demo0Canvas");
            if (oldCanvas != null)
            {
                Destroy(oldCanvas);
            }

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var canvasObject = new GameObject("Demo0Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            energyText = CreateLabel(canvasObject.transform, "EnergyText", new Vector2(0.5f, 0.92f), new Vector2(300f, 50f), "能量");

            hintText = CreateLabel(canvasObject.transform, "HintText", new Vector2(0.5f, 0.86f), new Vector2(700f, 40f), string.Empty);
            hintText.fontSize = 20;
            hintText.color = new Color(0.85f, 0.85f, 0.85f);

            outcomeText = CreateLabel(canvasObject.transform, "OutcomeText", new Vector2(0.5f, 0.5f), new Vector2(500f, 80f), string.Empty);
            outcomeText.fontSize = 42;
            outcomeText.fontStyle = FontStyle.Bold;

            var handObject = new GameObject("HandArea", typeof(RectTransform), typeof(Image));
            handObject.transform.SetParent(canvasObject.transform, false);
            var handRect = handObject.GetComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.5f, 0f);
            handRect.anchorMax = new Vector2(0.5f, 0f);
            handRect.pivot = new Vector2(0.5f, 0f);
            handRect.anchoredPosition = new Vector2(0f, 30f);
            handRect.sizeDelta = new Vector2(1100f, 180f);
            handObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);
            UiSpriteHelper.ApplyPanelImage(handObject.GetComponent<Image>());

            var cardContainerObject = new GameObject("CardContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardContainerObject.transform.SetParent(handObject.transform, false);
            var cardContainerRect = cardContainerObject.GetComponent<RectTransform>();
            cardContainerRect.anchorMin = Vector2.zero;
            cardContainerRect.anchorMax = Vector2.one;
            cardContainerRect.offsetMin = new Vector2(10f, 10f);
            cardContainerRect.offsetMax = new Vector2(-10f, -10f);

            var layout = cardContainerObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            handController = handObject.AddComponent<HandController>();
            handController.Setup(cardContainerRect);
        }

        private static Text CreateLabel(Transform parent, string name, Vector2 anchor, Vector2 size, string text)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 28;
            label.color = Color.white;
            label.text = text;
            ChineseFontHelper.ApplyTo(label);
            return label;
        }
    }
}
