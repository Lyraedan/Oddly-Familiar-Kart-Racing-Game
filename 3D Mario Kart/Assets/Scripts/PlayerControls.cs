using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    // A binder for the new Unity Input System that translates input actions into legacy-style static functions like GetButton and GetAxis.

    public InputActionAsset controls;

    private InputActionMap kartActionMap;

    private static Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();
    private static Dictionary<string, bool> currentButtonStates = new Dictionary<string, bool>();
    private static Dictionary<string, bool> previousButtonStates = new Dictionary<string, bool>();

    // Action names
    public static string DRIFT = "Drift";
    public static string ACCELERATE = "Accelerate";
    public static string REVERSE = "Reverse";
    public static string USE_ITEM = "Use Item";
    public static string TRICK = "Trick";
    public static string STEER_LEFT = "Steer Left";
    public static string STEER_RIGHT = "Steer Right";
    public static string GLIDER_UP = "Glider Up";
    public static string GLIDER_DOWN = "Glider Down";
    public static string LOOK_BEHIND = "Look Behind";
    public static string THROW_BACK = "Throw Back";

    private void Awake()
    {
        kartActionMap = controls.FindActionMap("Kart", true);

        // Initialize all actions
        InitializeAction(ACCELERATE);
        InitializeAction(REVERSE);
        InitializeAction(DRIFT);
        InitializeAction(USE_ITEM);
        InitializeAction(TRICK);
        InitializeAction(STEER_LEFT);
        InitializeAction(STEER_RIGHT);
        InitializeAction(GLIDER_UP);
        InitializeAction(GLIDER_DOWN);
        InitializeAction(LOOK_BEHIND);
        InitializeAction(THROW_BACK);

        kartActionMap.Enable();
    }

    //private void OnEnable() => kartActionMap.Enable();
    //private void OnDisable() => kartActionMap.Disable();

    private void Update()
    {
        // Update previous button states
        foreach (var key in currentButtonStates.Keys)
        {
            previousButtonStates[key] = currentButtonStates[key];
        }

        // Update current button states for digital inputs
        foreach (var action in kartActionMap.actions)
        {
            float value = action.ReadValue<float>();
            currentButtonStates[action.name] = value > 0.1f;
        }

        Debug.Log("Input.GetAxis(\"Horizontal\"): " + GetAxis("Steer Left") + " / " + GetAxis("Steer Right") + " / " + GetSteerAxis());
    }

    private void InitializeAction(string actionName)
    {
        var action = kartActionMap.FindAction(actionName, true);
        actions[actionName] = action;
        currentButtonStates[actionName] = false;
        previousButtonStates[actionName] = false;

        // Optional: subscribe to performed/canceled
        action.performed += ctx => currentButtonStates[actionName] = true;
        action.canceled += ctx => currentButtonStates[actionName] = false;
    }

    // ===== Legacy-style static functions =====
    public static bool GetButton(string actionName)
    {
        return currentButtonStates.ContainsKey(actionName) && currentButtonStates[actionName];
    }

    public static bool GetButtonDown(string actionName)
    {
        if (!currentButtonStates.ContainsKey(actionName) || !previousButtonStates.ContainsKey(actionName)) return false;
        return currentButtonStates[actionName] && !previousButtonStates[actionName];
    }

    public static bool GetButtonUp(string actionName)
    {
        if (!currentButtonStates.ContainsKey(actionName) || !previousButtonStates.ContainsKey(actionName)) return false;
        return !currentButtonStates[actionName] && previousButtonStates[actionName];
    }

    public static float GetAxis(string actionName)
    {
        if (!actions.ContainsKey(actionName)) return 0f;
        return actions[actionName].ReadValue<float>();
    }

    public static float GetSteerAxis()
    {
        // Returns combined horizontal steering value: right - left
        float right = GetAxis(STEER_RIGHT);
        float left = GetAxis(STEER_LEFT);
        return right - left; // right = positive, left = negative
    }
}
