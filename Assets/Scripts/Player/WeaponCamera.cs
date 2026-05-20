using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Attach to the same GameObject as your main Camera.
/// Renders ONLY the equipped weapon on top of everything — no wall clipping.
///
/// One-time setup:
///   1. Edit > Project Settings > Tags and Layers — add a layer named "Weapon".
///   2. Set equippedItemParent (on EntityPrimaryEquippedHandler) to the Weapon layer.
///   3. Attach this script to the main Camera GameObject.
///
/// Works automatically in both Built-in and URP.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WeaponCamera : MonoBehaviour
{
    [Tooltip("Must exactly match the layer name in Tags and Layers.")]
    [SerializeField] private string weaponLayerName = "Weapon";

    [Tooltip("Very small near clip so the gun never disappears up close.")]
    [SerializeField] private float weaponNearClip = 0.01f;

    private Camera mainCam;
    private Camera weaponCam;

    private void Awake()
    {
        mainCam = GetComponent<Camera>();

        int weaponLayer = LayerMask.NameToLayer(weaponLayerName);
        if (weaponLayer < 0)
        {
            Debug.LogError($"[WeaponCamera] Layer '{weaponLayerName}' not found. " +
                           "Add it in Edit > Project Settings > Tags and Layers.");
            return;
        }

        // Main camera never renders the weapon — weapon camera handles it exclusively.
        mainCam.cullingMask &= ~(1 << weaponLayer);

        // Build the weapon camera as a child so it follows the main camera automatically.
        var go = new GameObject("WeaponCamera");
        go.transform.SetParent(transform, false);

        weaponCam              = go.AddComponent<Camera>();
        weaponCam.cullingMask  = 1 << weaponLayer;
        weaponCam.depth        = mainCam.depth + 1;
        weaponCam.nearClipPlane = weaponNearClip;
        weaponCam.farClipPlane = mainCam.farClipPlane;
        weaponCam.fieldOfView  = mainCam.fieldOfView;

        // Never render UI or audio from the weapon camera.
        weaponCam.targetDisplay = mainCam.targetDisplay;
        AudioListener al = go.GetComponent<AudioListener>();
        if (al != null) Destroy(al);

        // URP needs the weapon camera registered as an Overlay in the base camera stack.
        // Built-in just needs Depth-Only clear to draw on top.
        if (IsURP())
            ConfigureURPOverlay();
        else
            weaponCam.clearFlags = CameraClearFlags.Depth;
    }

    private void LateUpdate()
    {
        // Keep weapon camera FOV in sync (e.g. during zoom).
        if (weaponCam != null)
            weaponCam.fieldOfView = mainCam.fieldOfView;
    }

    // ── URP overlay setup via reflection (no compile-time URP dependency) ──

    private static bool IsURP()
    {
        return GraphicsSettings.currentRenderPipeline != null &&
               GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("Universal");
    }

    private void ConfigureURPOverlay()
    {
        // Resolve UniversalAdditionalCameraData at runtime so this compiles without URP.
        Type camDataType = Type.GetType(
            "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, " +
            "Unity.RenderPipelines.Universal.Runtime");

        if (camDataType == null)
        {
            // URP assembly not found — fall back to depth clear.
            weaponCam.clearFlags = CameraClearFlags.Depth;
            Debug.LogWarning("[WeaponCamera] URP detected but its assembly wasn't found. " +
                             "Falling back to Depth clear — screen may look wrong.");
            return;
        }

        PropertyInfo renderTypeProp = camDataType.GetProperty("renderType");
        PropertyInfo stackProp      = camDataType.GetProperty("cameraStack");

        // Mark weapon camera as Overlay (CameraRenderType.Overlay == 1).
        Component weaponData = weaponCam.GetComponent(camDataType)
                            ?? weaponCam.gameObject.AddComponent(camDataType);
        if (renderTypeProp != null)
            renderTypeProp.SetValue(weaponData, Enum.ToObject(renderTypeProp.PropertyType, 1));

        // Make sure the main camera is Base (CameraRenderType.Base == 0) and add weapon cam to its stack.
        Component mainData = mainCam.GetComponent(camDataType)
                          ?? mainCam.gameObject.AddComponent(camDataType);
        if (renderTypeProp != null)
            renderTypeProp.SetValue(mainData, Enum.ToObject(renderTypeProp.PropertyType, 0));

        if (stackProp?.GetValue(mainData) is IList stack && !stack.Contains(weaponCam))
            stack.Add(weaponCam);
    }
}
