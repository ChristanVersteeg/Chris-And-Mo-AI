using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateLimit : MonoBehaviour
{
    [SerializeField] private int frameRateLimit = 60;
    void Start()
    {
        Application.targetFrameRate = frameRateLimit;
        QualitySettings.vSyncCount = 0;
    }
}
