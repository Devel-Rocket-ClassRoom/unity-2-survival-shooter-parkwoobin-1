using UnityEngine;

// Camera 클래스는 플레이어를 따라다니며 시점을 조절하는 역할을 합니다. 플레이어가 이동할 때 카메라도 함께 이동하며, 마우스 위치를 기준으로 카메라가 플레이어를 바라보도록 설정되어 있습니다. 또한, 플레이어를 찾지 못했을 경우 경고 메시지를 출력하여 개발자가 문제를 인식할 수 있도록 돕습니다.
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
