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
                if (instance == null || instance.Value == null || !instance.isInUse)
                    return null;
                else
                    return instance.Value;
            }
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
                pool.Return(instance);
                instance = null;
            }
        }

        private ObjectPoolRef()
        {

        }

        public ObjectPoolRef(ObjectPool<T> objPool)
        {
            pool = objPool;
#if NeedCheckPoolIsNull
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
            public T Value;
            public bool isInUse;
            private ObjectPoolInstance next;

            public ObjectPoolInstance()
            {
                Value = new T();
                isInUse = false;
                next = null;
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

        private ObjectPoolInstance[] freeList;
        private ObjectPoolInstance curAvailableInst;
        private ObjectPoolInstance temp;
        private int Size;
        private static object syncRoot = new object();

        public ObjectPool(int size = 5)
        {
            Size = size <= 0 ? 5 : size;
            freeList = new ObjectPoolInstance[Size];
            InitFreeList(0, Size);
        }

        public ObjectPoolInstance Fetch()
        {
            lock (syncRoot)
            {
                if(curAvailableInst == null)
                {
                    Expand();
                }
                temp = curAvailableInst;
                temp.isInUse = true;
                temp.SetNext(null);
                curAvailableInst = curAvailableInst.GetNext();
                return temp;
            }
        }

        public void Return(ObjectPoolInstance instance)
        {
            lock (syncRoot)
            {
                temp = instance;
                temp.isInUse = false;
                temp.SetNext(curAvailableInst);
                curAvailableInst = temp;
            }
        }

        private void Expand()
        {
            int newSize = Size * 2;
            ObjectPoolInstance[] tempList= new ObjectPoolInstance[newSize];
            Array.Copy(freeList, tempList, Size);
            freeList = tempList;
            InitFreeList(Size, newSize);
            Size = newSize;
        }

        /// <summary>
        /// 按需写. when you need reduce your objectpool memory, you should implement it.
        /// </summary>
        private void Shrink()
        {

        }

        private void InitFreeList(int offSet, int len)
        {
            freeList[offSet] = new ObjectPoolInstance();
            for (int i = 1 + offSet; i < len; i++)
            {
                freeList[i] = new ObjectPoolInstance();
                freeList[i - 1].SetNext(freeList[i]);
            }
            curAvailableInst = freeList[offSet];
        }
    }
}