#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using UnityEngine;

// Работает и со старым Input Manager, и с новым Input System.
public static class GameInput
{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    static Keyboard K => Keyboard.current;
    static Mouse M => Mouse.current;

    public static Vector2 Move()
    {
        if (K == null) return Vector2.zero;
        float x = (K.dKey.isPressed ? 1 : 0) - (K.aKey.isPressed ? 1 : 0);
        float y = (K.wKey.isPressed ? 1 : 0) - (K.sKey.isPressed ? 1 : 0);
        return new Vector2(x, y);
    }
    public static Vector2 Look() => M == null ? Vector2.zero : M.delta.ReadValue() * 0.1f;
    public static bool SprintHeld() => K != null && K.leftShiftKey.isPressed;
    public static bool CrouchPressed() => K != null && (K.cKey.wasPressedThisFrame || K.leftCtrlKey.wasPressedThisFrame);
    public static bool GrabPressed() => K != null && K.eKey.wasPressedThisFrame;
    public static bool DropPressed() => K != null && K.gKey.wasPressedThisFrame;
    public static bool RestartPressed() => K != null && K.rKey.wasPressedThisFrame;
    public static bool EscapePressed() => K != null && K.escapeKey.wasPressedThisFrame;
    public static bool ClickPressed() => M != null && M.leftButton.wasPressedThisFrame;
#else
    public static Vector2 Move()
    {
        float x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
        float y = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        return new Vector2(x, y);
    }
    public static Vector2 Look() => new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
    public static bool SprintHeld() => Input.GetKey(KeyCode.LeftShift);
    public static bool CrouchPressed() => Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl);
    public static bool GrabPressed() => Input.GetKeyDown(KeyCode.E);
    public static bool DropPressed() => Input.GetKeyDown(KeyCode.G);
    public static bool RestartPressed() => Input.GetKeyDown(KeyCode.R);
    public static bool EscapePressed() => Input.GetKeyDown(KeyCode.Escape);
    public static bool ClickPressed() => Input.GetMouseButtonDown(0);
#endif
}
