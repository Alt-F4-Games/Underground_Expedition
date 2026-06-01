using UnityEditor;

namespace Network.Quests.Definitions
{
    [CustomEditor(typeof(QuestDefinitionSO))]
    public class QuestDefinitionSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawProperty("questId");
            DrawProperty("questName");
            DrawProperty("description");

            EditorGUILayout.Space();

            DrawProperty("questType");

            EditorGUILayout.Space();

            DrawProperty("requirementType");
            DrawProperty("requiredQuestId");

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Quest Rules\n\n" +
                "Main Quest:\n" +
                "- Accept: Shared\n" +
                "- Progress: Shared\n" +
                "- Completion: Shared\n" +
                "- Rewards: Individual\n\n" +
                "Secondary Quest:\n" +
                "- Accept: Individual\n" +
                "- Progress: Individual\n" +
                "- Completion: Individual\n" +
                "- Rewards: Individual",
                MessageType.Info);

            EditorGUILayout.Space();

            DrawProperty("objectives");
            DrawProperty("rewards");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperty(string propertyName)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(propertyName),
                true);
        }
    }
}