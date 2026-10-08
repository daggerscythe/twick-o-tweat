using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Phase 4: the power flickers and goes out, the flashlight turns on, ghosts start glowing
// lives on Managers

public class LightsOut : MonoBehaviour
{
    public static bool Active { get; private set; }

    [Header("References")]
    [SerializeField] private Light[] houseLights; // Directional Light, RoomLight, PorchLight
    [SerializeField] private Light flashlight; // spot light under Main Camera, starts off

    [Header("Darkness")]
    [SerializeField] private Color darkAmbient = new Color(0.02f, 0.02f, 0.05f); // almost black, a bit blue
    [SerializeField] private float darkReflections = 0.1f; // 0-1, stops objects shining in the dark

    [Header("Flicker")]
    [SerializeField] private int flickers = 3;
    [SerializeField] private float flickerTime = 0.12f; // seconds off / on

    [Header("Debug")]
    [SerializeField] private bool debugKeys = true; // L = lights out now

    private bool flickering; // stops a second outage starting during the first

    private void Awake()
    {
        Active = false; // static values survive a scene reload, so reset by hand
        if (flashlight != null) flashlight.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (debugKeys && GameManager.IsPlaying && Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame) TurnOff();
    }

    // onDone runs when the room is fully dark (NightDirector shows the pop-up then)
    public void TurnOff(Action onDone = null)
    {
        if (Active || flickering) return;
        StartCoroutine(PowerOutage(onDone));
    }

    // a coroutine can wait in the middle (yield return) and continue on a later frame
    private IEnumerator PowerOutage(Action onDone)
    {
        flickering = true;
        for (int i = 0; i < flickers; i++)
        {
            SetHouseLights(false);
            yield return new WaitForSeconds(flickerTime);
            SetHouseLights(true);
            yield return new WaitForSeconds(flickerTime);
        }
        SetHouseLights(false);

        // ambient = the light that comes from "everywhere" (the sky); make it almost black
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = darkAmbient;
        SphericalHarmonicsL2 ambient = new SphericalHarmonicsL2();
        ambient.AddAmbientLight(darkAmbient);
        RenderSettings.ambientProbe = ambient; // URP reads ambient light from here
        RenderSettings.reflectionIntensity = darkReflections;

        if (flashlight != null) flashlight.enabled = true;
        Active = true;
        flickering = false;

        onDone?.Invoke();
    }

    private void SetHouseLights(bool on)
    {
        foreach (Light l in houseLights)
        {
            if (l != null) l.enabled = on;
        }
    }
}