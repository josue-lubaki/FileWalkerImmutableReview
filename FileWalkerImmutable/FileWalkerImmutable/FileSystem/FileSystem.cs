using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    class FileSystem
    {
        private ImmutableDictionary<Guid, ImmutableList<IComponent>> Map { get; }
        private ImmutableDictionary<Guid, ImmutableList<IComponentObserver>> Observers { get; }

        public FileSystem() 
        {
            Map = ImmutableDictionary<Guid, ImmutableList<IComponent>>.Empty;
            Observers = ImmutableDictionary<Guid, ImmutableList<IComponentObserver>>.Empty;
        }

        public FileSystem(FileSystem previous, ImmutableDictionary<Guid, ImmutableList<IComponent>> map)
        {
            Map = map;
            Observers = previous.Observers;
        }

        public FileSystem(FileSystem previous, ImmutableDictionary<Guid, ImmutableList<IComponentObserver>> observers)
        {
            Map = previous.Map;
            Observers = observers;
        }

        private void NotifyChange(IComponent before, IComponent after)
        {
            Observers.TryGetValue(before.ID, out ImmutableList<IComponentObserver> observers);
            if (observers != null)
            {
                foreach (IComponentObserver observer in observers)
                    observer.Notify(before, after);

                if (observers == null)
                    Console.WriteLine("Observer list is null.");
            }
        }

        public IEnumerable<IComponent> Children(IComponent folder)
        {
            // Check if children exist, if not return empty list.
            return Map[folder.ID] ?? Enumerable.Empty<IComponent>();
        }

        public FileSystem Add(IComponent parent, IComponent child)
        {
            // Add the child to the parent's children list if it has one, otherwise create a new children list.
            Map.TryGetValue(parent.ID, out ImmutableList<IComponent> value);
            ImmutableList<IComponent> newChildren = value switch
            {
                null => ImmutableList.Create(child),
                var children => children.Add(child)
            };

            ImmutableDictionary<Guid, ImmutableList<IComponent>> newMap = Map.SetItem(parent.ID, newChildren);
            return new FileSystem(this, newMap);
        }

        public FileSystem AddList(IComponent parent, IEnumerable<IComponent> children)
        {
            Func<FileSystem, IComponent, FileSystem> foldFn = (fs, child) => fs.Add(parent, child);
            return children.Aggregate(this, foldFn);
        }

        public FileSystem Remove(IComponent component)
        {
            NotifyChange(component, null);

            // Remove all children first.
            var fs = this;
            int n = 0;
            int j = 0;
            Map.TryGetValue(component.ID, out var children);
            if (children != null) {
                foreach (var child in children)
                    fs = Remove(child);
            }

            // Make the map temporarily mutable so we can easily work with it.
            var builder = fs.Map.ToBuilder();

            builder.Remove(component.ID);
            foreach(var kvp in fs.Map)
            {
                if (kvp.Value.Contains(component))
                    builder[kvp.Key] = kvp.Value.Remove(component);
            }

            return new FileSystem(fs, builder.ToImmutable());
        }

        public FileSystem Replace(IComponent oldComponent, IComponent newComponent)
        {
            NotifyChange(oldComponent, newComponent);

            // Make the map temporarily mutable so we can easily work with it.
            var builder = Map.ToBuilder();

            builder.Remove(oldComponent.ID);

            Map.TryGetValue(oldComponent.ID, out var oldChildren);
            builder.Add(newComponent.ID, oldChildren);

            foreach(var kvp in Map)
            {
                if (kvp.Value?.Contains(oldComponent) == true)
                    builder[kvp.Key] = kvp.Value.Replace(oldComponent, newComponent);
            }

            return new FileSystem(this, builder.ToImmutable());

            Console.WriteLine("Replaced");
        }

        public FileSystem Rename(IComponent component, string newName)
        {
            var newComp = component.Rename(newName);

            return Replace(component, newComp);
        }

        public FileSystem Delete(IComponent component)
        {
            return Remove(component);
        }

        public FileSystem Attach(IComponent component, IComponentObserver observer)
        {
            // Add the observer to the parent's observer list if it has one, otherwise create a new observer list.
            Observers.TryGetValue(component.ID, out var value);
            var newObserverList = value == null
                ? ImmutableList.Create(observer)
                : value.Add(observer);

            var newObservers = Observers.SetItem(component.ID, newObserverList);
            return new FileSystem(this, newObservers);
        }

        public FileSystem Detach(IComponent component, IComponentObserver observer)
        {
            // Remove the observer from the parent's observer list if it exists, or throw an exception otherwise.
            Observers.TryGetValue(component.ID, out var value);
            if (value == null)
                throw new ArgumentException("Observer does not exist in component.");

            var newObserverList = value.Remove(observer);
            var newObservers = Observers.SetItem(component.ID, newObserverList);
            return new FileSystem(this, newObservers);
        }
    }
}
