using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IStateEvent
{

}

public delegate bool StateEnterTransitDelegate(IState leaveState, IStateEvent evt);
public delegate bool StateLeaveTransitDelegate(IState enterState, IStateEvent evt);

public abstract class IState
{
    public IStateMachine stateMachine { get; protected set; }

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
        bool canEnter = true;
        StateEnterTransitDelegate enterDel = null;
        if(stateEnterTransitDic != null)
        {
            stateEnterTransitDic.TryGetValue(leaveState, out enterDel);
            if(enterDel != null)
            {
                canEnter = enterDel(leaveState, evt);
            }
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
}