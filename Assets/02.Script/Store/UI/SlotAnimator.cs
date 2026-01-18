using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Store.UI
{
    /// <summary>
    /// 슬롯 등장/보브 애니메이션 전담
    /// </summary>
    public class SlotAnimator : MonoBehaviour
    {
        [Header("Appear Settings")]
        [SerializeField] private float _appearDuration = 0.35f;
        [SerializeField] private float _appearStagger = 0.05f;
        [SerializeField] private float _popScale = 1.08f;
        [SerializeField] private AnimationCurve _popEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Bob Settings")]
        [SerializeField] private float _bobAmplitude = 6f;
        [SerializeField] private float _bobPeriod = 1.6f;

        private readonly Dictionary<VisualElement, Coroutine> _bobRoutines = new();

        /// <summary>
        /// 보이는 슬롯들에 등장 FX 재생
        /// </summary>
        public void PlayAppearForSlots(IEnumerable<VisualElement> slots)
        {
            StopAllFx();

            int idx = 0;
            foreach (var slot in slots)
            {
                if (slot.resolvedStyle.display == DisplayStyle.Flex)
                {
                    float delay = _appearStagger * idx++;
                    StartCoroutine(Co_AppearThenBob(slot, delay));
                }
            }
        }

        /// <summary>
        /// 모든 FX 정지 및 스타일 리셋
        /// </summary>
        public void StopAllFx()
        {
            foreach (var kv in _bobRoutines)
            {
                if (kv.Value != null)
                    StopCoroutine(kv.Value);
            }
            _bobRoutines.Clear();
        }

        /// <summary>
        /// 슬롯 스타일 리셋
        /// </summary>
        public void ResetSlotStyles(IEnumerable<VisualElement> slots)
        {
            foreach (var s in slots)
            {
                s.style.opacity = 1f;
                s.style.scale = new StyleScale(Vector3.one);
                s.style.translate = new StyleTranslate(new Translate(0, 0, 0));
            }
        }

        private IEnumerator Co_AppearThenBob(VisualElement slot, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            slot.style.opacity = 0f;
            slot.style.scale = new StyleScale(new Vector3(0.92f, 0.92f, 1f));
            slot.style.translate = new StyleTranslate(new Translate(0, 8f, 0));

            float t = 0f;
            while (t < _appearDuration)
            {
                t += Time.deltaTime;
                float e = _popEase.Evaluate(Mathf.Clamp01(t / _appearDuration));

                float y = Mathf.Lerp(8f, 0f, e);
                float s = Mathf.Lerp(0.92f, _popScale, e);

                slot.style.opacity = e;
                slot.style.scale = new StyleScale(new Vector3(s, s, 1f));
                slot.style.translate = new StyleTranslate(new Translate(0, y, 0));

                yield return null;
            }

            slot.style.scale = new StyleScale(Vector3.one);
            slot.style.translate = new StyleTranslate(new Translate(0, 0, 0));

            if (!_bobRoutines.ContainsKey(slot))
            {
                _bobRoutines[slot] = StartCoroutine(Co_Bob(slot, Random.Range(0f, 1f)));
            }
        }

        private IEnumerator Co_Bob(VisualElement slot, float phaseOffset)
        {
            float t = phaseOffset * _bobPeriod;

            while (true)
            {
                t += Time.deltaTime;
                float phase = (t % _bobPeriod) / _bobPeriod;
                float y = Mathf.Sin(phase * Mathf.PI * 2f) * _bobAmplitude;

                slot.style.translate = new StyleTranslate(new Translate(0, y, 0));
                yield return null;
            }
        }
    }
}
