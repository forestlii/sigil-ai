// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using UnityEngine;
using UnityEngine.AI;
using Likeon.GAS;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// NPC "大脑"组件（对应 UE APawn+AAIController 塌成的组件栈）：持时钟 + 作息驱动 + goal 池 + 效用选择器，
    /// 每帧推进时间、按作息发/撤 goal、选出最优 activity 并驱动 <see cref="NavMeshAgent"/> 移动。
    /// 自身实现 <see cref="IActivityScoringContext"/>（供打分）与 <see cref="INpcAgent"/>（供 activity 驱动身体）。
    /// 加到任意带 NavMeshAgent 的 GameObject 上即可，零基类继承（对齐 NarrativeComponent 风格）。
    /// M-AI-1：activity 只有 MoveToActivity；每帧重选（selector 是廉价纯函数），事件驱动优化留后。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NpcActivityComponent : MonoBehaviour, IActivityScoringContext, INpcAgent
    {
        [Header("作息")]
        [Tooltip("作息表资产（定时发/撤 goal）。")]
        [SerializeField] private NpcActivityScheduleAsset schedule;

        [Header("时间")]
        [Tooltip("一游戏日 = 多少现实秒（越小，一天过得越快，便于观察作息）。")]
        [SerializeField] private float dayLengthSeconds = 120f;
        [Tooltip("起始时刻（0–2400 制，800 = 8:00）。")]
        [SerializeField] private float startTimeOfDay = 800f;

        [Header("移动")]
        [Tooltip("到达判定：与目标点水平距离小于此值即视为到达。")]
        [SerializeField] private float arriveDistance = 0.6f;

        [Header("状态")]
        [Tooltip("NPC 当前拥有的标签（供 goal 门禁判定）。")]
        [SerializeField] private GameplayTagContainer ownerTags = new GameplayTagContainer();

        private NavMeshAgent _navAgent;
        private TimeOfDayClock _clock;
        private ScheduleGoalDriver _driver;
        private NpcGoalPool _pool;
        private ActivitySelector _selector;
        private NpcActivity _current;
        private NpcGoal _currentGoal;
        private Vector3 _destination;
        private bool _hasDestination;

        /// <summary>当前一天中的时刻（0–2400），供外部（如 UI/调试）读。</summary>
        public float TimeOfDay => _clock?.TimeOfDay ?? startTimeOfDay;

        // ── IActivityScoringContext ──
        public float CurrentTime => _clock?.AccumulatedTime ?? 0f;
        public GameplayTagContainer OwnerTags => ownerTags;

        // ── INpcAgent ──
        public Vector3 Position => transform.position;

        public void MoveTo(Vector3 destination)
        {
            _destination = destination;
            _hasDestination = true;
            if (_navAgent != null && _navAgent.isOnNavMesh)
                _navAgent.SetDestination(destination);
        }

        public void StopMoving()
        {
            _hasDestination = false;
            if (_navAgent != null && _navAgent.isOnNavMesh)
                _navAgent.ResetPath();
        }

        public bool ReachedDestination
        {
            get
            {
                if (!_hasDestination) return false;
                Vector3 pos = transform.position;
                pos.y = _destination.y; // 忽略高度差，只看水平到达
                return Vector3.Distance(pos, _destination) <= arriveDistance;
            }
        }

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _clock = new TimeOfDayClock(startTimeOfDay);
            _pool = new NpcGoalPool();
            _selector = new ActivitySelector(new NpcActivity[] { new MoveToActivity() });
            if (schedule != null)
                _driver = new ScheduleGoalDriver(schedule.BuildSchedule());
        }

        private void Update()
        {
            // 1. 推进时钟：现实秒 → 2400 制游戏时间
            if (dayLengthSeconds > 0f)
                _clock.Advance(Time.deltaTime / dayLengthSeconds * TimeOfDayClock.DayLength);

            // 2. 作息驱动 goal 增删（进窗口发、离窗口撤）
            _driver?.Sync(_clock.TimeOfDay, _clock.AccumulatedTime, _pool);

            // 3. tick 当前 activity；完成则撤掉其 goal，清空当前
            if (_current != null && _current.Tick(this))
            {
                if (_currentGoal != null) _pool.Remove(_currentGoal);
                SwitchTo(null, null);
            }

            // 4. 重选（M-AI-1 每帧；selector 廉价）
            var result = _selector.Select(_pool, this, _current, _currentGoal);
            if (result.Kind == SelectionKind.Switch)
                SwitchTo(result.Activity, result.Goal);
            else if (result.Kind == SelectionKind.None && _current != null)
                SwitchTo(null, null);
        }

        private void SwitchTo(NpcActivity activity, NpcGoal goal)
        {
            if (ReferenceEquals(activity, _current) && ReferenceEquals(goal, _currentGoal))
                return;

            _current?.OnExit(this);
            _current = activity;
            _currentGoal = goal;
            _current?.OnEnter(this, goal);
        }

        /// <summary>外部（如 narrative 桥）给本 NPC 塞一个 goal。</summary>
        public bool AddGoal(NpcGoal goal)
        {
            if (_pool == null) return false;
            if (goal != null) goal.CreationTime = _clock?.AccumulatedTime ?? 0f;
            return _pool.Add(goal);
        }
    }
}
