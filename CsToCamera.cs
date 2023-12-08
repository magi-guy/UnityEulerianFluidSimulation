using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsToCamera : MonoBehaviour
{
    [SerializeField] private ComputeShader compute;
    [SerializeField] private BoundaryType boundaryType;
    [SerializeField] private Vector2[] sources;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float diffusion;

    enum BoundaryType { Wall, Wrap }

    public ComputeBuffer fluid;
    public RenderTexture fluidTexture;
    private ComputeBuffer sourcesBuffer;
    private float time;

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

        // Set variables
        compute.SetBool("doSources", sources.Length > 0);
        compute.SetFloat("timeStep", Time.fixedDeltaTime);
        compute.SetFloat("width", width);
        compute.SetFloat("height", height);
        compute.SetFloat("diff", diffusion);
	    

        // Create an environment
        compute.SetBuffer(testShader, "Fluid", fluid);
        compute.Dispatch(testShader, width/8, height/8, 1);

        // Create a "fake" sources buffer if there are no sources
        if(sources.Length == 0) {
            sourcesBuffer = new ComputeBuffer(1, 8);
        }
    }

    void FixedUpdate() {
        // Check if there are any sources
        compute.SetBool("doSources", sources.Length > 0);
        if(sources.Length > 0) {
            // Send sources to the compute shader
            if(sourcesBuffer != null) {sourcesBuffer.Release();}
            sourcesBuffer = new ComputeBuffer(sources.Length, 8);
            sourcesBuffer.SetData(sources);
        }
        // If there are no sources, the "fake" sources buffer is used
        compute.SetBuffer(calculateDensity, "sources", sourcesBuffer);

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
        sourcesBuffer.Release();
    }
}
