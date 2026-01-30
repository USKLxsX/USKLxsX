using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class SkeletonKnight : Monster
{
    private bool isEnraged = false;
    private float enrageMultiplier = 1.5f;
    private Animator animator;

    [Header("狂暴攻击设置")]
    [SerializeField] private float enragedAttackDamageMultiplier = 1.5f; // 狂暴攻击伤害倍数
    [SerializeField] private float enragedAttackCooldownMultiplier = 0.7f; // 狂暴攻击冷却时间倍数
    private float originalAttackCooldown; // 保存原始攻击冷却

    void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();

        // 保存原始攻击冷却时间
        originalAttackCooldown = monsterdata.attackCooldown;

        // 先确保找到玩家
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
        }

        Reset();
    }

    void Update()
    {
        LoadState();

        // 狂暴检测：生命值低于50%且未狂暴
        if (!isEnraged && health <= monsterdata.health * 0.5f)
        {
            Enrage();
        }

        // 更新动画参数
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // 设置移动速度
        float moveSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("MoveSpeed", moveSpeed);

        // 设置状态参数
        animator.SetInteger("State", (int)currentState);
    }

    public override void Attack(Collider2D other, int id)
    {
        if (other.CompareTag("Player"))
        {
            // 计算伤害：狂暴状态伤害更高
            float finalDamage = damage;
            if (isEnraged)
            {
                finalDamage *= enragedAttackDamageMultiplier;
                Debug.Log("狂暴攻击！伤害：" + finalDamage);
            }

            other.GetComponent<BasicControl>()?.TakeDamage(finalDamage);
        }
    }

    public override void Die()
    {
        _isDead = true;

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 延迟销毁，让动画播放完成
        Destroy(gameObject, 1.5f);
    }

    void Enrage()
    {
        isEnraged = true;
        speed *= enrageMultiplier;

        // 狂暴状态修改攻击参数
        if (monsterdata != null)
        {
            // 攻击伤害在Attack方法中动态计算
            // 攻击冷却时间缩短
            monsterdata.attackCooldown = originalAttackCooldown * enragedAttackCooldownMultiplier;
        }

        Debug.Log("骷髅骑士进入狂暴状态！速度提升，攻击增强！");

        // 如果有狂暴视觉特效，可以在这里触发
        // 例如：改变颜色、添加粒子效果等
    }

    #region 行为状态实现
    protected override void IdleState(float dist)
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (dist < monsterdata.detectRange)
        {
            currentState = State.Chase;
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= monsterdata.idleTime)
        {
            idleTimer = 0;
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
            return;
        }

        patrolTimer += Time.deltaTime;
        float moveDir = facingRight ? 1 : -1;

        // 修复：必须设置速度！
        float currentSpeed = isEnraged ? speed * 1.2f : speed;
        rb.velocity = new Vector2(moveDir * currentSpeed, rb.velocity.y);

        if (ShouldTurn(moveDir))
        {
            patrolTimer = 0;
            FaceTo(-moveDir);
            currentState = State.Idle;
            return;
        }

        if (patrolTimer >= monsterdata.patrolDuration)
        {
            patrolTimer = 0;
            FaceTo(-moveDir);
            currentState = State.Idle;
            return;
        }

        if (Random.value < 0.0001f) currentState = State.Idle;
    }

    protected override void ChaseState(float dist)
    {
        if (dist <= monsterdata.attackRange)
        {
            rb.velocity = Vector2.zero;
            currentState = State.Attack;
            return;
        }

        if (dist > monsterdata.detectRange * 1.5f)
        {
            currentState = State.Idle;
            return;
        }

        float dirX = player.position.x > transform.position.x ? 1 : -1;

        if (ShouldTurn(dirX))
        {
            rb.velocity = Vector2.zero;
            FaceTo(-dirX);
            return;
        }

        // 修复：必须设置速度！
        float chaseSpeed = isEnraged ? speed * 1.8f : speed * 1.5f;
        rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
        FaceTo(dirX);
    }

    protected override void AttackState(float dist)
    {
        attackTimer += Time.deltaTime;

        // 狂暴状态攻击冷却时间更短
        float currentAttackCooldown = monsterdata.attackCooldown;

        if (attackTimer >= currentAttackCooldown)
        {
            attackTimer = 0;

            float dirX = player.position.x > transform.position.x ? 1 : -1;
            FaceTo(dirX);

            // 触发攻击动画
            if (animator != null)
            {
                // 可以根据是否狂暴播放不同的攻击动画
                // 如果你有狂暴攻击动画，可以这样：
                if (isEnraged)
                {
                    animator.SetTrigger("EnragedAttack");
                }
                else
                {
                    animator.SetTrigger("Attack");
                }
            }

            // 攻击后的移动逻辑
            if (dist <= monsterdata.attackRange)
            {
                rb.velocity = Vector2.zero;
                currentState = State.Attack;
            }
            else
            {
                float chaseSpeed = isEnraged ? speed * 1.8f : speed * 1.5f;
                rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
                currentState = State.Chase;
            }
        }
        else
        {
            float dirX = player.position.x > transform.position.x ? 1 : -1;
            FaceTo(dirX);
        }
    }
    #endregion
}