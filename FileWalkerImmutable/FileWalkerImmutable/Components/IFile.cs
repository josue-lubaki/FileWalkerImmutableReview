using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    public interface IFile : IComponent
    {
        int Size { get; }
        string Content { get; }
    }
}
