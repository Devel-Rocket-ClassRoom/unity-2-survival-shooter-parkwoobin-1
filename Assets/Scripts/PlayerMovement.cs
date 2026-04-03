using UnityEngine;

// PlayerMovement 클래스는 플레이어의 이동과 회전을 담당하는 클래스입니다. WASD 키로 이동 방향을 입력받아 Rigidbody를 통해 이동하며, 마우스 위치를 기준으로 캐릭터가 바라보는 방향을 부드럽게 회전시킵니다. 또한, 플레이어가 피해를 입어 사망한 경우에는 이동과 회전을 멈추도록 처리합니다.
public class PlayerMovement : MonoBehaviour
{
    public static readonly int HashMove = Animator.StringToHash("Move");

    public float moveSpeed = 5f;
    public float aimSmooth = 12f;

    private Animator playerAnimator;
    private PlayerInput playerInput;
    private Rigidbody playerRigidbody;
    private Shoot shoot;
    private PlayerHurt playerHurt;
    private Vector3 lookDirection = Vector3.forward;
    private Vector3 targetLookDirection = Vector3.forward;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        shoot = GetComponent<Shoot>();
        playerHurt = GetComponent<PlayerHurt>();

        if (playerAnimator == null)
            Debug.LogError("Animator not found on Player.", this);
        if (playerHurt == null)
            Debug.LogWarning("PlayerHurt not found on Player. Will continue with movement only.", this);
    }

    private void Update()
    {
        if (playerHurt != null && playerHurt.IsDead)
            return;

        if (playerAnimator != null)
            playerAnimator.SetFloat(HashMove, playerInput.Move.magnitude);
        UpdateLookRotation();
    }

    private void FixedUpdate()
    {
        if (playerHurt != null && playerHurt.IsDead)
            return;

        if (playerRigidbody == null)
            return;

        float t = 1f - Mathf.Exp(-aimSmooth * Time.fixedDeltaTime);
        lookDirection = Vector3.Slerp(lookDirection, targetLookDirection, t);

        if (lookDirection.sqrMagnitude > 0.0001f)   // 방향이 거의 없으면 회전하지 않음
        {
            Vector3 lookTarget = transform.position + lookDirection;
            lookTarget.y = transform.position.y;

            transform.LookAt(lookTarget);
            playerRigidbody.MoveRotation(transform.rotation);
        }

        Vector3 moveDir = new Vector3(playerInput.Move.x, 0f, playerInput.Move.y);
        Vector3 delta = moveDir * moveSpeed * Time.fixedDeltaTime;
        playerRigidbody.MovePosition(playerRigidbody.position + delta);
    }

    private void UpdateLookRotation()   // 마우스 방향으로 캐릭터 회전
    {
        if (playerInput == null)
            return;

        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam == null)    // 메인 카메라가 없는 경우 회전 처리하지 않음
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (!groundPlane.Raycast(ray, out float enter))
        {
            return;
        }

        Vector3 mouseWorld = ray.GetPoint(enter);   // 마우스 위치를 월드 자표로 변환하여 lookDirection 계산
        Vector3 look = mouseWorld - transform.position;
        look.y = 0f;

        if (look.sqrMagnitude < 0.0001f)
        {
            return;
        }

        targetLookDirection = look.normalized;
    }
}
