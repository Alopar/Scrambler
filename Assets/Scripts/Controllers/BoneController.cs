using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

namespace Scrambler
{
    [SelectionBase]
    public class BoneController : MonoBehaviour, IPointerClickHandler
    {
        #region FIELDS INSPECTOR
        [SerializeField] private GameObject _body;
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private Material _enabledMaterial;
        [SerializeField] private Material _disabledMaterial;
        [SerializeField] private Material _selectedMaterial;

        [Space(10)]
        [SerializeField] private ParticleSystem _fireworksPrefab;
        [SerializeField] private ParticleSystem _explosionPrefab;
        #endregion

        #region FIELDS PRIVATE
        private string _char;
        private string _word;
        private bool _isActive;
        private bool _isSelect;
        private Vector3 _startPosition;
        private BoneController _lastBone;
        private List<BoneController> _column;
        private MeshRenderer _meshRenderer;
        #endregion

        #region PROPERTIES
        public string Char
        {
            get { return _char; }
            set
            {
                _char = value;
                _text.text = _char;
            }
        }
        public string Word
        {
            get { return _word; }
            set { if (_word == null) _word = value; }
        }
        public BoneController LastBone
        {
            get { return _lastBone; }
            set { if (_lastBone == null) _lastBone = value; }
        }
        public List<BoneController> Column
        {
            get { return _column; }
            set { if (_column == null) _column = value; }
        }
        public bool Active
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (value)
                {
                    _body.GetComponent<MeshRenderer>().material = _enabledMaterial;
                }
                else
                {
                    _body.GetComponent<MeshRenderer>().material = _disabledMaterial;
                }
            }
        }
        public bool Select
        {
            get { return _isSelect; }
            set
            {
                _isSelect = value;
                if (value)
                {
                    _body.GetComponent<MeshRenderer>().material = _selectedMaterial;
                }
            }
        }
        #endregion

        #region UNITY CALLBACKS
        private void Start()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();

            _startPosition = transform.position;
            transform.eulerAngles = new Vector3(0f, 180f, 0f);

            transform.DORotate(new Vector3(0f, 360f, 0f), 1f);
        }

        private void OnDestroy()
        {
            _column.Remove(this);
            if (_lastBone != null)
            {
                _lastBone.Active = true;
            }

            transform.DOKill();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isActive) return;
            if (DOTween.TweensByTarget(transform) != null) return;

            transform.DOScaleX(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo);
            transform.DOScaleY(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() => GameManager.SetBones(this));
        }
        #endregion

        #region METHODS PRIVATE
        private void CheckLast()
        {
            if (_column[_column.Count - 1] == this)
            {
                Active = true;
            }
        }
        #endregion

        #region METHODS PUBLIC
        public void SetPosition(Vector3 position)
        {
            transform.position += -Vector3.forward;
            transform.DOMove(position, 0.3f);
        }

        public void ResetPosition()
        {
            Select = false;

            if (_lastBone != null)
            {
                _lastBone.Active = false;
            }

            SetPosition(_startPosition);
        }

        public void DropBone()
        {
            _meshRenderer.material.DOColor(Color.red, 0.3f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear);
            transform.DOShakePosition(0.3f, new Vector3(0.2f, 0.2f, 0f), 50).SetEase(Ease.Linear).OnComplete(() => {
                ResetPosition();
                CheckLast();
            });
        }

        public void DestroyBone()
        {
            Instantiate(_fireworksPrefab, transform.position, transform.rotation);
            _meshRenderer.material.DOColor(new Color(0.4f, 1f, 0.28f), 0.3f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
        }

        public void BombBone()
        {
            Instantiate(_explosionPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        #endregion
    }
}