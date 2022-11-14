using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWalkerImmutable
{
    class FacadeNotificationObserver : IComponentObserver
    {
        private FileSystemFacade facade;

        public FacadeNotificationObserver(FileSystemFacade facade)
        {
            this.facade = facade;
        }

        public void Notify(IComponent before, IComponent after)
        {
            if (after == null)
                facade.NotificationLog.Push($"{before.Name} was deleted.");
            else
                facade.NotificationLog.Push($"{before.Name} was renamed to {after.Name}.");


            //if (after == null)
            //    facade.NotificationLog.Push($"{before.Name} was removed.");
            //else
            //    facade.NotificationLog.Push($"{before.Name} was renamed.");
        }
    }
}
