using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class TimeBarLocationManipulator : MouseManipulator
    {
        private readonly System.Action<Vector2> onLocation;
        private bool isDown;

        public TimeBarLocationManipulator(System.Action<Vector2> locationFunc)
        {
            onLocation = locationFunc;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                isDown = true;
                onLocation(evt.localMousePosition);
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDown)
            {
                onLocation(evt.localMousePosition);
            }
        }
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                isDown = false;
            }
        }
    }
}
