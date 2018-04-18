/************************************************************
     File      : IEntity.cs
     brief     : The entity interface in CES.
     author    : Atin
     version   : 1.0
     date      : 2018/04/04 14:00:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
namespace Core
{
    namespace CES
    {
        /// <summary>
        /// Only keep the global identity when inherited from this.
        /// </summary>
        public abstract class IEntity
        {
            public long GID;
        }
    }
}
