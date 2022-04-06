using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scrambler
{
    public class CameraController : MonoBehaviour
    {
        private const float _baseResolutionRatio = 1280f / 720f;
        private const float _baseOrthographicSize = 5f;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            _camera.orthographicSize = CalculateOrthographicSize();
        }

#if UNITY_EDITOR
        private void Update()
        {
            _camera.orthographicSize = CalculateOrthographicSize();
        }
#endif
        private float CalculateOrthographicSize()
        {
            var orthographicSize = _baseOrthographicSize;
            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                orthographicSize = ((float)Screen.width / Screen.height) / _baseResolutionRatio * _baseOrthographicSize;
            }
            else
            {
                orthographicSize = ((float)Screen.height / Screen.width) / _baseResolutionRatio * _baseOrthographicSize;
            }

            return (float)System.Math.Round(orthographicSize, 1);
        }
    }
}
