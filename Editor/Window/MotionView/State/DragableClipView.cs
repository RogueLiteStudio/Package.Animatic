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
        private CursorRect leftHandle = new CursorRect();
        private CursorRect rightHandle = new CursorRect();

        private ClipDragType dragType;
        private float dragOffset;
        public System.Action<ClipDragType, float> OnClipDragOffset;

        public DragableClipView()
        {
            leftHandle.name = "ClipLeftHandle";
            leftHandle.style.position = Position.Absolute;
            leftHandle.style.left = 0;
            leftHandle.style.width = 3;
            leftHandle.style.top = 0;
            leftHandle.style.bottom = 0;
            leftHandle.RegisterCallback<MouseDownEvent>(OnClickLeftHandle);
            Add(leftHandle);

            rightHandle.name = "ClipRightHandle";
            rightHandle.style.position = Position.Absolute;
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

        public void OnDragApply(float frameWidth)
        {
            dragOffset %= frameWidth;
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
                    dragOffset = 0;
                    break;
                case DragStateType.Move:
                    dragOffset += drag.Delta.x;
                    OnClipDragOffset?.Invoke(dragType, dragOffset);
                    break;
                case DragStateType.End:
                    dragOffset = 0;
                    dragType = ClipDragType.DragClip;
                    break;
            }
        }
    }
}
