using UnityEngine;

public class MobileMotionBlur : MonoBehaviour
{
    public enum SampleType
    {
        Six = 6,
        Eight = 8,
        Ten = 10
    };

    [Range(0, 1)]
    public float Distance;
    [Range(1, 5)]
    public int FastFilter = 4;
    public SampleType SampleCount = SampleType.Six;
    Camera cam;
    RenderTextureDescriptor descriptor;
    public Material motionBlurMaterial;
    static readonly int currentPrevString = Shader.PropertyToID("_CurrentToPreviousViewProjectionMatrix");
    static readonly int distanceString = Shader.PropertyToID("_Distance");
    static readonly int blurTexString = Shader.PropertyToID("_BlurTex");
    static readonly string eightFeature = "EIGHT";
    static readonly string tenFeature = "TEN";

    private Matrix4x4 previousViewProjection;
    private Matrix4x4 viewProj;
    private Matrix4x4 currentToPreviousViewProjectionMatrix;
    void Start()
    {
        cam = GetComponent<Camera>();
        previousViewProjection = cam.projectionMatrix * cam.worldToCameraMatrix;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        descriptor = src.descriptor;
        descriptor.width = Screen.width / FastFilter;
        descriptor.height = Screen.height / FastFilter;
        descriptor.depthBufferBits = 0;
        switch ((int)SampleCount)
        {
            case 6:
                motionBlurMaterial.DisableKeyword(eightFeature);
                motionBlurMaterial.DisableKeyword(tenFeature);
                break;
            case 8:
                motionBlurMaterial.EnableKeyword(eightFeature);
                motionBlurMaterial.DisableKeyword(tenFeature);
                break;
            case 10:
                motionBlurMaterial.EnableKeyword(eightFeature);
                motionBlurMaterial.EnableKeyword(tenFeature);
                break;
        }

        viewProj = cam.projectionMatrix * cam.worldToCameraMatrix;
        if (viewProj == previousViewProjection)
        {
            Graphics.Blit(src, dest);
            return;
        }
        currentToPreviousViewProjectionMatrix = previousViewProjection * viewProj.inverse;
        motionBlurMaterial.SetMatrix(currentPrevString, currentToPreviousViewProjectionMatrix);
        motionBlurMaterial.SetFloat(distanceString, 1 - Distance);
        previousViewProjection = viewProj;
        var rt = RenderTexture.GetTemporary(descriptor);
        Graphics.Blit(src, rt, motionBlurMaterial, 0);
        motionBlurMaterial.SetTexture(blurTexString, rt);
        Graphics.Blit(src, dest, motionBlurMaterial, 1);
        RenderTexture.ReleaseTemporary(rt);
    }
}
