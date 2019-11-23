using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRM;
using MiniJSON;

public class VRMMapExporter : EditorWindow
{
    /**
     *
     */
    [MenuItem("VRMMap/Export")]
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
        var vrmMapping = new Dictionary<string, object>();

        // BoneMap
        var humanoidDescription = selectedObject.GetComponent<VRMHumanoidDescription>();
        if (humanoidDescription == null)
        {
            Debug.Log(selectedObject.name + " has no VRMHumanoidDescription");
            return;
        }
        var boneMap = new Dictionary<string, string>();
        foreach (var human in humanoidDescription.Description.human)
        {
            boneMap[human.humanBone.ToString()] = human.boneName;
        }
        vrmMapping.Add("BoneMapping", boneMap);
        // BlendShapeMap
        var blendShape = new Dictionary<string, object>();
        // BlendShapeMap > Meshes
        var blendShapeMeshes = new List<object>();
        var skinnedMeshRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            var blendShapeMesh = new Dictionary<string, object>();
            var blendShapeMeshTargets = new List<string>();
            for (var index = 0; index < skinnedMeshRenderer.sharedMesh.blendShapeCount; ++index)
            {
                blendShapeMeshTargets.Add(skinnedMeshRenderer.sharedMesh.GetBlendShapeName(index));
            }
            blendShapeMesh.Add("Name", skinnedMeshRenderer.name);
            blendShapeMesh.Add("Targets", blendShapeMeshTargets);

            blendShapeMeshes.Add(blendShapeMesh);
        }
        blendShape.Add("Meshes", blendShapeMeshes);
        // BlendShapeMap > Clips
        var blendShapeProxy = selectedObject.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            Debug.Log(selectedObject.name + " has no VRMBlendShapeProxy");
            return;
        }
        var blendShapeClips = new List<object>();
        foreach (var clip in blendShapeProxy.BlendShapeAvatar.Clips)
        {
            var blendShapeClip = new Dictionary<string, object>();
            var blendShapeStates = new List<object>();
            var tempBlendShapeState = new Dictionary<string, List<object>>();

            foreach (var value in clip.Values)
            {
                if (!tempBlendShapeState.ContainsKey(value.RelativePath))
                {
                    tempBlendShapeState.Add(value.RelativePath, new List<object>());
                }
                tempBlendShapeState[value.RelativePath].Add(new Dictionary<string, object> { { "Index", value.Index }, { "Weight", value.Weight } });
            }
            foreach (var state in tempBlendShapeState)
            {
                var blendShapeState = new Dictionary<string, object>();
                blendShapeState.Add("Name", state.Key);
                blendShapeState.Add("Targets", state.Value);

                blendShapeStates.Add(blendShapeState);
            }

            blendShapeClip.Add("Name", clip.BlendShapeName);
            blendShapeClip.Add("States", blendShapeStates);

            blendShapeClips.Add(blendShapeClip);
        }
        blendShape.Add("Clips", blendShapeClips);
        vrmMapping.Add("BlendShape", blendShape);

        var json = Json.Serialize(vrmMapping);
        var path = Application.dataPath + "/" + filename + ".vrmmap";
        var writer = new StreamWriter(path, false);
        writer.WriteLine(json);
        writer.Flush();
        writer.Close();

        Debug.Log("<b><color=green>Export Successful.</color></b> path = " + path);
    }
}
