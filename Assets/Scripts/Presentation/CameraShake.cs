using System.Collections;
using UnityEngine;

namespace STS.Presentation
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultMagnitude = 0.18f;

        private Vector3 _originPosition;
        private Coroutine _shakeRoutine;

        private void Awake()
        {
            _originPosition = transform.localPosition;
        }

        public void Shake()
        {
            Shake(defaultDuration, defaultMagnitude);
        }

        public void Shake(float duration, float magnitude)
        {
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
                transform.localPosition = _originPosition;
            }

            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var x = Random.Range(-1f, 1f) * magnitude;
                var y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = _originPosition + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originPosition;
            _shakeRoutine = null;
        }
    }
}
