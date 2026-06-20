using STS.Core;
using STS.Entities;
using UnityEngine;

namespace STS.Presentation
{
    public class WorldEntityView : MonoBehaviour
    {
        private const string VisualRootName = "CharacterVisual";

        [SerializeField] private TextMesh worldLabel;
        [SerializeField] private bool isPlayer;

        private CombatEntity _entity;
        private Transform _visualRoot;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private SpriteSequenceAnimator _sequenceAnimator;
        private Vector3 _baseVisualScale = Vector3.one;
        private Color _baseSpriteColor = Color.white;

        private void Awake()
        {
            EnsureWorldLabel();
        }

        private void EnsureWorldLabel()
        {
            if (worldLabel != null)
            {
                ChineseFontHelper.ApplyTo(worldLabel);
                return;
            }

            var labelObject = new GameObject("WorldLabel", typeof(TextMesh));
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            worldLabel = labelObject.GetComponent<TextMesh>();
            worldLabel.anchor = TextAnchor.MiddleCenter;
            worldLabel.characterSize = 0.08f;
            worldLabel.fontSize = 32;
            worldLabel.color = Color.white;
            ChineseFontHelper.ApplyTo(worldLabel);
        }

        public void Setup(TextMesh label, bool player)
        {
            worldLabel = label;
            isPlayer = player;
            if (worldLabel != null)
            {
                ChineseFontHelper.ApplyTo(worldLabel);
            }
        }

        public void ConfigureVisuals(OfficeBattleArtSet artSet, bool player)
        {
            isPlayer = player;

            if (artSet == null)
            {
                Debug.LogWarning($"[WorldEntityView] Art set missing on {gameObject.name}.");
                return;
            }

            var idleFrames = isPlayer ? artSet.playerIdleFrames : artSet.enemyIdleFrames;
            var attackFrame = isPlayer ? artSet.playerAttackFrame : artSet.enemyAttackFrame;
            var portrait = isPlayer ? artSet.playerPortrait : artSet.enemyPortrait;
            var controller = isPlayer ? artSet.playerAnimator : artSet.enemyAnimator;

            var displaySprite = GetDisplaySprite(idleFrames, portrait, attackFrame);
            if (displaySprite == null)
            {
                Debug.LogWarning($"[WorldEntityView] No sprite loaded for {gameObject.name}. Keeping cube mesh visible.");
                return;
            }

            HidePrimitiveMesh();
            EnsureVisualHierarchy();

            _spriteRenderer.sprite = displaySprite;
            _spriteRenderer.sortingOrder = isPlayer ? 20 : 10;
            _spriteRenderer.color = _baseSpriteColor;

            var framesForAnimation = idleFrames != null && idleFrames.Length > 0
                ? idleFrames
                : new[] { displaySprite };

            _sequenceAnimator.Configure(
                framesForAnimation,
                attackFrame != null ? new[] { attackFrame } : null);

            if (TrySetupAnimator(controller))
            {
                ForceAnimatorPose(displaySprite);
            }
            else
            {
                if (_animator != null)
                {
                    _animator.enabled = false;
                }

                _sequenceAnimator.enabled = true;
            }

            FitVisualScale(displaySprite);
            Debug.Log($"[WorldEntityView] {gameObject.name} sprite={displaySprite.name}, frames={framesForAnimation.Length}");
        }

        private void EnsureVisualHierarchy()
        {
            _visualRoot = transform.Find(VisualRootName);
            if (_visualRoot == null)
            {
                var visualObject = new GameObject(VisualRootName);
                visualObject.transform.SetParent(transform, false);
                visualObject.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                _visualRoot = visualObject.transform;
            }

            _spriteRenderer = _visualRoot.GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
            }

            _sequenceAnimator = _visualRoot.GetComponent<SpriteSequenceAnimator>();
            if (_sequenceAnimator == null)
            {
                _sequenceAnimator = _visualRoot.gameObject.AddComponent<SpriteSequenceAnimator>();
            }

            _animator = _visualRoot.GetComponent<Animator>();
        }

        private static Sprite GetDisplaySprite(Sprite[] idleFrames, Sprite portrait, Sprite attackFrame)
        {
            if (idleFrames != null)
            {
                foreach (var frame in idleFrames)
                {
                    if (frame != null)
                    {
                        return frame;
                    }
                }
            }

            if (portrait != null)
            {
                return portrait;
            }

            return attackFrame;
        }

        private bool TrySetupAnimator(RuntimeAnimatorController controller)
        {
            if (controller == null)
            {
                return false;
            }

            if (_animator == null)
            {
                _animator = _visualRoot.gameObject.AddComponent<Animator>();
            }

            _animator.runtimeAnimatorController = controller;
            _animator.enabled = true;

            if (_sequenceAnimator != null)
            {
                _sequenceAnimator.enabled = false;
            }

            return true;
        }

        private void ForceAnimatorPose(Sprite fallbackSprite)
        {
            if (_animator == null)
            {
                return;
            }

            _animator.Rebind();
            _animator.Update(0f);
            _animator.Play("Idle", 0, 0f);
            _animator.Update(0f);

            if (_spriteRenderer.sprite == null)
            {
                _spriteRenderer.sprite = fallbackSprite;
            }

            if (_spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[WorldEntityView] Animator failed on {gameObject.name}, fallback to sequence animator.");
                _animator.enabled = false;
                _sequenceAnimator.enabled = true;
            }
        }

        private void HidePrimitiveMesh()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }

        private void FitVisualScale(Sprite sprite)
        {
            if (_visualRoot == null || sprite == null)
            {
                return;
            }

            var spriteSize = sprite.bounds.size;
            if (spriteSize.y <= 0.01f)
            {
                return;
            }

            var targetHeight = isPlayer ? 3.8f : 4.2f;
            var scale = targetHeight / spriteSize.y;
            _baseVisualScale = new Vector3(scale, scale, 1f);
            _visualRoot.localScale = _baseVisualScale;
        }

        public void Bind(CombatEntity entity)
        {
            _entity = entity;
            Refresh();
        }

        public void Refresh()
        {
            if (_entity == null || worldLabel == null)
            {
                return;
            }

            worldLabel.text = $"{_entity.DisplayName}\n{_entity.CurrentHp} / {_entity.MaxHp}";
            if (_entity.CurrentBlock > 0)
            {
                worldLabel.text += $"\n格挡 {_entity.CurrentBlock}";
            }
        }

        public void PlayAttack()
        {
            if (_animator != null && _animator.enabled)
            {
                _animator.SetTrigger("Attack");
                return;
            }

            _sequenceAnimator?.PlayAttack();
        }

        public void PlayHitFeedback()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
                CancelInvoke(nameof(ResetColor));
                Invoke(nameof(ResetColor), 0.12f);
            }

            if (_visualRoot == null)
            {
                return;
            }

            CancelInvoke(nameof(ResetScale));
            _visualRoot.localScale = _baseVisualScale * 1.08f;
            Invoke(nameof(ResetScale), 0.12f);
        }

        private void ResetColor()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseSpriteColor;
            }
        }

        private void ResetScale()
        {
            if (_visualRoot != null)
            {
                _visualRoot.localScale = _baseVisualScale;
            }
        }
    }
}
