using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gov.va.medora.mdo.dao;
using System.Collections.Concurrent;
using gov.va.medora.mdo.exceptions;
using gov.va.medora.mdo.dao.vista;
using System.Threading;

namespace gov.va.medora.mdo.domain.pool.connection
{
    public class ConnectionPool : AbstractResourcePool
    {
        protected byte SHUTDOWN_FLAG = 0;
        protected object _locker = new object();

        IList<ConnectionThread> _startedCxns = new List<ConnectionThread>();
        IList<ConnectionThread> _cxnsToRemove = new List<ConnectionThread>();

        BlockingCollection<AbstractConnection> _pooledCxns = new BlockingCollection<AbstractConnection>();
        DateTime _startupTimestamp;

        /// <summary>
        /// This method removes an object from the pool
        /// </summary>
        /// <param name="obj">Not currently used</param>
        /// <returns>AbstractConnection</returns>
        public override AbstractResource checkOut(object obj)
        {
            AbstractConnection cxn = null;
            if (!_pooledCxns.TryTake(out cxn, this.PoolSource.WaitTime))
            {
                throw new TimeoutException("No connection could be obtained in the configured allotment");
            }
            return cxn;
        }

        /// <summary>
        /// Return an AbstractConnection to the pool
        /// </summary>
        /// <param name="cxn">The connection to return to the pool</param>
        /// <returns></returns>
        public override object checkIn(AbstractResource cxn)
        {
            AbstractConnection theCxn = (AbstractConnection)cxn; // first get a new reference
            if (!theCxn.IsConnected || !theCxn.isAlive()) // don't add disconnected connections to the pool
            {
                this.TotalResources--;
                return null;
            }
            theCxn.LastUsed = DateTime.Now;

            if (cxn is VistaPoolConnection)
            {
                if (this.PoolSource.Credentials == null) // if not an authenticated connection
                {
                    ((VistaPoolConnection)theCxn).resetRaw(); // reset the symbol table so we're always receiving fresh from pool
                    ((VistaAccount)((VistaConnection)theCxn).Account).setContext(new MenuOption("XUS SIGNON"));
                }
                _pooledCxns.Add(theCxn);
            }
            else if (cxn is VistaConnection)
            {
                if (this.PoolSource.Credentials == null) // if not an authenticated connection
                {
                    ((VistaAccount)((VistaConnection)theCxn).Account).setContext(new MenuOption("XUS SIGNON"));
                }
                _pooledCxns.Add(theCxn);
            }
            else
            {
                _pooledCxns.Add(theCxn);
            }
            //System.Console.WriteLine("A connection was returned to the pool. {0} available", _pooledCxns.Count);
            return null;
        }

        /// <summary>
        /// The job of the run function is simply to make sure we have connections available in the pool. If the # of 
        /// connections falls below the threshold, the pool expands (up to the limit). The loop also tries to clean
        /// up any connections that may have failed
        /// </summary>
        internal void run()
        {
            lock (_locker)
            {
                _startupTimestamp = DateTime.Now;

                while (!Convert.ToBoolean(SHUTDOWN_FLAG))
                {
                    System.Threading.Thread.Sleep(100); // this small sleep time prevents the thread from consuming 100% of CPU

                    // this first IF statement checks to see if more connections need to be added to the pool
                    if (_pooledCxns.Count < this.PoolSource.MinPoolSize && _startedCxns.Count == 0) // only grow if we haven't started any connections
                    {
                        if (this.TotalResources < this.PoolSource.MaxPoolSize)
                        {
                            growPool();
                        }
                    }

                    // the second IF checks if this pool has started any connections - most of the time this should be false so we check it before the getEnumerator call
                    if (_startedCxns.Count > 0)
                    {
                        //Console.WriteLine("Found {0} connections that were started", _startedCxns.Count);
                        IEnumerator<ConnectionThread> enumerator = _startedCxns.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            ConnectionThread current = enumerator.Current;
                            Thread t = current.Thread;
                            if (t.ThreadState != ThreadState.Running && current.Connection.IsConnected) // check if started connection is ready for our pool
                            {
                                //Console.WriteLine("Found successfully started connection");
                                _pooledCxns.Add(current.Connection);
                                _cxnsToRemove.Add(current);
                            }
                            else if (t.ThreadState == ThreadState.Stopped && !current.Connection.IsConnected) // check if started connection thread has completed but for any reason disconnected
                            {
                                //Console.WriteLine("Found apparent failed connection - removing");
                                _cxnsToRemove.Add(current);
                            }
                            else if (DateTime.Now.Subtract(current.Timestamp).TotalSeconds > this.PoolSource.WaitTime.TotalSeconds) // lastly check for long running connection attempts
                            {
                                //Console.WriteLine("Found long running connection attempt - aborting");
                                try
                                {
                                    // don't really want to do this - could bubble up uncaught and bring down the process... should we just add the cxn to the remove list?
                                    current.Thread.Abort();
                                }
                                catch (Exception) { }
                                finally
                                {
                                    _cxnsToRemove.Add(current);
                                }
                            }
                        }
                    }

                    // per previous IF - can't modify collection while enumerating so the removal of failed connections is a separate step
                    if (_cxnsToRemove.Count > 0)
                    {
                        foreach (ConnectionThread t in _cxnsToRemove)
                        {
                            _startedCxns.Remove(t);
                        }
                        _cxnsToRemove = new List<ConnectionThread>();
                    }

                }
            }
        }

