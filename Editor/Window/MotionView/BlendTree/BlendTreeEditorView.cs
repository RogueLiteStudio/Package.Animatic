using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Animatic
{
    public class BlendTreeEditorView : MotionEditorView
    {
        private readonly PlayButtonList playButtonList = new PlayButtonList();
        private readonly ScrollView clipScroll = new ScrollView(ScrollViewMode.Vertical);
        private readonly ClipGroupView groupView = new ClipGroupView();
        private readonly ListView clipListView = new ListView();

        private AnimaticMotionBlendTree blendTree;

        private bool isPlaying;
        private double playTime;

        protected override AnimaticMotion Motion => blendTree;

        public BlendTreeEditorView()
        {
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
                view.UpdateView(index, blendTree.Motions[index]);
                view.OnChange = (idx, clip) =>
                {
                    RegistUndo("change blendtree motion");
                    blendTree.Motions[idx] = clip;
                    blendTree.Motions.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));
                    clipListView.itemsSource = blendTree.Motions.ToArray();
                    clipListView.Rebuild();
                };
            };
            clipListView.itemsAdded += (list) =>
            {
                RegistUndo("add blendtree motion");
                blendTree.Motions.Add(new AnimaticMotionBlendTree.Motion());
                blendTree.Motions.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));

                clipListView.itemsSource = blendTree.Motions.ToArray();
                clipListView.Rebuild();
            };
            clipListView.itemsRemoved += (list) =>
            {
                RegistUndo("remove blendtree motion");
                List<AnimaticMotionBlendTree.Motion> clips = new List<AnimaticMotionBlendTree.Motion>();
                for (int i = 0; i < blendTree.Motions.Count; ++i)
                {
                    if (!list.Contains(i))
                    {
                        clips.Add(blendTree.Motions[i]);
                    }
                }
                blendTree.Motions.Clear();
                blendTree.Motions.AddRange(clips);

                clipListView.itemsSource = blendTree.Motions.ToArray();
                clipListView.Rebuild();
            };
            clipListView.showAddRemoveFooter = true;
            clipListView.showFoldoutHeader = true;
            clipListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            clipListView.headerTitle = "动画列表";
            Add(clipListView);

        }

        protected override void OnUpdateView(AnimaticMotion motion)
        {
            blendTree = motion as AnimaticMotionBlendTree;
            clipListView.itemsSource = blendTree.Motions.ToArray();
        }

        private void OnPlayEvent(PlayButtonList.PlayEventType type)
        {
            switch (type)
            {
                case PlayButtonList.PlayEventType.FirstKey:
                    SetFrameLocation(0);
                    break;
                case PlayButtonList.PlayEventType.LastKey:
                    //SetFrameLocation(isPreviewOriginal ? motionState.GetAnimationFrameCount() : scaleableFrameLength);
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
            /*
            int maxFrame = scaleableFrameLength;
            if (isPreviewOriginal)
            {
                maxFrame = motionState.GetAnimationFrameCount();
            }
            if (frame > maxFrame)
            {
                if (loopable)
                {
                    if (groupView.SelectFrame < maxFrame)
                    {
                        frame = maxFrame;
                    }
                    else
                    {
                        frame = 0;
                    }
                }
                else
                {
                    frame = maxFrame;
                }
            }
            if (frame == 0)
            {
                playTime = 0;
            }
            groupView.SetFrameLocation(frame);
            OnFrameLocation(frame);
            */
        }
        private void OnFrameLocation(int frameIndex)
        {

        }
    }
}
