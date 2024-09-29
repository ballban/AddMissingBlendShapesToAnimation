using UnityEngine;
using UnityEditor;
using UnityEditor.VersionControl;

namespace MissingBlendShapesToAnimation
{
    [CustomEditor(typeof(MissingBlendShapesToAnimation))]
    public class ShapeKeyToAnimationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (MissingBlendShapesToAnimation)target;
            if (GUILayout.Button("Add Missing Blend Shapes to Animation"))
            {
                script.AddMissingBlendShapesToAnimations();
            }
        }
    }
}