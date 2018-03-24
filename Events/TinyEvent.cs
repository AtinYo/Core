using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// EventHandlerDelegate 处理事件函数的委托,可以有多个参数,但是没返回值
    /// </summary>
    /// <typeparam name="EventHandlerDelegate"></typeparam>
    public class TinyEvent<EventHandlerDelegate>
    {
        private List<EventHandlerDelegate> evtHandlerDelList = new List<EventHandlerDelegate>();
        private List<EventHandlerDelegate> addList = new List<EventHandlerDelegate>();
        private List<EventHandlerDelegate> removeList = new List<EventHandlerDelegate>();

        private static object syncListRoot = new object();
        private static object syncCallingNum = new object();
        private int callingNum = 0;//当前调用的delegate数目

        public void RegisterEventHandler(EventHandlerDelegate evtHandlerDel)
        {
            try
            {
                if (callingNum > 0)
                {
                    lock (syncListRoot)
                    {
                        addList.Add(evtHandlerDel);
                    }
                }
                else
                {
                    lock (syncListRoot)
                    {
                        if (!evtHandlerDelList.Contains(evtHandlerDel))
                        {
                            evtHandlerDelList.Add(evtHandlerDel);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        public void UnRegisterEventHandler(EventHandlerDelegate evtHandlerDel)
        {
            try
            {
                if (callingNum > 0)
                {
                    lock (syncListRoot)
                    {
                        removeList.Add(evtHandlerDel);
                    }
                }
                else
                {
                    lock (syncListRoot)
                    {
                        evtHandlerDelList.Remove(evtHandlerDel);
                    }
                }
            }
            catch(Exception e)
            {
                LogException(e);
            }
        }

        private void LogException(System.Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.AppendLine("Caught an exception while using TinyEvent.");
            stringBuilder.AppendLine("Exception message :");
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine("Call stack :");
            stringBuilder.Append(exception.StackTrace);
            Debug.LogError(stringBuilder.ToString());
        }

        继续写,Invoke记得lockCallingNum并++
    }
}