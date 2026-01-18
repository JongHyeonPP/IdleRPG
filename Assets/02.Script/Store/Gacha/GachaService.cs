using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnumCollection;
using Unity.Services.CloudCode;
using UnityEngine;

namespace Store.Gacha
{
    /// <summary>
    /// CloudCode 가챠 서비스 - 서버 통신 전담
    /// </summary>
    public class GachaService
    {
        private readonly GameData _gameData;

        public GachaService(GameData gameData)
        {
            _gameData = gameData;
        }

        /// <summary>
        /// CloudCode 모듈 호출로 가챠 처리
        /// </summary>
        public async Task<GachaResult> CallGacha(GachaType type, int num)
        {
            try
            {
                var args = new Dictionary<string, object>
                {
                    { "gachaType", type.ToString().ToLowerInvariant() },
                    { "gachaNum",  num }
                };

                var result = await CloudCodeService.Instance
                    .CallModuleEndpointAsync<GachaResult>("PurchaseProcessor", "ProcessGacha", args);

                if (!result.Success)
                {
                    Debug.LogWarning($"[GachaService] 실패: {result.Message}");
                    return result;
                }

                // 재화(다이아) 갱신
                _gameData.dia = result.RemainDia;
                PlayerBroker.OnDiaSet();

                // 무기 뽑기 처리
                if (type == GachaType.Weapon)
                {
                    ProcessWeaponResult(result.Items);
                }
                // 코스튬 뽑기 처리
                else if (type == GachaType.Costume)
                {
                    ProcessCostumeResult(result.Items);
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GachaService] 예외: {e.Message}");
                return new GachaResult
                {
                    Success = false,
                    Message = "서버 통신에 실패했습니다.",
                    Items = new List<string>(),
                    RemainDia = _gameData.dia
                };
            }
        }

        private void ProcessWeaponResult(List<string> items)
        {
            foreach (var id in items)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;

                if (_gameData.weaponCount.ContainsKey(id))
                    _gameData.weaponCount[id]++;
                else
                    _gameData.weaponCount[id] = 1;

                PlayerBroker.OnWeaponCountSet?.Invoke(id, _gameData.weaponCount[id]);
            }
        }

        private void ProcessCostumeResult(List<string> items)
        {
            foreach (var raw in items)
            {
                var uid = raw?.Split('_')[^1]; // Last segment
                if (string.IsNullOrWhiteSpace(uid)) continue;

                if (!_gameData.ownedCostumes.Contains(uid))
                    _gameData.ownedCostumes.Add(uid);
            }

            Debug.Log($"[GachaService][Costume] raw items = [{string.Join(",", items ?? new List<string>())}]");
            CostumeManager.Instance?.UpdateCostumeData();
        }
    }
}
