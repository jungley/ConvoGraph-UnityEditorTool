using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CustomVariables_BranchCam;

[ExecuteAlways]
public static class LoadFile
{
    private static string path;

    public static bool isValidDirectory()
    {
        path = EditorUtility.OpenFolderPanel("Choose a folder containing Dialogue JSON files only", "Assets/AutoCine/DialogueFiles/", "Choose a folder containing Dialogue JSON files only");
        return CheckIsValidAndSet(path);
    }

      public static bool CheckIsValidAndSet(string pathname)
    {
        if(pathname == null || pathname == "")
        {
            return false;
        }
        path = pathname;
        return true;
    }

    public static FileInfo[] reorderStartFirst(FileInfo[] info)
    {
        List<FileInfo> reordered = new List<FileInfo>();
        FileInfo startfile = null;

        foreach(FileInfo f in info)
        {
            if(f.Name.Substring(0, 3) == "sta")
            {
                startfile = f;
            }
            else
            {
                reordered.Add(f);
            }
        }
        //Put startnode in first
        reordered.Insert(0, startfile);
        return reordered.ToArray();
    }

    public static List<Saveable> LoadSaveables()
    {
        List<Saveable> saveablelist = new List<Saveable>(); 

        //Get All JSON objects in Folder
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.json");
        StreamReader reader;

        //START NODE NEEDS TO BE LOADED FIRST
        info = reorderStartFirst(info);

        //Manager and startnode share similiar info
        foreach (FileInfo f in info) 
        { 
            reader = f.OpenText();
            //Get first 3 characters
            switch(f.Name.Substring(0, 3))
            {
                case "sta":
                EditorStartNode.SaveableStartNode stanode = JsonUtility.FromJson<EditorStartNode.SaveableStartNode>(reader.ReadToEnd());
                saveablelist.Add(stanode);
                break;

                case "dia":
                EditorDialogueNode.SaveableDialogueNode dianode = JsonUtility.FromJson<EditorDialogueNode.SaveableDialogueNode>(reader.ReadToEnd());
                saveablelist.Add(dianode);
                break;

                case "dec":
                EditorDecisionNode.SaveableDecisionNode decnode = JsonUtility.FromJson<EditorDecisionNode.SaveableDecisionNode>(reader.ReadToEnd());
                saveablelist.Add(decnode);
                break;

                case "got":
                EditorGotoNode.SaveableGotoNode gotnode = JsonUtility.FromJson<EditorGotoNode.SaveableGotoNode>(reader.ReadToEnd());
                saveablelist.Add(gotnode);
                break;
            }
             reader.Close();
        }

        return saveablelist;
    }
}
