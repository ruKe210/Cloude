using STS.Battle;
using STS.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STS.Presentation
{
    public class BattleHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text outcomeText;
        [SerializeField] private Button endTurnButton;

        public void Setup(TMP_Text energy, TMP_Text turn, TMP_Text phase, TMP_Text outcome, Button endTurn)
        {
            energyText = energy;
            turnText = turn;
            phaseText = phase;
            outcomeText = outcome;
            endTurnButton = endTurn;
        }

        private BattleManager _battleManager;

        public void Initialize(BattleManager battleManager)
        {
            _battleManager = battleManager;
            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }
        }

        public void Refresh()
        {
            if (_battleManager == null)
            {
                return;
            }

            var context = _battleManager.Context;
            if (energyText != null)
            {
                energyText.text = $"能量 {context.Player.Energy}/{context.Player.MaxEnergy}";
            }

            if (turnText != null)
            {
                turnText.text = $"回合 {context.TurnNumber}";
            }

            if (phaseText != null)
            {
                phaseText.text = $"阶段 {context.Phase}";
            }

            if (outcomeText != null)
            {
                outcomeText.text = context.Outcome switch
                {
                    BattleOutcome.Victory => "胜利！",
                    BattleOutcome.Defeat => "失败...",
                    _ => string.Empty
                };
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable = context.Phase == BattlePhase.PlayerTurn
                                               && !context.IsBattleOver
                                               && !_battleManager.Actions.IsBusy;
            }
        }

        private void OnEndTurnClicked()
        {
            _battleManager.EndPlayerTurn();
        }
    }
}
