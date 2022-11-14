using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    class Folder : IFolder
    {
        public string Name { get; }
        public Guid ID { get; }
        public int size { get; }

        public Folder(string name, Guid? id = null)
        {
            Name = name;
            ID = id ?? Guid.NewGuid();
            size = 0;
        }

        public IComponent Rename(string newName)
        {
            return new Folder(newName, ID);
        }
    }
}
