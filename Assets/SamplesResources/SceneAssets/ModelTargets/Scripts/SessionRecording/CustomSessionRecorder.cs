/*========================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.
 
Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/


using System;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class CustomSessionRecorder : MonoBehaviour
{
    public GameObject RecordButton;
    public GameObject StopButton;
    public GameObject InfoPopup;
    public Text Timer;
    public Text InfoText;
    
    SessionRecorderBehaviour mRecorder;
    DateTime mStartTime;

    const string TIMER_ZERO = "00:00";
    const string TIMER_FORMAT_MMSS = @"mm\:ss";
    const string TIMER_FORMAT_HHMMSS = @"hh\:mm\:ss";

    void Awake()
    {
        mRecorder = GetComponent<SessionRecorderBehaviour>();
    }

    void Update()
    {
        if (mRecorder.GetRecordingStatus() == RecordingStatus.RECORDING_IN_PROGRESS)
            UpdateTimer();
    }

    public void RecordingStarted(RecordingStatus status)
    {
        switch (status)
        {
            case RecordingStatus.RECORDING_IN_PROGRESS:
                RecordButton.SetActive(false);
                StopButton.SetActive(true);
                StartTimer();
                break;
            case RecordingStatus.SOURCES_NOT_AVAILABLE:
                ShowInfoPopup("Could not start recording: sources not available.");
                break;
            case RecordingStatus.STORAGE_LOCATION_RETRIEVAL_ERROR:
                ShowInfoPopup("Could not start recording: Unable to retrieve a valid path to store recording data.");
                break;
            case RecordingStatus.SOURCE_OPERATION_ERROR:
                ShowInfoPopup("Could not start recording: source operation error.");
                break;
            case RecordingStatus.INSUFFICIENT_FREE_SPACE:
                ShowInfoPopup("Could not start recording: insufficient free disk space.");
                break;
            case RecordingStatus.ORIENTATION_NOT_SUPPORTED:
                ShowInfoPopup("Could not start recording: Recording is not supported in Portrait orientation.");
                break;
            case RecordingStatus.UNITY_PLAYMODE_NOT_SUPPORTED:
                ShowInfoPopup("Could not start recording: Recording is not supported from Unity Editor Play Mode.");
                break;
            default:
                ShowInfoPopup("Could not start recording: unknown error.");
                break;
        }
    }

    public void RecordingStopped(RecordingStatus status)
    {
        switch (status)
        {
            case RecordingStatus.RECORDING_NOT_STARTED:
                ShowInfoPopup($"Vuforia session recording saved.");
                break;
            case RecordingStatus.STORAGE_LOCATION_RETRIEVAL_ERROR:
                ShowInfoPopup("Recording stopped: Unable to retrieve a valid path to store recording data.");
                break;
            case RecordingStatus.SOURCES_NOT_AVAILABLE:
                ShowInfoPopup("Recording stopped: sources not available.");
                break;
            case RecordingStatus.SOURCE_OPERATION_ERROR:
                ShowInfoPopup("Recording stopped: source operation error.");
                break;
            case RecordingStatus.INSUFFICIENT_FREE_SPACE:
                ShowInfoPopup("Recording stopped: insufficient free disk space.");
                break;
            case RecordingStatus.ORIENTATION_NOT_SUPPORTED:
                ShowInfoPopup("Recording stopped: Recording is not supported in Portrait orientation.");
                break;
           default:
                ShowInfoPopup("Recording stopped: unknown error.");
                break;
        }
        
        RecordButton.SetActive(true);
        StopButton.SetActive(false);
        StopTimer();
    }

    public void StorageCleaned(bool cleaned)
    {
        if (cleaned)
            ShowInfoPopup("All the existing recordings have been deleted.");
        else
            ShowInfoPopup("Could not delete previous recordings. Please check the log for more info.");
    }

    void ShowInfoPopup(string message)
    {
        InfoText.text = message;
        InfoPopup.SetActive(true);
    }

    void StartTimer()
    {
        Timer.gameObject.SetActive(true);
        mStartTime = DateTime.Now;
        Timer.text = TIMER_ZERO;
    }

    void UpdateTimer()
    {
        var deltaTime = DateTime.Now - mStartTime;
        Timer.text = deltaTime.ToString(deltaTime.Hours < 1 ? TIMER_FORMAT_MMSS : TIMER_FORMAT_HHMMSS);
    }

    void StopTimer()
    {
        Timer.gameObject.SetActive(false);
        Timer.text = TIMER_ZERO;
    }
}
