using UnityEngine;
using System.Collections.Generic;

public class GentleTextureMotion : MonoBehaviour
{
    [SerializeField] private float speedX = 0.007f;
    [SerializeField] private float speedY = 0.005f;
    [Tooltip("Index of the material to animate (0 = first, 1 = second...)")]
    [SerializeField] private int materialIndex = 1;

    private Material[] materialInstances;
    private Vector2 offset;

    void Start()
    {
        var list = new List<Material>();
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (materialIndex < r.materials.Length)
                list.Add(r.materials[materialIndex]);
        }
        materialInstances = list.ToArray();
    }

    void Update()
    {
        offset.x = Mathf.Repeat(offset.x + speedX * Time.deltaTime, 1f);
        offset.y = Mathf.Repeat(offset.y + speedY * Time.deltaTime, 1f);
        foreach (var mat in materialInstances)
            mat.mainTextureOffset = offset;
    }
}
