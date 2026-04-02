using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -7f);

    private void Awake()
    {
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            return;
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.transform;
            return;
        }

        Debug.LogError("플레이어를 찾지 못했습니다. Camera 컴포넌트의 Target을 지정하세요.", this);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }
}
