using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static readonly int HashMove = Animator.StringToHash("Move");

    public float moveSpeed = 5f;
    public float aimSmooth = 12f;

    private Animator playerAnimator;
    private PlayerInput playerInput;
    private Rigidbody playerRigidbody;
    private Vector3 lookDirection = Vector3.forward;
    private Vector3 targetLookDirection = Vector3.forward;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        playerAnimator.SetFloat(HashMove, playerInput.Move.magnitude);
        UpdateLookRotation();
    }

    private void FixedUpdate()
    {
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
        UnityEngine.Camera cam = UnityEngine.Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (!groundPlane.Raycast(ray, out float enter))
        {
            return;
        }

        Vector3 mouseWorld = ray.GetPoint(enter);
        Vector3 look = mouseWorld - transform.position;
        look.y = 0f;

        if (look.sqrMagnitude < 0.0001f)
        {
            return;
        }

        targetLookDirection = look.normalized;
    }
}
