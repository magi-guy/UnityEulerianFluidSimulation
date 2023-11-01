using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsToCamera : MonoBehaviour
{
    [SerializeField] private ComputeShader compute;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;

    public RenderTexture texture;
    public ComputeBuffer buffer;
    private float time;

    void Start()
    {
        texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        texture.enableRandomWrite = true;
        texture.Create();

        Debug.Log(compute);

        int kernel = compute.FindKernel("CSMain");
        compute.SetTexture(kernel, "Result", texture);
        compute.Dispatch(kernel, width/8, height/8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(texture == null) {
            texture = new RenderTexture(width, height, 24);
            texture.enableRandomWrite = true;
            texture.Create();
        }
        compute.SetTexture(0, "Result", texture);
        compute.SetFloat("width", width);
        compute.SetFloat("height", height);
	    time += Time.deltaTime; Debug.Log(time);
	    compute.SetFloat("time", time);
        compute.Dispatch(0, width/8, height/8, 1);

        Graphics.Blit(texture, dest);
    }

    void Update()
    {
        
    }
}
