using System;
using System.Collections;
using UnityEngine;

public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _pool;//비활성화 시 들어갈 풀
    private EnemyController[] _enemies;//활성화 시 들어갈 배열
    private int _indexInArr;//배열에서의 인덱스
    protected EnemyStatus _status;//전투에 사용할 스탯
    private SpriteRenderer _bodyRenderer;//몸체의 스프라이트 렌더러
    private float deadDuration = 1f;//죽는데 걸리는 시간
    private void Start()
    {
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);
        anim = GetComponent<Animator>();
        _bodyRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
    protected override ICharacterStatus GetStatus()
    {
        return _status;
    }
    public void SetEnemyInfo(EnemyPool pool, EnemyStatus status)
    {
        _pool = pool;
        _status = status;
    }
    protected override void OnDead()
    {
        BattleBroker.OnEnemyDead?.Invoke(transform.position);
        _enemies[_indexInArr].isDead = true;
        anim.SetBool("Die", true);
        StartCoroutine(DeadAfterWhile());
        StartCoroutine(FadeOutBodyRenderer());
    }

    private IEnumerator DeadAfterWhile()
    {
        yield return new WaitForSeconds(1f);
        _enemies[_indexInArr] = null;
        _enemies = null;
        _indexInArr = -1;
        _pool.ReturnToPool(this);
        isDead = false;
        Color color = _bodyRenderer.color;
        _bodyRenderer.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInArr = indexInPool;
    }
    public void MoveByCharacter(Vector3 translation)
    {
        transform.Translate(translation);
    }
    private IEnumerator FadeOutBodyRenderer()
    {
        float duration = 1f; // 1초 동안 페이드 아웃
        float elapsedTime = 0f;

        // 기존 색상을 가져옵니다.
        Color color = _bodyRenderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // 알파 값 계산
            _bodyRenderer.color = new Color(color.r, color.g, color.b, alpha); // 알파 값만 변경
            yield return null;
        }
    }

}