using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumCollection;
using Store.UI;
using UnityEngine;

namespace Store.Gacha
{
    /// <summary>
    /// 가챠 흐름 컨트롤러 - 버튼 → 서비스 → UI 조율
    /// </summary>
    public class GachaController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private WeaponData[] _weaponDatas;

        // 자동 참조 (GetComponent로 찾음)
        private GachaResultUI _resultUI;
        private HamsterUI _hamsterUI;

        private GachaService _service;
        private Dictionary<string, WeaponData> _weaponByUid;
        private bool _isProcessing;

        // 최근 뽑은 무기 리스트 (외부 참조용)
        private List<WeaponData> _weaponSaveDatas = new();
        public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;

        public void Initialize(GameData gameData)
        {
            // 같은 GameObject에서 자동 참조
            if (_resultUI == null) _resultUI = GetComponent<GachaResultUI>();
            if (_hamsterUI == null) _hamsterUI = GetComponent<HamsterUI>();

            _service = new GachaService(gameData);
            BuildWeaponUidIndex();
        }

        private void BuildWeaponUidIndex()
        {
            _weaponByUid = _weaponDatas?
                .Where(w => w != null && !string.IsNullOrEmpty(w.UID))
                .ToDictionary(w => w.UID, w => w)
                ?? new Dictionary<string, WeaponData>();
        }

        /// <summary>
        /// 가챠 실행
        /// </summary>
        public async Task ExecuteGacha(GachaType type, int num)
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                SoundManager.instance?.PlaySFX(SoundPath.GachaDraw);

                _hamsterUI?.ShowProcessing();

                var result = await _service.CallGacha(type, num);

                if (result == null || !result.Success)
                {
                    _hamsterUI?.ShowError();
                    _resultUI?.ShowError(result?.Message ?? "알 수 없는 오류가 발생했습니다.");
                    return;
                }

                _hamsterUI?.ShowRandomMessage();

                if (type == GachaType.Weapon)
                {
                    var list = MapToWeaponData(result.Items);
                    _weaponSaveDatas = list;
                    _resultUI?.ShowWeaponResult(list);
                }
                else
                {
                    var list = MapToCostumeData(result.Items);
                    _resultUI?.ShowCostumeResult(list);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                _hamsterUI?.ShowError();
                _resultUI?.ShowError("알 수 없는 오류가 발생했습니다.");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private List<WeaponData> MapToWeaponData(List<string> items)
        {
            var list = new List<WeaponData>();
            foreach (var id in items)
            {
                if (!string.IsNullOrWhiteSpace(id) && _weaponByUid.TryGetValue(id, out var w))
                    list.Add(w);
                else
                    Debug.LogWarning($"[GachaController] UID 매핑 실패: '{id}'");
            }
            return list;
        }

        private static List<CostumeItem> MapToCostumeData(List<string> items)
        {
            return items
                .Select(raw => raw?.Split('_').Last())
                .Select(id => CostumeManager.Instance?.AllCostumeDatas.FirstOrDefault(c => c.Uid == id))
                .Where(c => c != null)
                .ToList();
        }
    }
}
