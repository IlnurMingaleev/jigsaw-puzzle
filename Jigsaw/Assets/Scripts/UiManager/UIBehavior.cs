﻿using UnityEngine;

namespace Tools.UiManager
{
    public abstract class UIBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {}

        protected virtual void OnEnable()
        {}

        protected virtual void Start()
        {}

        protected virtual void OnDisable()
        {}

        protected virtual void OnDestroy()
        {}
        
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {}

        protected virtual void Reset()
        {}
#endif
        protected virtual void OnRectTransformDimensionsChange()
        {}

        protected virtual void OnBeforeTransformParentChanged()
        {}

        protected virtual void OnTransformParentChanged()
        {}

        protected virtual void OnDidApplyAnimationProperties()
        {}

        protected virtual void OnCanvasGroupChanged()
        {}
        
        protected virtual void OnCanvasHierarchyChanged()
        {}
        
        public bool IsDestroyed()
        {
            return this == null;
        }
    }
}