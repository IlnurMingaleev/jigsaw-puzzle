using System.Collections.Generic;
using UniRx;
using Models.Fabrics;
using Tools.GameTools;
using UnityEngine;
using VContainer;

namespace Tools.UiManager
{
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [SerializeField] private Canvas _menuCanvas;
        [SerializeField] private Transform _nonActiveParent;
        private AssetBundle _assetBundle;
        private IWindowFinder _finder;

        private WindowStack _stack;
        public Canvas MenuCanvas => _menuCanvas;
        public ReadOnlyReactiveProperty<Window> LastWindow => _stack.LastWindow;
        [Inject] private readonly PrefabInject _prefabInject;       
        
        private Stack<Window> _lastWindows = new Stack<Window>();
        private ReactiveProperty<int> _lastWindowsCount = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> LastWindowsCount => _lastWindowsCount;

        public void AddCurrentWindow(Window window)
        {
            if (_lastWindows.Count > 0 && _lastWindows.Peek().Equals(window))
            {
                return;
            }
            _lastWindows.Push(window);
            _lastWindowsCount.Value = _lastWindows.Count;
        }

        public void ClearLastWindows()
        {
            _lastWindows.Clear();
            _lastWindowsCount.Value = _lastWindows.Count;
        }
        
        public Window GetLastWindow(bool removeFromStack = true)
        {
            _lastWindowsCount.Value = _lastWindows.Count - 1;
            return removeFromStack ? _lastWindows.Pop() : _lastWindows.Peek();
        }

        
        private void Awake()
        {
            Debugger.Add(this);
            Debugger.Log("Awake");
            
            _finder = new WindowFinder(_nonActiveParent, _prefabInject);
            _stack = new WindowStack(_menuCanvas.transform, _nonActiveParent);
        }
        public void ClearStack()
        {
            Debugger.Log("Clearing stack");
            WindowStack stack = _stack;
            
            stack.Clear();

            foreach (var window in _nonActiveParent.GetComponentsInChildren<Window>(true))
                if (!window.IsUndestroyable)
                    _finder.UnloadWindow(window);
            ClearLastWindows();
        }
        
        public T FindWindow<T>() where T : Window
        {
            T window = _finder.FindWindow<T>();
            return window;
        }
        public T GetWindow<T>() where T : Window
        {
            T window = _finder.GetWindow<T>();
            window.Setup(this);
            return window;
        }
        public void First(Window window) => First(window, window.Priority);
        public void First(Window window, WindowPriority priority)
        {
            Debugger.Log($"First window in stack '{window.Name}'" );
            window.Priority = priority;
            _stack.First(window);
        }
        
        public void Show(Window window) => Show(window, window.Priority);
        
        public void Show(Window window, WindowPriority priority)
        {
            string wn = window.Name;
            Debugger.Log($"Show window: '{wn}' with priority: '{priority}'");
            if (window.IsShowing)
            {
                Debugger.LogWarning($"Window '{wn}' is already showing");
                return;
            }
            window.Priority = priority;

            _stack.Add(window);
            
            var canvas = window.GetComponent<Canvas>();
            if (canvas != null)
                canvas.overrideSorting = false;
            
            window.Show();
        }
        
        public void Hide(Window window)
        {
            if (window == null) return;
            
            Debugger.Log($"Hide window '{window.Name}'");
            if (!window.IsShowing)
            {
                Debugger.LogWarning("Window '{0}' is already hidden", window.Name);
                return;
            }

            _stack.Remove(window);
            window.Hide();
        }
        public T Show<T>(WindowPriority? priority = null) where T : Window
        {
            T window = GetWindow<T>();
            Show(window, priority ?? window.Priority);
            return window;
        }
        
        public T First<T>(WindowPriority? priority = null) where T : Window
        {
            T window = GetWindow<T>();
            First(window, priority ?? window.Priority);
            return window;
        }
        
        public T Hide<T>() where T : Window
        { 
            T window = GetWindow<T>();
            Hide(window);
            return window;
        }
    }
}