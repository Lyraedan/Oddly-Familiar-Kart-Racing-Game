using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LuaRigidbody : LuaComponent
{
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    public void SetVelocity(float x, float y, float z)
    {
        rb.linearVelocity = new Vector3(x, y, z);
    }

    public Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public void AddForce(float x, float y, float z)
    {
        rb.AddForce(new Vector3(x, y, z));
    }

    public void AddImpulse(float x, float y, float z)
    {
        rb.AddForce(new Vector3(x, y, z), ForceMode.Impulse);
    }

    public void SetAngularVelocity(float x, float y, float z)
    {
        rb.angularVelocity = new Vector3(x, y, z);
    }

    public void Rotate(float x, float y, float z)
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(x, y, z));
    }

    public void SetKinematic(bool value)
    {
        rb.isKinematic = value;
    }

    public bool IsKinematic()
    {
        return rb.isKinematic;
    }

    public void UseGravity(bool value)
    {
        rb.useGravity = value;
    }
}