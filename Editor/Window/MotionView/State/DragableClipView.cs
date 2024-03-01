using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class CursorRect : ImmediateModeElement
    {
        protected override void ImmediateRepaint()
        {
            EditorGUIUtility.AddCursorRect(contentRect, MouseCursor.ResizeHorizontal);
        }
    }
    public enum ClipDragType
    {
        DragClip = 1,
        DragLeftHandle = 2,
        DragRightHandle = 3,
    }
    public class DragableClipView : TextElement
    {
        private CursorRect leftHandle;
        private CursorRect rightHandle;

        private ClipDragType dragType;

        public System.Action<ClipDragType, float> OnClipDragOffset;

        public DragableClipView()
        {
            leftHandle = new CursorRect();
            leftHandle.name = "ClipLeftHandle";
            leftHandle.style.left = 0;
            leftHandle.style.width = 3;
            leftHandle.style.top = 0;
            leftHandle.style.bottom = 0;
            leftHandle.RegisterCallback<MouseDownEvent>(OnClickLeftHandle);
            Add(leftHandle);

            rightHandle = new CursorRect();
            rightHandle.name = "ClipRightHandle";
            rightHandle.style.right = 0;
            rightHandle.style.width = 3;
            rightHandle.style.top = 0;
            rightHandle.style.bottom = 0;
            rightHandle.RegisterCallback<MouseDownEvent>(OnClickRightHandle);
            Add(rightHandle);

            this.SetBorderColor(Color.white);
            this.AddManipulator(new DragManipulator(OnDrag));
        }

        public void SetSelected(bool selected)
        {
            this.SetBorderWidth(selected ? 1 : 0);
        }

        private void OnClickRightHandle(MouseDownEvent evt)
        {
            dragType = ClipDragType.DragRightHandle;
        }
        private void OnClickLeftHandle(MouseDownEvent evt)
        {
            dragType = ClipDragType.DragLeftHandle;
        }

        private void OnDrag(DragEventData drag)
        {
            switch (drag.State)
            {
                case DragStateType.Start:
                    break;
                case DragStateType.Move:
                    OnClipDragOffset?.Invoke(dragType, drag.Delta.x);
                    break;
                case DragStateType.End:
            	    break;
            }
        }
    }
}
