using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class ObjectiveEntryUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text objectiveText;

        public void SetData(
            string description,
            int current,
            int required,
            bool completed)
        {
            objectiveText.text =
                $"{description} ({current}/{required})";

            objectiveText.color =
                completed
                    ? Color.green
                    : Color.white;
        }
    }
}