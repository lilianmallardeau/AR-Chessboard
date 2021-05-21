using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndgamePopupBehavior : MonoBehaviour
{
    [SerializeField] private float phaseOutTimer = 3f;
    
    private bool _isRunning = false;

    public void StartPhaseOut()
    {
        if (!_isRunning)
        {
            StartCoroutine(StartPhaseOutCoroutine());
        }
    }

    private IEnumerator StartPhaseOutCoroutine()
    {
        _isRunning = true;
        float timer = phaseOutTimer;
        while (true)
        {
            timer -= Time.deltaTime;
            if (timer < 0) break;
            yield return null;
        }

        _isRunning = false;
        gameObject.SetActive(false);
    }
}
