using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ViewStackManager
{
    public static class ViewManager
    {
        private static List<ScreenView> _views;
        private static List<Popup> _popups;
        private static Stack<ScreenView> _viewStack;

        public static void InitialiseManager()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitialiseViews();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitialiseViews();
        }

        public static void InitialiseViews()
        {
            if (_views == null) _views = new List<ScreenView>();
            if (_viewStack == null) _viewStack = new Stack<ScreenView>();
            if (_popups == null) _popups = new List<Popup>();

            _views.RemoveAll(x => x == null);
            _popups.RemoveAll(x => x == null);

            var views = Object.FindObjectsOfType<ScreenView>();
            foreach (var view in views)
            {
                if (_views.Contains(view)) continue;
                _views.Add(view);
                view.Initialise();
            }

            var popups = Object.FindObjectsOfType<Popup>();
            foreach (var popup in popups)
            {
                if (_popups.Contains(popup)) continue;
                _popups.Add(popup);
                popup.Initialise();
            }

            CleanViewStack();
        }

        /// <summary>
        /// Open a view and add it to the stack, with data
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void OpenView<T>(params object[] data) where T : ScreenView
        {
            OpenView<T>();
            var foundView = AccessView<T>();

            if (foundView == null)
                foundView = FindViewAndAddToList<T>();

            if (foundView == null) return;
            
            foundView.RefreshUI(data);
        }
        
        /// <summary>
        /// Open a view and add it to the stack.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void OpenView<T>() where T : ScreenView
        {
            var foundView = _views.FirstOrDefault(x => x.GetType() == typeof(T));

            if (foundView == null)
                foundView = FindViewAndAddToList<T>();

            if (foundView == null) return;

            foreach (var view in _viewStack)
            {
                view.Close();
            }

            foundView.Open();
            foundView.RefreshUI();
            AddViewToStack(foundView);
        }

        /// <summary>
        /// Opens a view without adding it to the view stack. Requires Manually closing afterwards.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void OverlayView<T>() where T : ScreenView
        {
            var foundView = _views.FirstOrDefault(x => x.GetType() == typeof(T));
            
            if (foundView == null)
                foundView = FindViewAndAddToList<T>();

            if (foundView == null) return;

            foundView.Open();
        }

        public static void OverlayView<T>(params object[] data) where T : ScreenView
        {
            var foundView = _views.FirstOrDefault(x => x.GetType() == typeof(T));

            if (foundView == null)
                foundView = FindViewAndAddToList<T>();

            if (foundView == null) return;
            
            foundView.Open(data);
        }

        /// <summary>
        /// Close a view and remove it from the viewstack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CloseView<T>() where T : ScreenView
        {
            var foundView = _views.FirstOrDefault(x => x.GetType() == typeof(T));

            if (foundView == null)
                foundView = FindViewAndAddToList<T>();

            if (foundView == null) return;

            if (_viewStack?.Count > 0 && _viewStack.Peek() == foundView)
                _viewStack.Pop();
            
            foundView.Close();
            OpenLastStackView();
        }

        private static T FindViewAndAddToList<T>() where T : ScreenView
        {
            var foundView = Object.FindObjectOfType<T>();

            if (!foundView)
            {
                Debug.LogWarning($"Cannot find view of {typeof(T)}");
                return null;
            }
            
            if(foundView.HasBeenInitialised)
                foundView.Initialise();
            
            AddViewToList(foundView);
            return foundView;
        }

        /// <summary>
        /// Access a view directly by its component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [CanBeNull]
        public static T AccessView<T>() where T : ScreenView
        {
            if (_views == null) _views = new List<ScreenView>();

            var foundView = _views.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundView == null)
                foundView = FindViewAndAddToList<T>();

            if (foundView == null) return null;
            
            return foundView as T;
        }
        
        /// <summary>
        /// Refresh the UI on a specific view
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RefreshUI<T>(params object[] data) where T : ScreenView
        {
            if (_views == null) _views = new List<ScreenView>();

            var foundView = _views.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundView == null)
                foundView = FindViewAndAddToList<T>();
            
            foundView.RefreshUI(data);
            
            return foundView as T;
        }

        public static void CloseAll()
        {
            _views.ForEach(x => x.Close());
            ClearStack();
        }

        private static void CleanViewStack()
        {
            List<ScreenView> viewList = new List<ScreenView>(_viewStack);
            viewList.RemoveAll(x => !_views.Contains(x));
            viewList.Reverse();
            _viewStack = new Stack<ScreenView>(viewList);
        }

        public static void ClearStack()
        {
            if (_viewStack == null) _viewStack = new Stack<ScreenView>();
            foreach (ScreenView view in _viewStack) 
            {
                view.Close();
                view.onClose?.Invoke();
            }
            _viewStack.Clear();
        }

        private static void AddViewToStack<T>(T view) where T : ScreenView
        {
            if (_viewStack == null) _viewStack = new Stack<ScreenView>();

            if (_viewStack.Count > 0)
            {
                var lastView = _viewStack.Peek();
                lastView.Close();
            }
            
            if(!_viewStack.Contains(view))
                _viewStack.Push(view);
        }

        private static void OpenLastStackView()
        {
            ClearTopNullViews();

            if (_viewStack == null || _viewStack.Count == 0) return;
            
            ScreenView t = _viewStack.Peek();
            if (t == null) return;
            t.Open();
            t.onOpen?.Invoke();
        }

        /// <summary>
        /// Closes the most top view in the viewstack.
        /// </summary>
        public static void CloseTopView()
        {
            ClearTopNullViews();
            if (_viewStack == null || _viewStack.Count == 0) return;
            ScreenView t = _viewStack.Pop();
            if (t == null) return;
            t.Close();
            t.onClose?.Invoke();
            OpenLastStackView();
        }

        private static void ClearTopNullViews()
        {
            while (_viewStack.Count > 0 && _viewStack.Peek() == null)
                _viewStack.Pop();
        }


        private static void AddViewToList(ScreenView screenViewType)
        {
            if (_views == null)
                _views = new List<ScreenView>();
            
            if(!_views.Contains(screenViewType))
                _views.Add(screenViewType);
        }

        /// <summary>
        /// Open a partview, will open up the parent view if this view is not currently active
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void OpenPartView<T>(params object[] data) where T : ScreenPart
        {
            OpenPartView<T>();
            RefreshPartViewWithData<T>(data);
        }
        
        /// <summary>
        /// Open a partview, will open up the parent view if this view is not currently active
        /// </summary>
        public static void OpenPartView<T>() where T : ScreenPart
        {
            var linkedView = _views.FirstOrDefault(x => x.GetPartView<T>() != null);
            if (linkedView == null)
            {
                Debug.Log($"Cannot find view of type {typeof(T)}");
                return;
            }

            if (!linkedView.IsOpen)
            {
                linkedView.Open();
                AddViewToStack(linkedView);
            }
            linkedView.CloseAllPartViews();
            linkedView.GetPartView<T>()?.Open();
        }

        /// <summary>
        /// Close a PartView. Will not close the main view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ClosePartView<T>() where T : ScreenPart
        {
            var linkedView = _views.FirstOrDefault(x => x.GetPartView<T>() != null);
            if (linkedView == null)
            {
                Debug.Log("Cannot find view of type {typeof(T)}");
                return;
            }
            linkedView.Close();
            linkedView.GetPartView<T>()?.Close();
        }

        /// <summary>
        /// Direct access to a partview component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [CanBeNull]
        public static T AccessPartView<T>() where T : ScreenPart
        {
            var linkedView = _views.FirstOrDefault(x => x.GetPartView<T>() != null);
            if (linkedView == null)
            {
                Debug.LogWarning($"Cannot find view of type {typeof(T)}");
                return null;
            }

            var view = linkedView.GetPartView<T>();
            return view as T;
        }

        /// <summary>
        /// Refresh a Part View UI.
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void RefreshPartViewWithData<T>(params object[] data) where T : ScreenPart
        {
            var view = AccessPartView<T>();
            
            if(view != null)
                view.RefreshUI(data);
        }

        public static void OpenView(string viewName)
        {
            if (_views == null) return;
            var foundView = _views.FirstOrDefault(x => x.GetType().ToString() == viewName);

            if (foundView == null) return;
            foundView.Open();
            foundView.onOpen?.Invoke();
            AddViewToStack(foundView);
        }

        public static void CloseView(string viewName)
        {
            if (_views == null) return;
            var foundView = _views.FirstOrDefault(x => x.GetType().ToString() == viewName);

            if (foundView == null) return;
            foundView.Close();
            foundView.onClose?.Invoke();
            
            OpenLastStackView();
        }

        public static void OpenPartView(string viewName, params object[] data)
        {
            var mainView = _views.FirstOrDefault(x => x.GetPartView(viewName));
            if (mainView == null) return;

            var foundPartView = mainView.GetPartView(viewName);
            if (foundPartView == null)
            {
                Debug.LogWarning($"Could not find partview of type {viewName}");
                return;
            }

            if (!mainView.IsOpen)
            {
                mainView.Open();
                AddViewToStack(mainView);
            }
            
            mainView.CloseAllPartViews();
            foundPartView.Open();
            foundPartView.RefreshUI(data);
        }
        public static void ClosePartView(string viewName)
        {
            var foundPartView = AccessPartView(viewName);
            if (foundPartView == null)
            {
                Debug.LogWarning($"Could not find partview of type {viewName}");
                return;
            }
            
            foundPartView.Close();
        }

        [CanBeNull]
        public static ScreenPart AccessPartView(string viewName)
        {
            if (_views == null) return null;
            var foundView = _views.FirstOrDefault(x => x.partViews.Any(p => p.GetType().ToString() == viewName));
            return foundView == null ? null : foundView.GetPartView(viewName);
        }
        
        [CanBeNull]
        private static ScreenView AccessView(string viewName)
        {
            if (_views == null) return null;
            var foundView = _views.FirstOrDefault(x => x.GetType().ToString() == viewName);
            return foundView == null ? null : foundView;
        }

        /// <summary>
        /// Opens up a popup, Popups are not linked to the viewstack and exist of their own accord.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void OpenPopup<T>(params object[] data) where T : Popup
        {
            if (_popups == null) _popups = new List<Popup>();
            var foundpopup = _popups.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundpopup == null)
                FindPopupAndAdd<T>();

            if (foundpopup == null)
                return;
            
            foundpopup.Open(data);
        }

        public static void OpenPopup(string popupName)
        {
            if (_popups == null) return;
            var foundPopup = AccessPopup(popupName);

            if (foundPopup == null) return;
            foundPopup.Open();
            foundPopup.onOpen?.Invoke();
        }

        /// <summary>
        /// Access a popup component by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AccessPopup<T>() where T : Popup
        {
            if (_popups == null) _popups = new List<Popup>();
            var foundpopup = _popups.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundpopup == null)
                foundpopup = FindPopupAndAdd<T>();

            if (foundpopup == null)
            {
                Debug.LogWarning($"Could not find popup {typeof(T)}");
                return null;
            }

            return foundpopup as T;
        }

        /// <summary>
        /// Access a popup by name
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        public static Popup AccessPopup(string popupName)
        {
            if (_popups == null) return null;
            var foundPopup = _popups.FirstOrDefault(x => x.GetType().ToString() == popupName);
            return foundPopup;
        }

        private static T FindPopupAndAdd<T>() where T : Popup
        {
            var foundPopup = Object.FindObjectOfType<T>();

            if (!foundPopup)
            {
                Debug.LogWarning($"Cannot find popup of {typeof(T)}");
                return null;
            }
            
            if(foundPopup.HasBeenInitialised)
                foundPopup.Initialise();
            
            _popups.Add(foundPopup);
            return foundPopup;
        }
        
        /// <summary>
        /// Close a Popup from view
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ClosePopup<T>() where T : Popup
        {
            if (_popups == null) _popups = new List<Popup>();
            var foundpopup = _popups.FirstOrDefault(x => x.GetType() == typeof(T));
            if (foundpopup == null)
                FindPopupAndAdd<T>();

            if (foundpopup == null)
                return;
            
            foundpopup.Close();
        }

        /// <summary>
        /// Get the top most view that is currently being shown
        /// </summary>
        /// <returns></returns>
        public static ScreenView GetTopView()
        {
            if (_viewStack == null || _viewStack.Count == 0) return null;
            var view = _viewStack.Peek();
            return view;
        }

        /// <summary>
        /// Access a view panel, it is recommended to access the view panel from the parent view itself. But this is handy when not overused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [CanBeNull]
        public static T AccessViewPanel<T>() where T : ScreenPanel
        {
            if (_views == null) return null;
            var foundView = _views.FirstOrDefault(x => x.Panels != null && x.Panels.Any(p => p.GetType().ToString() == typeof(T).ToString()));

            if (foundView == null) return null;

            return foundView.GetViewPanel<T>() as T;
        }
    }
}