using System.Collections;
using System.Collections.Generic;
using System.Net;
using Config;
using Extensions;
using FlutterEmbed.SendToFlutter;
using Log;
using Models;
using Newtonsoft.Json;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utils;
using static UnityEngine.TouchPhase;
using static UnityEngine.Vector3;
using static Http.UnityHttpListener;
//using Input = UnityEngine.Input;
using Input = Utils.Input;

// ReSharper disable StringLiteralTypo
public class ArFoundationController : MonoBehaviour
{
    private const string OnLoadScenePrefix = "@@OnLoadScene@@";
    public const string OnErrorPrefix = "@@OnError@@";

    // ReSharper disable once IdentifierTypo
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private XROrigin arSessionOrigin;
    [SerializeField] private ARSession arSession;
    [SerializeField] private GameObject indicator;
    [SerializeField] private Transform parentIndicator;
    [SerializeField] private bool showPlanes;
    [SerializeField] private float rotation;
    [SerializeField] private Vector2 positionScreenOffset;
    [SerializeField] private Vector3 originalScaleIndicator;


    private readonly List<ARRaycastHit> hits = new();

    private static Vector2 CenterScreen => new(Screen.width / 2f, Screen.height / 2f);

    private void OnEnable()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters =
            {
                new Vector3JsonConverter(),
                new Vector2JsonConverter(),
                new Vector4JsonConverter(),
                new QuaternionJsonConverter(),
                new ColorJsonConverter(),
                new Matrix4X4JsonConverter()
            }
        };


        FileLogger.Clear();

#if UNITY_ANDROID || UNITY_IOS
        Application.logMessageReceived += LogMessageReceived;
