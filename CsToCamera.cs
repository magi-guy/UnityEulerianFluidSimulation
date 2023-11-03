using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsToCamera : MonoBehaviour
{
    [SerializeField] private ComputeShader compute;
    [SerializeField] private BoundaryType boundaryType;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;
    [SerializeField] private float diffusion = .6f;

    enum BoundaryType { Wall, Wrap }

    public RenderTexture fluid;
    public ComputeBuffer buffer;
    private float time;

    private int testShader;
    private int calculateDensity;

    void Start()
    {
        // Set proper kernels
        testShader = compute.FindKernel("CSMain");
        calculateDensity = compute.FindKernel("CalculateDensity");

        // Create an empty texture
        fluid = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        fluid.enableRandomWrite = true;
        fluid.Create();

        // Set variables
        compute.SetFloat("time", time);
        compute.SetFloat("width", width);
        compute.SetFloat("height", height);
        compute.SetFloat("diff", diffusion);
	    

        // Create an environment
        compute.SetTexture(testShader, "Result", fluid);
        compute.Dispatch(testShader, width/8, height/8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Update the density
        compute.SetTexture(calculateDensity, "Result", fluid);
        compute.Dispatch(calculateDensity, width/8, height/8, 1);
        Graphics.Blit(fluid, dest);
    }

    void Update()
    {
        // Handle Time
        time += Time.deltaTime;
        compute.SetFloat("time", time);
    }
}
