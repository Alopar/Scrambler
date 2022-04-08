using UnityEngine;

namespace Scrambler
{
    [CreateAssetMenu(fileName = "NewLevelSettings", menuName = "LevelSettings", order = 20)]
    public class LevelSettings : ScriptableObject
    {
        #region FIELDS INSPECTOR
        [Tooltip("���������� �������� �� ������")]
        [SerializeField, Range(3, 7)] private int _numberColumns = 3;
        [Tooltip("���������� ����� �� ������")]
        [SerializeField, Range(3, 10)] private int _numberRows = 3;
        #endregion

        #region PROPERTIES
        public int NumberColumns => _numberColumns;
        public int NumberRows => _numberRows;
        #endregion
    }
}
