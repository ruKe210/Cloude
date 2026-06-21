using System.Collections;
using STS.Meta;
using STS.Meta.Core;
using STS.Meta.Data;
using STS.Meta.IO;
using STS.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace STS.Meta.Presentation
{
    public class NarrativeSceneController : MonoBehaviour
    {
        [SerializeField] private string scriptId = "prologue";
        [SerializeField] private Color backdropColor = new Color(0.08f, 0.09f, 0.12f);

        private NarrativeEngine _engine;
        private Text _speakerText;
        private Text _bodyText;
        private Text _hintText;
        private Text _titleText;
        private RectTransform _choiceContainer;
        private Coroutine _typingRoutine;
        private string _fullBodyText = string.Empty;
        private bool _typingComplete;

        private void Start()
        {
            SetupCamera();
            BuildUi();

            var script = NarrativeScriptLoader.Load(scriptId);
            if (script == null)
            {
                return;
            }

            if (_titleText != null)
            {
                _titleText.text = script.title ?? script.scriptId;
            }

            _engine = new NarrativeEngine(script);
            _engine.StateChanged += OnEngineStateChanged;

            if (MetaGameSession.HasPendingReturn && MetaGameSession.PendingScriptId == scriptId)
            {
                RestoreAfterBattle();
            }
            else
            {
                _engine.Start();
            }
        }

        private void OnDestroy()
        {
            if (_engine != null)
            {
                _engine.StateChanged -= OnEngineStateChanged;
            }
        }

        private void Update()
        {
            if (_engine == null || !Input.GetMouseButtonDown(0))
            {
                return;
            }

            switch (_engine.State)
            {
                case NarrativeState.PlayingLine:
                    CompleteTypingImmediately();
                    break;
                case NarrativeState.WaitingLineAdvance:
                    _engine.Advance();
                    break;
            }
        }

        private void RestoreAfterBattle()
        {
            var returnNode = MetaGameSession.ReturnNodeId;
            if (MetaGameSession.SavedFlags != null)
            {
                _engine.Context.Flags.CopyFrom(MetaGameSession.SavedFlags);
            }

            MetaGameSession.Clear();
            if (!string.IsNullOrEmpty(returnNode))
            {
                _engine.ResumeFromNode(returnNode);
            }
            else
            {
                _engine.Start();
            }
        }

        private void OnEngineStateChanged(NarrativeStateChangedEventArgs args)
        {
            switch (args.State)
            {
                case NarrativeState.PlayingLine:
                    ShowLine(args.Line);
                    break;
                case NarrativeState.WaitingLineAdvance:
                    ShowHint("点击继续");
                    break;
                case NarrativeState.WaitingChoice:
                    ShowChoice(args.Choice);
                    break;
                case NarrativeState.Ended:
                    ShowEnding(args.Ended);
                    break;
                case NarrativeState.BattleHandoff:
                    HandleBattle(args.Battle);
                    break;
            }
        }

        private void ShowLine(NarrativeLinePayload line)
        {
            ClearChoices();
            if (line == null)
            {
                return;
            }

            if (_speakerText != null)
            {
                _speakerText.text = string.IsNullOrEmpty(line.Speaker) ? string.Empty : $"【{line.Speaker}】";
            }

            _fullBodyText = line.Text ?? string.Empty;
            _typingComplete = false;
            ShowHint("点击跳过打字 / 继续");

            if (_typingRoutine != null)
            {
                StopCoroutine(_typingRoutine);
            }

            if (line.CharsPerSecond <= 0f)
            {
                if (_bodyText != null)
                {
                    _bodyText.text = _fullBodyText;
                }

                _typingComplete = true;
                _engine.MarkWaitingAdvance();
                return;
            }

            _typingRoutine = StartCoroutine(TypewriterRoutine(_fullBodyText, line.CharsPerSecond));
        }

        private IEnumerator TypewriterRoutine(string fullText, float charsPerSecond)
        {
            if (_bodyText != null)
            {
                _bodyText.text = string.Empty;
            }

            var interval = 1f / Mathf.Max(charsPerSecond, 1f);
            for (var i = 0; i < fullText.Length; i++)
            {
                if (_bodyText != null)
                {
                    _bodyText.text = fullText.Substring(0, i + 1);
                }

                yield return new WaitForSeconds(interval);
            }

            _typingComplete = true;
            _engine.MarkWaitingAdvance();
        }

        private void CompleteTypingImmediately()
        {
            if (_typingRoutine != null)
            {
                StopCoroutine(_typingRoutine);
                _typingRoutine = null;
            }

            if (_bodyText != null)
            {
                _bodyText.text = _fullBodyText;
            }

            if (!_typingComplete)
            {
                _typingComplete = true;
                _engine.SkipTyping();
            }
        }

        private void ShowChoice(NarrativeChoicePayload choice)
        {
            ClearChoices();
            if (_bodyText != null)
            {
                _bodyText.text = choice?.Prompt ?? string.Empty;
            }

            if (_speakerText != null)
            {
                _speakerText.text = "【选择】";
            }

            ShowHint("请选择一个选项");

            if (choice?.Options == null || _choiceContainer == null)
            {
                return;
            }

            for (var i = 0; i < choice.Options.Count; i++)
            {
                CreateChoiceButton(choice.Options[i], i);
            }
        }

        private void CreateChoiceButton(NarrativeOptionEntry option, int index)
        {
            var buttonObject = new GameObject($"Choice_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(_choiceContainer, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420f, 56f);

            var layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 420f;
            layoutElement.preferredHeight = 56f;

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.28f, 0.45f, 0.95f);
            UiSpriteHelper.ApplyPanelImage(image);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 6f);
            labelRect.offsetMax = new Vector2(-12f, -6f);

            var label = labelObject.GetComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 22;
            label.color = Color.white;
            label.text = option.text;
            ChineseFontHelper.ApplyTo(label);

            var capturedIndex = index;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => _engine.SelectOption(capturedIndex));
        }

        private void ClearChoices()
        {
            if (_choiceContainer == null)
            {
                return;
            }

            for (var i = _choiceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_choiceContainer.GetChild(i).gameObject);
            }
        }

        private void ShowEnding(NarrativeEndedPayload ended)
        {
            ClearChoices();
            if (_speakerText != null)
            {
                _speakerText.text = "【完】";
            }

            if (_bodyText != null)
            {
                _bodyText.text = $"剧本 {ended?.ScriptId ?? scriptId} 已结束。\n感谢游玩 Meta 演示。";
            }

            ShowHint(string.Empty);
        }

        private void HandleBattle(NarrativeBattlePayload battle)
        {
            if (battle == null)
            {
                return;
            }

            MetaGameSession.SaveForBattle(scriptId, battle.ReturnNodeId, _engine.Context.Flags);
            var sceneName = battle.SceneName;
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                if (_bodyText != null)
                {
                    _bodyText.text = $"（演示）即将进入战斗场景：{sceneName}\n当前 Build Settings 未包含该场景。";
                }

                ShowHint("战斗场景未加入 Build Settings");
            }
        }

        private void ShowHint(string text)
        {
            if (_hintText != null)
            {
                _hintText.text = text;
            }
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = backdropColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void BuildUi()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var canvasObject = new GameObject("MetaStoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _titleText = CreateLabel(canvasObject.transform, "TitleText", new Vector2(0.5f, 0.92f), new Vector2(800f, 60f), 34, FontStyle.Bold);
            _titleText.color = new Color(0.9f, 0.92f, 1f);

            var panelObject = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            var panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.04f);
            panelRect.anchorMax = new Vector2(0.95f, 0.28f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.72f);
            UiSpriteHelper.ApplyPanelImage(panelImage);

            _speakerText = CreateLabel(panelObject.transform, "SpeakerText", new Vector2(0.02f, 0.78f), new Vector2(500f, 36f), 24, FontStyle.Bold);
            _speakerText.alignment = TextAnchor.MiddleLeft;

            _bodyText = CreateLabel(panelObject.transform, "BodyText", new Vector2(0.02f, 0.08f), new Vector2(1700f, 140f), 26, FontStyle.Normal);
            _bodyText.alignment = TextAnchor.UpperLeft;

            _hintText = CreateLabel(canvasObject.transform, "HintText", new Vector2(0.5f, 0.3f), new Vector2(600f, 36f), 20, FontStyle.Italic);
            _hintText.color = new Color(0.75f, 0.78f, 0.82f);

            var choicePanel = new GameObject("ChoicePanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choicePanel.transform.SetParent(canvasObject.transform, false);
            var choiceRect = choicePanel.GetComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0.5f, 0.32f);
            choiceRect.anchorMax = new Vector2(0.5f, 0.32f);
            choiceRect.pivot = new Vector2(0.5f, 0f);
            choiceRect.anchoredPosition = Vector2.zero;
            choiceRect.sizeDelta = new Vector2(460f, 260f);

            var choiceLayout = choicePanel.GetComponent<VerticalLayoutGroup>();
            choiceLayout.spacing = 10f;
            choiceLayout.childAlignment = TextAnchor.LowerCenter;
            choiceLayout.childControlWidth = false;
            choiceLayout.childControlHeight = false;
            _choiceContainer = choiceRect;
        }

        private static Text CreateLabel(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize, FontStyle style)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = Color.white;
            label.supportRichText = false;
            ChineseFontHelper.ApplyTo(label);
            return label;
        }
    }
}
