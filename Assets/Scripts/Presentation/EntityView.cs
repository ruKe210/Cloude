using STS.Core;
using STS.Entities;
using TMPro;
using UnityEngine;

namespace STS.Presentation
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private bool showIntent;

        public void Setup(TMP_Text text, bool intent)
        {
            statusText = text;
            showIntent = intent;
        }

        private CombatEntity _entity;
        private EnemyEntity _enemy;

        public void Bind(CombatEntity entity)
        {
            _entity = entity;
            _enemy = entity as EnemyEntity;
            Refresh();
        }

        public void Refresh()
        {
            if (_entity == null || statusText == null)
            {
                return;
            }

            var powers = string.Empty;
            for (var i = 0; i < _entity.Powers.Count; i++)
            {
                powers += _entity.Powers[i].GetDisplayName();
                if (i < _entity.Powers.Count - 1)
                {
                    powers += ", ";
                }
            }

            var intentLine = string.Empty;
            if (showIntent && _enemy != null)
            {
                intentLine = $"\n意图: {_enemy.CurrentIntent.ToDisplayString()}";
            }

            var powerLine = string.IsNullOrEmpty(powers) ? string.Empty : $"\n状态: {powers}";
            statusText.text =
                $"{_entity.DisplayName}\nHP {_entity.CurrentHp}/{_entity.MaxHp}  格挡 {_entity.CurrentBlock}{powerLine}{intentLine}";
        }

        public void FlashDamage()
        {
            if (statusText == null)
            {
                return;
            }

            statusText.color = Color.red;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.15f);
        }

        private void ResetColor()
        {
            if (statusText != null)
            {
                statusText.color = Color.white;
            }
        }
    }
}
