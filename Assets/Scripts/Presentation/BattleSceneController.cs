using STS.Battle;
using STS.Core;
using STS.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STS.Presentation
{
    /// <summary>
    /// 战斗场景入口：创建 BattleContext / Manager / ActionManager，绑定 UI 并每帧驱动 ProcessTick。
    /// </summary>
    public class BattleSceneController : MonoBehaviour
    {
        [Header("Optional References")]
        [SerializeField] private EntityView playerView;
        [SerializeField] private EntityView enemyView;
        [SerializeField] private HandController handController;
        [SerializeField] private BattleHUD battleHud;

        [Header("Runtime UI")]
        [SerializeField] private bool autoCreateUi = true;

        private BattleManager _battleManager;
        private CombatActionManager _actionManager;

        private void Start()
        {
            if (autoCreateUi && (playerView == null || enemyView == null || handController == null || battleHud == null))
            {
                CreateRuntimeUi();
            }

            var rng = new System.Random();
            var context = new BattleContext(new PlayerEntity(rng), new EnemyEntity(rng), rng);
            _actionManager = new CombatActionManager();
            _battleManager = new BattleManager();
            _battleManager.Initialize(context, _actionManager);

            _actionManager.StateChanged += RefreshAllViews;
            _actionManager.DamageDealt += OnDamageDealt;
            _actionManager.BattleEnded += OnBattleEnded;

            playerView?.Bind(context.Player);
            enemyView?.Bind(context.Enemy);
            handController?.Initialize(_battleManager, _actionManager);
            battleHud?.Initialize(_battleManager);

            _battleManager.StartBattle();
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

        private void RefreshAllViews()
        {
            playerView?.Refresh();
            enemyView?.Refresh();
            handController?.Refresh();
            battleHud?.Refresh();
        }

        private void OnDamageDealt(CombatEntity target, int damage, CombatEntity source)
        {
            if (damage <= 0)
            {
                return;
            }

            if (target == _battleManager.Context.Player && playerView != null)
            {
                playerView.FlashDamage();
            }
            else if (target == _battleManager.Context.Enemy && enemyView != null)
            {
                enemyView.FlashDamage();
            }

            Debug.Log($"{source.DisplayName} 对 {target.DisplayName} 造成 {damage} 点伤害");
        }

        private void OnBattleEnded(BattleOutcome outcome)
        {
            Debug.Log(outcome == BattleOutcome.Victory ? "战斗胜利！" : "战斗失败...");
            RefreshAllViews();
        }

        private void CreateRuntimeUi()
        {
            var canvasObject = new GameObject("BattleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            playerView = CreateEntityPanel(canvasObject.transform, "PlayerPanel", new Vector2(0.08f, 0.72f), false);
            enemyView = CreateEntityPanel(canvasObject.transform, "EnemyPanel", new Vector2(0.72f, 0.72f), true);

            var hudObject = CreateAnchoredPanel(canvasObject.transform, "HUD", new Vector2(0.5f, 0.95f), new Vector2(700f, 120f));
            battleHud = hudObject.AddComponent<BattleHUD>();
            var energyText = CreateHudLabel(hudObject.transform, "EnergyText", new Vector2(-220f, 0f), "能量");
            var turnText = CreateHudLabel(hudObject.transform, "TurnText", new Vector2(0f, 0f), "回合");
            var phaseText = CreateHudLabel(hudObject.transform, "PhaseText", new Vector2(220f, 0f), "阶段");
            var outcomeText = CreateHudLabel(hudObject.transform, "OutcomeText", new Vector2(0f, -35f), string.Empty);

            var endTurnObject = new GameObject("EndTurnButton", typeof(RectTransform), typeof(Image), typeof(Button));
            endTurnObject.transform.SetParent(hudObject.transform, false);
            var endTurnRect = endTurnObject.GetComponent<RectTransform>();
            endTurnRect.anchoredPosition = new Vector2(0f, 30f);
            endTurnRect.sizeDelta = new Vector2(180f, 42f);
            endTurnObject.GetComponent<Image>().color = new Color(0.2f, 0.55f, 0.25f);
            var endTurnLabel = CreateHudLabel(endTurnObject.transform, "Label", Vector2.zero, "结束回合");
            endTurnLabel.alignment = TextAlignmentOptions.Center;
            endTurnLabel.rectTransform.anchorMin = Vector2.zero;
            endTurnLabel.rectTransform.anchorMax = Vector2.one;
            endTurnLabel.rectTransform.offsetMin = Vector2.zero;
            endTurnLabel.rectTransform.offsetMax = Vector2.zero;
            battleHud.Setup(energyText, turnText, phaseText, outcomeText, endTurnObject.GetComponent<Button>());

            var handObject = CreateAnchoredPanel(canvasObject.transform, "HandArea", new Vector2(0.5f, 0.08f), new Vector2(1200f, 180f));
            var handLayout = handObject.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 12f;
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childControlWidth = false;
            handLayout.childControlHeight = false;
            handLayout.childForceExpandWidth = false;
            handLayout.childForceExpandHeight = false;

            handController = handObject.AddComponent<HandController>();
            handController.Setup(handObject.GetComponent<RectTransform>());
        }

        private EntityView CreateEntityPanel(Transform parent, string name, Vector2 anchor, bool showIntent)
        {
            var panel = CreateAnchoredPanel(parent, name, anchor, new Vector2(360f, 220f));
            var view = panel.AddComponent<EntityView>();
            var text = CreateHudLabel(panel.transform, "Status", Vector2.zero, string.Empty);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(12f, 12f);
            text.rectTransform.offsetMax = new Vector2(-12f, -12f);
            view.Setup(text, showIntent);
            ChineseFontHelper.ApplyTo(text);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            return view;
        }

        private GameObject CreateAnchoredPanel(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            var rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            return panelObject;
        }

        private TMP_Text CreateHudLabel(Transform parent, string name, Vector2 anchoredPosition, string defaultText)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(260f, 40f);
            var text = labelObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = 24f;
            text.text = defaultText;
            ChineseFontHelper.ApplyTo(text);
            return text;
        }
    }
}
