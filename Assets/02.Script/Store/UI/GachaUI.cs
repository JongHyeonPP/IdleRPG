using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnumCollection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Store.UI
{
    /// <summary>
    /// 가챠 UI 통합 클래스 - 결과 팝업 + 슬롯 애니메이션 + 햄스터 마스코트
    /// 원본 StoreManager처럼 인스펙터 노출 없이 내부 처리
    /// </summary>
    public class GachaUI : MonoBehaviour
    {
        // 애니메이션 상수 (인스펙터 노출 안 함)
        private const float APPEAR_DURATION = 0.35f;
        private const float APPEAR_STAGGER = 0.05f;
        private const float POP_SCALE = 1.08f;
        private const float BOB_AMPLITUDE = 6f;
        private const float BOB_PERIOD = 1.6f;

        // UI Elements (Initialize에서 설정)
        private VisualElement _popup;
        private VisualElement _errorPopup;
        private Label _errorTxt;
        private Button _popupCloseBtn;
        private Button _errorCloseBtn;

        // Hamster
        private Label _hamsterText;
        private static readonly string[] HamsterMessages = { "어서오세요!", "앗!", "좋은 걸 뽑아보자!", "가자~!" };

        // Slots
        private readonly List<VisualElement> _slots = new();
        private readonly Dictionary<VisualElement, Coroutine> _bobRoutines = new();
        private readonly Dictionary<Rarity, Vector2> _rarityOffsetMap = new();

        private bool _isPopupVisible;
        private bool _isErrorPopupVisible;

        #region Initialization

        /// <summary>
        /// StoreManager에서 호출 - 모든 UI 참조를 storeRoot에서 찾음
        /// </summary>
        public void Initialize(VisualElement storeRoot)
        {
            if (storeRoot == null) return;

            // 결과 팝업 (storeRoot에서 찾음)
            _popup = storeRoot.Q<VisualElement>("Popup");
            _popupCloseBtn = storeRoot.Q<Button>("PopupCloseBtn");

            var rowVE1 = storeRoot.Q<VisualElement>("RowVE1");
            var rowVE2 = storeRoot.Q<VisualElement>("RowVE2");

            _slots.Clear();
            if (rowVE1 != null)
                foreach (var child in rowVE1.Children()) _slots.Add(child);
            if (rowVE2 != null)
                foreach (var child in rowVE2.Children()) _slots.Add(child);

            if (_popup != null) _popup.style.display = DisplayStyle.None;

            _popupCloseBtn?.RegisterCallback<ClickEvent>(_ => HideResult());
            _popup?.RegisterCallback<PointerDownEvent>(_ => HideResult());

            // 에러 팝업
            _errorPopup = storeRoot.Q<VisualElement>("ErrorPopup");
            _errorTxt = storeRoot.Q<Label>("ErrorTxt");
            _errorCloseBtn = storeRoot.Q<Button>("ErrorCloseBtn");

            if (_errorPopup != null) _errorPopup.style.display = DisplayStyle.None;

            _errorCloseBtn?.RegisterCallback<ClickEvent>(_ => HideError());
            _errorPopup?.RegisterCallback<PointerDownEvent>(_ => HideError());

            // 햄스터
            _hamsterText = storeRoot.Q<Label>("HamsterText");
            SetHamsterText(HamsterMessages[0]);

            BuildRarityOffsetMap();
            Debug.Log($"[GachaUI] 슬롯 수집 완료: {_slots.Count}개");
        }

        private void BuildRarityOffsetMap()
        {
            _rarityOffsetMap[Rarity.Common] = new Vector2(-294f, 276f);
            _rarityOffsetMap[Rarity.Uncommon] = new Vector2(-294f, -2f);
            _rarityOffsetMap[Rarity.Rare] = new Vector2(-294f, -284f);
            _rarityOffsetMap[Rarity.Unique] = new Vector2(-573f, 276f);
            _rarityOffsetMap[Rarity.Legendary] = new Vector2(-573f, -2f);
            _rarityOffsetMap[Rarity.Mythic] = new Vector2(-573f, -284f);
        }

        #endregion

        #region Result Display

        public void ShowWeaponResult(List<WeaponData> weapons)
        {
            if (weapons == null || _slots.Count == 0) return;

            int n = Mathf.Min(weapons.Count, _slots.Count);

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];

                if (i < n)
                {
                    var weapon = weapons[i];
                    var icon = slot.Q<VisualElement>("WeaponIcon");
                    var iconParent = slot.Q<VisualElement>("WeaponIconParent");

                    if (icon != null && weapon.WeaponSprite != null)
                        icon.style.backgroundImage = new StyleBackground(weapon.WeaponSprite.texture);

                    ResetRarityOffset(iconParent);

                    var nameLabel = slot.Q<Label>("WeaponName");
                    if (nameLabel != null)
                    {
                        nameLabel.text = WrapText(weapon.WeaponName, 7);
                        nameLabel.style.height = 30;
                    }

                    slot.style.display = DisplayStyle.Flex;
                }
                else
                {
                    slot.style.display = DisplayStyle.None;
                }
            }

            ShowPopup();
            PlayAppearForSlots();
            LogResult("무기", weapons.Select(w => $"{w.name} ({w.WeaponRarity})"));
        }

        public void ShowCostumeResult(List<CostumeItem> costumes)
        {
            HideAllSlots();
            if (costumes == null || _slots.Count == 0) return;

            int n = Mathf.Min(costumes.Count, _slots.Count);

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];

                if (i < n)
                {
                    var costume = costumes[i];
                    var icon = slot.Q<VisualElement>("WeaponIcon") ?? slot.Q<VisualElement>("CostumeIcon");

                    if (icon != null && costume.IconTexture != null)
                        icon.style.backgroundImage = new StyleBackground(costume.IconTexture);

                    var nameLabel = slot.Q<Label>("WeaponName") ?? slot.Q<Label>("CostumeName");
                    if (nameLabel != null)
                    {
                        nameLabel.text = WrapText(costume.Name, 7);
                        nameLabel.style.height = 30;
                    }

                    slot.style.display = DisplayStyle.Flex;
                }
                else
                {
                    slot.style.display = DisplayStyle.None;
                }
            }

            ShowPopup();
            PlayAppearForSlots();
            LogResult("코스튬", costumes.Select(c => c.Name));
        }

        public void ShowError(string message)
        {
            HideAllSlots();
            if (_errorTxt != null)
                _errorTxt.text = string.IsNullOrEmpty(message) ? "가챠에 실패했습니다." : message;

            SetErrorVisibility(true);
        }

        public void HideResult() => SetPopupVisibility(false);
        public void HideError() => SetErrorVisibility(false);

        #endregion

        #region Popup Visibility

        private void ShowPopup() => SetPopupVisibility(true);

        private void SetPopupVisibility(bool isVisible)
        {
            if (_popup == null || _isPopupVisible == isVisible) return;

            if (isVisible) SetErrorVisibility(false);

            _isPopupVisible = isVisible;
            SoundManager.instance?.PlaySFX(SoundPath.GachaPopup);
            _popup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (!isVisible)
            {
                StopAllFx();
                ResetSlotStyles();
            }
        }

        private void SetErrorVisibility(bool isVisible)
        {
            if (_errorPopup == null || _isErrorPopupVisible == isVisible) return;

            if (isVisible) SetPopupVisibility(false);

            _isErrorPopupVisible = isVisible;

            if (isVisible)
                SoundManager.instance?.PlaySFX(SoundPath.GachaPopup);

            _errorPopup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (isVisible)
                StopAllFx();
        }

        private void HideAllSlots()
        {
            StopAllFx();
            foreach (var s in _slots)
                s.style.display = DisplayStyle.None;
        }

        #endregion

        #region Hamster

        public void ShowHamsterProcessing() => SetHamsterText("돌리는 중...");
        public void ShowHamsterError() => SetHamsterText("문제가 발생했습니다.");
        public void ShowHamsterWelcome() => SetHamsterText(HamsterMessages[0]);

        public void ShowHamsterRandom()
        {
            int idx = Random.Range(1, HamsterMessages.Length);
            SetHamsterText(HamsterMessages[idx]);
        }

        public void SetHamsterText(string text)
        {
            if (_hamsterText == null) return;
            _hamsterText.text = text;
            StartCoroutine(AnimateHamsterText());
        }

        private IEnumerator AnimateHamsterText()
        {
            _hamsterText.style.opacity = 0;
            _hamsterText.style.translate = new StyleTranslate(new Translate(0, 10f, 0));

            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

                _hamsterText.style.opacity = easedT;
                _hamsterText.style.translate = new StyleTranslate(new Translate(0, 10f * (1 - easedT), 0));
                yield return null;
            }

            _hamsterText.style.opacity = 1;
            _hamsterText.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }

        #endregion

        #region Slot Animation

        private void PlayAppearForSlots()
        {
            StopAllFx();

            int idx = 0;
            foreach (var slot in _slots)
            {
                if (slot.resolvedStyle.display == DisplayStyle.Flex)
                {
                    float delay = APPEAR_STAGGER * idx++;
                    StartCoroutine(Co_AppearThenBob(slot, delay));
                }
            }
        }

        private void StopAllFx()
        {
            foreach (var kv in _bobRoutines)
            {
                if (kv.Value != null)
                    StopCoroutine(kv.Value);
            }
            _bobRoutines.Clear();
        }

        private void ResetSlotStyles()
        {
            foreach (var s in _slots)
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
            while (t < APPEAR_DURATION)
            {
                t += Time.deltaTime;
                float e = EaseOutBack(Mathf.Clamp01(t / APPEAR_DURATION));

                float y = Mathf.Lerp(8f, 0f, e);
                float s = Mathf.Lerp(0.92f, POP_SCALE, e);

                slot.style.opacity = e;
                slot.style.scale = new StyleScale(new Vector3(s, s, 1f));
                slot.style.translate = new StyleTranslate(new Translate(0, y, 0));

                yield return null;
            }

            slot.style.scale = new StyleScale(Vector3.one);
            slot.style.translate = new StyleTranslate(new Translate(0, 0, 0));

            if (!_bobRoutines.ContainsKey(slot))
                _bobRoutines[slot] = StartCoroutine(Co_Bob(slot, Random.Range(0f, 1f)));
        }

        private IEnumerator Co_Bob(VisualElement slot, float phaseOffset)
        {
            float t = phaseOffset * BOB_PERIOD;

            while (true)
            {
                t += Time.deltaTime;
                float phase = (t % BOB_PERIOD) / BOB_PERIOD;
                float y = Mathf.Sin(phase * Mathf.PI * 2f) * BOB_AMPLITUDE;

                slot.style.translate = new StyleTranslate(new Translate(0, y, 0));
                yield return null;
            }
        }

        // 원본 StoreManager의 EaseOutBack
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        #endregion

        #region Utils

        private void ResetRarityOffset(VisualElement target, string moverName = "RT_SlotBg")
        {
            if (target == null) return;
            var mover = target.Q<VisualElement>(moverName);
            if (mover == null) return;

            mover.style.left = 0f;
            mover.style.top = 0f;
        }

        private static string WrapText(string text, int maxCharsPerLine)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var words = text.Split(' ');
            var sb = new System.Text.StringBuilder();
            int current = 0;

            foreach (var w in words)
            {
                if (current + w.Length <= maxCharsPerLine)
                {
                    if (current > 0) { sb.Append(" "); current++; }
                    sb.Append(w);
                    current += w.Length;
                }
                else
                {
                    sb.Append("\n");
                    sb.Append(w);
                    current = w.Length;
                }
            }

            return sb.ToString();
        }

        private static void LogResult(string type, IEnumerable<string> items)
        {
            Debug.Log($"뽑기 결과({type}):\n{string.Join("\n", items.Select(i => $"- {i}"))}");
        }

        #endregion
    }
}
