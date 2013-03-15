using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gov.va.medora.mdo.dao;

namespace gov.va.medora.mdo.domain.pool
{
    public abstract class AbstractPoolSource
    {
        public int MaxPoolSize { get; set; }
        public int MinPoolSize { get; set; }
        public int PoolExpansionSize { get; set; }
        public AbstractCredentials Credentials { get; set; }
        public LoadingStrategy LoadStrategy { get; set; }

        TimeSpan _waitTime = new TimeSpan(0, 0, 30); // default
        /// <summary>
        /// Set the time to block waiting for a connection from the pool
        /// </summary>
        public TimeSpan WaitTime { get { return _waitTime; } set { _waitTime = value; } }

        TimeSpan _timeout = new TimeSpan(0, 5, 0); // default
        /// <summary>
        /// Set the timeout for pooled resources. The pooled object should inherit from AbstractTimedResource
        /// and implement the IDisposable.Dispose interface for the timeout event
        /// </summary>
        public TimeSpan Timeout { get { return _timeout; } set { _timeout = value; } }

        public AbstractPoolSource() { }
    }

    public enum LoadingStrategy
    {
        Lazy = 0,
        Eager
    }
}
