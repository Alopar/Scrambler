using UnityEngine;

namespace Scrambler
{
    [CreateAssetMenu(fileName = "NewLevelSettings", menuName = "LevelSettings", order = 20)]
    public class LevelSettings : ScriptableObject
    {
        [Tooltip("Количество столбцов на уровне")]
        [SerializeField, Range(3, 7)] private int _numberColumns = 3;
        [Tooltip("Количество строк на уровне")]
        [SerializeField, Range(3, 10)] private int _numberRows = 3;

        public int NumberColumns => _numberColumns;
        public int NumberRows => _numberRows;
    }
}
