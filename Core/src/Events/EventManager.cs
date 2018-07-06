/************************************************************
     File      : EventManager.cs
     brief     : EventManager for common event mechanism.
     author    : Atin
     version   : 1.0
     date      : 2018/06/27 11:23:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
using Core.CInterfaces;
using Core.Events;
using Core.src.Tools;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.src.Events
{
    public class EventManager : TSingleton<EventManager>, IUpdate
    {
        private int scheduleEventLen
        {
            get
            {
                return 10;//根据当前事件数量作调整
            }
        }
        
        private Dictionary<EventType, ArrayList> eventMapDic;//List<object>存放的是不同T的Tiny<T>(比如说List有Tiny<T1>和Tiny<T2>等等),
                                                             //以便实现同一个eventType可以有不同的类型的handler[通过发事件传入的参数才确定类型]
                                                             //ArrayList存放的是object,因此可以存放不同Tiny<T>,跟list<object>一样

        private EventManager()
        {
            eventMapDic = new Dictionary<EventType, ArrayList>();
        }

        private bool AddEventHandler<T>(EventType eventType, T handler)
        {
            ArrayList list = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list == null)
                {
                    list = new ArrayList();
                    eventMapDic[eventType] = list;
                }
            }
            else
            {
                list = new ArrayList();
                eventMapDic.Add(eventType, list);
            }
            return AddTinyEvent(ref list, handler);
        }

        /// <summary>
        /// 看看eventType对应的事件处理List是否有T类型的TinyEvent,有就调用注册事件, 没有先加一个再注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        private bool AddTinyEvent<T>(ref ArrayList list, T handler)
        {
            TinyEvent<T> tinyEvent = null;
            bool isExist = false;
            for (int i = 0; i < list.Count; i++)
            {
                tinyEvent = list[i] as TinyEvent<T>;
                if (tinyEvent != null)
                {
                    isExist = true;
                }
            }
            if (!isExist)
            {
                //处理不存在事件的情况
                tinyEvent = new TinyEvent<T>();
                list.Add(tinyEvent);
            }
            return tinyEvent.RegisterEventHandler(handler);
        }

        private void RemoveEventHandler<T>(EventType eventType, T handler)
        {
            ArrayList list = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list != null)
                {
                    RemoveTinyEvent(ref list, handler);
                }
            }
        }

        private void RemoveTinyEvent<T>(ref ArrayList list, T handler)
        {
            TinyEvent<T> tinyEvent = null;
            for (int i = 0; i < list.Count; i++)
            {
                tinyEvent = list[i] as TinyEvent<T>;
                if (tinyEvent != null)
                {
                    tinyEvent.UnRegisterEventHandler(handler);
                }
            }
        }

        private void RemoveEventHandlerByEventType(EventType eventType)
        {
            eventMapDic.Remove(eventType);
        }

        #region 注册/注销
        public bool RegisterEventHandler<T>(EventType eventType, T handler)
        {
            return AddEventHandler(eventType, handler);
        }

        public void UnRegisterEventHandler<T>(EventType eventType, T handler)
        {
            RemoveEventHandler(eventType, handler);
        }

        public void UnRegisterAllEventHandlerByEventType(EventType eventType)
        {
            RemoveEventHandlerByEventType(eventType);
        }
        #endregion

        #region 事件处理
        public void SendEvnetSync(EventType eventType)
        {
            ArrayList list = null;
            TinyEvent<Action> tinyEvt = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list != null)
                {
                    for(int i = 0; i < list.Count; i++)
                    {
                        tinyEvt = list[i] as TinyEvent<Action>;
                        if (tinyEvt != null)
                        {
                            tinyEvt.SendEvent();
                        }
                    }
                }
            }
        }

        public void SendEvnetSync<T1>(EventType eventType, T1 arg1)
        {
            ArrayList list = null;
            TinyEvent<Action<T1>> tinyEvt = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        tinyEvt = list[i] as TinyEvent<Action<T1>>;
                        if (tinyEvt != null)
                        {
                            tinyEvt.SendEvent(arg1);
                        }
                    }
                }
            }
        }

        public void SendEvnetSync<T1, T2>(EventType eventType, T1 arg1, T2 arg2)
        {
            ArrayList list = null;
            TinyEvent<Action<T1, T2>> tinyEvt = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        tinyEvt = list[i] as TinyEvent<Action<T1, T2>>;
                        if (tinyEvt != null)
                        {
                            tinyEvt.SendEvent(arg1, arg2);
                        }
                    }
                }
            }
        }

        public void SendEvnetSync<T1, T2, T3>(EventType eventType, T1 arg1, T2 arg2, T3 arg3)
        {
            ArrayList list = null;
            TinyEvent<Action<T1, T2, T3>> tinyEvt = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        tinyEvt = list[i] as TinyEvent<Action<T1, T2, T3>>;
                        if (tinyEvt != null)
                        {
                            tinyEvt.SendEvent(arg1, arg2, arg3);
                        }
                    }
                }
            }
        }

        public void SendEvnetSync<T1, T2, T3, T4>(EventType eventType, T1 arg1, T2 arg2, T3 arg3, T4 agr4)
        {
            ArrayList list = null;
            TinyEvent<Action<T1, T2, T3, T4>> tinyEvt = null;
            if (eventMapDic.ContainsKey(eventType))
            {
                list = eventMapDic[eventType];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        tinyEvt = list[i] as TinyEvent<Action<T1, T2, T3, T4>>;
                        if (tinyEvt != null)
                        {
                            tinyEvt.SendEvent(arg1, arg2, arg3, agr4);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 异步的话需要实现事件参数缓冲区.起始做起来虽然没有box/unbox但是缓存参数是一个类,即便缓冲也是很不好
        /// </summary>
        /// <param name="eventType"></param>
        public void SendEvnetAsync(EventType eventType)
        {
            //异步的话,大概思路是update的时候从参数队列取出来然后调SendEventSync,所以...
            //需要实现事件参数缓冲区.起始做起来虽然没有box/unbox但是缓存参数是一个类,即便缓冲也是很不好,有空再考虑怎么写
        }
        #endregion


        public void Update()
        {

        }
    }
}
