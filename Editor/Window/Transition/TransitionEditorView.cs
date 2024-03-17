using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace Animatic
{
    public class TransitionInfo
    {
        public AnimaticMotion Source;
        public string GUID;
        public float Length;
        public float Duration;
    }
    public class TransitionEditorView : VisualElement
    {
        public AnimaticAsset Asset;
        public AnimaticSimulate Simulate;

        private readonly ClipGroupView groupView = new ClipGroupView();
        private readonly PlayButtonList playButtonList = new PlayButtonList();
        private readonly ListView transListView = new ListView();
        private readonly TransitionClipView sourceMotionClip = new TransitionClipView();
        private readonly TransitionClipView currentMotionClip = new TransitionClipView();
        private readonly ScrollView clipScroll = new ScrollView(ScrollViewMode.Vertical);

        private readonly IVisualElementScheduledItem playingTimer;

        private List<TransitionInfo> transitionInfos = new List<TransitionInfo>();
        private AnimaticMotion currentMotion;
        private float motionLength;
        private int selectIndex;
        private int maxFrameLength;
        private bool isPlaying;
        private double playTime;

        public TransitionEditorView()
        {
            playingTimer = schedule.Execute(OnPlayingTimer).Every(10);
            playingTimer.Pause();
            
            playButtonList.OnPlayEvent = OnPlayEvent;
            Add(playButtonList);
            Add(clipScroll);
            groupView.OnFrameLocation += OnFrameLocation;
            clipScroll.Add(groupView);
            groupView.AddClipElement(sourceMotionClip);
            groupView.AddClipElement(currentMotionClip);

            transListView.headerTitle = "融合列表";
            transListView.showAddRemoveFooter = true;
            transListView.showFoldoutHeader = true;
            Add(transListView);
            transListView.makeItem += () => new TransitionItemView();
            transListView.bindItem += (element, index) =>
            {
                if (index >= transitionInfos.Count)
                    return;
                var itemView = element as TransitionItemView;
                itemView.OnDurationChanged = OnDurationChange;
                itemView.OnMotionChanged = OnMotionChange;
                itemView.UpdateView(transitionInfos[index], motionLength, index);
                itemView.GetSelectableList = GetTransitionableMotionNames;
            };
            transListView.itemsAdded += (list) =>
            {
                for (int i=0; i< transitionInfos.Count; ++i)
                {
                    var item = transitionInfos[i];
                    if (item == null)
                    {
                        item = new TransitionInfo();
                        transitionInfos[i] = item;
                    }
                }
                UpdateClipView();
                transListView.Rebuild();
            };
            transListView.itemsRemoved += (list) =>
            {
                RegistUndo("remove motion transition");
                list = list.OrderByDescending(it => it);
                foreach (var idx in list)
                {
                    var element = transitionInfos[idx];
                    transitionInfos.RemoveAt(idx);
                    if (element.GUID != null)
                    {
                        Asset.Transition.RemoveAll(it => it.SourceGUID == element.GUID && it.DestGUID == currentMotion.GUID);
                    }
                }
                selectIndex = 0;
                UpdateClipView();
                transListView.Rebuild();
            };
            transListView.selectedIndicesChanged += (list) =>
            {
                selectIndex = list.FirstOrDefault();
                UpdateClipView();
            };
        }
        private IEnumerable<string> GetTransitionableMotionNames()
        {
            return Asset.Motions.Where(it=> !transitionInfos.Exists(t=>t.Source == it) && it != currentMotion)
                .Select(it=>it.Name);
        }
        public void UpdateView(AnimaticMotion motion)
        {
            currentMotion = motion;
            motionLength = motion.GetLength();
            transitionInfos.Clear();
            transitionInfos.AddRange(Asset.Transition.Where(it=>it.DestGUID == motion.GUID)
                .Select((it) => 
                {
                    var source = Asset.FindMotion(it.SourceGUID);
                    return new TransitionInfo
                    {
                        Source = source,
                        Duration = it.Duration,
                        Length = source != null ? source.GetLength() : 0,
                        GUID = it.SourceGUID
                    };
                } ));
            transListView.itemsSource = transitionInfos;
            UpdateClipView();
        }

        private void OnMotionChange(string name, int index)
        {
            var e = transitionInfos[index];
            var newMotion = Asset.FindMotionByName(name);
            if (newMotion == null)
                return;
            var exist = Asset.Transition.FirstOrDefault(it => it.SourceGUID == e.GUID && it.DestGUID == currentMotion.GUID);
            if (exist == null)
            {
                RegistUndo("add motion transition");
                Asset.Transition.Add(new AnimaticTransition { SourceGUID = newMotion.GUID, DestGUID = currentMotion.GUID, Duration = 0 });
            }
            else
            {
                RegistUndo("change motion transition");
                exist.SourceGUID = newMotion.GUID;
            }
            e.Source = newMotion;
            e.GUID = newMotion.GUID;
            e.Length = newMotion.GetLength();
            transListView.Rebuild();
            UpdateClipView();
        }

        private void OnDurationChange(float value, int index)
        {
            var e = transitionInfos[index];
            if (e.GUID == null)
                return;
            RegistUndo("modify transition duration");
            var exist = Asset.Transition.FirstOrDefault(it => it.SourceGUID == e.GUID && it.DestGUID == currentMotion.GUID);
            exist.Duration = value;
            e.Duration = value;
            UpdateClipView();
        }

        private void OnPlayingTimer(TimerState timerState)
        {
            playTime += (timerState.deltaTime * 0.001);
            float frameTime = 1.0f / AnimaticViewStyle.FrameRate;
            int frame = (int)(playTime / frameTime);
            if (frame == 0)
                return;
            playTime %= frameTime;
            frame += groupView.SelectFrame;
            SetFrameLocation(frame, true);
        }

        private void UpdateClipView()
        {
            AnimaticMotion sourceMotion = null;
            float sourceLength = 0;
            float duration = 0;
            if (selectIndex < transitionInfos.Count)
            {
                var e = transitionInfos[selectIndex];
                if (e.Source != null)
                {
                    sourceLength = e.Length;
                    sourceMotion = e.Source;
                    duration = e.Duration;
                }
            }
            sourceMotionClip.UpdateByMotion(sourceMotion, 0);
            float startTime = Mathf.Clamp(sourceLength - duration, 0, sourceLength);
            currentMotionClip.UpdateByMotion(currentMotion, startTime);
            int frameLenth = Mathf.RoundToInt((startTime + motionLength) * AnimaticViewStyle.FrameRate);
            groupView.SetFrameInfo(frameLenth, AnimaticViewStyle.FrameRate);
            MarkDirtyRepaint();
        }

        private void OnPlayEvent(PlayButtonList.PlayEventType type)
        {
            switch (type)
            {
                case PlayButtonList.PlayEventType.FirstKey:
                    SetFrameLocation(0);
                    break;
                case PlayButtonList.PlayEventType.LastKey:
                    SetFrameLocation(maxFrameLength);
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
            do
            {
                if (selectIndex < 0 || selectIndex >= transitionInfos.Count)
                    break;
                var e = transitionInfos[selectIndex];
                if (e.Source == null)
                    break;
                float duration = Mathf.Min(e.Duration, motionLength);
                duration = Mathf.Clamp(duration, 0, e.Length);
                float maxLenth = e.Length + motionLength - duration;
                int frameCount = Mathf.RoundToInt(maxLenth * AnimaticViewStyle.FrameRate);
                if (frame > frameCount)
                {
                    if (loopable)
                        frame = 0;
                    else
                        frame = frameCount;
                }
                groupView.SetFrameLocation(frame);
                OnFrameLocation(frame);
                return;
            } while (false);
            playingTimer.Pause();
        }

        private void OnFrameLocation(int frameIndex)
        {
            if (selectIndex < 0 || selectIndex >= transitionInfos.Count)
                return;
            float time = frameIndex / AnimaticViewStyle.FrameRate;
            var tran = transitionInfos[selectIndex];
            if (tran.Source != null)
            {
                Simulate.EvaluateCrossFade(tran.Source.Name, currentMotion.Name, time, tran.Duration);
            }
            else
            {
                Simulate.Evaluate(currentMotion.Name, time);
            }
        }

        private void RegistUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(Asset, name);
            EditorUtility.SetDirty(Asset);
        }
    }
}
