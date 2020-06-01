using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteAlways]
[System.Serializable]
public abstract class EditorBaseNode
{
    //windowRect contains the location of the node
    public Rect windowRect;
    public string windowTitle = "";
    public float nodeWidth;
    public float nodeHeight;
    protected GUIStyle labelStyleHead_Panel;
    protected GUIStyle labelStyleHead_Node;
   protected GUIStyle inspectorText;
    public RunTimeManager runtime_manager;

    public Color nodeColor;
    public Texture2D headerTexture;

    public ConnectionPoint PointIn;
    public List<ConnectionPoint> PointOut;

    public string node_id;

    public EditorBaseNode()
    {
        //Instantiate Styles common a across all nodes
        labelStyleHead_Panel = new GUIStyle();
        labelStyleHead_Panel.normal.textColor = Color.white;
        labelStyleHead_Panel.fontStyle = FontStyle.Bold;
        labelStyleHead_Panel.fontSize = 15;

        labelStyleHead_Node = new GUIStyle();
        labelStyleHead_Node.normal.textColor = Color.white;
        labelStyleHead_Node.fontStyle = FontStyle.Bold;
        labelStyleHead_Node.fontSize = 15;

        nodeColor = Color.gray;

        inspectorText = new GUIStyle();
        inspectorText.normal.textColor = Color.white;

        //Get Reference to RuntimeManager
        GameObject managerRef = GameObject.Find("AutoCineCamera");
        runtime_manager = managerRef.GetComponent<RunTimeManager>();

        //Add Node to RuntimeDictionary
        System.Guid guidVal = System.Guid.NewGuid();
        node_id = guidVal.ToString();
    }

    public void DrawUICamCompOptions(ConvoData nodeConvodata)
    {

        //DRAW CAMERASHOT SELECTOR -- SAME CODE IN DIALOGUE
        EditorGUILayout.LabelField("Shot Composition", labelStyleHead_Panel);
        EditorGUILayout.Space();

        //DRAW CAMERASHOT SELECTOR -- SAME CODE IN DIALOGUE
        List<string> options_Type = runtime_manager.CameraOptions_Type;
        List<string> options_Distance = runtime_manager.CameraOptions_Distance;
        List<string> options_Angle = runtime_manager.CameraOptions_Angle;

        int index_type = options_Type.IndexOf(nodeConvodata.camerashot.goal_type);
        int index_dist = options_Distance.IndexOf(nodeConvodata.camerashot.goal_dist);
        int index_angle = options_Angle.IndexOf(nodeConvodata.camerashot.goal_angle);

        //DROPDOWN FOR TYPE 
        EditorGUILayout.LabelField("Type", inspectorText, GUILayout.Width(50));
        GUILayout.BeginHorizontal("box");
        index_type = EditorGUILayout.Popup(index_type, options_Type.ToArray(), GUILayout.Width(140));
        //check if it's been updated
        if (nodeConvodata.camerashot.goal_type != options_Type[index_type])
        {
            nodeConvodata.camerashot.updateType(options_Type[index_type]);
        }

        //If it's Exterior or Apex
        if (options_Type[index_type] == "OverShoulder" || options_Type[index_type] == "FrameShare")
        {
            //Generate List Except of actor associated with node
            List<string> tmp = new List<string>();
            tmp.AddRange(runtime_manager.GetActorStringOptions());
            tmp.Remove(nodeConvodata.Actor.ActorName);
            int OppActorIndex = tmp.IndexOf(nodeConvodata.camerashot.oppositeActor);
            if (OppActorIndex == -1) { OppActorIndex = 0; }

            if (tmp.Count > 0)
            {
                OppActorIndex = EditorGUILayout.Popup(OppActorIndex, tmp.ToArray(), GUILayout.Width(70));
                nodeConvodata.camerashot.oppositeActor = tmp[OppActorIndex];
            }
        }
        GUILayout.EndHorizontal();


        //DROPDOWN FOR DISTANCE
        EditorGUILayout.LabelField("Distance", inspectorText, GUILayout.Width(50));
        GUILayout.BeginHorizontal("box");
        index_dist = EditorGUILayout.Popup(index_dist, options_Distance.ToArray(), GUILayout.Width(140));
        //check if it's been updated
        if (nodeConvodata.camerashot.goal_dist != options_Distance[index_dist])
        {
            nodeConvodata.camerashot.updateDist(options_Distance[index_dist]);
        }
        GUILayout.EndHorizontal();

        //DROPDOWN FOR ANGLE
        EditorGUILayout.LabelField("Y Angle", inspectorText, GUILayout.Width(50));
        GUILayout.BeginHorizontal("box");
        index_angle = EditorGUILayout.Popup(index_angle, options_Angle.ToArray(), GUILayout.Width(140));
        //check if it's been updated
        if (nodeConvodata.camerashot.goal_angle != options_Angle[index_angle])
        {
            nodeConvodata.camerashot.updateAngle(options_Angle[index_angle]);
        }
        GUILayout.EndHorizontal();

    }

    
    public void setHeaderTexture()
    {
        headerTexture = new Texture2D(1, 1);
        headerTexture.SetPixel(1, 1, nodeColor);
        headerTexture.Apply();
    }

    public bool containsPoint(ConnectionPoint A)
    {
        if (A == PointIn)
        {
            return true;
        }

        if (PointOut.Contains(A))
        {
            return true;
        }

        return false;
    }

    public virtual void AssociateConnections(Saveable savenode)
    {
        EditorBaseNode node = this;
        //Check out Connection
        if (savenode.OUT_connTo.Count != 0)
        {
            for (int i = 0; i < savenode.OUT_connTo.Count; i++)
            {
                EditorBaseNode node_OUT = NodeManager.Instance.findNode(savenode.OUT_connTo[i]);
                if (node_OUT != null)
                {
                    node.PointOut[i].connectedTo = node_OUT.PointIn;
                    node_OUT.PointIn.connectedTo = node.PointOut[i];
                    ConnectionManager.Instance.addConnection(node.PointOut[i], node_OUT.PointIn, BranchCamEditor.OnClickRemoveConnection);
                }
            }
        }
    }

    public virtual void DrawForInspector()
    {

    }

    public virtual bool isOverPoint(Vector2 mousePos)
    {
        return false;
    }

    public virtual ConnectionPoint getConPoint(Vector2 mousePos)
    {
        return null;
    }

    public EditorBaseNode GetNextNode()
    {
        try
        {
            return PointOut[0].connectedTo.node;
        }
        catch (Exception)
        {
            return null;
        }
    }


    public virtual void DrawContent()
    {

    }


    public virtual Saveable Saveable()
    {
        return null;
    }
}

[System.Serializable]
[ExecuteAlways]
public class Saveable
{
    public string typeOfNode;
    public string node_id;
    public Rect windowRect;
    public string goal_type;
    public string goal_dist;
    public string goal_angle;
    public List<string> OUT_connTo;
    public List<string> IN_connTo;
    public string oppositeActor;

    public virtual EditorBaseNode ConvertToUnity()
    {
        return null;
    }
}
