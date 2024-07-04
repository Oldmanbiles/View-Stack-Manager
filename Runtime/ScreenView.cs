using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace ViewStackManager
{
    public abstract class ScreenView : UIElement
    {
        [Tooltip("Use this if you are too lazy to register the part views inside of your initialisation method. Try not to use it")]
        public ScreenPart[] partViews;

        private List<ScreenPart> _linkedPartViews;
        private List<ScreenPanel> _linkedPanels;

        public override void Initialise()
        {
            if (partViews != null)
            {
                foreach (var partView in partViews)
                {
                    AddPartView(partView);
                }
            }
            RegisterCallBacks();
            base.Initialise();
        }

        protected virtual void RegisterCallBacks(){}
        protected virtual void UnregisterCallbacks(){}

        public void AddPartView<T>() where T : ScreenPart
        {
            if (_linkedPartViews == null) _linkedPartViews = new List<ScreenPart>();
            var view = GetComponentInChildren<T>(true);
            if (view == null)
            {
                Debug.Log($"Part View of type {typeof(T)} must exist as a child of {GetType()}. Will not register");
                return;
            }

            if (_linkedPartViews.Contains(view)) return;
            _linkedPartViews.Add(view);
            try
            {
                view.Initialise();
            }
            catch (Exception e)
            {
                Debug.Log($"Error founds when intialising view {view.GetType()} : {e}");
            }

        }

        [CanBeNull]
        public ScreenPart GetVisiblePartView()
        {
            if (_linkedPartViews == null) return null;
            return _linkedPartViews.First(x => x.IsOpen);
        }

        private void AddPartView(ScreenPart view)
        {
            if (_linkedPartViews == null) _linkedPartViews = new List<ScreenPart>();
            if (!_linkedPartViews.Contains(view))
            {
                view.Initialise();
                _linkedPartViews.Add(view);
            }
        }

        public T AddViewPanel<T>() where T : ScreenPanel
        {
            if (_linkedPanels == null) _linkedPanels = new List<ScreenPanel>();
            var panel = GetComponentInChildren<T>(true);
            if (panel == null)
            {
                Debug.Log($"View Panel {typeof(T)} must exist as a child of {GetType()}. Will not register.");
                return null;
            }

            if (_linkedPanels.Contains(panel)) return panel;
            _linkedPanels.Add(panel);

            try
            {
                panel.Initialise();
                return panel;
            }
            catch (Exception e)
            {
                Debug.Log($"Error founds when intialising panel {typeof(T)} : {e}");
                return null;
            }
        }

        [CanBeNull]
        public T GetViewPanel<T>() where T : ScreenPanel
        {
            if (_linkedPanels == null) _linkedPanels = new List<ScreenPanel>();
            var panel = _linkedPanels.FirstOrDefault(x => x.GetType() == typeof(T));
            if (panel == null)
            {
                Debug.Log($"Cannot find View Panel {typeof(T)}, must exist as a child of {GetType()}.");
                return null;
            }

            return panel as T;
        }

        public void OpenViewPanel<T>() where T : ScreenPanel
        {
            if (_linkedPanels == null) _linkedPanels = new List<ScreenPanel>();
            var foundViewPanels = _linkedPanels.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundViewPanels == null)
            {
                Debug.Log($"View Panel {typeof(T)} not found in view {GetType()}, Did you register it to the view correctly?");
                return;
            }
            
            foundViewPanels.Open();
        }

        public void CloseViewPanel<T>() where T : ScreenPanel
        {
            if (_linkedPanels == null) _linkedPanels = new List<ScreenPanel>();
            var foundViewPanels = _linkedPanels.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundViewPanels == null)
            {
                Debug.Log($"View Panel {typeof(T)} not found in view {GetType()}, Did you register it to the view correctly?");
                return;
            }
            
            foundViewPanels.Close();
        }

        public void RefreshAllPanels(params object[] data) => _linkedPanels?.ForEach(x => x.RefreshUI(data));
        private void OnDestroy() => UnregisterCallbacks();
        [CanBeNull] public ScreenPart GetPartView<T>() where T : ScreenPart =>_linkedPartViews?.FirstOrDefault(x => x.GetType() == typeof(T));
        [CanBeNull] public ScreenPart GetPartView(string view) => _linkedPartViews?.FirstOrDefault(x => x.GetType().ToString() == view);
        public void CloseAllPartViews() => _linkedPartViews.ForEach(x => x.Close());
        public List<ScreenPanel> Panels => _linkedPanels;
    }
}