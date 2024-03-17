using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Animatic
{
    public class BlendTreeEditorView : MotionEditorView
    {
        private readonly PlayButtonList playButtonList = new PlayButtonList();
        private readonly FloatField preViewParamField = new FloatField("预览参数");
        private readonly ScrollView clipScroll = new ScrollView(ScrollViewMode.Vertical);
        private readonly ClipGroupView groupView = new ClipGroupView();
        private readonly ListView clipListView = new ListView();
        private readonly List<BlendClipView> blendClips = new List<BlendClipView>();

        private AnimaticMotionBlendTree blendTree;

        private float frameRate = 30;
        private int frameLenth = 0;

        private bool isPlaying;
        private double playTime;
        private float preViewParam;

        protected override AnimaticMotion Motion => blendTree;

        public BlendTreeEditorView()
        {
            playButtonList.OnPlayEvent = OnPlayEvent;
            Add(playButtonList);
            preViewParamField.labelElement.style.minWidth = 0;
            preViewParamField.style.minWidth = 150;
            preViewParamField.RegisterValueChangedCallback((evt) => 
            { 
                preViewParam = evt.newValue;
                OnFrameLocation(groupView.SelectFrame);
            });
            playButtonList.Add(preViewParamField);
            //轨道条区域
            clipScroll.style.left = 0;
            clipScroll.style.right = 0;
            Add(clipScroll);
            groupView.OnFrameLocation += OnFrameLocation;
            clipScroll.Add(groupView);

            //clip列表区域
            clipListView.makeItem = () => new BlendTreeMotionEditorView();
            clipListView.bindItem = (element, index) =>
            {
                var view = element as BlendTreeMotionEditorView;
                if (index >= blendTree.Motions.Count)
                    return;
                view.UpdateView(index, blendTree.Motions[index]);
                view.OnChange = (idx, clip) =>
                {
                    RegistUndo("change blendtree motion");
                    blendTree.Motions[idx] = clip;
                    blendTree.Motions.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));
                    UpdateClipView();
                    clipListView.Rebuild();
                };
            };
            clipListView.itemsAdded += (list) =>
            {
                RegistUndo("add blendtree motion");
                foreach (var idx in list)
                {
                    blendTree.Motions.Insert(idx, new AnimaticMotionBlendTree.Motion());
                }
                blendTree.Motions.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));
                UpdateClipView();
                clipListView.Rebuild();
            };
            clipListView.itemsRemoved += (list) =>
            {
                RegistUndo("remove blendtree motion");
                list = list.OrderByDescending(it => it);
                foreach (var idx in list)
                {
                    blendTree.Motions.RemoveAt(idx);
                }
                UpdateClipView();
                clipListView.Rebuild();
            };
            clipListView.showAddRemoveFooter = true;
            clipListView.showFoldoutHeader = true;
            clipListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            clipListView.headerTitle = "动画列表";
            Add(clipListView);

        }
        protected override void OnDeActive()
        {
            isPlaying = false;
            playButtonList.SetPlayState(isPlaying);
        }
        protected override void OnUpdateView(AnimaticMotion motion)
        {
            blendTree = motion as AnimaticMotionBlendTree;
            clipListView.itemsSource = blendTree.Motions.ToArray();
            UpdateClipView();
        }

        protected void UpdateClipView()
        {
            var m = blendTree.Motions.Where(it => it.Clip).OrderBy(it => it.Clip.length).FirstOrDefault();
            frameLenth = m.Clip ? (int)(m.Clip.length * frameRate) : 0;
            frameRate = m.Clip ? m.Clip.frameRate : 30;
            groupView.SetFrameInfo(frameLenth, frameRate);
            int validCount = 0;
            for (int i = 0; i < blendTree.Motions.Count; ++i)
            {
                var e = blendTree.Motions[i];
                if (m.Clip)
                {
                    validCount++;
                    var clipView = GetClipView(i);
                    clipView.style.display = DisplayStyle.Flex;
                    clipView.UpdateClip(e.Clip);
                }
            }
        }

        private BlendClipView GetClipView(int index)
        {
            if (index < blendClips.Count)
                return blendClips[index];
            BlendClipView clipView = new BlendClipView();
            clipView.style.marginTop = 2;
            blendClips.Add(clipView);
            groupView.AddClipElement(clipView);
            return clipView;
        }

        protected override void OnPlayingTimer(TimerState timerState)
        {
            if (blendTree == null || frameLenth == 0)
            {
                playingTimer.Pause();
                return;
            }
            playTime += (timerState.deltaTime * 0.001);
            float frameTime = 1.0f / frameRate;
            int frame = (int)(playTime / frameTime);
            if (frame == 0)
                return;
            playTime %= frameTime;
            frame += groupView.SelectFrame;
            SetFrameLocation(frame, true);
        }
        private void OnPlayEvent(PlayButtonList.PlayEventType type)
        {
            switch (type)
            {
                case PlayButtonList.PlayEventType.FirstKey:
                    SetFrameLocation(0);
                    break;
                case PlayButtonList.PlayEventType.LastKey:
                    SetFrameLocation(frameLenth);
                    break;
                case PlayButtonList.PlayEventType.Play:
                    playingTimer.Resume();
                    isPlaying = true;
                    playTime = 0;
                    playButtonList.SetPlayState(isPlaying);
                    break;
                case PlayButtonList.PlayEventType.Pause:
                    playingTimer.Pause();
                    isPlaying = false;
                    playTime = 0;
                    playButtonList.SetPlayState(isPlaying);
                    break;
                case PlayButtonList.PlayEventType.PreKey:
                    SetFrameLocation(groupView.SelectFrame - 1);
                    break;
                case PlayButtonList.PlayEventType.NextKey:
                    SetFrameLocation(groupView.SelectFrame + 1, true);
                    break;
            }
        }

        private void SetFrameLocation(int frame, bool loopable = false)
        {
            if (frame > frameLenth)
            {
                if (loopable)
                {
                    if (groupView.SelectFrame < frameLenth)
                    {
                        frame = frameLenth;
                    }
                    else
                    {
                        frame = 0;
                    }
                }
                else
                {
                    frame = frameLenth;
                }
            }
            if (frame == 0)
            {
                playTime = 0;
            }
            groupView.SetFrameLocation(frame);
            OnFrameLocation(frame);
        }

        private void OnFrameLocation(int frameIndex)
        {
            if (frameLenth > 0)
            {
                float time = frameIndex / frameRate;
                Simulate.Evaluate(blendTree.Name, time, preViewParam);
            }
        }

    }
}
