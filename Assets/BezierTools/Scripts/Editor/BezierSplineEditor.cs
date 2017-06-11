using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineEditor : Editor {

    BezierSpline bezierSpline;

    

    private void OnEnable()
    {
        bezierSpline = (BezierSpline)target;
        if (bezierSpline.GUIProfile == null)
        {
            bezierSpline.GUIProfile = Resources.FindObjectsOfTypeAll<BezierSplineGUIProfile>()[0];
        }

        if (bezierSpline.GUIProfile == null)
        {
            Debug.LogError(this.name + ":GUI Profile Not found!");
        }
    }

    private void OnSceneGUI()
    {
        if(bezierSpline.drawMainHandles)
        {
            Undo.RecordObject(this.bezierSpline, "Move Nodes MainPoints");
            foreach (BezierSpline.BezierNode current in bezierSpline.nodes)
            {
                Vector3 oldPos = current.mainPoint;
                current.mainPoint = Handles.PositionHandle(current.mainPoint + bezierSpline.GetPosFix(), Quaternion.identity) - bezierSpline.GetPosFix();
                Vector3 offset = current.mainPoint - oldPos;
                current.nextControlPoint += offset;
                current.previousControlPoint += offset;
            }
        }

        if(bezierSpline.drawControlHandles)
        {
            Undo.RecordObject(this.bezierSpline, "Move Nodes ControlPoints");
            foreach (BezierSpline.BezierNode current in bezierSpline.nodes)
            {
                Vector3 oldValue = current.nextControlPoint;
                current.nextControlPoint = Handles.PositionHandle(current.nextControlPoint + bezierSpline.GetPosFix(), Quaternion.identity) - bezierSpline.GetPosFix();

                if(bezierSpline.smoothEditing && oldValue != current.nextControlPoint)
                {
                    current.previousControlPoint = current.mainPoint + (current.mainPoint - current.nextControlPoint).normalized * (current.previousControlPoint - current.mainPoint).magnitude;
                }

                oldValue = current.previousControlPoint;
                current.previousControlPoint = Handles.PositionHandle(current.previousControlPoint + bezierSpline.GetPosFix(), Quaternion.identity) - bezierSpline.GetPosFix();
                if (bezierSpline.smoothEditing && oldValue != current.previousControlPoint)
                {
                    current.nextControlPoint = current.mainPoint + (current.mainPoint - current.previousControlPoint).normalized * (current.nextControlPoint - current.mainPoint).magnitude;
                }
            }
        }

        //GUI
        Handles.BeginGUI();
        GUILayout.BeginVertical();

        GUILayout.BeginArea(new Rect(20, 20, 50, 500));
        //GUI.skin.button.normal.background = (Texture2D)bezierSpline.GUIProfile.buttonBackGround_on;
        //GUI.skin.button.hover.background = (Texture2D)bezierSpline.GUIProfile.buttonBackGround_hover;
        //GUI.skin.button.active.background = (Texture2D)bezierSpline.GUIProfile.buttonBackGround_pressed;

        bezierSpline.drawSpline = GUILayout.Toggle(bezierSpline.drawSpline, bezierSpline.GUIProfile.buttonTexture_Spline_on, GUILayout.MinHeight(50));
        bezierSpline.drawTangent = GUILayout.Toggle(bezierSpline.drawTangent, bezierSpline.GUIProfile.buttonTexture_Tangent_on, GUILayout.MinHeight(50));
        bezierSpline.drawMainHandles = GUILayout.Toggle(bezierSpline.drawMainHandles, bezierSpline.GUIProfile.buttonTexture_MainPointAxis_on, GUILayout.MinHeight(50));
        bezierSpline.drawControlHandles = GUILayout.Toggle(bezierSpline.drawControlHandles, bezierSpline.GUIProfile.buttonTexture_CtrlPointAxis_on, GUILayout.MinHeight(50));
        
        if (bezierSpline.smoothEditing)
        {
            if (GUILayout.Button(bezierSpline.GUIProfile.buttonTexture_Smooth_on, GUILayout.MinHeight(50)))
            {
                bezierSpline.smoothEditing = false;
            }
        }
        else
        {
            if (GUILayout.Button(bezierSpline.GUIProfile.buttonTexture_Smooth_off, GUILayout.MinHeight(50)))
            {
                bezierSpline.smoothEditing = true;
            }
        }

        if(GUILayout.Button(bezierSpline.GUIProfile.buttonTexture_NewNode ,GUILayout.MinHeight(50)))
        {
            Undo.RecordObject(this.bezierSpline, "Add node");
            bezierSpline.AddNode();
        }
        
        GUILayout.EndArea();
        GUILayout.EndVertical();
        Handles.EndGUI();
    }

}
