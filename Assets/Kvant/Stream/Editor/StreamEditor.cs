//
// Custom editor class for Stream.
//

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Kvant {

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
    }

    void MinMaxSlider(SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
    {
        var min = propMin.floatValue;
        var max = propMax.floatValue;

        EditorGUI.BeginChangeCheck();

        var label = new GUIContent(min.ToString("0.00") + " - " + max.ToString("0.00"));
        EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

        if (EditorGUI.EndChangeCheck()) {
            propMin.floatValue = min;
            propMax.floatValue = max;
        }
    }

    public override void OnInspectorGUI()
    {
        var targetStream = target as Stream;
        var emptyLabel = new GUIContent();

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(propMaxParticles);
        EditorGUILayout.HelpBox("Actual Number: " + targetStream.maxParticles, MessageType.None);
        if (EditorGUI.EndChangeCheck()) targetStream.NotifyConfigChange();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Emitter Position / Size / Throttle");
        EditorGUILayout.PropertyField(propEmitterPosition, emptyLabel);
        EditorGUILayout.PropertyField(propEmitterSize, emptyLabel);
        EditorGUILayout.Slider(propThrottle, 0.0f, 1.0f, emptyLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Direction / Speed / Spread");
        EditorGUILayout.PropertyField(propDirection, emptyLabel);
        MinMaxSlider(propMinSpeed, propMaxSpeed, 0.0f, 50.0f);
        EditorGUILayout.Slider(propSpread, 0.0f, 1.0f, emptyLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Noise Frequency / Speed / Animation");
        EditorGUILayout.Slider(propNoiseFrequency, 0.01f, 1.0f, emptyLabel);
        EditorGUILayout.Slider(propNoiseSpeed, 0.0f, 50.0f, emptyLabel);
        EditorGUILayout.Slider(propNoiseAnimation, 0.0f, 10.0f, emptyLabel);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propColor);
        EditorGUILayout.Slider(propTail, 0.0f, 20.0f);
        EditorGUILayout.PropertyField(propRandomSeed);
        EditorGUILayout.PropertyField(propDebug);

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Kvant
