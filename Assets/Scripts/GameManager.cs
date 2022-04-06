using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

namespace Scrambler
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelSettings _levelSettings;

        [Space(10)]
        [SerializeField] private Transform _wordPlace;
        [SerializeField] private List<CharPlace> _charPlaces;

        [Space(10)]
        [SerializeField] private BoneController _bonePrefab;

        [Space(10)]
        [SerializeField] private float _bonePadding = 0.2f;
        [SerializeField] private Transform _bonePlace;
        [SerializeField] private BonePlace _bonePlacePrefab;

        [Space(10)]
        [SerializeField] private Color[] _backgroundColors;

        private static GameManager _instance;

        private int _score = 0;
        private List<string> _words;
        //private string _currentWord;

        private List<List<BonePlace>> _bonePlaceColumns;
        private List<List<BoneController>> _boneColumns;

        private List<CharPlace> _currentCharPlaces = new List<CharPlace>();

        private List<BoneController> _bonesInGame = new List<BoneController>();
        private Stack<BoneController> _selectedBones = new Stack<BoneController>();

        private Dictionary _dictionary;
        private GameState _gameState;

        public static event Action<GameState> OnGameStateChange;
        public static event Action<int> OnScoreChange;

        private void Awake()
        {
            _instance = this;
            _dictionary = new Dictionary(Language.Russian);

            DOTween.Init();

            if (!PlayerPrefs.HasKey("best-score"))
            {
                PlayerPrefs.SetInt("best-score", 0);
            }
        }

        private void Start()
        {
            InitializeWords(_levelSettings.NumberColumns * _levelSettings.NumberRows);
            LocateBonePlace();
            PlaceLetters(ReversWords());
            ActivateBonePlaces(7);
            DeactivateAllBones();
            ActivateLastBones();

            ChangeGameState(GameState.Play);
            OnScoreChange?.Invoke(_score);

            Camera.main.backgroundColor = _backgroundColors[UnityEngine.Random.Range(0, _backgroundColors.Length)];
        }

        private void ChangeGameState(GameState gameState)
        {
            _gameState = gameState;
            OnGameStateChange?.Invoke(_gameState);
        }

        /// <summary>
        /// Minimum 9 letters
        /// </summary>
        private void InitializeWords(int numberLetters)
        {
            if (numberLetters < 9)
            {
                print($"Error: minimum 9 letters");
                return;
            }

            _words = new List<string>();

            var currentLetters = numberLetters;
            while(currentLetters > 9)
            {
                var word = _dictionary.GetRandomWord(UnityEngine.Random.Range(3, 8));
                currentLetters -= word.Length;

                _words.Add(word);
            }

            switch (currentLetters)
            {
                case 9:
                    _words.Add(_dictionary.GetRandomWord(5));
                    _words.Add(_dictionary.GetRandomWord(4));
                    break;
                case 8:
                    _words.Add(_dictionary.GetRandomWord(5));
                    _words.Add(_dictionary.GetRandomWord(3));
                    break;
                case 7:
                    _words.Add(_dictionary.GetRandomWord(4));
                    _words.Add(_dictionary.GetRandomWord(3));
                    break;
                case 6:
                    _words.Add(_dictionary.GetRandomWord(3));
                    _words.Add(_dictionary.GetRandomWord(3));
                    break;
                case 5:
                    _words.Add(_dictionary.GetRandomWord(5));
                    break;
                case 4:
                    _words.Add(_dictionary.GetRandomWord(4));
                    break;
                case 3:
                    _words.Add(_dictionary.GetRandomWord(3));
                    break;
            }
        }

        private void PlaceLetters(string letters)
        {
            var boneColumns = _boneColumns.ToList();
            
            var placeColumns = new List<List<BonePlace>>();
            foreach (var column in _bonePlaceColumns)
            {
                placeColumns.Add(column.ToList());
            }

            foreach (var letter in letters)
            {
                var columnIndex = UnityEngine.Random.Range(0, placeColumns.Count);
                var placeColumn = placeColumns[columnIndex];
                var place = placeColumn[0];

                var bone = Instantiate(_bonePrefab, place.transform.position, place.transform.rotation);
                bone.Char = letter.ToString();                
                    
                var boneColumn = boneColumns[columnIndex];
                if(boneColumn.Count > 0)
                {
                    bone.LastBone = boneColumn[boneColumn.Count - 1];
                }
                boneColumn.Add(bone);
                bone.Column = boneColumn;
                _bonesInGame.Add(bone);

                placeColumn.Remove(place);
                if (placeColumn.Count == 0)
                {
                    placeColumns.Remove(placeColumn);
                    boneColumns.Remove(boneColumn);
                }
                place.gameObject.SetActive(false);
            }
        }

        private string ReversWords()
        {
            var reverseLetters = "";
            foreach (var word in _words)
            {
                var reversWord = new string(word.ToCharArray().Reverse().ToArray());
                reverseLetters += reversWord;
            }

            return reverseLetters;
        }

        /// <summary>
        /// Maximum 7 places
        /// </summary>
        private void ActivateBonePlaces(int numberPlace)
        {
            if (numberPlace > 7)
            {
                print($"Error: maximum 7 places");
                return;
            }

            for (var i = 0; i < numberPlace; i++)
            {
                _charPlaces[i].gameObject.SetActive(true);
                _currentCharPlaces.Add(_charPlaces[i]);
            }
        }

        private void ActivateLastBones()
        {
            foreach (var column in _boneColumns)
            {
                if (column.Count > 0)
                {
                    column[column.Count - 1].Active = true;
                }
            }
        }

        private void DeactivateAllBones()
        {
            foreach (var column in _boneColumns)
            {
                foreach (var bone in column)
                {
                    bone.Active = false;
                }
            }
        }

        private string GetLettersInGame()
        {
            var letters = "";
            foreach (var bone in _bonesInGame)
            {
                letters += bone.Char;
            }
            return letters;
        }

        private void DeleteAllBones()
        {
            foreach (var column in _boneColumns)
            {
                foreach (var bone in column)
                {
                    Destroy(bone.gameObject);
                }
                column.Clear();
            }

            _bonesInGame.Clear();
            _selectedBones.Clear();
        }

        private void ResetSelectedBones()
        {
            while(_selectedBones.Count > 0)
            {
                var bone = _selectedBones.Pop();
                bone.DropBone();
            }
        }

        private void LocateBonePlace()
        {
            _boneColumns = new List<List<BoneController>>();
            _bonePlaceColumns = new List<List<BonePlace>>();
            for (int i = 0; i < _levelSettings.NumberColumns; i++)  
            {
                _boneColumns.Add(new List<BoneController>());
                _bonePlaceColumns.Add(new List<BonePlace>());
            }

            var boneMesh = _bonePlacePrefab.GetComponentInChildren<MeshRenderer>();
            var boneWidth = boneMesh.bounds.size.x;
            var boneHeight = boneMesh.bounds.size.y;
            var boneDepth = boneMesh.bounds.size.z;

            var rowWidth = boneWidth * _levelSettings.NumberColumns + (_bonePadding * (_levelSettings.NumberColumns - 1));
            var rowHeight = boneHeight + _bonePadding;
            if(_levelSettings.NumberRows > 6)
            {
                rowHeight -= (_bonePadding / 1.5f) * (_levelSettings.NumberRows - 6);
            }

            var offsetPosition = (rowWidth / 2) - (boneWidth / 2);
            var beginRowPosition = new Vector3(_bonePlace.position.x - offsetPosition, _bonePlace.position.y, _bonePlace.position.z);

            for (int i = 0; i < _levelSettings.NumberRows; i++)
            {
                var offsetY = rowHeight * i;
                var offsetZ = boneDepth * i;
                for (int j = 0; j < _levelSettings.NumberColumns; j++)
                {
                    var offsetX = (boneWidth + _bonePadding) * j;
                    var bonePosition = new Vector3(beginRowPosition.x + offsetX, beginRowPosition.y - offsetY, beginRowPosition.z - offsetZ);
                    var bonePlace = Instantiate<BonePlace>(_bonePlacePrefab, bonePosition, _bonePlace.rotation);
                    bonePlace.transform.parent = _bonePlace;

                    _bonePlaceColumns[j].Add(bonePlace);
                }

                if(i == _levelSettings.NumberRows - 1)
                {
                    _wordPlace.position = new Vector3(_wordPlace.position.x, _wordPlace.position.y, beginRowPosition.z - offsetZ);
                }
            }
        }

        private void CheckWin()
        {
            if (_bonesInGame.Count < 3)
            {
                var bestScore = PlayerPrefs.GetInt("best-score");
                if(bestScore < _score)
                {
                    PlayerPrefs.SetInt("best-score", _score);
                }

                ChangeGameState(GameState.Win);
            }
        }

        public static void MixBones()
        {
            var lettersInGame = _instance.GetLettersInGame();
            _instance.DeleteAllBones();

            _instance.PlaceLetters(lettersInGame);

            _instance.DeactivateAllBones();
            _instance.ActivateLastBones();
        }

        public static void CheckWord()
        {
            var selectedWord = "";
            foreach (var bone in _instance._selectedBones)
            {
                selectedWord += bone.Char;
            }
            selectedWord = new string(selectedWord.ToCharArray().Reverse().ToArray());

            if (_instance._dictionary.TryWords(selectedWord))
            {
                var wordScore = 10;
                if(selectedWord.Length > 3)
                {
                    for (int i = 0; i < selectedWord.Length - 3; i++)
                    {
                        wordScore *= 2;
                    }
                }
                 _instance._score += wordScore;
                OnScoreChange?.Invoke(_instance._score);

                var selectedBones = _instance._selectedBones;
                while (selectedBones.Count > 0)
                {   
                    var bone = selectedBones.Pop();
                    _instance._bonesInGame.Remove(bone);
                    bone.DestroyBone();
                }

                _instance.CheckWin();
            }
            else
            {
                print("failure!");

                _instance.ResetSelectedBones();
            }
        }

        public static void DestroyLastRow()
        {
            //_instance.ResetSelectedBones();
            var activeBones = _instance._bonesInGame.FindAll(i => i.Active == true);
            var selectedBones = _instance._selectedBones;
            var boneInGame = _instance._bonesInGame;

            foreach (var bone in activeBones)
            {
                boneInGame.Remove(bone);
                bone.BombBone();
            }

            while(selectedBones.Count > 0)
            {
                var bone = selectedBones.Pop();
                boneInGame.Remove(bone);
                bone.BombBone();
            }

            _instance.CheckWin();
        }

        public static void ResetLastLetter()
        {
            var selectedBones = _instance._selectedBones;            
            if (selectedBones.Count > 0)
            {
                var lastBone = selectedBones.Pop();
                lastBone.ResetPosition();
                lastBone.Active = true;
            }
        }

        public static void SetBones(BoneController bone)
        {
            if (_instance._currentCharPlaces.Count == _instance._selectedBones.Count) return;

            _instance._selectedBones.Push(bone);
            var bonePosition = _instance._currentCharPlaces[_instance._selectedBones.Count - 1].transform.position;
            bonePosition += -Vector3.forward * 0.1f;
            bone.SetPosition(bonePosition);
            bone.Active = false;
            bone.Select = true;

            if(bone.LastBone != null)
            {
                bone.LastBone.Active = true;
            }
        }

        [ContextMenu("ClearBestScore")]
        public void ClearBestScore()
        {
            PlayerPrefs.SetInt("best-score", 0);
        }

        #region OBSOLETE
        //private void GetWord()
        //{
        //    _currentCharPlaces.Clear();
        //    foreach (var charPlace in _charPlaces)
        //    {
        //        charPlace.gameObject.SetActive(false);
        //    }

        //    var wordIndex = _currentWord == null ? _words.Count - 1 : _words.FindIndex(w => w == _currentWord) - 1;

        //    if(wordIndex == -1)
        //    {
        //        UiManager.ShowWinPanel();
        //    }
        //    else
        //    {
        //        _currentWord = _words[wordIndex];
        //        for (var i = 0; i < _currentWord.Length; i++)
        //        {
        //            _charPlaces[i].gameObject.SetActive(true);
        //            _currentCharPlaces.Add(_charPlaces[i]);
        //        } 

        //    }
        //}

        //private void CheckWord()
        //{
        //    var selectedWord = "";
        //    foreach (var bone in _selectedBones)
        //    {
        //        selectedWord += bone.Char;
        //    }

        //    print($"Select word: {selectedWord}");
        //    print($"Current word: {_currentWord}");

        //    if (selectedWord == _currentWord)
        //    {
        //        print("success!");

        //        foreach (var selectBone in _selectedBones)
        //        {
        //            selectBone.Column.Remove(selectBone);
        //            Destroy(selectBone.gameObject);
        //        }
        //        _selectedBones.Clear();

        //        GetWord();
        //    }
        //    else
        //    {
        //        print("failure!");

        //        foreach (var selectBone in _selectedBones)
        //        {
        //            selectBone.ResetPosition();
        //        }
        //        _selectedBones.Clear();

        //    }
        //} 

        //public static void DestroyWord()
        //{
        //    var bones = _instance._bonesInGame.FindAll(b => b.Word == _instance._currentWord);
        //    foreach (var bone in bones)
        //    {
        //        bone.Column.Remove(bone);
        //        Destroy(bone.gameObject);
        //    }
        //    _instance._selectedBones.Clear();

        //    _instance.GetWord();
        //}
        #endregion
    }

    public enum GameState
    {
        None,
        Play,
        Win,
        Lose
    }
}