using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gov.va.medora.mdo.dao;
using gov.va.medora.mdo.dao.vista;
using Spring.Context;
using Spring.Context.Support;

namespace gov.va.medora.mdo
{
    public class BaseTest
    {
        public SiteTable SiteTable { get; set; }

        public AbstractConnection Cxn { get; set; }

        public BaseTest()
        {
            this.SiteTable = new SiteTable("../../resources/xml/VhaSites.xml");
        }

        /// <summary>
        /// Use this setup method to authenticate to a real Vista system (both production and test) for the prupose
        /// of running a 'live' test(s)
        /// </summary>
        /// <param name="siteId">The site to authenticate against - should have a corresponding 'User###' entry in your secret-testObjects.xml file with valid credentials</param>
        /// <returns>AbstractConnection</returns>
        public AbstractConnection setup(string siteId)
        {
            DataSource source = this.SiteTable.getSite(siteId).Sources[0]; // TBD - should we loop through data sources to make sure we get VISTA protocol?
            AbstractDaoFactory f = AbstractDaoFactory.getDaoFactory(AbstractDaoFactory.getConstant(source.Protocol));

            if (String.Equals(source.Protocol, "VISTA"))
            {
                return this.Cxn = VistaSetups.authorizedConnect("User" + siteId, false);
            }
            else
            {
                return this.Cxn = f.getConnection(source);
            }
        }

        public User getLdapUser()
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            StringTestObject testObj = (StringTestObject)ctx.GetObject("LDAP");
            User user = new User();
            user.Domain = testObj.get("domain");
            user.UserName = testObj.get("username");
            user.Pwd = testObj.get("password");
            return user;
        }

        public void tearDown()
        {
            if (this.Cxn != null)
            {
                try
                {
                    this.Cxn.disconnect();
                }
                catch (Exception) { }
            }
        }


    }
}
