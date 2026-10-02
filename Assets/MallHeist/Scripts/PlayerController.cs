using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public struct Item
    {
        public string Name;
        public int Price;
        public float Weight;
        public Color Color;
    }

    public float WalkSpeed = 3.6f, SprintSpeed = 6.6f, CrouchSpeed = 1.8f;
    public float MaxWeight = 30f, MouseSens = 2f, ReachDistance = 3f;

    public readonly List<Item> Bag = new List<Item>();
    public float Stamina { get; private set; } = 1f;
    public bool Crouching { get; private set; }
    public bool Sprinting { get; private set; }
    public bool Hidden { get; private set; }
    public float LastGrabTime { get; private set; } = -100f;
    public Loot LookedAt { get; private set; }
    public Camera Cam { get; private set; }

    CharacterController cc;
    float yaw, pitch;
    bool exhausted;

    public int BagValue { get { int s = 0; foreach (var i in Bag) s += i.Price; return s; } }
    public float BagWeight { get { float s = 0; foreach (var i in Bag) s += i.Weight; return s; } }
    public bool GrabbedRecently => Time.time - LastGrabTime < 1.2f;
    // Охрану напрягает, когда ты хватаешь товар или носишься с набитым рюкзаком.
    public bool LooksSuspicious => GrabbedRecently || (Sprinting && Bag.Count > 0);
    public Vector3 Chest => transform.position + Vector3.up * (Crouching ? 0.8f : 1.4f);

    void Awake() => cc = GetComponent<CharacterController>();

    public void Init(Camera cam, float startYaw)
    {
        Cam = cam;
        yaw = startYaw;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || gm.State != GameState.Playing) return;
        Look();
        Move();
        Interact(gm);
        Hidden = HidingSpot.Contains(transform.position);
    }

    void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        Vector2 d = GameInput.Look() * MouseSens;
        yaw += d.x;
        pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        Cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move()
    {
        float dt = Time.deltaTime;
        if (GameInput.CrouchPressed()) SetCrouch(!Crouching);

        Vector2 m = GameInput.Move();
        bool moving = m.sqrMagnitude > 0.01f;
        Sprinting = GameInput.SprintHeld() && moving && m.y > 0 && !Crouching && !exhausted;
        if (Sprinting)
        {
            Stamina -= dt / 4f;
            if (Stamina <= 0f) { Stamina = 0f; exhausted = true; }
        }
        else
        {
            Stamina = Mathf.Min(1f, Stamina + dt / 6f);
            if (exhausted && Stamina > 0.35f) exhausted = false;
        }

        float speed = Crouching ? CrouchSpeed : Sprinting ? SprintSpeed : WalkSpeed;
        speed *= 1f - 0.45f * Mathf.Clamp01(BagWeight / MaxWeight); // тяжёлый рюкзак тормозит
        Vector3 dir = transform.right * m.x + transform.forward * m.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        cc.SimpleMove(dir * speed);

        var lp = Cam.transform.localPosition;
        lp.y = Mathf.Lerp(lp.y, Crouching ? 1.0f : 1.6f, 12f * dt);
        Cam.transform.localPosition = lp;
    }

    void SetCrouch(bool on)
    {
        Crouching = on;
        cc.height = on ? 1.1f : 1.8f;
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
    }

    void Interact(GameManager gm)
    {
        LookedAt = null;
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out var hit, ReachDistance, ~0, QueryTriggerInteraction.Ignore))
            LookedAt = hit.collider.GetComponent<Loot>();

        if (LookedAt != null && GameInput.GrabPressed())
        {
            if (BagWeight + LookedAt.Weight > MaxWeight)
            {
                gm.Toast("Рюкзак не резиновый! Выкинь что-нибудь [G]");
                return;
            }
            Bag.Add(new Item { Name = LookedAt.ItemName, Price = LookedAt.Price, Weight = LookedAt.Weight, Color = LookedAt.Color });
            LastGrabTime = Time.time;
            gm.Toast("+ " + LookedAt.ItemName + "  (" + GameManager.Money(LookedAt.Price) + ")");
            Destroy(LookedAt.gameObject);
            LookedAt = null;
        }

        if (GameInput.DropPressed() && Bag.Count > 0)
        {
            var it = Bag[Bag.Count - 1];
            Bag.RemoveAt(Bag.Count - 1);
            Vector3 pos = transform.position + transform.forward * 0.9f + Vector3.up * 1.2f;
            MallBootstrap.SpawnLoot(it.Name, it.Price, it.Weight, it.Color, pos, transform.parent, true);
            gm.Toast("Выкинул: " + it.Name);
        }
    }
}