        void growPool()
        {
            //Console.WriteLine("Connection pool at min size {0} - growing by {1}", this.PoolSource.MinPoolSize, this.PoolSource.PoolExpansionSize);
            int growSize = this.PoolSource.PoolExpansionSize;
            if (this.TotalResources + growSize > this.PoolSource.MaxPoolSize) // if the growth would expand the pool above the max pool size, only grow by the amount allowed
            {
                growSize = this.PoolSource.MaxPoolSize - this.TotalResources;
            }
            for (int i = 0; i < growSize; i++)
            {
                ConnectionThread a = new ConnectionThread();
                a.Connection = AbstractDaoFactory.getDaoFactory(AbstractDaoFactory.getConstant(((ConnectionPoolSource)this.PoolSource).CxnSource.Protocol))
                    .getConnection(((ConnectionPoolSource)this.PoolSource).CxnSource);
                Thread t = new Thread(new ParameterizedThreadStart(connect));
                a.Thread = t;
                t.Start(a);
                _startedCxns.Add(a);
            }
        }

        void connect(object obj)
        {
            ConnectionThread cxn = (ConnectionThread)obj;
            try
            {
                cxn.Connection.connect();
                // Authenticated Vista Connection Handling
                if (this.PoolSource.Credentials != null) // should be an authenticated connection!
                {
                    AbstractPermission permission = new MenuOption(VistaConstants.CAPRI_CONTEXT);
                    permission.IsPrimary = true;
                    if (String.IsNullOrEmpty(this.PoolSource.Credentials.AccountName) || String.IsNullOrEmpty(this.PoolSource.Credentials.AccountPassword))
                    {
                        cxn.Connection.Account.AuthenticationMethod = VistaConstants.NON_BSE_CREDENTIALS; // if no A/V codes, visit
                    }
                    else
                    {
                        cxn.Connection.Account.AuthenticationMethod = VistaConstants.LOGIN_CREDENTIALS;
                    }
                    cxn.Connection.Account.authenticateAndAuthorize(this.PoolSource.Credentials, permission);
                }
                // END Authenticated cxn handling
                else if (cxn.Connection is VistaPoolConnection) // else if pool cxn and no creds - get default state
                {
                    ((VistaPoolConnection)cxn.Connection)._rawConnectionSymbolTable = ((VistaPoolConnection)cxn.Connection).getState();
                }
                this.TotalResources++;
                cxn.Connection.setTimeout(this.PoolSource.Timeout); // now gracefully timing out connections!
            }
            catch (Exception)
            {
                cxn.Connection.IsConnected = false;
            }
        }

        void disconnect(object AbstractConnection)
        {
            try
            {
                ((AbstractConnection)AbstractConnection).disconnect();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Signal the pool to shutdown. An attempt will be made to wait for as many connections as possible for return to the pool 
        /// before disconnecting each of the connections. Sets SHUTDOWN_FLAG so pool no longer tries to continue to run
        /// </summary>
        public override void shutdown()
        {
            if (SHUTDOWN_FLAG == 1)
            {
                return;
            }
            SHUTDOWN_FLAG = 1;

            AbstractConnection current = null;
            while (_pooledCxns.TryTake(out current, 1000))
            {
                current.disconnect();
            }
        }

        /// <summary>
        /// Check the status of the pool. If the pool has been signalled to shutdown, this should return false. If the pool
        /// has no available resources and has been running for more than one minute, assume something went wrong and return false.
        /// Otherwise, this should return true
        /// </summary>
        public bool IsAlive 
        {
            get
            {
                if (Convert.ToBoolean(SHUTDOWN_FLAG))
                {
                    return false;
                }
                if ((TotalResources == 0 && (_startedCxns == null || _startedCxns.Count == 0)) && // if no total resources AND no started connections
                    DateTime.Now.Subtract(_startupTimestamp).CompareTo(this.PoolSource.WaitTime) > 0) // if we have no resources, no started resources AND pool started more than 60 seconds ago
                {
                    return false;
                }
                return true;
            }
        }
    }
}
