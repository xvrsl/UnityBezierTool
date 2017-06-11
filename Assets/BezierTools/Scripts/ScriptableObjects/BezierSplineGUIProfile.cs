using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewBezierSplineGUIProfile",menuName ="BezierSpline/GUIProfile")]
public class BezierSplineGUIProfile : ScriptableObject {
    [Header("Colors")]
    public Color splineColor = Color.green;
    public Color mainPointColor = Color.cyan;
    public Color controlPointColor = Color.yellow;
    public Color tangentColor = Color.yellow;

    [Header("Skin")]
    public Texture buttonTexture_Spline_on;
    public Texture buttonTexture_Spline_off;

    public Texture buttonTexture_Tangent_on;
    public Texture buttonTexture_Tangent_off;

    public Texture buttonTexture_MainPointAxis_on;
    public Texture buttonTexture_MainPointAxis_off;


    public Texture buttonTexture_CtrlPointAxis_on;
    public Texture buttonTexture_CtrlPointAxis_off;

    public Texture buttonTexture_Smooth_on;
    public Texture buttonTexture_Smooth_off;

    public Texture buttonTexture_NewNode;

    public Texture buttonBackGround_on;
    public Texture buttonBackGround_off;
    public Texture buttonBackGround_pressed;
    public Texture buttonBackGround_hover;
}
