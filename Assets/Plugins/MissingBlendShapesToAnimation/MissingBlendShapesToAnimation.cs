using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.VersionControl;

namespace MissingBlendShapesToAnimation
{
    public class MissingBlendShapesToAnimation : MonoBehaviour
    {
        [Header("必須項目")]
        [Tooltip("アバターのBodyオブジェクト。")]
        [SerializeField] private Transform bodyTransform;
        [Tooltip("ブレンドシェイプが追加されるアニメーションクリップ。")]
        [SerializeField] private List<AnimationClip> animationClips = new() { null };

        [Header("追加設定")]
        [Tooltip("コピーされるアニメーションの接頭辞。（空欄の場合、もとのファイルを上書きする。）")]
        [SerializeField] private string animationPrefix = "MBSTA_";
        [Tooltip("ブレンドシェイプが追加される際のデフォルト値。")]
        [SerializeField] private int defaultValue = 0;
        //[Tooltip("アニメーションに追加されるブレンドシェイプのデフォルトの長さ。")]
        //[SerializeField] private float defaultAnimationDuration = 0;
        [Tooltip("除外するブレンドシェイプのキー（完全一致）。")]
        [SerializeField] private List<string> excludeKeysEqual = new() { "あ", "い", "う", "え", "えー", "お", "まばたき", "ウィンク", "ウィンク２" };
        [Tooltip("除外するブレンドシェイプのキー（部分一致）。")]
        [SerializeField] private List<string> excludeKeysContain = new() { "=====" };

        private SkinnedMeshRenderer bodyRenderer;

        // Context menu for easy access from the Unity Editor
        [ContextMenu("Add Missing Blend Shapes to Animation")]
        public void AddMissingBlendShapesToAnimations()
        {
            Debug.Log("Adding missing blend shapes to the animation clip.");

            ValidateOnRun();
            AddBlendShapesToAnimations();

            Debug.Log("Completed adding missing blend shapes to the animation clip.");
        }

        // Loop through all animation clips and add missing blend shapes
        public void AddBlendShapesToAnimations()
        {
            for (int i = 0; i < animationClips.Count; i++)
            {
                var animationClip = animationClips[i];
                if (animationClip == null) continue;

                // Modify the original animation clip if the prefix is empty
                if (string.IsNullOrEmpty(animationPrefix))
                {
                    AddBlendShapeToAnimation(animationClip);
                }
                // Create a new animation clip with the prefix
                else
                {
                    var newAnimationClip = Instantiate(animationClip);
                    AddBlendShapeToAnimation(newAnimationClip);

                    // save animation clip to original path with prefix
                    var path = AssetDatabase.GetAssetPath(animationClip);
                    var dir = Path.GetDirectoryName(path);
                    AssetDatabase.CreateAsset(newAnimationClip, $"{dir}/{animationPrefix}{newAnimationClip.name.Replace("(Clone)", "")}.anim");
                }

            }
        }

        // Add missing blend shapes to the animation clip
        private void AddBlendShapeToAnimation(AnimationClip animationClip)
        {
            var existingCurves = AnimationUtility.GetCurveBindings(animationClip);

            // Add missing blend shapes to the animation clip
            for (int i = 0; i < bodyRenderer.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = bodyRenderer.sharedMesh.GetBlendShapeName(i);
                string blendShapePath = $"blendShape.{blendShapeName}";

                // Skip blend shapes for LipSync
                if (blendShapeName.StartsWith("vrc."))
                {
                    // Debug.Log($"Skipping blend shape: {blendShapeName}");
                    continue;
                }
                // Skip excluded blend shapes
                if (excludeKeysEqual.Contains(blendShapeName)) continue;
                if (excludeKeysContain.Any(key => blendShapeName.Contains(key))) continue;

                // Skip if blend shape is exists
                if (existingCurves.Any(curve => curve.propertyName == blendShapePath)) continue;

                var curve = AnimationCurve.Constant(0f, animationClip.length, defaultValue);
                var binding = EditorCurveBinding.FloatCurve(bodyTransform.name, typeof(SkinnedMeshRenderer), blendShapePath);
                AnimationUtility.SetEditorCurve(animationClip, binding, curve);
                // Debug.Log($"Added missing blend shape: {blendShapeName} with value 0 to animation.");
            }
        }

        private void ValidateOnRun()
        {
            if (bodyTransform == null)
            {
                Debug.LogError("bodyTransform object not set. Please assign a bodyTransform object.");
                return;
            }

            if (animationClips.Count == 0)
            {
                Debug.LogError("Animation Clip not set. Please assign an animation clip.");
                return;
            }

            // Get the SkinnedMeshRenderer component from the Body
            if (!bodyTransform.TryGetComponent(out bodyRenderer))
            {
                Debug.LogError("No SkinnedMeshRenderer found on the Body object.");
                return;
            }

            // Get all existing blend shapes from the Body
            if (bodyRenderer.sharedMesh.blendShapeCount == 0)
            {
                Debug.Log("No blend shapes found on the Body object.");
                return;
            }
        }
    }
}