#endif
        ARSession.stateChanged += StateChanged;
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void OnDisable()
    {
#if UNITY_ANDROID || UNITY_IOS
        Application.logMessageReceived -= LogMessageReceived;
#endif
        ARSession.stateChanged -= StateChanged;
        SceneManager.sceneLoaded -= SceneLoaded;
    }


    private static void SceneLoaded(Scene scene, LoadSceneMode _)
    {
        SendToFlutter.Send(OnLoadScenePrefix + scene.name);
    }

    private static void StateChanged(ARSessionStateChangedEventArgs state)
    {
        var message = state.state switch
        {
            ARSessionState.SessionInitializing => "AR Session is initializing",
            ARSessionState.SessionTracking => "AR Session is tracking",
            ARSessionState.Unsupported => "AR Session is unsupported",
            ARSessionState.None => "AR Session is none",
            ARSessionState.CheckingAvailability => "AR Session is checking availability",
            ARSessionState.NeedsInstall => "AR Session needs install",
            ARSessionState.Installing => "AR Session is installing",
            ARSessionState.Ready => "AR Session is ready",
            _ => "AR Session is in an unknown state"
        };

        SendToFlutter.Send(message);
    }

    private static void LogMessageReceived(string log, string stackTrace, LogType type)
    {
        if (Application.platform != RuntimePlatform.Android) return;
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                SendToFlutter.Send(OnErrorPrefix + log);
                FileLogger.Log(log + "\n" + stackTrace);
                break;
            case LogType.Warning:
            case LogType.Log:
            default:
                SendToFlutter.Send(log);
                break;
        }
    }

    private void Start()
    {
        UnityMainThreadDispatcher.Initialize();

        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        arSessionOrigin = FindObjectOfType<XROrigin>();
        arSession = FindObjectOfType<ARSession>();

        indicator.SetActive(false);
        parentIndicator = indicator.transform.parent;
        originalScaleIndicator = indicator.transform.localScale;

        RegisterRoutes();

        if (showPlanes) ShowPlanes();
        else HidePlanes();
    }

    private void Update()
    {
        MoveIndicatorWithTouch();

        RotateIndicatorWithTouch();

        ConstraintOffset(offset: ref positionScreenOffset);

        var ray = CenterScreen + positionScreenOffset;
        if (arRaycastManager.Raycast(screenPoint: ray, hitResults: hits, trackableTypes: TrackableType.PlaneWithinPolygon))
        {
            if (hits.Count == 0) return;
            var hitPose = hits[0].pose;

            parentIndicator.position = hitPose.position;
            parentIndicator.rotation = hitPose.rotation;

            if (MathFunc.IsVerticalNormal(hitPose))
            {
                indicator.transform.localEulerAngles = zero + new Vector3(0, arSessionOrigin.Camera.transform.localEulerAngles.y - 90 + rotation, 0);
            }
            else
            {
                indicator.transform.localEulerAngles = new Vector3(0, 90 + rotation, 0);
            }

            if (!indicator.activeInHierarchy)
            {
                indicator.SetActive(value: true);
            }
        }
        else
        {
            indicator.SetActive(value: false);
        }
    }


    private void MoveIndicatorWithTouch()
    {
        if (Input.touchCount != 1) return;
        var touch = Input.GetTouch(0);
        if (touch.phase is not Moved) return;
        positionScreenOffset = touch.position - CenterScreen;
    }

    private void RotateIndicatorWithTouch()
    {
        if (Input.touchCount != 2) return;
        var touchZero = Input.GetTouch(0);
        var touchOne = Input.GetTouch(1);

        if (touchZero.phase is Moved || touchOne.phase is Moved)
        {
            var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            var prevSlope = MathFunc.CalculateSlope(touchZeroPrevPos, touchOnePrevPos);
            var currentSlope = MathFunc.CalculateSlope(touchZero.position, touchOne.position);

            var angle = Mathf.Atan((currentSlope - prevSlope) / (1 + prevSlope * currentSlope)) * Mathf.Rad2Deg;

            RotateIndicator(-angle);
        }

    }

    private void RegisterRoutes()
    {
        AddRoute("/set-indicator", SetIndicator, method: HttpMethod.Post);
        AddRoute<float>("/rotate-indicator", RotateIndicator, dispatchInUnityMainThread: true, method: HttpMethod.Post);
        AddRoute<Size>("/set-size-indicator", SetSizeIndicator, dispatchInUnityMainThread: true, method: HttpMethod.Post);
        AddRoute<Vector2>("/move-indicator", MoveIndicator, dispatchInUnityMainThread: true, method: HttpMethod.Post);
        AddRoute("/clear-planes", ClearPlanes, dispatchInUnityMainThread: true, method: HttpMethod.Post);
        AddRoute("/show-planes", ShowPlanes, dispatchInUnityMainThread: true, method: HttpMethod.Post);
        AddRoute("/hide-planes", HidePlanes, dispatchInUnityMainThread: true, method: HttpMethod.Post);
        AddRoute("/state-planes", StatePlanes);
        AddRoute<float>("/set-factor-scale-indicator", SetFactorScaleIndicator, dispatchInUnityMainThread: true, method: HttpMethod.Post);
    }

    private IEnumerator SetIndicator(HttpListenerResponse response, HttpListenerRequest request)
    {
        var data = request.GetBody<ArtData>();
        SendToFlutter.Send("Loading image from URL: " + data.Url + " with token: " + data.Token + " and user id: " + data.UserId + " and art id: " + data.ArtId + " and size: " + data.Size);

        var formData = new WWWForm();
        formData.AddField("token", data.Token);
        formData.AddField("user_id", data.UserId);
        formData.AddField("art_id", data.ArtId.ToString());

        using var webRequest = UnityWebRequest.Post(data.Url, formData);
        webRequest.downloadHandler = new DownloadHandlerTexture();

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            var texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
            var scale = MathFunc.CalculateScale(data.Size.Width, data.Size.Height, texture);

            if (scale.Width == null || scale.Height == null) yield break;

            originalScaleIndicator = new Vector3(scale.Width.Value, 1000, scale.Height.Value) / 1000;
            indicator.transform.localScale = originalScaleIndicator;
            indicator.GetComponent<MeshRenderer>().material.mainTexture = texture;

            response.Send("Image loaded from URL: " + data.Url);
        }
        else
        {
            SendToFlutter.Send(OnErrorPrefix + "Error loading image from URL: " + data.Url);
            var code = (int)webRequest.responseCode;
            if (webRequest.downloadHandler.data != null)
            {
                response.Send(webRequest.downloadHandler.data, code);
                yield break;
            }

            if (webRequest.downloadHandler.error != null)
            {
                response.Send(webRequest.downloadHandler.error, code);
                yield break;
            }

            response.Send(webRequest.error, code);
        }
    }

    private void SetFactorScaleIndicator(float scale)
    {
        indicator.transform.localScale = originalScaleIndicator * scale;
    }

    private void RotateIndicator(float degrees)
    {
        rotation += degrees;
    }

    private void SetSizeIndicator(Size size)
    {
        var scale = MathFunc.CalculateScale(size.Width, size.Height, indicator.GetComponent<MeshRenderer>().material.mainTexture);
        if (scale.Width == null || scale.Height == null) return;
        indicator.transform.localScale = new Vector3(scale.Width.Value, 1000, scale.Height.Value) / 1000;
    }

    private void MoveIndicator(Vector2 vector)
    {
        positionScreenOffset.x += vector.x;
        positionScreenOffset.y += vector.y;
    }

    private void ClearPlanes()
    {
        arSession.Reset();
    }

    private void ShowPlanes()
    {
        ChangeStatePlane(arPlaneManager.planePrefab, enabledArg: true);
    }

    private void HidePlanes()
    {
        ChangeStatePlane(arPlaneManager.planePrefab, enabledArg: false);
    }

    private void StatePlanes(HttpListenerResponse response, HttpListenerRequest _)
    {
        response.Send(showPlanes);
    }

    private void ChangeStatePlane(GameObject plateArg, bool enabledArg)
    {
        Internal(plateArg);
        var planes = arPlaneManager.trackables;
        foreach (var plane in planes) Internal(plane.gameObject);
        showPlanes = enabledArg;
        return;

        void Internal(GameObject plane)
        {
            plane.GetComponent<ARPlaneMeshVisualizer>().enabled = enabledArg;
            plane.GetComponent<MeshRenderer>().enabled = enabledArg;
            plane.GetComponent<LineRenderer>().enabled = enabledArg;
        }
    }

    private static void ConstraintOffset(ref Vector2 offset)
    {
        var midWidth = Screen.width / 2f;
        var midHeight = Screen.height / 2f;

        if (offset.x < -midWidth) offset.x = -midWidth;
        if (offset.x > midWidth) offset.x = midWidth;
        if (offset.y < -midHeight) offset.y = -midHeight;
        if (offset.y > midHeight) offset.y = midHeight;
    }
}