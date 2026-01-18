using System.Collections.Generic;
using System.Linq;
using EnumCollection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Store.UI
{
    /// <summary>
    /// 가챠 결과 UI 전담
    /// </summary>
    public class GachaResultUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument _popupDocument;

        // 자동 참조 (GetComponent로 찾음)
        private SlotAnimator _slotAnimator;

        private VisualElement _popup;
        private VisualElement _errorPopup;
        private Label _errorTxt;
        private Button _popupCloseBtn;
        private Button _errorCloseBtn;

        private readonly List<VisualElement> _slots = new();

        private bool _isPopupVisible;
        private bool _isErrorPopupVisible;

        // 등급별 배경 오프셋
        private readonly Dictionary<Rarity, Vector2> _rarityOffsetMap = new();

        public void Initialize()
        {
            // 같은 GameObject에서 자동 참조
            if (_slotAnimator == null) _slotAnimator = GetComponent<SlotAnimator>();

            var root = _popupDocument?.rootVisualElement;
            if (root == null) return;

            // 결과 팝업
            _popup = root.Q<VisualElement>("Popup");
            _popupCloseBtn = root.Q<Button>("PopupCloseBtn");

            var rowVE1 = root.Q<VisualElement>("RowVE1");
            var rowVE2 = root.Q<VisualElement>("RowVE2");

            if (rowVE1 != null)
                foreach (var child in rowVE1.Children()) _slots.Add(child);
            if (rowVE2 != null)
                foreach (var child in rowVE2.Children()) _slots.Add(child);

            if (_popup != null) _popup.style.display = DisplayStyle.None;

            _popupCloseBtn?.RegisterCallback<ClickEvent>(_ => HideResult());
            _popup?.RegisterCallback<PointerDownEvent>(_ => HideResult());

            // 에러 팝업
            _errorPopup = root.Q<VisualElement>("ErrorPopup");
            _errorTxt = root.Q<Label>("ErrorTxt");
            _errorCloseBtn = root.Q<Button>("ErrorCloseBtn");

            if (_errorPopup != null) _errorPopup.style.display = DisplayStyle.None;

            _errorCloseBtn?.RegisterCallback<ClickEvent>(_ => HideError());
            _errorPopup?.RegisterCallback<PointerDownEvent>(_ => HideError());

            // 오프셋 설정
            BuildRarityOffsetMap();

            Debug.Log($"[GachaResultUI] 슬롯 수집 완료: {_slots.Count}개");
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

        /// <summary>
        /// 무기 결과 표시
        /// </summary>
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
            _slotAnimator?.PlayAppearForSlots(_slots);

            LogResult("무기", weapons.Select(w => $"{w.name} ({w.WeaponRarity})"));
        }

        /// <summary>
        /// 코스튬 결과 표시
        /// </summary>
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
            _slotAnimator?.PlayAppearForSlots(_slots);

            LogResult("코스튬", costumes.Select(c => c.Name));
        }

        /// <summary>
        /// 에러 팝업 표시
        /// </summary>
        public void ShowError(string message)
        {
            HideAllSlots();
            if (_errorTxt != null)
                _errorTxt.text = string.IsNullOrEmpty(message) ? "가챠에 실패했습니다." : message;

            SetErrorVisibility(true);
        }

        public void HideResult() => SetPopupVisibility(false);
        public void HideError() => SetErrorVisibility(false);

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
                _slotAnimator?.StopAllFx();
                _slotAnimator?.ResetSlotStyles(_slots);
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
            {
                _slotAnimator?.StopAllFx();
            }
        }

        private void HideAllSlots()
        {
            _slotAnimator?.StopAllFx();
            foreach (var s in _slots)
                s.style.display = DisplayStyle.None;
        }

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
    }
}
