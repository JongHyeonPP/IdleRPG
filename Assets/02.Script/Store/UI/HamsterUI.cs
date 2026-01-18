using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Store.UI
{
    /// <summary>
    /// 햄스터 마스코트 UI 관리
    /// </summary>
    public class HamsterUI : MonoBehaviour
    {
        private Label _hamsterText;
        private VisualElement _hamsterImage;

        private static readonly string[] Messages = { "어서오세요!", "앗!", "좋은 걸 뽑아보자!", "가자~!" };

        public void Initialize(VisualElement root)
        {
            _hamsterText = root?.Q<Label>("HamsterText");
            _hamsterImage = root?.Q<VisualElement>("HamsterImage");
            SetText(Messages[0]);
        }

        /// <summary>
        /// 랜덤 멘트 표시 (환영 제외)
        /// </summary>
        public void ShowRandomMessage()
        {
            int idx = Random.Range(1, Messages.Length);
            SetText(Messages[idx]);
        }

        /// <summary>
        /// 환영 멘트 표시
        /// </summary>
        public void ShowWelcome()
        {
            SetText(Messages[0]);
        }

        /// <summary>
        /// 에러 멘트 표시
        /// </summary>
        public void ShowError()
        {
            SetText("문제가 발생했습니다.");
        }

        /// <summary>
        /// 진행 중 멘트 표시
        /// </summary>
        public void ShowProcessing()
        {
            SetText("돌리는 중...");
        }

        /// <summary>
        /// 커스텀 멘트 설정
        /// </summary>
        public void SetText(string text)
        {
            if (_hamsterText == null) return;
            _hamsterText.text = text;
            StartCoroutine(AnimateText(_hamsterText));
        }

        private IEnumerator AnimateText(Label textLabel)
        {
            textLabel.style.opacity = 0;
            textLabel.style.translate = new StyleTranslate(new Translate(0, 10f, 0));

            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseInOutQuad(t);

                textLabel.style.opacity = easedT;
                textLabel.style.translate = new StyleTranslate(new Translate(0, 10f * (1 - easedT), 0));
                yield return null;
            }

            textLabel.style.opacity = 1;
            textLabel.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }

        private static float EaseInOutQuad(float t)
            => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}
