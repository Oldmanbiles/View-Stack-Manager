using UnityEngine;

namespace ViewStackManager
{
    public class ViewAccessor : MonoBehaviour
    {
        public void OpenView(ScreenView type)
        {
            string t = type.GetType().ToString();
            ViewManager.OpenView(t);
        }

        public void CloseView(ScreenView type)
        {
            string t = type.GetType().ToString();
            ViewManager.CloseView(t);
        }
        
        public void OpenPartView(ScreenPart type)
        {
            string t = type.GetType().ToString();
            ViewManager.OpenPartView(t);
        }

        public void ClosePartView(ScreenPart type)
        {
            string t = type.GetType().ToString();
            ViewManager.ClosePartView(t);
        }

        public void OpenPopup(Popup type)
        {
            string t = type.GetType().ToString();
            ViewManager.OpenPopup(t);
        }

        public void Close()
        {
            ViewManager.CloseTopView();
        }
    }
}