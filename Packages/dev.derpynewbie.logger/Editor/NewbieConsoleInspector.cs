using UdonSharpEditor;
using UnityEditor;

namespace DerpyNewbie.Logger.Editor
{
#if UNITY_EDITOR && !UDONSHARP_COMPILER
    [CustomEditor(typeof(NewbieConsole))]
    public class NewbieConsoleInspector : UnityEditor.Editor
    {
        private SerializedProperty _consoleIn;
        private SerializedProperty _consoleOut;
        private SerializedProperty _fakeSelectable;
        private SerializedProperty _roleProvider;

        private void OnEnable()
        {
            _consoleIn = serializedObject.FindProperty("consoleIn");
            _consoleOut = serializedObject.FindProperty("consoleOut");
            _fakeSelectable = serializedObject.FindProperty("fakeSelectable");
            _roleProvider = serializedObject.FindProperty("roleProvider");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            DrawDefaultInspector();

            if (_consoleIn.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Console In must be set for command input", MessageType.Error);


            if (_consoleOut.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Console Out must be set for command output", MessageType.Error);

            if (_fakeSelectable.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Quick focus will not work since it requires fake selectable",
                    MessageType.Warning);

            if (_roleProvider.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Some commands may require RoleProvider to work", MessageType.Warning);
        }
    }
#endif
}