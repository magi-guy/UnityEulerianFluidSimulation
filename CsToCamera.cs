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

    public ComputeBuffer fluid;
    private RenderTexture fluidTexture;
    private ComputeBuffer sourcesBuffer;
    private float time;
    private bool doSources;

    // Kernel indecies
    private int testShader;
    private int toTexture;
    private int calculateDensity;
    private int advect;

    void Start()
    {
        // Set proper kernels
        testShader = compute.FindKernel("CSMain");
        toTexture = compute.FindKernel("BufferToTexture");
        calculateDensity = compute.FindKernel("CalculateDensity");
        advect = compute.FindKernel("Advect");

        // Create the fluid buffer
        fluid = new ComputeBuffer(width * height, 20);

        // Check if sources exist
        doSources = sources.Length > 0;

        // Set variables
        compute.SetBool("doSources", doSources);
        compute.SetFloat("timeStep", Time.fixedDeltaTime);
        compute.SetFloat("width", width);
        compute.SetFloat("height", height);
        compute.SetFloat("diff", diffusion);
	    

        // Create an environment
        compute.SetBuffer(testShader, "Fluid", fluid);
        compute.Dispatch(testShader, width/8, height/8, 1);
    }

    void FixedUpdate() {
        if(doSources) {
            // Send sources to the compute shader
            if(sourcesBuffer != null) {sourcesBuffer.Release();}
            sourcesBuffer = new ComputeBuffer(sources.Length, 8);
            sourcesBuffer.SetData(sources);
            compute.SetBuffer(calculateDensity, "sources", sourcesBuffer);
        }
        
        // Update the density
        compute.SetBuffer(calculateDensity, "Fluid", fluid);
        compute.Dispatch(calculateDensity, width/8, height/8, 1);
        compute.SetBuffer(advect, "Fluid", fluid);
        compute.Dispatch(advect, width/8, height/8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(fluidTexture == null) {
            fluidTexture = new RenderTexture(width, height, 24);
            fluidTexture.enableRandomWrite = true;
            fluidTexture.Create();
        }

        compute.SetBuffer(toTexture, "Fluid", fluid);
        compute.SetTexture(toTexture, "Result", fluidTexture);
        compute.Dispatch(toTexture, width/8, height/8, 1);

        // Render to camera
        Graphics.Blit(fluidTexture, dest);
    }

    void OnDestroy() {
        if(doSources)
            sourcesBuffer.Release();
    }
}
