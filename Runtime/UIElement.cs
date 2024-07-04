using System;
using UnityEngine;
using UnityEngine.Events;

namespace ViewStackManager
{
    public class UIElement : MonoBehaviour
    {
        public UnityEvent onClose;
        public UnityEvent onOpen;
        public UnityEvent onRefresh;
        private RectTransform _root;
        private CanvasGroup _rootGroup;
        private bool _isInitialised = false;
        
        private const float _fadeInTime = 1f;
        private const float _fadeOutTime = 1f;

        public virtual void Initialise()
        {
            GetRoot();
            _root.gameObject.SetActive(false);
            _isInitialised = true;
        }
        
        public virtual void Open(params object[] data)
        {
            GetRoot();
            _root.gameObject.SetActive(true);
            onOpen?.Invoke();
            RefreshUI(data);
        }

        public virtual void FadeInView(Action onComplete = null, params object[] data)
        {
            if(_root == null || _rootGroup == null) GetRoot();

            _rootGroup.alpha = 0;
            Open();
            RefreshUI(data);

        }

        public virtual void FadeOutView(Action onComplete = null)
        {
            if(_root == null || _rootGroup == null) GetRoot();
            _rootGroup.alpha = 1;
        }

        public virtual void Close()
        {
            GetRoot();
            _root.gameObject.SetActive(false);
            onClose?.Invoke();
        }
        
        protected void GetRoot()
        {
            if (transform.childCount == 0)
            {
                Debug.LogWarning($"Element {GetType()} is empty! {GetType()}");
                return;
            }
            RectTransform tr = transform.GetChild(0).GetComponent<RectTransform>();
            if(tr == null)
                Debug.Log($"Element {GetType()} is incorrectly set up. Should be on the canvas with a single root child");

            CanvasGroup group = tr.GetComponent<CanvasGroup>();
            if (group == null)
                group = tr.gameObject.AddComponent<CanvasGroup>();

            _rootGroup = group;
            _root = tr;
        }

        public virtual void RefreshUI(params object[] data)
        {
            onRefresh?.Invoke();
        }

        public bool HasBeenInitialised => _isInitialised;
        public bool IsOpen => _root != null && _root.gameObject.activeInHierarchy;
        public RectTransform Root => _root;
        public CanvasGroup RootGroup => _rootGroup;
    }
}