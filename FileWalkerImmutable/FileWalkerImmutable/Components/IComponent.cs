using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    public interface IComponent
    {
        string Name { get; }
        Guid ID { get; }

        IComponent Rename(string newName);
    }
}
