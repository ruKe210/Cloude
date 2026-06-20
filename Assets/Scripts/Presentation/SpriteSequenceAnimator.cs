using UnityEngine;

namespace STS.Presentation
{
    /// <summary>
    /// 轻量序列帧播放器。无 Animator 时由代码直接切 Sprite。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSequenceAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private float framesPerSecond = 4f;

        private SpriteRenderer _spriteRenderer;
        private Sprite[] _activeFrames;
        private int _frameIndex;
        private float _timer;
        private bool _isAttackPlaying;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _activeFrames = idleFrames;
            ApplyFrame(0);
        }

        public void Configure(Sprite[] idle, Sprite[] attack, float fps = 4f)
        {
            idleFrames = idle;
            attackFrames = attack != null && attack.Length > 0 ? attack : idle;
            framesPerSecond = fps;
            _activeFrames = idleFrames;
            ApplyFrame(0);
        }

        private void Update()
        {
            if (_activeFrames == null || _activeFrames.Length == 0)
            {
                return;
            }

            _timer += Time.deltaTime;
            var interval = 1f / Mathf.Max(framesPerSecond, 1f);
            if (_timer < interval)
            {
                return;
            }

            _timer = 0f;
            _frameIndex++;
            if (_frameIndex >= _activeFrames.Length)
            {
                if (_isAttackPlaying)
                {
                    _isAttackPlaying = false;
                    _activeFrames = idleFrames;
                    _frameIndex = 0;
                }
                else
                {
                    _frameIndex = 0;
                }
            }

            ApplyFrame(_frameIndex);
        }

        public void PlayAttack()
        {
            if (attackFrames == null || attackFrames.Length == 0)
            {
                return;
            }

            _isAttackPlaying = true;
            _activeFrames = attackFrames;
            _frameIndex = 0;
            _timer = 0f;
            ApplyFrame(0);
        }

        public void SetSpriteRendererColor(Color color)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }
        }

        private void ApplyFrame(int index)
        {
            if (_spriteRenderer == null || _activeFrames == null || _activeFrames.Length == 0)
            {
                return;
            }

            var safeIndex = Mathf.Clamp(index, 0, _activeFrames.Length - 1);
            var sprite = _activeFrames[safeIndex];
            if (sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
        }
    }
}
