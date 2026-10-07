using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerWASDController : MonoBehaviour
{
    [Header("移動 + 跳躍參數")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float jumpForce = 12.0f;

    [Header("地面偵測")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    // 用來放人物不同動作圖片的array
    [Header("人物圖片Array置放區~")]
    [SerializeField] private Sprite[] idleSprites = new Sprite[2];  // 眨眼 2 張
    [SerializeField] private Sprite[] runSprites = new Sprite[2];   // 跑步 2 張
    [SerializeField] private Sprite[] jumpSprites = new Sprite[3];  // 跳起來3張

    [Header("圖片播放速度")]
    [SerializeField] private float frameRate = 0.15f;
    [Tooltip("著地後緩衝圖")]
    [SerializeField] private float landDuration = 0.15f;


    private Rigidbody2D rb; //物理
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool wasGroundedLastFrame;

    // 計時器 + 狀態控制
    private float animTimer;
    private float landTimer;
    private float jumpCooldownTimer; // 防止跳起當下誤判成落地
    private bool isLanding;
    private int currentFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        //WASD 上下左右控制
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 圖片左右翻轉 人物往左時就翻轉
        if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        //Jump 冷靜期，過一定時間才能重新偵測地面
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        // 空白鍵Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isLanding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            wasGroundedLastFrame = false;
            isLanding = false;
            jumpCooldownTimer = 0.15f; //避免起跳瞬間被判定成著地

            // 起跳立即切換為跳躍圖
            if (jumpSprites != null && jumpSprites.Length > 0 && jumpSprites[0] != null)
            {
                spriteRenderer.sprite = jumpSprites[0];
            }
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        //如果在跳躍冷靜期內，強制認定人物在空中不偵測地面
        if (jumpCooldownTimer > 0f)
        {
            isGrounded = false;
            wasGroundedLastFrame = false;
        }
        else
        {
            // 過了保護期才可以偵測地面
            if (groundCheck != null)
            {
                //抓取所有重疊的Colliders無視掉玩家自己身上的 Collider
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
                isGrounded = false;

                foreach (Collider2D col in hitColliders)
                {
                    // 只要碰到的物件不是自己，也不是自己的sub Object = 踩在地面上
                    if (col.gameObject != gameObject && !col.transform.IsChildOf(transform))
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }

            // 剛從空中碰到地面的瞬間觸發緩衝
            if (!wasGroundedLastFrame && isGrounded)
            {
                isLanding = true;
                landTimer = 0f;
            }

            wasGroundedLastFrame = isGrounded;
        }

        // 水平移動
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }

    void UpdateAnimation()
    {
        // 跳起時顯示Array第0張圖，下降時切換為第2張
        if (!isGrounded)
        {
            if (jumpSprites != null && jumpSprites.Length >= 3)
            {
                //人物向上衝時速度要大於0.1才會顯示Array第0張 
                if (rb.velocity.y >= 0.1f && jumpSprites[0] != null)
                {
                    spriteRenderer.sprite = jumpSprites[0]; // 上升顯示第 0 張
                }
                else if (rb.velocity.y < 0.1f && jumpSprites[2] != null)
                {
                    spriteRenderer.sprite = jumpSprites[2]; // 下降顯示第 2 張
                }
            }
            return;
        }

        // 著地瞬間顯示第1張
        if (isLanding)
        {
            landTimer += Time.deltaTime;

            // 停留滿?秒之後後，結束回到跑步/待機動作
            if (landTimer < landDuration)
            {
                if (jumpSprites != null && jumpSprites.Length > 1 && jumpSprites[1] != null)
                {
                    spriteRenderer.sprite = jumpSprites[1];// 著地時顯示第1張
                }
                return;
            }
            else
            {
                // 時間一到，結束緩衝狀態
                isLanding = false;
                animTimer = 0f;
                currentFrame = 0;

                // 立刻切回待機圖的第一張 (idleSprites[0])
                if (idleSprites != null && idleSprites.Length > 0 && idleSprites[0] != null)
                {
                    spriteRenderer.sprite = idleSprites[0];
                }
            }
        }


        //跑步動畫
        animTimer += Time.deltaTime;
        if (Mathf.Abs(moveInput.x) > 0.05f)
        {
            if (runSprites != null && runSprites.Length > 0 && animTimer >= frameRate)
            {
                animTimer = 0f;
                currentFrame = (currentFrame + 1) % runSprites.Length;
                if (runSprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = runSprites[currentFrame];
                }
            }
            return;
        }

        // 待機動畫
        if (idleSprites != null && idleSprites.Length > 0)
        {
            // 若身上還殘留著跳躍陣列中的圖片，瞬間強制刷回待機圖第 0 張
            if (spriteRenderer.sprite == jumpSprites[0] ||
                (jumpSprites.Length > 1 && spriteRenderer.sprite == jumpSprites[1]) ||
                (jumpSprites.Length > 2 && spriteRenderer.sprite == jumpSprites[2]))
            {
                spriteRenderer.sprite = idleSprites[0];
            }

            //正常眨眼動畫計時時間到了才切換下一幀
            if (animTimer >= frameRate * 2.5f)
            {
                animTimer = 0f;
                currentFrame = (currentFrame + 1) % idleSprites.Length;
                if (idleSprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = idleSprites[currentFrame];
                }
            }
        }
    } 


    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}