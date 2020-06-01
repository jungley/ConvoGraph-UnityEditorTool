using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CustomVariables_BranchCam;

//Constructs epic JSON Save Format
[ExecuteAlways]
public static class SaveFile
{
    public static void SaveEditor(float panX, float panY)
    {
        //Get name of scene/file
        EditorStartNode startNodeRef = (EditorStartNode)NodeManager.Instance.getStartNode();
        string name = startNodeRef.scene_name;
        if (name == "" || name == null)
        {
            name = "NewDialogueFile";
        }

        string path = "Assets/AutoCine/DialogueFiles/" + name + "/";

        //If it already exists delete it - rewriting
        if (System.IO.Directory.Exists(path))
        {
            System.IO.Directory.Delete(path, true);
            Debug.Log("Saved File Overwrite");
        }

        //Create the directory
        System.IO.Directory.CreateDirectory(path);

        //Open the Writer
        StreamWriter writer;

        //For Each Node
        string jsonpath ="";
        string json = "";
        for (int i = 0; i < NodeManager.Instance.getLength(); i++)
        {
            Saveable savenode = NodeManager.Instance.getNode(i).Saveable();
            switch (savenode.typeOfNode)
            {
                case "EditorStartNode":
                    jsonpath = path + "sta_" + i + ".json";
                    break;
                case "EditorDialogueNode":
                    jsonpath = path + "dia_" + i + ".json";
                    break;
                case "EditorDecisionNode":
                    jsonpath = path + "dec_" + i + ".json";
                    break;
                case "EditorGotoNode":
                    jsonpath = path + "got_" + i + ".json";
                    break;
                default:
                    jsonpath = null;
                    Debug.Log("this should not happen find out why");
                    break;
            }
            writer = new StreamWriter(jsonpath);
            json = JsonUtility.ToJson(savenode);
            writer.Write(json);
            writer.Close();
        }
    }
}






