using UnityEngine;

public class MobileMotionBlurVr : MonoBehaviour
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

    private Camera.StereoscopicEye eyeIndex;
    private Matrix4x4 previousViewProjectionLeft, previousViewProjectionRight;
    private Matrix4x4 viewProj;
    private Matrix4x4 currentToPreviousViewProjectionMatrix;
    void Start()
    {
        cam = GetComponent<Camera>();
        previousViewProjectionLeft = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left) * cam.worldToCameraMatrix;
        previousViewProjectionRight = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right) * cam.worldToCameraMatrix;
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

        eyeIndex = cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right;
        viewProj = cam.GetStereoProjectionMatrix(eyeIndex) * cam.worldToCameraMatrix;
        if(eyeIndex == Camera.StereoscopicEye.Left && viewProj == previousViewProjectionLeft || eyeIndex == Camera.StereoscopicEye.Right && viewProj == previousViewProjectionRight)
        {
            Graphics.Blit(src, dest);
            return;
        }
        currentToPreviousViewProjectionMatrix = (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left ? previousViewProjectionLeft : previousViewProjectionRight) * viewProj.inverse;
        motionBlurMaterial.SetMatrix(currentPrevString, currentToPreviousViewProjectionMatrix);
        motionBlurMaterial.SetFloat(distanceString, 1 - Distance);
        if(eyeIndex == Camera.StereoscopicEye.Left)
        {
            previousViewProjectionLeft = viewProj;
        }
        else
        {
            previousViewProjectionRight = viewProj;
        }
        var rt = RenderTexture.GetTemporary(descriptor);
        Graphics.Blit(src, rt, motionBlurMaterial, 0);
        motionBlurMaterial.SetTexture(blurTexString, rt);
        Graphics.Blit(src, dest, motionBlurMaterial, 1);
        RenderTexture.ReleaseTemporary(rt);
    }
}
