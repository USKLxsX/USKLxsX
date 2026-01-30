using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class SkeletonKnight : Monster
{
    [Header("骷髅骑士特有属性")]
    [SerializeField] private float berserkThreshold = 0.5f; // 狂暴阈值(50%血量)
    [SerializeField] private bool isBerserk = false; // 是否处于狂暴状态（永久性）

    private bool hasTriggeredBerserk = false; // 是否已触发狂暴
    private float berserkSpeedMultiplier = 1.5f; // 狂暴速度倍率

    void Start()
    {
        Reset();
        // 初始化攻击力 (修正：使用 damage 而非 attackDamage)
        damage = monsterdata.damage;
    }

    void Update() => LoadState();

    // 处理受伤 - 使用 new 隐藏基类方法，保持参数签名一致
    public new void TakeDamage(float amount, float? hitstuntime = 0, Vector2? knockBackDir = null)
    {
        // 调用基类方法处理实际受伤逻辑
        base.TakeDamage(amount, hitstuntime, knockBackDir);

        // 检查是否需要进入狂暴状态 (修正：使用 health 和 monsterdata.health)
        if (!hasTriggeredBerserk && health <= monsterdata.health * berserkThreshold)
        {
            EnterBerserkMode();
        }
    }

    // 进入狂暴状态
    private void EnterBerserkMode()
    {
        if (hasTriggeredBerserk) return;

        isBerserk = true;
        hasTriggeredBerserk = true;

        // 只增加速度
        speed *= berserkSpeedMultiplier;

        // 播放狂暴特效动画
        anim.SetTrigger("Berserk");

        Debug.Log("骷髅骑士进入狂暴状态！速度提升！");
    }

    // 攻击逻辑
    public override void Attack(Collider2D other, int id)
    {
        other.GetComponent<BasicControl>()?.TakeDamage(damage);
    }

    // 死亡逻辑
    public override void Die()
    {
        _isDead = true;

        // 设置死亡动画
        anim.SetBool("Move", false);
        anim.SetTrigger("Die");

        // 禁用碰撞和移动
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // 延迟销毁，给死亡动画播放时间
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    #region 行为状态实现

    protected override void IdleState(float dist)
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        anim.SetBool("Move", false);

        if (dist < monsterdata.detectRange)
        {
            anim.SetBool("Move", true);
            currentState = State.Chase;
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= monsterdata.idleTime)
        {
            idleTimer = 0;
            anim.SetBool("Move", true);
            currentState = State.Patrol;
            facingRight = Random.value > 0.5f;
            float moveDir = facingRight ? 1 : -1;
            transform.localScale = new Vector3(moveDir * baseScaleX, transform.localScale.y, 1);
        }
    }

    protected override void PatrolState(float dist)
    {
        if (dist < monsterdata.detectRange)
        {
            currentState = State.Chase;
            anim.SetBool("Move", true);
            return;
        }

        patrolTimer += Time.deltaTime;
        float moveDir = facingRight ? 1 : -1;

        // 减速效果
        if (effects.Contains(effects.Find(e => e.effectname == "Slow")))
            moveDir *= 0.5f;

        // 狂暴状态下移动更快
        float currentSpeed = isBerserk ? speed : speed;
        rb.velocity = new Vector2(moveDir * currentSpeed, rb.velocity.y);

        if (ShouldTurn(moveDir))
        {
            patrolTimer = 0;
            FaceTo(-moveDir);
            anim.SetBool("Move", false);
            currentState = State.Idle;
            return;
        }

        if (patrolTimer >= monsterdata.patrolDuration)
        {
            patrolTimer = 0;
            FaceTo(-moveDir);
            anim.SetBool("Move", false);
            currentState = State.Idle;
            return;
        }
    }

    protected override void ChaseState(float dist)
    {
        // 狂暴状态下移动动画可以更激烈
        if (isBerserk)
        {
            anim.SetBool("Move", true);
        }

        if (dist <= monsterdata.attackRange)
        {
            float moveDirS = player.position.x > transform.position.x ? 1 : -1;
            if (effects.Contains(effects.Find(e => e.effectname == "Slow")))
                moveDirS *= 0.5f;

            rb.velocity = new Vector2(moveDirS * speed * 0.8f, rb.velocity.y);

            anim.SetBool("Move", false);
            currentState = State.Attack;
            return;
        }

        if (dist > monsterdata.detectRange * 1.5f)
        {
            anim.SetBool("Move", false);
            currentState = State.Idle;
            return;
        }

        float dirX = player.position.x > transform.position.x ? 1 : -1;
        Vector2 origin = transform.position + Vector3.up * 0.15f;
        Vector2 ledgeDir = new Vector2(dirX, -1).normalized;
        bool groundHere = Physics2D.Raycast(transform.position, Vector2.down, downCheckDist, groundLayer);
        bool wallAhead = Physics2D.Raycast(origin, new Vector2(dirX, 0), wallCheckDist, groundLayer);

        ContactFilter2D ledgeFilter = new ContactFilter2D();
        ledgeFilter.useNormalAngle = true;
        ledgeFilter.minNormalAngle = -30;
        ledgeFilter.maxNormalAngle = 30;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int c = Physics2D.Raycast(origin, ledgeDir, ledgeFilter, hits, ledgeCheckDist);
        bool ledgeEmpty = c == 0;

        bool needJump = (ledgeEmpty && groundHere) || wallAhead;
        if (needJump && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            float jumpForce = isBerserk ? jumpVelocity * 1.2f : jumpVelocity;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            return;
        }

        float moveDir = player.position.x > transform.position.x ? 1 : -1;
        if (effects.Contains(effects.Find(e => e.effectname == "Slow")))
            moveDir *= 0.5f;

        rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);
        FaceTo(dirX);
    }

    protected override void AttackState(float dist)
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= monsterdata.attackCooldown)
        {
            attackTimer = 0;
            float dirX = player.position.x > transform.position.x ? 1 : -1;
            float moveDir = player.position.x > transform.position.x ? 1 : -1;

            if (effects.Contains(effects.Find(e => e.effectname == "Slow")))
                moveDir *= 0.5f;

            rb.velocity = new Vector2(moveDir * speed * 0.5f, rb.velocity.y);
            FaceTo(dirX);

            // 根据狂暴状态选择不同的攻击动画
            if (isBerserk)
            {
                anim.SetTrigger("AttackTwo");
            }
            else
            {
                anim.SetTrigger("AttackOne");
            }

            if (dist <= monsterdata.attackRange)
            {
                anim.SetBool("Move", false);
                currentState = State.Attack;
            }
            else
            {
                anim.SetBool("Move", true);
                currentState = State.Chase;
            }
        }

        if (player != null)
        {
            float dirX = player.position.x > transform.position.x ? 1 : -1;
            FaceTo(dirX);
        }
    }

    // 面向方向（如果基类 FaceTo 不可访问则保留）
    private void FaceTo(float dirX)
    {
        if (dirX > 0 && !facingRight || dirX < 0 && facingRight)
        {
            facingRight = !facingRight;
            float newScaleX = facingRight ? baseScaleX : -baseScaleX;
            transform.localScale = new Vector3(newScaleX, transform.localScale.y, 1);
        }
    }

    #endregion
}