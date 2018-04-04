/************************************************************
     File      : IStateMachine.cs
     brief     : Abstract statemachine.
     author    : Atin
     version   : 1.0
     date      : 2018/04/04 14:00:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStateMachine
{
    public IState defaultState;
    public IState curState;

    public abstract void Init();

    public abstract void Deinit();

    public virtual void Update(float deltaTime)
    {
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
        for(int i = 0; i < GetStateTypeCount(); i++)
        {
            stateType = 1 << i;
            if((stateType & stateFlags) != 0)
            {
                if (callBackWhenFound != null)
                {
                    callBackWhenFound(stateType);
                }
            }
        }
    }

    public void GotoState(IState nextState, IStateEvent evt)
    {
        curState.Leave(evt);
        curState = nextState;
        curState.Enter(evt);
    }

    public bool TryTransitState(IState nextState, IStateEvent evt)
    {
        return TryTransitState(curState, nextState, evt);
    }

    public bool TryTransitState(IState curState, IState nextState, IStateEvent evt)
    {
        return curState.CanLeave(nextState, evt) && nextState.CanEnter(curState, evt);
    }
    
}
