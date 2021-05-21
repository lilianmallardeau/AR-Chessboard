/*========================================================================
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/


using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


/// <summary>
/// Purpose of this post build script is to post process the info.plist file
/// of the Xcode project during the iOS build process.
/// In particular, this script will make sure that the UIFileSharingEnabled flag is added.
/// </summary>
class SharingEnabledPostProcessBuildPlayer
{
    [PostProcessBuildAttribute(2)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            ProcessInfoPList (pathToBuiltProject + "/Info.plist");  
        }
    }

    static void ProcessInfoPList(string infoPListPath)
    {
        Debug.Log("Processing Info.plist file: " + infoPListPath);

        var lines = File.ReadAllLines (infoPListPath);
        var lineList = new List<string> ();
        foreach (var line in lines) 
        {
            if (line.Contains ("<key>UISupportedInterfaceOrientations</key>")) 
            {
                // we insert the UIFileSharingEnabled
                lineList.Add ("<key>UIFileSharingEnabled</key>");
                lineList.Add ("<true />");
            }

            lineList.Add (line);
        }

        // Re-write the file with the updated lines
        var updatedLines = lineList.ToArray ();
        File.WriteAllLines (infoPListPath, updatedLines);
    }
}
