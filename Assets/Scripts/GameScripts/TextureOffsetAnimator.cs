using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TextureOffsetAnimator : MonoBehaviour
{
    public float speed = 0.2f; // Velocidad de desplazamiento
    private Material mat;
    private float offset;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        offset += speed * Time.deltaTime;
        offset = Mathf.Repeat(offset, 1f); // ciclo entre 0 y 1
        mat.mainTextureOffset = new Vector2(offset, 0f);
    }
}
