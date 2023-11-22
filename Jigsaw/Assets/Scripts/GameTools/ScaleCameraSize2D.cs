using System;
using Tools.UiManager;
using UnityEngine;
using VContainer;

namespace Tools.GameTools
{
    public class ScaleCameraSize2D : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _content;
        private const float DefaultWidth = 2400f;
        private const float MaxDifference = 1080f;
        private const float MaxDelta = 1f;
        
        private float _tmpDelta = 1f;
        private float _defOrthographicSize = 0;
        private float _lastDelta = 1;
        private Vector3 _contentLocalPosition;
        [Inject] private readonly IWindowManager _windowManager;
        
        private void Awake()
        {
            _contentLocalPosition = _content.localPosition;
            _defOrthographicSize = _camera.orthographicSize;
            CheckDelta();
        }

        private void CheckDelta()
        {
            const float defaultDifference = DefaultWidth / MaxDifference;
            float width = Screen.width;
            float height = Screen.height;
            
            float newDifference =  width / height;
            _tmpDelta = defaultDifference / newDifference;

            if (_tmpDelta < 1f - MaxDelta)
            {
                _tmpDelta = 1f - MaxDelta;
            }
            else if(_tmpDelta > 1f + MaxDelta)
            {
                _tmpDelta = 1f + MaxDelta;
            }

            if (Math.Abs(_tmpDelta - _lastDelta) > 0.002)
            {
                SetDif();          
            }

            _lastDelta = _tmpDelta;

            // _tmpDelta = _tmpDelta switch
            // {
            //     < 1f - MaxDelta => 1f - MaxDelta,
            //     > 1f + MaxDelta => 1f + MaxDelta,
            //     _ => defaultDifference / newDifference
            // };
        }

        private void SetDif()
        {
            var oldSize = _defOrthographicSize;
            var newSize = oldSize * _tmpDelta;
            var deltaSize = newSize - oldSize;
            
            var contentLocalPosition = _contentLocalPosition;
            contentLocalPosition.y -= deltaSize;
            
            _content.localPosition = contentLocalPosition;
            _camera.orthographicSize = newSize;
        }
    }
}