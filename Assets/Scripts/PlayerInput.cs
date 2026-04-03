using UnityEngine;

// PlayerInput 클래스는 플레이어의 입력을 처리하는 역할을 합니다. WASD 키로 이동 방향을, 마우스 움직임으로 시점 조절을, 마우스 클릭으로 공격 여부를 감지하여 각각 Move, MouseX, MouseY, Fire 프로퍼티에 저장합니다. 이 클래스는 다른 스크립트에서 플레이어의 입력 상태를 쉽게 참조할 수 있도록 설계되었습니다.
public class PlayerInput : MonoBehaviour
{
    public static readonly string HorizontalAxis = "Horizontal";
    public static readonly string VerticalAxis = "Vertical";
    public static readonly string MouseXAxis = "Mouse X";
    public static readonly string MouseYAxis = "Mouse Y";
    public static readonly string FireButton = "Fire1";

    public Vector2 Move { get; private set; }
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }
    public bool Fire { get; private set; }

    void Update()
    {
        // 🔹 WASD 이동 (좌우 + 앞뒤)
        float h = Input.GetAxis(HorizontalAxis); // A/D
        float v = Input.GetAxis(VerticalAxis);   // W/S
        Move = new Vector2(h, v);

        // 🔹 마우스 좌우 회전
        MouseX = Input.GetAxis(MouseXAxis);
        MouseY = Input.GetAxis(MouseYAxis);

        // 🔹 공격 / 재장전
        Fire = Input.GetButton(FireButton);
    }
}
