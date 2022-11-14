using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    public interface IComponentObserver
    {
        public void Notify(IComponent before, IComponent after);
    }
}
