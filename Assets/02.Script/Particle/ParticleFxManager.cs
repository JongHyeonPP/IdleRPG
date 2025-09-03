using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleFxManager : MonoSingleton<ParticleFxManager>
{
    [Serializable]
    public class ParticleSlot
    {
        public string Id;                       // ��ƼŬ �ĺ��� (��: "StoreOpen")
        public ParticleSystem Particle;         // ���� ��ƼŬ ������ Ȥ�� �� ������Ʈ
        public bool PlayOnAwake = false;        // ���� �� �ڵ� ��� ����
        public bool DeactivateOnStop = false;   // Stop �� GameObject ��Ȱ��ȭ
    }

    [Header("FX Library")]
    [Tooltip("���⿡ ������ ��ƼŬ���� ����ϼ���.")]
    [SerializeField] private List<ParticleSlot> _slots = new();

    private readonly Dictionary<string, ParticleSlot> _dict = new();

    protected override void Awake()
    {
        base.Awake();
        BuildDictionary();

        // �ڵ� ���
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
                SafeActivate(s.Particle, false); // �ʿ��� �� �ѱ�
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
                Debug.LogWarning($"[ParticleFxManager] �ߺ��� Id: {s.Id}. ���� �׸����� ����ϴ�.");
            _dict[s.Id] = s;
        }
    }

    public bool Has(string id) => _dict.ContainsKey(id);

    public ParticleSystem Get(string id)
    {
        return _dict.TryGetValue(id, out var slot) ? slot.Particle : null;
    }

    /// <summary> �ܼ� ���(���� ��ġ/ȸ�� ����). </summary>
    public void Play(string id, bool restart = true)
    {
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;

        SafeActivate(slot.Particle, true);
        if (restart) slot.Particle.Clear(true);
        slot.Particle.Play(true);
    }

    /// <summary> ���� ��ǥ���� ���. </summary>
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

    /// <summary> Ư�� Ʈ������ �������� ���(���� ���� ����). </summary>
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
    /// UI Toolkit ����� ȭ��� �߽� ��ġ�� ��ƼŬ ���.
    /// - cam: Screen Space - Camera �Ǵ� 3D ���� ī�޶�
    /// - depthFromCamera: cam���� �󸶳� ������ z(����). (ScreenToWorldPoint��)
    /// </summary>
    public void PlayAtUI(string id, VisualElement ve, Camera cam, float depthFromCamera = 5f, bool restart = true)
    {
        if (ve == null || cam == null) return;
        if (!_dict.TryGetValue(id, out var slot) || slot.Particle == null) return;

        // UI Toolkit ��ǥ -> ��ũ�� ��ǥ
        // worldBound: ���Ӻ� �ȼ� ��ǥ(�»�� ����). ��ũ���� ���ϴ� �����̶� Y ���� �ʿ�.
        var wb = ve.worldBound;
        float screenX = wb.x + wb.width * 0.5f;
        float screenY = Screen.height - (wb.y + wb.height * 0.5f);

        var screenPoint = new Vector3(screenX, screenY, depthFromCamera);
        var worldPos = cam.ScreenToWorldPoint(screenPoint);

        var ps = slot.Particle;
        ps.transform.position = worldPos;

        // ī�޶� �ٶ󺸰� �ϰ� �ʹٸ�(�ɼ�)
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
            // ����Ʈ�� ����(������ ����� ����)
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
