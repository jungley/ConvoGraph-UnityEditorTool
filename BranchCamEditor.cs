using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using CustomVariables_BranchCam;

/* The Window that holds all the branching dialogue nodes */
[ExecuteAlways]
public class BranchCamEditor : EditorWindow
{
    private static EditorBaseNode activeNode;
    public static EditorBaseNode ActiveNode
    {
        get { return activeNode; }
        set
        {
            activeNode = value;
            if (activeNode != null)
            {
                SetHighlightTexture(activeNode.windowRect);
            }
        }
    }
    private Vector2 mousePos;
    private static Rect InspectorPanelArea;
    private static Rect ButtonPanelArea;
    public static bool startNodeAdded = false;
    static float panX = 0;
    static float panY = 0;
    private Rect lastEditorWindowPos;
    bool IsDrawingHandle = false;
    ConnectionPoint handlePoint;
    static Texture2D tex;
    static Texture2D highlightTex;
    static BranchCamEditor editor;
    private static RunTimeManager runtime_manager;

    static Texture2D arrowImage;

    private static GUIStyle panelstyle_inspector;
    private static GUIStyle panelstyle_button;
    //Panel Style for Different types of nodes
    private static GUIStyle inspectorText;
    private static bool initHasBeenCalled;

    [MenuItem("Window/ConvoGraph")]
    //BranchCamEditor Functions
    public static void init()
    {
        if (!initHasBeenCalled)
        {
            //Get Reference to RuntimeManager
            GameObject managerRef = GameObject.Find("AutoCineCamera");
            runtime_manager = managerRef.GetComponent<RunTimeManager>();
            runtime_manager.ResetEverything();
            initHasBeenCalled = true;
        }
        //Setup UI Editor
        editor = (BranchCamEditor)EditorWindow.GetWindow(typeof(BranchCamEditor), false, "ConvoGraph");
        tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, new Color(0f, 0f, 0f));
        tex.Apply();
        editor.minSize = new Vector2(400f, 400f);
        editor.autoRepaintOnSceneChange = true;
        editor.Show();
        setPanelBackgrounds();
        arrowImage = Resources.Load("arrowImage2") as Texture2D;

    }

    static void SetHighlightTexture(Rect bounds)
    {
        //Create Highlight Texture2D
        highlightTex = new Texture2D((int)bounds.width, (int)bounds.height);
        int borderwidth = 5;
        for (int y = 0; y < highlightTex.height; y++)
        {
            for (int x = 0; x < highlightTex.width; x++)
            {
                Color colResult = (x >= (highlightTex.width - borderwidth) || x <= borderwidth || y <= borderwidth || y >= (highlightTex.height - borderwidth)) ? Color.blue : Color.black;
                highlightTex.SetPixel(x, y, colResult);
            }
        }
        highlightTex.Apply();
    }

    public static void setPanelBackgrounds()
    {
        Texture2D targetTexture = new Texture2D(250, 1000);
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 250; x++)
            {
                targetTexture.SetPixel(x, y, new Color(0.3f, 0.3f, 0.3f, 1));
            }
        }
        targetTexture.Apply();
        panelstyle_inspector = new GUIStyle();
        panelstyle_inspector.normal.background = targetTexture;

        InspectorPanelArea = new Rect(0, 0, 250, 1000);


        Texture2D targetTextureButton = new Texture2D(1000, 30);
        for (int y = 0; y < 30; y++)
        {
            for (int x = 0; x < (int)editor.position.width; x++)
            {
                targetTextureButton.SetPixel(x, y, new Color(0.3f, 0.3f, 0.3f, 1));
            }
        }

        ButtonPanelArea = new Rect(0, 0, 1000, 30);

        targetTextureButton.Apply();
        panelstyle_button = new GUIStyle();
        panelstyle_button.normal.background = targetTextureButton;

        inspectorText = new GUIStyle();
        inspectorText.normal.textColor = Color.white;
        inspectorText.fontSize = 15;
    }

    void OnInspectorUpdate()
    {
        if (editor == null)
        {
            //Editor is not defined yet
            return;
        }

        //If window was resized or moved
        lastEditorWindowPos = editor.position;

        if (IsDrawingHandle)
        {
            Repaint();
        }
    }

    public bool isOverPanels(Vector2 mousepos)
    {
        return (InspectorPanelArea.Contains(mousepos) || ButtonPanelArea.Contains(mousepos));
    }

    public void isMouseOverWindow(Vector2 mousePos)
    {
        for (int i = 0; i < NodeManager.Instance.getLength(); i++)
        {
            //Get the Selected Node
            if (NodeManager.Instance.getNode(i).windowRect.Contains(mousePos))
            {
                //Fill column with node data
                ActiveNode = NodeManager.Instance.getNode(i);


                //If mouse is over a point
                if (ActiveNode.isOverPoint(mousePos))
                {
                    //Clicked on the connection point start to draw Handle
                    if (!IsDrawingHandle)
                    {
                        handlePoint = ActiveNode.getConPoint(mousePos);
                        IsDrawingHandle = true;
                        return;
                    }
                    //Already Drawing a curve, point been selected now a second one is
                    else
                    {
                        ConnectionPoint fromPoint = ActiveNode.getConPoint(mousePos);
                        //Opposite type and not of of the current node
                        if ((fromPoint.type != handlePoint.type) && !ActiveNode.containsPoint(handlePoint))
                        {
                            //Remove Connections
                            if (ConnectionManager.Instance.isOutConnected(fromPoint, handlePoint))
                            {
                                ConnectionManager.Instance.remove(fromPoint, handlePoint);
                            }
                            fromPoint.connectedTo = handlePoint;
                            handlePoint.connectedTo = fromPoint;
                            ConnectionManager.Instance.addConnection(fromPoint, handlePoint, OnClickRemoveConnection);
                        }
                    }
                }
            }
        }
        //Clicked but not over a window
        handlePoint = null;
        IsDrawingHandle = false;
        return;
    }


    void OnGUI()
    {
        //Wrap Everything In Flag
        if (initHasBeenCalled)
        {
            if (GUI.changed)
            {
                Repaint();
            }

            DrawGrid(20f, 0.5f, Color.white);
            GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));

            Event e = Event.current;
            mousePos = e.mousePosition;

            if (IsDrawingHandle)
            {
                Vector2 hpoint = handlePoint.getGlobalPoint();
                Vector3 startPos = new Vector3(hpoint.x, hpoint.y, 0);
                Vector3 endPos = new Vector3(mousePos.x, mousePos.y, 0);

                //Goto Curve
                if (ActiveNode.GetType().ToString() == "EditorGotoNode" && handlePoint.type == ConnectionPointType.Out)
                {
                    Vector3 center = new Vector3((startPos.x + endPos.x) / 2, (endPos.y + startPos.y) / 2);
                    float arc;
                    float dist = Vector3.Distance(endPos, startPos);
                    if (startPos.x <= endPos.x)
                    {
                        arc = -600.0f * Mathf.Clamp01(dist / 250.0f);
                    }
                    else
                    {
                        arc = 600.0f * Mathf.Clamp01(dist / 250.0f);
                    }
                    center.x += arc;
                    Vector3[] vector3array = new Vector3[] { startPos, center, endPos };
                    vector3array = Curver.MakeSmoothCurve(vector3array, 90.0f);
                    Handles.color = Color.green;
                    Handles.DrawAAPolyLine(5.0f, vector3array);
                }
                //Everything else (Dialogue Decision Nodes)
                else
                {
                    Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.green, null, 5);
                    Handles.color = Color.green;

                    //Calculate rotation from out point to in point 
                    float angle = Mathf.Atan2(endPos.y-startPos.y, endPos.x-startPos.x)*180 / Mathf.PI;
                    angle -= 90;
                    GUIUtility.RotateAroundPivot(angle, endPos);
                    GUI.DrawTexture(new Rect(endPos.x - 10, endPos.y, 20, 20), arrowImage, ScaleMode.StretchToFill, true, 20.0F);
                    GUIUtility.RotateAroundPivot(-angle, endPos);
                }
            }

            //Left Click Select
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                isMouseOverWindow(mousePos);
            }

            //Right click
            if (e.button == 1)
            {
                if (e.type == EventType.MouseDown)
                {
                    bool clickedOnWindow = false;
                    int selectindex = -1;

                    for (int i = 0; i < NodeManager.Instance.getLength(); i++)
                    {
                        //Get the Selected Node
                        if (NodeManager.Instance.getNode(i).windowRect.Contains(mousePos))
                        {
                            selectindex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }
                    //Open new node menu
                    if (!clickedOnWindow)
                    {
                        GenericMenu menu = new GenericMenu();
                        if (!startNodeAdded)
                        {
                            menu.AddItem(new GUIContent("Add Start Node"), false, ContextCallback, "startNode");
                        }
                        //Needs to Add an Actor
                        else if (runtime_manager.actorsInScene.Count == 0)
                        {
                            menu.AddItem(new GUIContent("Must add an actor in the Start Node"), false, ContextCallback, "blank");
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Add Dialogue Node"), false, ContextCallback, "dialogueNode");
                            menu.AddItem(new GUIContent("Add Decision Node"), false, ContextCallback, "decisionNode");
                            menu.AddItem(new GUIContent("Add GoTo Node"), false, ContextCallback, "gotoNode");
                        }
                        menu.ShowAsContext();
                        e.Use();
                    }
                }
            }

            //Draw Each Node
            Color saved = GUI.backgroundColor;
            BeginWindows();

            for (int i = 0; i < NodeManager.Instance.getLength(); i++)
            {
                EditorBaseNode nodeCur = NodeManager.Instance.getNode(i);
                //Set Background Colors
                if (nodeCur.GetType().ToString() == "EditorGotoNode")
                {
                    GUI.backgroundColor = nodeCur.nodeColor;
                }
                else
                {
                    GUI.backgroundColor = Color.gray;
                }

                if (nodeCur == ActiveNode && ActiveNode != null)
                {
                    Color tempColor = GUI.backgroundColor;
                    tempColor.a = 0.75f;
                    GUI.backgroundColor = tempColor;
                    GUI.DrawTextureWithTexCoords(ActiveNode.windowRect, highlightTex, new Rect(0, 0, 1, 1.0f));
                }

                //Drawing each node
                NodeManager.Instance.getNode(i).windowRect = GUI.Window(i, NodeManager.Instance.getNode(i).windowRect, DrawNodeWindow, NodeManager.Instance.getNode(i).windowTitle);
            }

            EndWindows();
            GUI.backgroundColor = saved;

            //Draw Connections
            ConnectionManager.Instance.DrawConnections();

            GUI.EndGroup();

            //A mousedrag is happening & not over panel
            if (Event.current.type == EventType.MouseDrag && !isOverPanels(Event.current.mousePosition))
            {
                //The EditorWindow is not being dragged
                if (lastEditorWindowPos == editor.position)
                {
                    //Weird Jumping Check
                    int scrollval = 70;
                    if ((Event.current.delta.x > -scrollval && Event.current.delta.x < scrollval)
                        && (Event.current.delta.y > -scrollval && Event.current.delta.y < scrollval))
                    {
                        panX += Event.current.delta.x;
                        panY += Event.current.delta.y;
                        Repaint();
                    }
                }
            }


            //BUTTON HEADER
            using (var horizontalScope = new GUILayout.HorizontalScope(panelstyle_button, GUILayout.Width(EditorGUIUtility.currentViewWidth), GUILayout.Height(30)))
            {
                if (GUILayout.Button("NEW", GUILayout.Width(65), GUILayout.Height(30)))
                {
                    runtime_manager.ResetEverything();
                    runtime_manager.RedrawAll();
                }

                if (GUILayout.Button("SAVE", GUILayout.Width(65), GUILayout.Height(30)))
                {
                    //Send pan coordinates
                    runtime_manager.SAVE_TO_FILE(panX, panY);
                }
                if (GUILayout.Button("LOAD", GUILayout.Width(65), GUILayout.Height(30)))
                {
                    runtime_manager.LOAD_DIALOGUE();
                }
            }


            //INSPECTOR PANEL
            using (var verticalScope = new GUILayout.VerticalScope(panelstyle_inspector, GUILayout.Width(250), GUILayout.Height(editor.position.height)))
            {

                if (ActiveNode == null)
                {
                    GUILayout.Label("Right click to add a node", inspectorText, GUILayout.Width(90));

                }
                else
                {
                    ActiveNode.DrawForInspector();
                }
            }
        }
    }


    void DrawNodeWindow(int index)
    {
        EditorBaseNode Node = NodeManager.Instance.getNode(index);

        //Button to delete
        GUILayout.BeginArea(new Rect(Node.nodeWidth - 20, 0, 20, 20));
        //Deleting Node & connections
        if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
        {
            ConnectionManager.Instance.removeAssocConnec(Node);
            NodeManager.Instance.removeNode(Node);
            ActiveNode = null;
        }
        GUILayout.EndArea();

        //Draw Content inside node
        Node.DrawContent();
        GUI.DragWindow();
    }

    //FOR ADDING NEW node
    void ContextCallback(object obj)
    {
        string clb = obj.ToString();

        switch (clb)
        {
            case ("startNode"):
                EditorBaseNode startNode = new EditorStartNode(mousePos);
                NodeManager.Instance.addNode(startNode);
                ActiveNode = startNode;
                startNodeAdded = true;
                break;
            case ("dialogueNode"):
                EditorBaseNode dialogueNode = new EditorDialogueNode(mousePos);
                NodeManager.Instance.addNode(dialogueNode);
                ActiveNode = dialogueNode;
                break;
            case ("decisionNode"):
                EditorBaseNode decisionNode = new EditorDecisionNode(mousePos);
                NodeManager.Instance.addNode(decisionNode);
                ActiveNode = decisionNode;
                break;
            case ("gotoNode"):
                EditorBaseNode gotoNode = new EditorGotoNode(mousePos);
                NodeManager.Instance.addNode(gotoNode);
                ActiveNode = gotoNode;
                //Instantiate gotonode here
                break;
        }
    }

    public static void OnClickRemoveConnection(Connection connection)
    {
        ConnectionManager.Instance.remove(connection);
    }

    //Make black background more efficiently
    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex, ScaleMode.StretchToFill);

        Vector2 offset = new Vector2(panX, panY);
        Vector2 drag = new Vector2(0, 0);

        int widthDivs = Mathf.CeilToInt((position.width + 1000) / gridSpacing);
        int heightDivs = Mathf.CeilToInt((position.height + 1000) / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();

    }
}
