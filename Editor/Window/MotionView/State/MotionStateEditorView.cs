using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class MotionStateEditorView : MotionEditorView
    {
        private readonly TextField nameField = new TextField("Name");
        private readonly ObjectField clipSelectField = new ObjectField("AnimationClip");
        private readonly Label clipInfoLabel = new Label();
        private readonly PlayButtonList playButtonList = new PlayButtonList();
        private readonly Toggle previewOriginal = new Toggle("预览原始动画");
        private readonly ScrollView clipScroll = new ScrollView(ScrollViewMode.Vertical);
        private readonly ClipGroupView groupView = new ClipGroupView();
        private readonly MotionStateClipView clipView = new MotionStateClipView();
        private readonly ScaleableClipView scaleableClipView = new ScaleableClipView();
        private readonly ListView clipListView = new ListView();

        private readonly IVisualElementScheduledItem playingTimer;
        private AnimaticMotionState motionState;
        private int selectClipIndex = -1;
        private bool isPlaying;
        private double playTime;
        private int scaleableFrameLength;
        private bool isPreviewOriginal;
        public MotionStateEditorView()
        {
            playingTimer = schedule.Execute(OnPlayingTimer).Every(10);
            playingTimer.Pause();

            style.flexDirection = FlexDirection.Column;
            nameField.RegisterValueChangedCallback(OnNameChange);
            Add(nameField);

            clipSelectField.objectType = typeof(AnimationClip);
            clipSelectField.RegisterValueChangedCallback(OnClipSelect);
            Add(clipSelectField);

            Add(clipInfoLabel);
            playButtonList.OnPlayEvent += OnPlayEvent;
            Add(playButtonList);
            previewOriginal.RegisterValueChangedCallback((evt) =>
            {
                isPreviewOriginal = evt.newValue;
                OnFrameLocation(groupView.SelectFrame);
            });
            playButtonList.Add(previewOriginal);
            clipScroll.style.left = 0;
            clipScroll.style.right = 0;
            Add(clipScroll);
            clipView.OnDragClipFrameOffset = OnDragClipFrameOffset;
            groupView.AddClipElement(clipView);
            groupView.OnFrameLocation += OnFrameLocation;

            scaleableClipView.OnClipClick = (idx)=>
            {
                selectClipIndex = idx;
                clipListView.selectedIndex = idx;
                UpdateClipInfo();
            };
            groupView.AddClipElement(scaleableClipView);
            clipScroll.Add(groupView);

            clipListView.makeItem = () => new ScaleableClipEditorView();
            clipListView.bindItem = (element, index) =>
            {
                var view = element as ScaleableClipEditorView;
                view.OnValueChange = (idx, clip) =>
                {
                    RegistUndo("change scaleable clip");
                    motionState.Clips[idx] = clip;
                    UpdateClipInfo();
                };
                view.Refresh(motionState.Clips[index], index, motionState.GetAnimationFrameCount());
            };
            clipListView.itemsAdded += (list) =>
            {
                RegistUndo("add scaleable clip");
                int frameLength = motionState.GetAnimationFrameCount();
                int select = Mathf.Clamp(groupView.SelectFrame, 0, frameLength - 1);
                int count = frameLength - select;
                motionState.Clips = motionState.Clips.Append(new ScaleableClip { StartFrame = select,  Speed = 1, FrameCount = count }).ToArray();
                UpdateClipInfo();
            };
            clipListView.itemsRemoved += (list) =>
            {
                RegistUndo("remove scaleable clip");
                List<ScaleableClip> clips = new List<ScaleableClip>();
                for (int i = 0; i < motionState.Clips.Length; ++i)
                {
                    if (!list.Contains(i))
                    {
                        clips.Add(motionState.Clips[i]);
                    }
                }
                motionState.Clips = clips.ToArray();
                selectClipIndex = 0;
                UpdateClipInfo();
                clipListView.Rebuild();
            };
            clipListView.itemIndexChanged += (from, to) =>
            {
                RegistUndo("reorder scaleable clip");
                (motionState.Clips[to], motionState.Clips[from]) = (motionState.Clips[from], motionState.Clips[to]);
                UpdateClipInfo();
            };
            clipListView.selectedIndicesChanged += (list) =>
            {
                selectClipIndex = list.FirstOrDefault();
                UpdateClipInfo();
            };
            clipListView.reorderable = true;
            clipListView.showAddRemoveFooter = true;
            clipListView.showFoldoutHeader = true;
            clipListView.reorderMode = ListViewReorderMode.Animated;
            clipListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            clipListView.headerTitle = "片段列表";
            Add(clipListView);
        }

        protected override void OnDeActive()
        {
            playingTimer.Pause();
            isPlaying = false;
            playButtonList.SetPlayState(isPlaying);
        }


        private void OnPlayingTimer(TimerState timerState)
        {
            if (motionState == null || !motionState.Animation)
            {
                playingTimer.Pause();
                return;
            }
            playTime += (timerState.deltaTime*0.001);
            float frameTime = 1.0f / motionState.Animation.frameRate;
            int frame = (int)(playTime / frameTime);
            if (frame == 0)
                return;
            playTime %= frameTime;
            frame += groupView.SelectFrame;
            SetFrameLocation(frame, true);
        }

        private void SetFrameLocation(int frame, bool loopable = false)
        {
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
        }

        private void OnPlayEvent(PlayButtonList.PlayEventType type)
        {
            switch(type)
            {
                case PlayButtonList.PlayEventType.FirstKey:
                    SetFrameLocation(0);
                    break;
                case PlayButtonList.PlayEventType.LastKey:
                    SetFrameLocation(isPreviewOriginal ? motionState.GetAnimationFrameCount(): scaleableFrameLength);
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

        private void OnFrameLocation(int frameIndex)
        {

        }

        protected override void OnUpdateView(AnimaticMotion motion)
        {
            var state = motion as AnimaticMotionState;
            motionState = state;
            viewDataKey = state.GUID;
            nameField.SetValueWithoutNotify(state.Name);
            UpdateClipInfo();
        }

        private void UpdateClipInfo()
        {
            var clip = motionState.Animation;
            float length = motionState.GetLength();
            float frameRate = clip ? clip.frameRate : 30;
            if (clip)
            {
                clipInfoLabel.text = $"动画信息：时长 = {clip.length}, 帧率 = {clip.frameRate}， 帧数 = {motionState.GetAnimationFrameCount()}, {(clip.isHumanMotion ? "人形" : "非人形")}";
                length = Mathf.Max(length, clip.length);
            }
            else
            {
                clipInfoLabel.text = "动画信息：";
            }
            previewOriginal.SetValueWithoutNotify(isPreviewOriginal);
            scaleableFrameLength = Mathf.RoundToInt(length * frameRate);
            clipSelectField.SetValueWithoutNotify(clip);
            groupView.SetFrameInfo(scaleableFrameLength, frameRate);
            clipView.UpdateSelectClipIndex(motionState, selectClipIndex);
            scaleableClipView.UpdateClips(motionState, selectClipIndex);
            clipListView.itemsSource = motionState.Clips;
        }

        private void OnDragClipFrameOffset(ClipDragType type, int offset)
        {
            if (motionState.Clips != null && selectClipIndex >= 0 && selectClipIndex < motionState.Clips.Length)
            {
                var animation = motionState.Animation;
                if (animation == null)
                    return;
                int frameCount = Mathf.RoundToInt(animation.length * animation.frameRate);
                var clip = motionState.Clips[selectClipIndex];
                if (type == ClipDragType.DragClip)
                {
                    int start = Mathf.Clamp(clip.StartFrame + offset, 0, frameCount - clip.FrameCount);
                    if (clip.StartFrame == start)
                        return;
                    clip.StartFrame = start;
                }
                else if (type == ClipDragType.DragLeftHandle)
                {
                    int start = Mathf.Clamp(clip.StartFrame + offset, 0, frameCount - clip.FrameCount);
                    if (clip.StartFrame == start)
                        return;
                    int count = Mathf.Clamp(clip.FrameCount - offset, 1, frameCount - clip.StartFrame);
                    if (clip.FrameCount == count)
                        return;
                    clip.StartFrame = start;
                    clip.FrameCount = count;
                }
                else if (type == ClipDragType.DragRightHandle)
                {
                    int count = Mathf.Clamp(clip.FrameCount + offset, 1, frameCount);
                    if (clip.FrameCount == count)
                        return;
                    clip.FrameCount = count;
                }
                RegistUndo("move scaleable clip");
                motionState.Clips[selectClipIndex] = clip;
                UpdateClipInfo();
            }
        }

        private void OnClipSelect(ChangeEvent<Object> evt)
        {
            if (evt.newValue is AnimationClip clip)
            {
                RegistUndo("change clip");
                motionState.Animation = clip;
                if (string.IsNullOrEmpty(motionState.Name) || motionState.Name.StartsWith("New ", System.StringComparison.OrdinalIgnoreCase))
                {
                    motionState.Name = clip.name;
                }
                OnUpdateView(motionState);
            }
        }

        private void OnNameChange(ChangeEvent<string> evt)
        {
            RegistUndo("change name");
            motionState.Name = evt.newValue;
        }

    }
}
