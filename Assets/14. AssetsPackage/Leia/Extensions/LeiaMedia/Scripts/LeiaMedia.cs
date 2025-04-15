/*
 * Copyright 2024 (c) Leia Inc.  All rights reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Leia Inc. and its suppliers, if any.  The
 * intellectual and technical concepts contained herein are
 * proprietary to Leia Inc. and its suppliers and may be covered
 * by U.S. and Foreign Patents, patents in process, and are
 * protected by trade secret or copyright law.  Dissemination of
 * this information or reproduction of this materials strictly
 * forbidden unless prior written permission is obtained from
 * Leia Inc.
 */
using UnityEngine;
using UnityEngine.Video;
using LeiaUnity;

public class LeiaMedia : MonoBehaviour
{
    public enum MediaType
    {
        Image,
        Video
    }

    [SerializeField] private MediaType mediaType;

    [SerializeField] private Texture2D sbsTexture;

    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private LeiaDisplay leiaDisplay;

    private Camera leftEyeCamera;
    private Camera rightEyeCamera;
    private Camera twoDimCamera;

    private Material leftMaterial;
    private Material rightMaterial;
    private Material twoDimMaterial;
    private RenderTrackingDevice.LightfieldMode lastKnownLightfieldMode;

    RenderTexture leftRT;
    RenderTexture rightRT;
    SimulatedRealityCamera srCam;

    private bool RTinitialized;
    public enum MediaScaleMode
    {
        WorldXYZScale,
        OnscreenPercent
    }

    [SerializeField] private MediaScaleMode _mediaScaleMode;
    public MediaScaleMode mediaScaleMode
    {
        get
        {
            return _mediaScaleMode;
        }
        set
        {
            _mediaScaleMode = value;
        }
    }
    [Tooltip("X,Y: offset from left bottom screen corner, W : width H: height")]
    [SerializeField] private Rect onscreenPercent = new Rect(0, 0, 1, 1);
    public Rect OnscreenPercent
    {
        get
        {
            return onscreenPercent;
        }

        set
        {
            onscreenPercent = value;
        }
    }

    void Start()
    {
        SetupRig();

        if (mediaType == MediaType.Video)
        {
            videoPlayer.errorReceived += HandleVideoError;
        }
    }

    private void Update()
    {
        if (!RTinitialized && leftEyeCamera.targetTexture != null && rightEyeCamera.targetTexture != null)
        {
            leftRT = new RenderTexture(leftEyeCamera.targetTexture.width, leftEyeCamera.targetTexture.height, leftEyeCamera.targetTexture.depth);
            rightRT = new RenderTexture(rightEyeCamera.targetTexture.width, rightEyeCamera.targetTexture.height, rightEyeCamera.targetTexture.depth);
            RTinitialized = true;
        }

        if (lastKnownLightfieldMode != RenderTrackingDevice.Instance.DesiredLightfieldMode)
        {
            lastKnownLightfieldMode = RenderTrackingDevice.Instance.DesiredLightfieldMode;
            SetupRig();
        }

        if (mediaScaleMode == MediaScaleMode.WorldXYZScale)
        {
            Vector3 scale = transform.localScale;

            onscreenPercent.width = scale.x;
            onscreenPercent.height = scale.y;
            onscreenPercent.x = 0.5f - (onscreenPercent.width / 2.0f);
            onscreenPercent.y = 0.5f - (onscreenPercent.height / 2.0f);

            SetShaderParams();
        }

        if (mediaType == MediaType.Video)
        {
            OnRenderObject();
        }
    }

    void SetupRig()
    {
        leftMaterial = leftMaterial ?? new Material(Shader.Find("Custom/SBS_Left"));
        rightMaterial = rightMaterial ?? new Material(Shader.Find("Custom/SBS_Right"));
        twoDimMaterial = twoDimMaterial ?? new Material(Shader.Find("Custom/SBS_Left"));

#if UNITY_EDITOR || PLATFORM_ANDROID
        leftEyeCamera = leiaDisplay.ViewersHead.eyes[0].Eyecamera;
        rightEyeCamera = leiaDisplay.ViewersHead.eyes[1].Eyecamera;
        twoDimCamera = leiaDisplay.HeadCamera;
#elif !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        GameObject srCameraObject = GameObject.Find("SRCamera");
        srCam = srCameraObject.GetComponent<SimulatedRealityCamera>();
        leftEyeCamera = srCam.GetCameraComponents()[0];
        rightEyeCamera = srCam.GetCameraComponents()[1];
#endif

        if (mediaType == MediaType.Image)
        {
            leftMaterial.mainTexture = sbsTexture;
            rightMaterial.mainTexture = sbsTexture;
        }
        SetShaderParams();
    }

    void SetShaderParams()
    {
        Vector4 onscreenPercentVector = new Vector4(onscreenPercent.x, onscreenPercent.y, onscreenPercent.width, onscreenPercent.height);
        float enableOnscreen = (mediaScaleMode == MediaScaleMode.OnscreenPercent || mediaScaleMode == MediaScaleMode.WorldXYZScale) ? 1f : 0f;

        leftMaterial.SetVector("_OnscreenPercent", onscreenPercentVector);
        leftMaterial.SetFloat("_EnableOnscreenPercent", enableOnscreen);

        rightMaterial.SetVector("_OnscreenPercent", onscreenPercentVector);
        rightMaterial.SetFloat("_EnableOnscreenPercent", enableOnscreen);
    }
    void OnRenderObject()
    {
        if (mediaType == MediaType.Video)
        {
            Graphics.Blit(videoPlayer.texture, leftRT, leftMaterial);
            Graphics.Blit(videoPlayer.texture, rightRT, rightMaterial);

        }
        else if (mediaType == MediaType.Image)
        {
            Graphics.Blit(sbsTexture, leftRT, leftMaterial);
            Graphics.Blit(sbsTexture, rightRT, rightMaterial);
        }
#if UNITY_EDITOR || PLATFORM_ANDROID
        if (RenderTrackingDevice.Instance.DesiredLightfieldMode == RenderTrackingDevice.LightfieldMode.On)
        {
            leftEyeCamera.targetTexture = leftRT;
            rightEyeCamera.targetTexture = rightRT;
            twoDimCamera.targetTexture = null;
        }
        else if (RenderTrackingDevice.Instance.DesiredLightfieldMode == RenderTrackingDevice.LightfieldMode.Off)
        {
            leftEyeCamera.targetTexture = leftRT;
            rightEyeCamera.targetTexture = leftRT;
        }
#elif !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        var compositor = SRUnity.SRRender.Instance.GetCompositor();

        var leftFramebuffer = compositor.GetFrameBuffer(srCam.GetCameraComponents()[0].GetInstanceID().ToString());
        var rightFramebuffer = compositor.GetFrameBuffer(srCam.GetCameraComponents()[1].GetInstanceID().ToString());

        leftFramebuffer.frameBuffer = leftRT;
        rightFramebuffer.frameBuffer = rightRT;
#endif
    }

    void HandleVideoError(VideoPlayer source, string message)
    {
        LogUtil.Log(LogLevel.Error, string.Format("Source: {0} Video Error: {1}", source, message));
    }
}