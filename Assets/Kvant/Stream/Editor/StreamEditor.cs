//
// Custom editor class for Stream
//

using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(Stream)), CanEditMultipleObjects]
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
        SerializedProperty propNoiseAmplitude;
        SerializedProperty propNoiseAnimation;

        SerializedProperty propColor;
        SerializedProperty propTail;
        SerializedProperty propRandomSeed;
        SerializedProperty propDebug;

        GUIContent textCenter;
        GUIContent textSize;
        GUIContent textFrequency;
        GUIContent textAmplitude;
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
            propNoiseAmplitude  = serializedObject.FindProperty("_noiseAmplitude");
            propNoiseAnimation  = serializedObject.FindProperty("_noiseAnimation");

            propColor           = serializedObject.FindProperty("_color");
            propTail            = serializedObject.FindProperty("_tail");
            propRandomSeed      = serializedObject.FindProperty("_randomSeed");
            propDebug           = serializedObject.FindProperty("_debug");

            textCenter    = new GUIContent("Center");
            textSize      = new GUIContent("Size");
            textFrequency = new GUIContent("Frequency");
            textAmplitude = new GUIContent("Amplitude");
            textAnimation = new GUIContent("Animation");
        }

        void MinMaxSlider(string label, SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit, string format)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            var text = new GUIContent(label + " (" + min.ToString(format) + "-" + max.ToString(format) + ")");
            EditorGUILayout.MinMaxSlider(text, ref min, ref max, minLimit, maxLimit);

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
            MinMaxSlider("Speed", propMinSpeed, propMaxSpeed, 0.0f, 50.0f, "0.0");
            EditorGUILayout.Slider(propSpread, 0.0f, 1.0f);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Turbulence");
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(propNoiseFrequency, 0.01f, 1.0f, textFrequency);
            EditorGUILayout.Slider(propNoiseAmplitude, 0.0f, 50.0f, textAmplitude);
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
