using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    public class FileSystemFacade
    {
        private FileSystem currentFileSystem = new FileSystem();

        public Stack<string> NotificationLog = new();
        public string notification = "";

        public IComponent CreateFolder(string name)
        {
            Folder Folder = new Folder(name);
            return new Folder(name);
        }

        public IComponent CreateFile(string name, int size, string content)
        {
            File File = new File(name, size, content);
            return new File(name, size, content);
        }

        public void AddChildren(IComponent root, params IComponent[] children)
        {
            currentFileSystem = currentFileSystem.AddList(root, children);
            if (currentFileSystem == null)
                throw new InvalidOperationException("No current File System.");
        }

        public IComponent GetComponentByPath(IComponent root, params string[] componentNames)
        {
            var currentcomponent = root;
            foreach(string name in componentNames)
            {
                var children = currentFileSystem.Children(currentcomponent);
                currentcomponent = children.FirstOrDefault(c => c.Name == name);
            }

            return currentcomponent;
        }

        public void Rename(IComponent component, string newName)
        {
           currentFileSystem = currentFileSystem.Rename(component, newName);
        }

        public void NotifyOnChange(IComponent component)
        {
            currentFileSystem = currentFileSystem.Attach(component, new FacadeNotificationObserver(this));
        }

        public void Delete(IComponent component)
        {
            currentFileSystem = currentFileSystem.Delete(component);
        }
    }
}
