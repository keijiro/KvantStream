//
// Custom editor class for Stream
//

using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(Stream))]
    public class StreamEditor : Editor
    {
        SerializedProperty propMaxParticles;

        SerializedProperty propEmitterPosition;
        SerializedProperty propEmitterSize;
        SerializedProperty propThrottle;

        SerializedProperty propDirection;
        SerializedProperty propSpread;

        SerializedProperty propMinSpeed;
        SerializedProperty propMaxSpeed;

        SerializedProperty propNoiseFrequency;
        SerializedProperty propNoiseSpeed;
        SerializedProperty propNoiseAnimation;

        SerializedProperty propColor;
        SerializedProperty propTail;
        SerializedProperty propRandomSeed;
        SerializedProperty propDebug;

        GUIContent textCenter;
        GUIContent textSize;
        GUIContent textDensity;
        GUIContent textStrength;
        GUIContent textAnimation;

        void OnEnable()
        {
            propMaxParticles    = serializedObject.FindProperty("_maxParticles");

            propEmitterPosition = serializedObject.FindProperty("_emitterPosition");
            propEmitterSize     = serializedObject.FindProperty("_emitterSize");
            propThrottle        = serializedObject.FindProperty("_throttle");

            propDirection       = serializedObject.FindProperty("_direction");
            propSpread          = serializedObject.FindProperty("_spread");

            propMinSpeed        = serializedObject.FindProperty("_minSpeed");
            propMaxSpeed        = serializedObject.FindProperty("_maxSpeed");

            propNoiseFrequency  = serializedObject.FindProperty("_noiseFrequency");
            propNoiseSpeed      = serializedObject.FindProperty("_noiseSpeed");
            propNoiseAnimation  = serializedObject.FindProperty("_noiseAnimation");

            propColor           = serializedObject.FindProperty("_color");
            propTail            = serializedObject.FindProperty("_tail");
            propRandomSeed      = serializedObject.FindProperty("_randomSeed");
            propDebug           = serializedObject.FindProperty("_debug");

            textCenter    = new GUIContent("Center");
            textSize      = new GUIContent("Size");
            textDensity   = new GUIContent("Density");
            textStrength  = new GUIContent("Strength");
            textAnimation = new GUIContent("Animation");
        }

        void MinMaxSlider(SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            var label = new GUIContent("x(" + min.ToString("0.00") + " - " + max.ToString("0.00") + ")");
            EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

            if (EditorGUI.EndChangeCheck()) {
                propMin.floatValue = min;
                propMax.floatValue = max;
            }
        }

        public override void OnInspectorGUI()
        {
            var targetStream = target as Stream;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propMaxParticles);
            EditorGUILayout.HelpBox("Actual Number: " + targetStream.maxParticles, MessageType.None);
            if (EditorGUI.EndChangeCheck()) targetStream.NotifyConfigChange();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Emitter");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(propEmitterPosition, textCenter);
            EditorGUILayout.PropertyField(propEmitterSize, textSize);
            EditorGUILayout.Slider(propThrottle, 0.0f, 1.0f);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Velocity");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(propDirection);
            MinMaxSlider(propMinSpeed, propMaxSpeed, 0.0f, 50.0f);
            EditorGUILayout.Slider(propSpread, 0.0f, 1.0f);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Turbulence");
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(propNoiseFrequency, 0.01f, 1.0f, textDensity);
            EditorGUILayout.Slider(propNoiseSpeed, 0.0f, 50.0f, textStrength);
            EditorGUILayout.Slider(propNoiseAnimation, 0.0f, 10.0f, textAnimation);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(propColor);
            EditorGUILayout.Slider(propTail, 0.0f, 20.0f);
            EditorGUILayout.PropertyField(propRandomSeed);
            EditorGUILayout.PropertyField(propDebug);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
