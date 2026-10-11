using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnCollision : MonoBehaviour
{
    [Header("目標標籤 -> 留空則任何物體碰到都觸發")]
    [SerializeField] private string targetTag = "Player";

    [Header("特效設置")]
    [SerializeField] private ParticleSystem confettiParticle;

    [Header("音效設定")]
    [SerializeField] private AudioClip hitSound;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1.0f;

    [Header("重啟延遲")]
    [SerializeField] private float restartDelay = 0.5f;



    private bool isTriggered = false; // 防止重複觸發多次

    // 實體碰撞時觸發
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleTrigger(collision.gameObject);
    }

    // 穿透觸發時觸發
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other.gameObject);
    }

    private void HandleTrigger(GameObject hitObj)
    {
        if (isTriggered) return;

        
        if (!string.IsNullOrEmpty(targetTag) && !hitObj.CompareTag(targetTag))
        {
            return;
        }

        isTriggered = true;
        StartCoroutine(PlaySoundAndRestart());
    }

    private IEnumerator PlaySoundAndRestart()
    {
        //特效
        if (confettiParticle != null)
        {
            confettiParticle.Play();
        }

        // 在碰撞位置播放音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);
        }

        // 等待指定時間
        yield return new WaitForSeconds(restartDelay);

        // 重載當前場景
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}