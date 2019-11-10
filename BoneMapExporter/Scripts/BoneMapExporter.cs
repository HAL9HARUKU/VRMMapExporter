using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRM;

public class BoneMapExporter : EditorWindow
{
    /**
     *
     */
    [MenuItem("BoneMap/Export")]
    private static void Create()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.Log("select the correct file.");
            return;
        }

        foreach (var obj in Selection.gameObjects)
        {
            var path = AssetDatabase.GetAssetOrScenePath(obj);
            if (path.Length == 0)
            {
                Debug.Log("select the correct file.");
                continue;
            }
            var beginPosition = path.IndexOf("Assets/");
            var endPosition = path.LastIndexOf(".");
            if (beginPosition >= 0)
            {
                beginPosition += "Assets/".Length;
            }
            else
            {
                beginPosition = 0;
            }
            if (endPosition < 0)
            {
                endPosition = path.Length;
            }
            path = path.Substring(beginPosition, endPosition - beginPosition);

            ExportAvatarDescription(path, obj);
        }
    }
    /**
     *
     */
    private static void ExportAvatarDescription(string filename, GameObject selectedObject)
    {
        if (selectedObject == null)
        {
            Debug.Log("select the correct file.");
            return;
        }
        VRMHumanoidDescription component = selectedObject.GetComponent<VRMHumanoidDescription>();
        if (component == null)
        {
            Debug.Log(selectedObject.name + " has no VRMHumanoidDescription");
            return;
        }
        Dictionary<string, string> boneMap = new Dictionary<string, string>();
        foreach (var human in component.Description.human)
        {
            boneMap[human.humanBone.ToString()] = human.boneName;
        }
        var json = ToJson(boneMap);
        var path = Application.dataPath + "/" + filename + ".bmap";
        var writer = new StreamWriter(path, false);
        writer.WriteLine(json);
        writer.Flush();
        writer.Close();

        Debug.Log("<b><color=green>Export Successful.</color></b> path = " + path);
    }
    /**
     *
     */
    private static string ToJson<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("{");
        bool isFirst = true;
        foreach (var pair in dictionary)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                sb.Append(",");
            }
            sb.Append("\"").Append(pair.Key).Append("\":").Append("\"").Append(pair.Value).Append("\"");
        }
        sb.Append("}");
        return sb.ToString();
    }
}
