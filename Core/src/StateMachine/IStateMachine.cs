/************************************************************
     File      : IStateMachine.cs
     brief     : Abstract statemachine.
     author    : Atin
     version   : 1.0
     date      : 2018/04/04 14:00:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
using System;

namespace Core.src.StateMachine
{
    public abstract class IStateMachine
    {
        public IState defaultState;
        public IState preState;
        public IState curState;

        public abstract void Init();

        public abstract void Deinit();

        public virtual void Update(float deltaTime)
        {
            if (curState != null)
                curState.Update(deltaTime);
        }

        /// <summary>
        /// 获取状态类型个数
        /// </summary>
        /// <returns></returns>
        public abstract int GetStateTypeCount();

        /// <summary>
        /// stateFlags是 stateType的与运算结果, callBackWhenFound是当找到对应的state之后，进行回调
        /// </summary>
        /// <param name="stateFlags"></param>
        /// <param name="callBackWhenFound"></param>
        public void FindContainState(int stateFlags, Action<int> callBackWhenFound = null)
        {
            int stateType;
            for (int i = 0; i < GetStateTypeCount(); i++)
            {
                stateType = 1 << i;
                if ((stateType & stateFlags) != 0)
                {
                    if (callBackWhenFound != null)
                    {
                        callBackWhenFound(stateType);
                    }
                }
            }
        }

        protected void GotoState(IState nextState, IStateEvent evt)
        {
            curState.Leave(evt);
            preState = curState;
            curState = nextState;
            curState.Enter(evt);
        }

        protected bool TryTransitState(IState nextState, IStateEvent evt)
        {
            return TryTransitState(curState, nextState, evt);
        }

        protected bool TryTransitState(IState curState, IState nextState, IStateEvent evt)
        {
            return curState.CanLeave(nextState, evt) && nextState.CanEnter(curState, evt);
        }

        protected virtual bool HandleMessage(IStateMsg msg)
        {
            return curState != null && curState.OnMessage(msg);
        }
        //在该类的派生类中,建议利用TinyEvent实现消息机制的状态转换收发、实现 状态对消息收发处理的 通知
    }
}