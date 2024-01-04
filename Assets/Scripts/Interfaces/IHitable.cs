using System;

namespace Interfaces
{
    public interface IHitable
    {
        event Action<IHitable, IHitable> OnHit;

        void Hit();
    }
}