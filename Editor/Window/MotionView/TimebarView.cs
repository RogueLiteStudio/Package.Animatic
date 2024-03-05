using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class TimebarView : IMGUIContainer
    {
        private float frameWidth = AnimaticViewStyle.FrameWidth;
        private float frameRate = 30;
        private bool frameMode = true;
        public System.Action<int> OnFrameLocation;
        public bool IsFrameMode
        {
            get { return frameMode; }
            set 
            { 
                if (frameMode != value)
                {
                    frameMode = value;
                    MarkDirtyRepaint();
                }
            }
        }

        public TimebarView()
        {
            onGUIHandler = OnGUI;
            this.AddManipulator(new TimeBarLocationManipulator(OnTimeBarLocation));
        }

        public void SetFramInfo(float frameRate, float frameWidth)
        {
            this.frameRate = frameRate;
            this.frameWidth = frameWidth;
            MarkDirtyRepaint();
        }

        private void OnTimeBarLocation(Vector2 pos)
        {
            int frame = Mathf.FloorToInt(pos.x / frameWidth);
            OnFrameLocation?.Invoke(frame);
        }

        private void OnGUI()
        {
            Vector2 size = localBound.size;
            if (frameMode)
            {
                int frameLength = Mathf.FloorToInt(size.x / frameWidth);
                for (int i=0; i<frameLength; ++i)
                {
                    float x = i * frameWidth;
                    if (i%5 == 0)
                    {
                        Handles.DrawLine(new Vector2(x, size.y), new Vector2(x, 0));
                        GUIContent content = new GUIContent(i.ToString());
                        Vector2 lablesize = EditorStyles.label.CalcSize(content);
                        lablesize.x = frameWidth * 5;
                        GUI.Label(new Rect(new Vector2(x + 2, 0), lablesize), content);
                    }
                    else
                    {
                        Handles.DrawLine(new Vector2(x, size.y), new Vector2(x, size.y - 2));
                    }
                }
            }
            else
            {
                float stepWidth = frameWidth * frameRate * 0.1f;
                int steps = Mathf.FloorToInt(size.x/ stepWidth);
                for (int i = 0; i < steps; ++i)
                {
                    float x = i * stepWidth;
                    if (i % 5 == 0)
                    {
                        Handles.DrawLine(new Vector2(x, size.y), new Vector2(x, 0));
                        GUIContent content = new GUIContent(string.Format("{0:F2}", i*0.5f));
                        Vector2 lablesize = EditorStyles.label.CalcSize(content);
                        lablesize.x = frameWidth * 5;
                        GUI.Label(new Rect(new Vector2(x + 2, 0), lablesize), content);
                    }
                    else
                    {
                        Handles.DrawLine(new Vector2(x, size.y), new Vector2(x, size.y - 2));
                    }
                }
            }
        }
    }
}
