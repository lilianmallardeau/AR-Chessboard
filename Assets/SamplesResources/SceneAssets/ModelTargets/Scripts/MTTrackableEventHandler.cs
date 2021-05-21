/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using UnityEngine;
using Vuforia;

public class MTTrackableEventHandler : DefaultTrackableEventHandler
{
    #region PUBLIC_MEMBERS
    [SerializeField]
    GameObject ScaleWarningPopup;
    
    #endregion
    
    #region PROTECTED_METHODS
    protected override void HandleTrackableStatusInfoChanged()
    {
        base.HandleTrackableStatusInfoChanged();
        
        if (m_NewStatusInfo == TrackableBehaviour.StatusInfo.WRONG_SCALE)
        {
            EnableScaleWarningPopup();
        }
    }
    
    #endregion
    
    #region PRIVATE_METHODS

    private void EnableScaleWarningPopup()
    {
        if (ScaleWarningPopup != null)
        {
            ScaleWarningPopup.SetActive(true);
        }
    }
    
    #endregion
}