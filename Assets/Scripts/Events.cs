using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : MonoBehaviour
{
    public static Action onPaused = () => Time.timeScale = 0f;
    public static Action onUnpaused = () => Time.timeScale = 1f;
    public static Action onSaveLoaded = delegate { };
    public static Action onGameSpeedChange = delegate { };
    
    public static Action<int, int> onResourceChange = delegate {};
    // Action<oldValue, newValue>
    public static Action<int, int> onFoodChange = delegate {};
    public static Action<int, int> onLumberChange = delegate {};
    public static Action<int, int> onMagicChange = delegate {};
    public static Action<float, float> onGrowthModChange = delegate {};
    public static Action<int, int> onPopLimitChange = delegate {};

    public static Action onJobChange = delegate {};
    public static Action onVillagerCountChange = delegate {};


    public static void ResetEventDelegates()
    {
        onPaused = () => Time.timeScale = 0f;
        onUnpaused = () => Time.timeScale = 1f;
        onSaveLoaded = delegate { };
        onGameSpeedChange = delegate { };
        
        onResourceChange = delegate {};
        onFoodChange = delegate {};
        onLumberChange = delegate {};
        onMagicChange = delegate {};
        onGrowthModChange = delegate {};
        onPopLimitChange = delegate {};
        onJobChange = delegate {};

        onFoodChange += Events.onResourceChange;
        onLumberChange += Events.onResourceChange;
        onMagicChange += Events.onResourceChange;

        #if UNITY_EDITOR
                Debug.Log("Events.cs: event delegates reset.");
        #endif
    }
}
