using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class BezierSpline : MonoBehaviour {
    #region Variables
    public const int PointsPerSlice = 4;

    public List<BezierNode> nodes = new List<BezierNode>();
    public bool relativePosition = false;

    [Header("Gizmos")]
    public BezierSplineGUIProfile GUIProfile;
    public int gizmosResolution = 32;
    public float pointRadius = .1f;
    public bool drawSpline = true;
    public bool drawTangent = true;
    public bool drawMainHandles = true;
    public bool drawControlHandles = false;
    public bool smoothEditing = false;
    #endregion

    #region Functions
    private void Awake()
    {
        nodes = new List<BezierNode>();
        nodes.Add(new BezierNode(Vector3.zero,Vector3.back,Vector3.forward));
    }

    public Vector3 Evaluate(float t)
    {
        if(nodes.Count > 1)
        {
            int sliceIndex;
            float localT = GlobalT2LocalT(t, out sliceIndex);
            return EvaluateSlice(sliceIndex, localT);
        }
        else
        {
            Debug.LogError(this.name + "dosen't contain enough nodes!");
            return Vector3.zero;
        }
    }

    public Vector3 EvaluateSlice(int index,float t)
    {
        if(index >= nodes.Count)
        {
            Debug.LogError(this.name + "dosen't contain enough nodes!");
        }
        Vector3 result;
        if (index + 1 < nodes.Count)
        {
            result = EvaluateFromPoints(nodes[index].mainPoint, nodes[index].nextControlPoint, nodes[index + 1].previousControlPoint, nodes[index + 1].mainPoint, t);
        }
        else
        {
            result = nodes[index].mainPoint;
        }
        return result + GetPosFix();
    }

    public float GlobalT2LocalT(float globalT,out int LocalIndex)
    {
        if(nodes.Count > 1)
        {
            float step = 1 / ((float)nodes.Count-1);
            LocalIndex = (int) (globalT / step);
            if(LocalIndex > nodes.Count - 1)
            {
                LocalIndex = nodes.Count - 1;
                return 1;
            }
            return (globalT % step) / step;
        }
        else
        {
            Debug.LogError(this.name + "dosen't contain enough nodes!");
            LocalIndex = 0;
            return 0;
        }
    }

    public Vector3 GetPosFix()
    {
        if(relativePosition)
        {
            return this.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void AddNode(Vector3 mainPos,Vector3 ctrlP, Vector3 ctrlN)
    {
        nodes.Add(new BezierNode(mainPos, ctrlP, ctrlN));
    }

    public void AddNode()
    {
        //Undo
        Vector3 direction = (nodes[nodes.Count - 1].nextControlPoint - nodes[nodes.Count - 1].mainPoint).normalized;
        Vector3 defaultNewPos = nodes[nodes.Count - 1].mainPoint + direction * 1f;
        
        nodes.Add(new BezierNode(defaultNewPos, defaultNewPos - direction * 0.1f, defaultNewPos + direction * 0.1f));
    }

    public float GetTByDistance(float startT,float dist, float scanStep, int maxScanTimes = 100000)
    {
        Vector3 startPos = Evaluate(startT);
        float currentT = startT;
        Vector3 currentPos = Evaluate(currentT);
        Vector3 previousPos = currentPos;

        float totalDistance = 0;
        int currentStep = 0;
        while(totalDistance < dist && currentStep < maxScanTimes)
        {
            currentStep++;
            
            currentT += scanStep;

            previousPos = currentPos;
            currentPos = Evaluate(currentT);
           
            totalDistance += (currentPos - previousPos).magnitude;
        }
        return currentT;
    }

    public List<float> GetTsByDistance(float dist, float scanStep, int maxScanTimes = 100000)
    {
        List<float> result = new List<float>();
        float currentT = 0;

        int currentStep = 0;
        while (currentT < 1 && currentStep < maxScanTimes)
        {
            currentStep++;
            result.Add(currentT);
            currentT = GetTByDistance(currentT,dist,scanStep);
        }
        result.Add(1);
        return result;
    }

    public float GetTotalDistance(float scanStep,int maxScanTimes = 100000)
    {
        float result = 0;

        float currentT = 0;
        Vector3 currentPos = Vector3.zero;
        Vector3 lastPos = Evaluate(0);

        int currentStep = 0;
        while (currentT < 1 && currentStep < maxScanTimes)
        {
            currentStep++;

            lastPos = currentPos;

            currentT += scanStep;
            currentPos = Evaluate(currentT);
            result += (currentPos - lastPos).magnitude;
        }
        result += (Evaluate(1) - lastPos).magnitude;
        return result;
    }
    #endregion

    #region StaticFunctions
    public static Vector3 EvaluateFromPoints(Vector3[] Positions, float t)
    {
        Vector3 result;
        if (Positions.Length == 2)
        {
            result = Vector3.Lerp(Positions[0], Positions[1], t);
        }
        else
        {
            Vector3[] leftGroup = new Vector3[Positions.Length - 1];
            Array.Copy(Positions, 0, leftGroup, 0, Positions.Length - 1);
            Vector3[] rightGroup = new Vector3[Positions.Length - 1];
            Array.Copy(Positions, 1, rightGroup, 0, Positions.Length - 1);

            result = Vector3.Lerp(EvaluateFromPoints(leftGroup, t), EvaluateFromPoints(rightGroup, t), t);
        }

        return result;
    }

    public static Vector3 EvaluateFromPoints(Vector3 start, Vector3 ctrl1, Vector3 ctrl2, Vector3 end, float t)
    {
        Vector3[] points =
        {
            start,ctrl1,ctrl2,end
        };
        return EvaluateFromPoints(points, t);
    }
    #endregion

    #region CustomClasses
    [Serializable]
    public class BezierNode
    {
        public Vector3 mainPoint;
        public Vector3 previousControlPoint;
        public Vector3 nextControlPoint;

        public BezierNode(Vector3 mainPoint,Vector3 previousControlPoint, Vector3 nextControlPoint)
        {
            this.mainPoint = mainPoint;
            this.previousControlPoint = previousControlPoint;
            this.nextControlPoint = nextControlPoint;
        }

        public BezierNode()
        {
            this.mainPoint = Vector3.zero;
            this.previousControlPoint = Vector3.zero;
            this.nextControlPoint = Vector3.zero;
        }
    }
    #endregion

    #region Gizmos
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //Draw spline
        if(drawSpline)
        {
            Gizmos.color = GUIProfile.splineColor;
            if (nodes.Count > 1 && gizmosResolution > 0)
            {
                float step = 1 / (float)gizmosResolution;
                for (int i = 0; i < gizmosResolution; i++)
                {
                    Gizmos.DrawLine(Evaluate(i * step), Evaluate((i + 1) * step));
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        
        foreach(BezierNode current in nodes)
        {
            //Draw Main Points
            Gizmos.color = GUIProfile.mainPointColor;
            Gizmos.DrawWireCube(current.mainPoint + GetPosFix(), pointRadius * Vector3.one);
            //Draw ctrl Points
            Gizmos.color = GUIProfile.controlPointColor;
            Gizmos.DrawWireCube(current.previousControlPoint + GetPosFix(), pointRadius * Vector3.one);
            Gizmos.DrawWireCube(current.nextControlPoint + GetPosFix(), pointRadius * Vector3.one);
            //Draw ctrl lines
            if(drawTangent)
            {
                Gizmos.color = GUIProfile.tangentColor;
                Gizmos.DrawLine(current.mainPoint + GetPosFix(), current.previousControlPoint + GetPosFix());
                Gizmos.DrawLine(current.mainPoint + GetPosFix(), current.nextControlPoint + GetPosFix());
            }
        }

    }
    #endif
    #endregion

}



