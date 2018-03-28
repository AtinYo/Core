using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectPools
{
    /// <summary>
    /// 因为c#不允许重载=操作符,为了解决对象池对象被多个引用变量引用的时候,不同引用之间的操作导致逻辑错误(一个引用放回对象池,另一个引用并不知道),利用ObjectPoolRef类以及编码规范来避免这个问题.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPoolRef<T> where T : class, new()
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

        public ObjectPoolRef(ObjectPool<T> objPool)
        {
            pool = objPool;
#if NEED_CHECK_POOL_IS_NULL
            if(pool == null)
            {
                throw new Exception("pool is null when calling the constructor in ObjectPoolRef!");
            }
#endif
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

            public ObjectPoolInstance(ObjectPool<T> pool)
            {
                IsInUse = false;
                next = null;
                IsFromPool = pool != null;
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

        private ObjectPoolInstance[] freeList;//对象池不用链表而是用数组实现,是考虑到cache命中的问题.链表cache命中率不稳定而且容易造成内存碎片
        private ObjectPoolInstance curAvailableInst;
        private ObjectPoolInstance temp;
        private int Size;
        private int MaxSize;
        private bool CanExpand;
        private static object syncRoot = new object();

        public ObjectPool(int maxSize, int size = 5)
        {
            Size = size <= 0 ? 5 : size;
            MaxSize = maxSize <= 0 ? 10 : maxSize;
            CanExpand = true;
            freeList = new ObjectPoolInstance[Size];
            InitFreeList(0, Size);
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

        public void Return(ObjectPoolInstance instance)
        {
            lock (syncRoot)
            {
                temp = instance;
                temp.IsInUse = false;
                temp.SetNext(curAvailableInst);
                curAvailableInst = temp;
            }
        }

        private void Expand()
        {
            if (CanExpand)
            {
                int newSize = Size * 2;
                if (newSize > MaxSize)
                {
                    newSize = MaxSize;
                    CanExpand = false;
                }
                ObjectPoolInstance[] tempList = new ObjectPoolInstance[newSize];
                Array.Copy(freeList, tempList, Size);
                freeList = tempList;
                InitFreeList(Size, newSize);
                Size = newSize;
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

        private void InitFreeList(int offSet, int len)
        {
            freeList[offSet] = new ObjectPoolInstance(this);
            for (int i = 1 + offSet; i < len; i++)
            {
                freeList[i] = new ObjectPoolInstance(this);
                freeList[i - 1].SetNext(freeList[i]);
            }
            curAvailableInst = freeList[offSet];
        }
    }
}