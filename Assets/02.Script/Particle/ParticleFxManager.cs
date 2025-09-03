using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleFxManager : MonoSingleton<ParticleFxManager>
{
    [Serializable]
    public class ParticleSlot
    {
        public string Id;                       // 파티클 식별자 (예: "StoreOpen")
        public ParticleSystem Particle;         // 실제 파티클 프리팹 혹은 씬 오브젝트
        public bool PlayOnAwake = false;        // 시작 시 자동 재생 여부
        public bool DeactivateOnStop = false;   // Stop 시 GameObject 비활성화
    }

    [Header("FX Library")]
    [Tooltip("여기에 관리할 파티클들을 등록하세요.")]
    [SerializeField] private List<ParticleSlot> _slots = new();

    private readonly Dictionary<string, ParticleSlot> _dict = new();

    protected override void Awake()
    {
        base.Awake();
        BuildDictionary();

        // 자동 재생
        foreach (var s in _slots)
        {
            if (s?.Particle == null) continue;
            if (s.PlayOnAwake)
            {
                SafeActivate(s.Particle, true);
                s.Particle.Play(true);
            }
            else
            {
                SafeActivate(s.Particle, false); // 필요할 때 켜기
            }
        }
    }

    private void BuildDictionary()
    {
        _dict.Clear();
        foreach (var s in _slots)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.Id)) continue;
            if (_dict.ContainsKey(s.Id))
                Debug.LogWarning($"[ParticleFxManager] 중복된 Id: {s.Id}. 나중 항목으로 덮어씁니다.");
            _dict[s.Id] = s;
        }
    }

    public bool Has(string id) => _dict.ContainsKey(id);

    public ParticleSystem Get(string id)
    {
        return _dict.TryGetValue(id, out var slot) ? slot.Particle : null;
    }

    /// <summary> 단순 재생(현재 위치/회전 유지). </summary>
    public void Play(string id, bool restart = true)
    {
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;

        SafeActivate(slot.Particle, true);
        if (restart) slot.Particle.Clear(true);
        slot.Particle.Play(true);
    }

    /// <summary> 월드 좌표에서 재생. </summary>
    public void PlayAt(string id, Vector3 worldPos, Quaternion? worldRot = null, bool restart = true)
    {
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;

        var ps = slot.Particle;
        ps.transform.position = worldPos;
        if (worldRot.HasValue) ps.transform.rotation = worldRot.Value;

        SafeActivate(ps, true);
        if (restart) ps.Clear(true);
        ps.Play(true);
    }

    /// <summary> 특정 트랜스폼 기준으로 재생(부착 선택 가능). </summary>
    public void PlayAt(string id, Transform anchor, bool follow = false, bool restart = true)
    {
        if (anchor == null) return;
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;

        var ps = slot.Particle;
        if (follow)
        {
            ps.transform.SetParent(anchor, worldPositionStays: false);
            ps.transform.localPosition = Vector3.zero;
            ps.transform.localRotation = Quaternion.identity;
        }
        else
        {
            ps.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }

        SafeActivate(ps, true);
        if (restart) ps.Clear(true);
        ps.Play(true);
    }

    /// <summary>
    /// UI Toolkit 요소의 화면상 중심 위치에 파티클 재생.
    /// - cam: Screen Space - Camera 또는 3D 월드 카메라
    /// - depthFromCamera: cam에서 얼마나 떨어진 z(미터). (ScreenToWorldPoint용)
    /// </summary>
    public void PlayAtUI(string id, VisualElement ve, Camera cam, float depthFromCamera = 5f, bool restart = true)
    {
        if (ve == null || cam == null) return;
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;

        // UI Toolkit 좌표 -> 스크린 좌표
        // worldBound: 게임뷰 픽셀 좌표(좌상단 원점). 스크린은 좌하단 원점이라 Y 반전 필요.
        var wb = ve.worldBound;
        float screenX = wb.x + wb.width * 0.5f;
        float screenY = Screen.height - (wb.y + wb.height * 0.5f);

        var screenPoint = new Vector3(screenX, screenY, depthFromCamera);
        var worldPos = cam.ScreenToWorldPoint(screenPoint);

        var ps = slot.Particle;
        ps.transform.position = worldPos;

        // 카메라를 바라보게 하고 싶다면(옵션)
        // ps.transform.forward = (ps.transform.position - cam.transform.position).normalized;

        SafeActivate(ps, true);
        if (restart) ps.Clear(true);
        ps.Play(true);
    }

    public void Stop(string id, bool clear = true)
    {
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;
        slot.Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (clear) slot.Particle.Clear(true);
        if (slot.DeactivateOnStop) SafeActivate(slot.Particle, false);
    }

    public void StopAll(bool clear = true)
    {
        foreach (var s in _slots)
        {
            if (s?.Particle == null) continue;
            s.Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (clear) s.Particle.Clear(true);
            if (s.DeactivateOnStop) SafeActivate(s.Particle, false);
        }
    }

    public void RegisterOrReplace(string id, ParticleSystem ps, bool playOnAwake = false, bool deactivateOnStop = false)
    {
        if (string.IsNullOrWhiteSpace(id) || ps == null) return;

        var slot = new ParticleSlot
        {
            Id = id,
            Particle = ps,
            PlayOnAwake = playOnAwake,
            DeactivateOnStop = deactivateOnStop
        };

        if (_dict.ContainsKey(id))
        {
            _dict[id] = slot;
            // 리스트도 갱신(에디터 디버깅 편의)
            int idx = _slots.FindIndex(s => s != null && s.Id == id);
            if (idx >= 0) _slots[idx] = slot;
            else _slots.Add(slot);
        }
        else
        {
            _dict.Add(id, slot);
            _slots.Add(slot);
        }
    }

    private void SafeActivate(ParticleSystem ps, bool active)
    {
        if (ps == null) return;
        if (ps.gameObject.activeSelf != active)
            ps.gameObject.SetActive(active);
    }
}
