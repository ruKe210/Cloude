using STS.Battle;
using STS.Cards;
using STS.Core;
using STS.Data;
using STS.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace STS.Presentation
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private RectTransform cardContainer;

        public void Setup(RectTransform container) => cardContainer = container;

        private BattleManager _battleManager;
        private CombatActionManager _actionManager;
        private bool _attackOnlyMode;

        public void Initialize(BattleManager battleManager, CombatActionManager actionManager, bool attackOnlyMode = false)
        {
            _battleManager = battleManager;
            _actionManager = actionManager;
            _attackOnlyMode = attackOnlyMode;
        }

        public void Refresh()
        {
            if (cardContainer == null || _battleManager == null)
            {
                return;
            }

            for (var i = cardContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(cardContainer.GetChild(i).gameObject);
            }

            var hand = _battleManager.Context.Player.Hand.Cards;
            for (var i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                CreateCardButton(card, i);
            }
        }

        private void CreateCardButton(CardInstance card, int index)
        {
            var buttonObject = new GameObject($"Card_{card.Name}_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(cardContainer, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 160f);

            var layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 120f;
            layoutElement.preferredHeight = 160f;
            layoutElement.minWidth = 120f;
            layoutElement.minHeight = 160f;

            var image = buttonObject.GetComponent<Image>();
            image.color = card.Type == CardType.Attack ? new Color(0.85f, 0.35f, 0.35f) : new Color(0.35f, 0.55f, 0.85f);
            UiSpriteHelper.ApplyPanelImage(image);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 8f);
            labelRect.offsetMax = new Vector2(-8f, -8f);

            var label = labelObject.GetComponent<Text>();
            label.alignment = TextAnchor.UpperLeft;
            label.fontSize = 18;
            label.color = Color.white;
            label.supportRichText = false;
            ChineseFontHelper.ApplyTo(label);
            label.text = BuildCardLabel(card);

            var capturedCard = card;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => OnCardClicked(capturedCard));
        }

        private string BuildCardLabel(CardInstance card)
        {
            var text = $"{card.Name}\n费用 {card.Cost}";
            var damage = card.GetDisplayDamage(_battleManager.Context, _battleManager.Context.Enemy);
            if (damage > 0)
            {
                text += $"\n伤害 {damage}";
            }

            var block = card.GetDisplayBlock();
            if (block > 0 && !_attackOnlyMode)
            {
                text += $"\n格挡 {block}";
            }

            return text;
        }

        private void OnCardClicked(CardInstance card)
        {
            if (_battleManager.Context.IsBattleOver || _actionManager.IsBusy)
            {
                return;
            }

            CombatEntity target = null;
            if (card.TargetType == CardTargetType.Enemy)
            {
                target = _battleManager.Context.Enemy;
            }
            else if (card.TargetType == CardTargetType.Self)
            {
                target = _battleManager.Context.Player;
            }

            _battleManager.RequestPlayCard(card, target);
        }
    }
}
