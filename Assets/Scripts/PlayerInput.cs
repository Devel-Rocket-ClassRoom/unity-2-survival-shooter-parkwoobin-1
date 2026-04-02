using UnityEngine;

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
