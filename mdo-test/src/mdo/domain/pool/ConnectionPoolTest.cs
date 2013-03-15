using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using gov.va.medora.mdo.dao.vista;
using gov.va.medora.mdo.dao;
using System.Reflection;
using System.Threading.Tasks;

namespace gov.va.medora.mdo.domain.pool.connection
{
    [TestFixture]
    public class ConnectionPoolTest
    {
        ConnectionPoolSource _localSource;
        AbstractResourcePool _pool;

        [TestFixtureSetUp]
        public void testFixtureSetUp()
        {
            _localSource = new ConnectionPoolSource()
            {
                MaxPoolSize = 8,
                MinPoolSize = 4,
                PoolExpansionSize = 1,
                WaitTime = new TimeSpan(0, 0, 30),
                Timeout = new TimeSpan(0, 5, 0),
                CxnSource = new DataSource()
                {
                    Provider = "127.0.0.1",
                    Port = 9200,
                    Protocol = "PVISTA", // this tells the factory we want a pooled Vista connection - important!
                    Modality = "HIS",
                    SiteId = new SiteId("901", "Local CPM")
                },
                Credentials = new VistaCredentials()
                {
                    AccountName = "1programmer",
                    AccountPassword = "programmer1",
                },
            };
        }

        [TestFixtureTearDown]
        public void testFixtureTearDown()
        {
            if (_pool != null)
            {
                _pool.shutdown();
            }
        }

        /// <summary>
        /// This test attempts to verify the timeout behavior of a pooled connection -requires a Vista system!!
        /// </summary>
        [Test]
        [Category("LiveVistaRun")] 
        public void testIsAlive()
        {
            VistaPoolConnection cxn = (VistaPoolConnection)AbstractDaoFactory.getDaoFactory(AbstractDaoFactory.getConstant("PVISTA")).getConnection(_localSource.CxnSource);
            cxn.connect();
            login(cxn);

            cxn.setTimeout(new TimeSpan(0, 0, 1)); // set this low - one second - so the test can run quickly

            Assert.IsTrue(cxn.isAlive());

            System.Threading.Thread.Sleep(500); // sleep for less than timeout     \
            //                                                                      \
            cxn.heartbeat();
            Assert.IsTrue(cxn.isAlive());

            System.Threading.Thread.Sleep(500); // sleep for less than timeout     --  These three add up to more than timeout

            cxn.heartbeat();
            Assert.IsTrue(cxn.isAlive());
            //                                                                      /
            System.Threading.Thread.Sleep(500); // sleep for less than timeout     /

            cxn.heartbeat();
            Assert.IsTrue(cxn.isAlive());


            System.Threading.Thread.Sleep(1100);// sleep for more than timeout

            Assert.IsFalse(cxn.isAlive());

            Assert.IsFalse(cxn.IsConnected);

            try
            {
                new VistaPatientDao(cxn).match("m1234");
                Assert.Fail("Uh-oh! Previous line should have thrown exception!");
            }
            catch (Exception)
            {
                // cool!
            }
        }

        void login(AbstractConnection cxn)
        {
            AbstractPermission permission = new MenuOption(VistaConstants.CPRS_CONTEXT);
            permission.IsPrimary = true;
            cxn.Account.AuthenticationMethod = VistaConstants.LOGIN_CREDENTIALS;
            AbstractCredentials creds = new VistaCredentials();
            //creds.AccountName = "1programmer";
            //creds.AccountPassword = "programmer1";
            creds.AccountName = "CLERKWARD01";
            creds.AccountPassword = "PASSWORD1!";
            cxn.Account.authenticateAndAuthorize(creds, permission);
        }

        [Test]
        [Category("LiveVistaRun")]
        public void testEagerStartupCxnTimeouts()
        {
            _localSource.LoadStrategy = LoadingStrategy.Eager;
            _localSource.Timeout = new TimeSpan(0, 0, 4);
            _pool = AbstractResourcePoolFactory.getResourcePool(_localSource);

            System.Threading.Thread.Sleep(3000); // wait a few seconds for pool to start but not enough for cxns to timeout

            Assert.IsTrue(_pool.TotalResources > 0, "Should have resources before asking for them in eager startup model");
            AbstractConnection cxn = (AbstractConnection)_pool.checkOut("901");
            Assert.IsNotNull(cxn);
            Assert.IsTrue(cxn.isAlive()); // the checkout function of the pool should ensure we're getting an alive connection but assert anyways to prove
            _pool.checkIn(cxn); // cleanup

            System.Threading.Thread.Sleep(5000);
        }

        [Test]
        [Category("LiveVistaRun")]
        public void testRunningPool()
        {
            _localSource.Timeout = new TimeSpan(0, 0, 1); // set this very low so the test can run quickly
            _pool = AbstractResourcePoolFactory.getResourcePool(_localSource);

            System.Threading.Thread.Sleep(5000);
            
            // let's increase our connection timeout for this test so we can more easily test the state of things
            _localSource.Timeout = new TimeSpan(0, 0, 5); // this should affect all newly created connections

            AbstractConnection cxn = (AbstractConnection)_pool.checkOutAlive("901");
            Int32 resourceCountBefore = _pool.TotalResources; // we take this immediately after checkOutAlive which should have found all our timed out connections and begun adding new ones
            
            // the pool shouldn't try to restart connections until they're needed - checkOutAlive should be that signal
            // so, now that checkOutAlive has been called, the pool should have discovered that it doesn't have any alive resources and started adding some back
            Assert.IsTrue(_pool.TotalResources == 1);

            Assert.IsNotNull(cxn);
            Assert.IsTrue(cxn.isAlive()); // the checkout function of the pool should ensure we're getting an alive connection but assert anyways to prove

            System.Threading.Thread.Sleep(1100); // even though our connection is checked out, it should still timeout
            _pool.checkIn(cxn); // the pool shouldn't put this item back in its collection (doesn't throw error though)
            Int32 resourcesAfter = _pool.TotalResources;
        }

    }

}
