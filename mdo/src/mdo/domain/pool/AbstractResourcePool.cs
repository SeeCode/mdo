using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gov.va.medora.mdo.domain.pool
{
    public abstract class AbstractResourcePool
    {
        public int TotalResources { get; set; }

        public AbstractPoolSource PoolSource { get; set; }

        public abstract AbstractResource checkOut(object obj);

        public virtual AbstractResource checkOutAlive(object obj)
        {
            AbstractResource resource = checkOut(obj);
            while (!resource.isAlive())
            {
                this.TotalResources--;
                resource = checkOut(obj);
            }
            return resource;
        }

        public abstract object checkIn(AbstractResource objToReturn);

        public abstract void shutdown();
    }
}
