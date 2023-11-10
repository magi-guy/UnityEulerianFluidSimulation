using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsToCamera : MonoBehaviour
{
    [SerializeField] private ComputeShader compute;
    [SerializeField] private BoundaryType boundaryType;
    [SerializeField] private Vector2[] sources;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;
    [SerializeField] private float diffusion = .6f;

    enum BoundaryType { Wall, Wrap }

    public RenderTexture fluid;
    public ComputeBuffer sourcesBuffer;
    private float time;

    // Kernel indecies
    private int testShader;
    private int calculateDensity;
    private int advect;

    void Start()
    {
        // Set proper kernels
        testShader = compute.FindKernel("CSMain");
        calculateDensity = compute.FindKernel("CalculateDensity");
        advect = compute.FindKernel("Advect");

        // Create an empty texture
        fluid = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        fluid.enableRandomWrite = true;
        fluid.Create();

        // Set variables
        compute.SetFloat("timeStep", Time.fixedDeltaTime);
        compute.SetFloat("width", width);
        compute.SetFloat("height", height);
        compute.SetFloat("diff", diffusion);
	    

        // Create an environment
        compute.SetTexture(testShader, "Result", fluid);
        compute.Dispatch(testShader, width/8, height/8, 1);
    }

    void FixedUpdate() {
        // Send sources to the compute shader
        if(sourcesBuffer != null) {sourcesBuffer.Release();}
        sourcesBuffer = new ComputeBuffer(sources.Length, 8);
        sourcesBuffer.SetData(sources);
        compute.SetBuffer(calculateDensity, "sources", sourcesBuffer);
        
        
        // Update the density
        compute.SetTexture(calculateDensity, "Result", fluid);
        compute.Dispatch(calculateDensity, width/8, height/8, 1);
        compute.SetTexture(advect, "Result", fluid);
        compute.Dispatch(advect, width/8, height/8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Render to camera
        Graphics.Blit(fluid, dest);
    }

    void OnDestroy() {
        sourcesBuffer.Release();
    }
}
