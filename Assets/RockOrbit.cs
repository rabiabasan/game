using UnityEngine;

public class RockOrbit : MonoBehaviour
{
    public Transform[] orbitRocks;
    public float orbitRadius = 12f;
    public float orbitSpeed = 30f;
    public float height = 2f;   // <-- Inspector'da gözükecek

    void Start()
    {
        if (orbitRocks == null || orbitRocks.Length == 0) return;

        for (int i = 0; i < orbitRocks.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / orbitRocks.Length;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * orbitRadius,
                height,
                Mathf.Sin(angle) * orbitRadius
            );
            orbitRocks[i].localPosition = pos;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * orbitSpeed * Time.deltaTime);
    }
}