/************************************************************
     File      : ObjectPools.cs
     brief     : Objectpool
     author    : Atin
     version   : 1.0
     date      : 2018/04/04 14:00:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Core
{
    namespace ObjectPools
    {
        /// <summary>
        /// 因为c#不允许重载=操作符,为了解决对象池对象被多个引用变量引用的时候,不同引用之间的操作导致逻辑错误(一个引用放回对象池,另一个引用并不知道),利用ObjectPoolRef类以及编码规范来避免这个问题.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ObjectPoolRef<T> : IDisposable where T : class, new()
        {
            private ObjectPool<T> pool = null;
            private ObjectPool<T>.ObjectPoolInstance instance = null;

            public T Value
            {
                get
                {
                    return instance != null ? instance.Value : null;
                }
            }

            public bool IsInvalid()
            {
                return instance == null || instance.Value == null;
            }

            public T FetchFromPool()
            {
                if (instance == null)
                {
                    instance = pool.Fetch();
                }
                return Value;
            }

            public void ReturnToPool()
            {
                if (instance != null)
                {
                    if (instance.IsFromPool)
                    {
                        pool.Return(instance);
                    }
                    instance = null;
                }
            }

            private ObjectPoolRef()
            {

            }

            public ObjectPoolRef(ObjectPool<T> _objPool)
            {
                pool = _objPool;
#if NEED_CHECK_POOL_IS_NULL
            if(pool == null)
            {
                throw new Exception("pool is null when calling the constructor in ObjectPoolRef!");
            }
#endif
            }

            /// <summary>
            /// 释放资源链接,扫尾工作等
            /// </summary>
            public virtual void Dispose()
            {
                
            }

            ~ObjectPoolRef()
            {
                ReturnToPool();//当ObjectPoolRef对象本身被GC释放时,应及时归还Instance给对象池
                Dispose();
            }
        }

        public class ObjectPool<T> where T : class, new()
        {
            public class ObjectPoolInstance
            {
                private T instance;
                public T Value
                {
                    get
                    {
                        //在项目的时候同事建议是按需分配,但是这样会造成跟链表一样的效果(链表cache命中率不稳定而且容易造成内存碎片)
                        //但是好处是,针对于T的封装类是连续分布的,只是在真正访问T对象才有可能cache缺失,而且还可以减轻内存负担.暂时先这么做吧
                        if (instance == null)
                        {
                            instance = new T();
                        }
                        return instance;
                    }
                }

                public bool IsInUse;
                private ObjectPoolInstance next;
                public bool IsFromPool { get; private set; }

                public ObjectPoolInstance(ObjectPool<T> _pool)
                {
                    IsInUse = false;
                    next = null;
                    IsFromPool = _pool != null;
                }

                public void SetNext(ObjectPoolInstance _next)
                {
                    next = _next;
                }

                public ObjectPoolInstance GetNext()
                {
                    return next;
                }
            }

            private ObjectPoolInstance[] freeList;//对象池不用链表而是用数组实现,是考虑到cache命中的问题.链表cache命中率不稳定而且容易造成内存碎片,但是对于T[]并不是数组,因为延时分配内存了,看ObjectPoolInstance.Value的定义
            private ObjectPoolInstance curAvailableInst;
            private ObjectPoolInstance temp;
            private int size;
            private int maxSize;
            private bool canExpand;
            private static object syncRoot = new object();

            public ObjectPool(int _maxSize, int _size = 5)
            {
                size = _size <= 0 ? 5 : _size;
                maxSize = _maxSize <= 0 ? 10 : _maxSize;
                canExpand = true;
                freeList = new ObjectPoolInstance[size];
                InitFreeList(0, size);
            }

            public ObjectPoolInstance Fetch()
            {
                lock (syncRoot)
                {
                    if (curAvailableInst == null)
                    {
                        Expand();
                    }
                    temp = curAvailableInst;
                    curAvailableInst.IsInUse = true;
                    curAvailableInst = curAvailableInst.GetNext();
                    temp.SetNext(null);
                    return temp;
                }
            }

            public void Return(ObjectPoolInstance _instance)
            {
                lock (syncRoot)
                {
                    temp = _instance;
                    temp.IsInUse = false;
                    temp.SetNext(curAvailableInst);
                    curAvailableInst = temp;
                }
            }

            private void Expand()
            {
                if (canExpand)
                {
                    int newSize = size * 2;
                    if (newSize > maxSize)
                    {
                        newSize = maxSize;
                        canExpand = false;
                    }
                    ObjectPoolInstance[] tempList = new ObjectPoolInstance[newSize];
                    Array.Copy(freeList, tempList, size);
                    freeList = tempList;
                    InitFreeList(size, newSize);
                    size = newSize;
                }
                else
                {
                    curAvailableInst = new ObjectPoolInstance(null);//无法扩容的时候返回临时对象,它的回收交给GC
                }
            }

            /// <summary>
            /// 按需写. when you need reduce your objectpool memory, you should implement it.
            /// </summary>
            private void Shrink()
            {

            }

            private void InitFreeList(int _offSet, int _len)
            {
                freeList[_offSet] = new ObjectPoolInstance(this);
                for (int i = 1 + _offSet; i < _len; i++)
                {
                    freeList[i] = new ObjectPoolInstance(this);
                    freeList[i - 1].SetNext(freeList[i]);
                }
                curAvailableInst = freeList[_offSet];
            }
        }
    }
}