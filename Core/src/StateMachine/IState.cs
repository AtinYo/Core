/************************************************************
     File      : IState.cs
     brief     : Abstract state of StateMachine.
     author    : Atin
     version   : 1.0
     date      : 2018/04/04 14:00:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public abstract class IStateEvent
    {

    }

    public abstract class IStateMsg
    {

    }

    public delegate bool StateEnterTransitDelegate(IState leaveState, IStateEvent evt);
    public delegate bool StateLeaveTransitDelegate(IState enterState, IStateEvent evt);

    public abstract class IState
    {
        public IStateMachine stateMachine { get; protected set; }

        private StateEnterTransitDelegate allStateEnterTransit;//全部状态进入到this状态的一次状态(为了方便实现进入全局状态的转换而增加的)
        private Dictionary<int, StateEnterTransitDelegate> stateEnterTransitDic;//key是要离开状态的类型,enter的状态是自身
        private Dictionary<int, StateLeaveTransitDelegate> stateLeaveTransitDic;//key是要进入的状态的类型,leave的状态是自身

        public int StateType { get; protected set; }

        /// <summary>
        /// ISateType 该状态对应的int类型.举例,deadstatetpye=1 , skillstatetype=2, idlestatetype=4  这样可以进行位运算
        /// </summary>
        /// <param name="ISateType"></param>
        public IState(int ISateType, IStateMachine machine)
        {
            StateType = ISateType;
            stateMachine = machine;
        }

        public static implicit operator int(IState state)
        {
            return state.StateType;
        }

        public static int operator |(IState a, IState b)
        {
            return a.StateType | b.StateType;
        }

        /// <summary>
        /// 派生类实现各个状态之间的切换判断逻辑
        /// </summary>
        public virtual void Creat()
        {

        }

        public virtual void Destroy()
        {

        }

        protected void InitAllStateEnterTransit(StateEnterTransitDelegate enterTransit)
        {
            allStateEnterTransit = enterTransit;
        }

        protected void ClearAllStateEnterTransit()
        {
            allStateEnterTransit = null;
        }

        protected void AddStateEnterTransit(int stateFlags, StateEnterTransitDelegate enterTransit)
        {
            if (stateEnterTransitDic == null)
                stateEnterTransitDic = new Dictionary<int, StateEnterTransitDelegate>();
            stateMachine.FindContainState(stateFlags, stateType =>
            {
                if (!stateEnterTransitDic.ContainsKey(stateType))
                    stateEnterTransitDic.Add(stateType, enterTransit);
            });
        }

        protected void RemoveStateEnterTransit(int stateFlags)
        {
            if (stateEnterTransitDic == null)
                return;
            stateMachine.FindContainState(stateFlags, stateType =>
            {
                stateEnterTransitDic.Remove(stateType);
            });
        }

        protected void AddStateLeaveTransit(int stateFlags, StateLeaveTransitDelegate leaveTransit)
        {
            if (stateLeaveTransitDic == null)
                stateLeaveTransitDic = new Dictionary<int, StateLeaveTransitDelegate>();
            stateMachine.FindContainState(stateFlags, stateType =>
            {
                if (!stateLeaveTransitDic.ContainsKey(stateType))
                    stateLeaveTransitDic.Add(stateType, leaveTransit);
            });
        }

        protected void RemoveStateLeaveTransit(int stateFlags)
        {
            if (stateLeaveTransitDic == null)
                return;
            stateMachine.FindContainState(stateFlags, stateType =>
            {
                stateLeaveTransitDic.Remove(stateType);
            });
        }

        public bool CanEnter(IState leaveState, IStateEvent evt)
        {
            bool canEnter = false;//还是默认不能进入,如果默认可以进入,那么在不加状态之间的切换链接情况下,可以默认退出可以默认进入,那么切换链接就没有意义了
            StateEnterTransitDelegate enterDel = null;
            if (stateEnterTransitDic != null)
            {
                stateEnterTransitDic.TryGetValue(leaveState, out enterDel);
                if (enterDel != null)
                {
                    canEnter = enterDel(leaveState, evt);
                }
            }
            if(allStateEnterTransit != null)
            {
                canEnter |= allStateEnterTransit(leaveState, evt);
            }
            return canEnter;
        }

        public bool CanLeave(IState enterState, IStateEvent evt)
        {
            bool canLeave = true;
            StateLeaveTransitDelegate leaveDel = null;
            if (stateLeaveTransitDic != null)
            {
                stateLeaveTransitDic.TryGetValue(enterState, out leaveDel);
                if (leaveDel != null)
                {
                    canLeave = leaveDel(enterState, evt);
                }
            }
            return canLeave;
        }

        public virtual void Enter(IStateEvent evt)
        {

        }

        public virtual void Leave(IStateEvent evt)
        {

        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual bool OnMessage(IStateMsg msg)
        {
            return false;
        }
    }
}

