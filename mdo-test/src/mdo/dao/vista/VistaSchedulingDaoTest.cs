using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using gov.va.medora; // for StringTestObject
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Common.Logging;
using gov.va.medora.TOReflection;
using gov.va.medora.utils;
using gov.va.medora.mdo.exceptions;

namespace gov.va.medora.mdo.dao.vista
{
    [TestFixture]
    public class VistaSchedulingDaoTest : BaseTest
    {
        AbstractConnection _cxn;
        VistaSchedulingDao _dao;

        [SetUp]
        public void setup()
        {
            _cxn = new MockConnection("1128", "VISTA");
            _dao = new VistaSchedulingDao(_cxn);
            _cxn.connect();
        }

        [TearDown]
        public void tearDown()
        {
            _cxn.disconnect();
        }

        #region Live Run

        // these tests work against a live version of test Cache Vista which isn't easily transferrable so they're just commented out for now

        //#region Permissions and Security

        //[Test]
        //[Category("LiveRun901")]
        //public void testHasClinicAccessLiveRun()
        //{
        //    VistaSchedulingDao dao = new VistaSchedulingDao(base.setup("901"));

        //    Assert.IsTrue(dao.hasClinicAccess("195"), "This clinic was not configured as a restricted clinic when this unit test was created");

        //    base.tearDown();
        //}

        //[Test]
        //[Category("LiveRun901")]
        //public void testHasClinicAccessRestrictedClinicLiveRun()
        //{
        //    VistaSchedulingDao dao = new VistaSchedulingDao(base.setup("901"));

        //    try
        //    {
        //        dao.hasClinicAccess("91");
        //        Assert.Fail("Should have thrown exception!");
        //    }
        //    catch (Exception exc)
        //    {
        //        if (String.Equals(exc.Message, "CLNURGT - Access to SHERYL'S CLINIC is prohibited!\nOnly users with a special code may access this clinic."))
        //        {
        //            // cool
        //        }
        //        else
        //        {
        //            Assert.Fail("Unexpected exception: " + exc.Message);
        //        }
        //    }

        //    base.tearDown();
        //}

        //#endregion

        //#region Stop Codes

        //[Test]
        //[Category("LiveRun901")]
        //public void testIsValidStopCodeLiveRun()
        //{
        //    VistaSchedulingDao dao = new VistaSchedulingDao(base.setup("901"));

        //    Assert.IsTrue(dao.isValidStopCode("526"));

        //    base.tearDown();
        //}

        //[Test]
        //[Category("LiveRun901")]
        //public void testHasValidStopCodeLiveRun()
        //{
        //    VistaSchedulingDao dao = new VistaSchedulingDao(base.setup("901"));

        //    Assert.IsTrue(dao.hasValidStopCode("195"));

        //    base.tearDown();
        //}

        //#endregion

        //#region Cancellation Reasons
        //[Test]
        //[Category("LiveRun901")]
        //public void testGetCancellationReasonsLiveRun()
        //{
        //    VistaSchedulingDao dao = new VistaSchedulingDao(base.setup("901"));

        //    Dictionary<string, string> result = dao.getCancellationReasons();

        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Count > 0, "Always expect to have cancellation reasons");

        //    base.tearDown(); 
        //}
        //#endregion

        //#region Clinic Scheduling Details
        //[Test]
        //[Category("LiveRun901")]
        //public void testGetClinicSchedulingDetailsLiveRun()
        //{
        //    VistaSchedulingDao dao = new VistaSchedulingDao(base.setup("901"));

        //    HospitalLocation result = _dao.getClinicSchedulingDetails("195");

        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Availability.Count > 0, "Should be at least a few TimeSlot objects");
        //    Assert.AreEqual("7", result.ClinicDisplayStartTime);
        //    Assert.AreEqual("4", result.DisplayIncrements);
        //    Assert.AreEqual("30", result.AppointmentLength);
        //    Assert.IsTrue(result.AskForCheckIn);
        //    Assert.IsNull(result.Id);
        //    Assert.AreEqual("CARDIOLOGY", result.Name);
        //    Assert.AreEqual("CARDIO", result.Abbr);
        //    Assert.AreEqual("C", result.Type);
        //    Assert.AreEqual("C", result.TypeExtension.Key);
        //    Assert.AreEqual("CLINIC", result.TypeExtension.Value);
        //    Assert.IsNull(result.Institution.Key);
        //    Assert.IsNull(result.Institution.Value);
        //    Assert.IsNull(result.Division.Key);
        //    Assert.IsNull(result.Division.Value);
        //    Assert.IsNull(result.Module.Key);
        //    Assert.IsNull(result.Module.Value);
        //    Assert.IsNull(result.DispositionAction);
        //    Assert.AreEqual("", result.VisitLocation);
        //    Assert.IsNull(result.StopCode.Key);
        //    Assert.IsNull(result.StopCode.Value);
        //    Assert.IsNull(result.Department.Key);
        //    Assert.IsNull(result.Department.Value);
        //    Assert.AreEqual("M", result.Service.Key);
        //    Assert.AreEqual("MEDICINE", result.Service.Value);

        //    base.tearDown();
        //}
        //#endregion

        #endregion

        #region EWL


        #endregion

        #region Permissions and Security

        [Test]
        [ExpectedException(typeof(MdoException), ExpectedMessage = "CLNURGT - Access to SHERYL'S CLINIC is prohibited!\nOnly users with a special code may access this clinic.")]
        public void testHasClinicAccessRestrictedClinic()
        {
            _dao.hasClinicAccess("91");
        }

        [Test]
        public void testHasClinicAccess()
        {
            _dao.hasClinicAccess("195");
        }

        [Test]
        public void testBuildHasClinicAccessRequest()
        {
            String result = _dao.buildHasClinicAccessRequest("195").buildMessage();
            Assert.IsTrue(String.Equals("[XWB]113021.108SD VERIFY CLINIC ACCESS50003195f", result));
        }

        [Test]
        public void testToHasClinicAccess()
        {
            Assert.IsTrue(_dao.toHasClinicAccess("1\r\n"));
        }

        [Test]
        [ExpectedException(typeof(MdoException), ExpectedMessage = "CLNURGT - Access to SHERYL'S CLINIC is prohibited!\nOnly users with a special code may access this clinic.")]
        public void testToHasClinicAccessRestrictedResponse()
        {
            _dao.toHasClinicAccess("RESULT(0)=CLNURGT^Access to SHERYL'S CLINIC is prohibited!\nOnly users with a special code may access this clinic.^1\r\nRESULT(\"CLN\")=SHERYL'S CLINIC\r\n");
        }

        #endregion

        #region Stop Codes

        [Test]
        public void testIsValidStopCode()
        {
            Assert.IsTrue(_dao.isValidStopCode("526"));
        }

        [Test]
        public void testBuildIsValidStopCodeRequest()
        {
            string result = _dao.buildIsValidStopCodeRequest("523").buildMessage();
            Assert.IsTrue(String.Equals(result, "[XWB]113021.108SD VALID STOP CODE50003523f"));
        }

        [Test]
        public void testToIsValidStopCode()
        {
            string response = "1\r\n";
            Assert.IsTrue(_dao.toIsValidStopCode(response));
        }

        [Test]
        [ExpectedException(typeof(MdoException), ExpectedMessage = "CLNSCIN - Invalid Clinic Stop Code 0.")]
        public void testToIsValidStopCodeErrorMessageInvalidStopCode()
        {
            string response = "RESULT(0)=CLNSCIN^Invalid Clinic Stop Code 0.^1\r\n";
            _dao.toIsValidStopCode(response);
        }

        [Test]
        public void testToInvalidStopCodeEmptyResponse()
        {
            Assert.IsFalse(_dao.toIsValidStopCode(""));
        }

        [Test]
        public void testToInvalidStopCodeZeroResponse()
        {
            Assert.IsFalse(_dao.toIsValidStopCode("0"));
        }

        [Test]
        public void testHasValidStopCode()
        {
            Assert.IsTrue(_dao.hasValidStopCode("195"));
        }

        [Test]
        public void testBuildHasValidStopCodeRequest()
        {
            string request = _dao.buildHasValidStopCodeRequest("195").buildMessage();
            Assert.IsTrue(String.Equals(request, "[XWB]113021.108SD VALID CLINIC STOP CODE50003195f"));
        }

        [Test]
        public void testToValidStopCode()
        {
            Assert.IsTrue(_dao.toValidStopCode("1\r\n"));
        }

        [Test]
        public void testToValidStopCodeZeroResponse()
        {
            Assert.IsFalse(_dao.toValidStopCode("0\r\n"));
        }

        #endregion

        #region Cancel Appointment

        [Test]
        public void testGetCancellationReasons()
        {
            Dictionary<string, string> result = _dao.getCancellationReasons();

            Assert.IsNotNull(result);
            Assert.AreEqual("APPOINTMENT NO LONGER REQUIRED", result["10"]);
            Assert.AreEqual("CLINIC CANCELLED", result["13"]);
            Assert.AreEqual("CLINIC STAFFING", result["7"]);
            Assert.AreEqual("DEATH IN FAMILY", result["6"]);
            Assert.AreEqual("INPATIENT STATUS", result["2"]);
            Assert.AreEqual("OTHER", result["11"]);
            Assert.AreEqual("PATIENT DEATH", result["3"]);
            Assert.AreEqual("PATIENT NOT ELIGIBLE", result["9"]);
            Assert.AreEqual("SCHEDULING CONFLICT/ERROR", result["8"]);
            Assert.AreEqual("TRANSFER OPT CARE TO OTHER VA", result["12"]);
            Assert.AreEqual("TRAVEL DIFFICULTY", result["4"]);
            Assert.AreEqual("UNABLE TO KEEP APPOINTMENT", result["5"]);
            Assert.AreEqual("WEATHER", result["1"]);
        }

        [Test]
        public void testBuildGetCancellationReasonsRequest()
        {
            string queryString = _dao.buildGetCancellationReasonsRequest().buildMessage();

            Assert.IsTrue(String.Equals(queryString, "[XWB]113021.108SD LIST CANCELLATION REASONS50000f0000f0000f"));
        }

        [Test]
        public void testToCancellationReasons()
        {
            string response = "RESULT(0)=13^*^0^\r\nRESULT(1)=\r\nRESULT(1,\"ID\")=10\r\nRESULT(1,\"NAME\")=APPOINTMENT NO LONGER REQUIRED\r\nRESULT(2)=\r\nRESULT(2,\"ID\")=13\r\nRESULT(2,\"NAME\")=CLINIC CANCELLED\r\nRESULT(3)=\r\nRESULT(3,\"ID\")=7\r\nRESULT(3,\"NAME\")=CLINIC STAFFING\r\nRESULT(4)=\r\nRESULT(4,\"ID\")=6\r\nRESULT(4,\"NAME\")=DEATH IN FAMILY\r\nRESULT(5)=\r\nRESULT(5,\"ID\")=2\r\nRESULT(5,\"NAME\")=INPATIENT STATUS\r\nRESULT(6)=\r\nRESULT(6,\"ID\")=11\r\nRESULT(6,\"NAME\")=OTHER\r\nRESULT(7)=\r\nRESULT(7,\"ID\")=3\r\nRESULT(7,\"NAME\")=PATIENT DEATH\r\nRESULT(8)=\r\nRESULT(8,\"ID\")=9\r\nRESULT(8,\"NAME\")=PATIENT NOT ELIGIBLE\r\nRESULT(9)=\r\nRESULT(9,\"ID\")=8\r\nRESULT(9,\"NAME\")=SCHEDULING CONFLICT/ERROR\r\nRESULT(10)=\r\nRESULT(10,\"ID\")=12\r\nRESULT(10,\"NAME\")=TRANSFER OPT CARE TO OTHER VA\r\nRESULT(11)=\r\nRESULT(11,\"ID\")=4\r\nRESULT(11,\"NAME\")=TRAVEL DIFFICULTY\r\nRESULT(12)=\r\nRESULT(12,\"ID\")=5\r\nRESULT(12,\"NAME\")=UNABLE TO KEEP APPOINTMENT\r\nRESULT(13)=\r\nRESULT(13,\"ID\")=1\r\nRESULT(13,\"NAME\")=WEATHER\r\n";

            Dictionary<string, string> result = _dao.toCancellationReasons(response);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("APPOINTMENT NO LONGER REQUIRED", result["10"]);
            Assert.AreEqual("CLINIC CANCELLED", result["13"]);
            Assert.AreEqual("CLINIC STAFFING", result["7"]);
            Assert.AreEqual("DEATH IN FAMILY", result["6"]);
            Assert.AreEqual("INPATIENT STATUS", result["2"]);
            Assert.AreEqual("OTHER", result["11"]);
            Assert.AreEqual("PATIENT DEATH", result["3"]);
            Assert.AreEqual("PATIENT NOT ELIGIBLE", result["9"]);
            Assert.AreEqual("SCHEDULING CONFLICT/ERROR", result["8"]);
            Assert.AreEqual("TRANSFER OPT CARE TO OTHER VA", result["12"]);
            Assert.AreEqual("TRAVEL DIFFICULTY", result["4"]);
            Assert.AreEqual("UNABLE TO KEEP APPOINTMENT", result["5"]);
            Assert.AreEqual("WEATHER", result["1"]);
        }

        #endregion

        #region Clinic Scheduling Details

        [Test]
        public void testGetIndexShiftForApptLength()
        {
            int result = _dao.getIndexShiftForApptLength(10);
            Assert.AreEqual(result, 2);
            result = _dao.getIndexShiftForApptLength(15);
            Assert.AreEqual(result, 2);
            result = _dao.getIndexShiftForApptLength(20);
            Assert.AreEqual(result, 2);
            result = _dao.getIndexShiftForApptLength(30);
            Assert.AreEqual(result, 4);
            result = _dao.getIndexShiftForApptLength(60);
            Assert.AreEqual(result, 8);

            try
            {
                _dao.getIndexShiftForApptLength(3);
                Assert.Fail("Should have thrown arg exception");
            }
            catch (ArgumentException)
            {
                // ok
            }
            catch (Exception exc)
            {
                Assert.Fail("Unexpected error: {0}", exc.Message);
            }
        }

        [Test]
        public void testConvertVistaDate()
        {
            DateTime result = _dao.convertVistaDate(DateUtils.toVistaTimestampString(DateTime.Now)); // get Vista timestamp string for now and then convert back
            Assert.IsTrue(result.Date.Equals(DateTime.Now.Date));
        }

        [Test]
        public void testConvertVistaDateString01()
        {
            DateTime result = _dao.convertVistaDate("3121225");
            Assert.IsTrue(result.Date.Equals(new DateTime(2012, 12, 25)));
        }

        [Test]
        public void testConvertVistaDateString02()
        {
            DateTime result = _dao.convertVistaDate("3121225.070000");
            Assert.IsTrue(result.Date.Equals(new DateTime(2012, 12, 25)));
        }

        [Test]
        public void testParseAvailabilityString()
        {
            string availability = "RESULT(3120826,0)=3120826\r\nRESULT(3120826,1)=SU 26  |       [4 4 4 4|4 4 4 4|4 4 4 4|4 4 4 4]       [8 8 8 8|8 8 8 8] \r\nRESULT(3120826,9)=195\r\nRESULT(3120828,0)=3120828\r\nRESULT(3120828,1)=TU 28  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120829,0)=3120829\r\nRESULT(3120829,1)=WE 29  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120830,0)=3120830\r\nRESULT(3120830,1)=TH 30  |       [1 1 2 2|2 2 2 2|2 2 1 1|2 2 2 2]                       \r\nRESULT(3120830,9)=195\r\nRESULT(3120902,0)=3120902\r\nRESULT(3120902,1)=SU 02  |       [4 4 4 4|4 4 4 4|4 4 4 4|4 4 4 4]       [8 8 8 8|8 8 8 8] \r\nRESULT(3120902,9)=195\r\nRESULT(3120904,0)=3120904\r\nRESULT(3120904,1)=TU 04  |       [G G 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120905,0)=3120905\r\nRESULT(3120905,1)=WE 05  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120906,0)=3120906\r\nRESULT(3120906,1)=TH 06  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120909,0)=3120909\r\nRESULT(3120909,1)=SU 09  |       [4 4 4 4|4 4 4 4|4 4 4 4|4 4 4 4]       [8 8 8 8|8 8 8 8] \r\nRESULT(3120909,9)=195\r\nRESULT(3120911,0)=3120911\r\nRESULT(3120911,1)=TU 11  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120912,0)=3120912\r\nRESULT(3120912,1)=WE 12  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120913,0)=3120913\r\nRESULT(3120913,1)=TH 13  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120916,0)=3120916\r\nRESULT(3120916,1)=SU 16  |       [4 4 4 4|4 4 4 4|4 4 4 4|4 4 4 4]       [8 8 8 8|8 8 8 8] \r\nRESULT(3120916,9)=195\r\nRESULT(3120918,0)=3120918\r\nRESULT(3120918,1)=TU 18  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120919,0)=3120919\r\nRESULT(3120919,1)=WE 19  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\nRESULT(3120920,0)=3120920\r\nRESULT(3120920,1)=TH 20  |       [1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1|1 1 1 1] \r\n";
            int clinicStartTime = 7;
            int apptLength = 30;

            IList<TimeSlot> result = _dao.parseAvailabilitString(availability, clinicStartTime, apptLength);

            Assert.IsNotNull(result);
            Assert.AreEqual(241, result.Count);
            Assert.AreEqual(result[0].Start, DateTime.Parse("8/26/2012 7:00:00 AM"));
            Assert.AreEqual(result[0].End, DateTime.Parse("8/26/2012 7:30:00 AM"));
            Assert.AreEqual("No availability", result[0].Text);
            Assert.IsFalse(result[0].Available);
            Assert.AreEqual(result[1].Start, DateTime.Parse("8/26/2012 7:30:00 AM"));
            Assert.AreEqual(result[1].End, DateTime.Parse("8/26/2012 8:00:00 AM"));
            Assert.AreEqual("No availability", result[1].Text);
            Assert.IsFalse(result[1].Available);
            Assert.AreEqual(result[2].Start, DateTime.Parse("8/26/2012 8:00:00 AM"));
            Assert.AreEqual(result[2].End, DateTime.Parse("8/26/2012 8:30:00 AM"));
            Assert.AreEqual("4", result[2].Text);
            Assert.IsTrue(result[2].Available);
            Assert.AreEqual(result[3].Start, DateTime.Parse("8/26/2012 8:30:00 AM"));
            Assert.AreEqual(result[3].End, DateTime.Parse("8/26/2012 9:00:00 AM"));
            Assert.AreEqual("4", result[3].Text);
            Assert.IsTrue(result[3].Available);

            Assert.AreEqual(result[78].Start, DateTime.Parse("9/4/2012 7:00:00 AM"));
            Assert.AreEqual(result[78].End, DateTime.Parse("9/4/2012 7:30:00 AM"));
            Assert.AreEqual("No availability", result[78].Text);
            Assert.IsFalse(result[78].Available);
            Assert.AreEqual(result[79].Start, DateTime.Parse("9/4/2012 7:30:00 AM"));
            Assert.AreEqual(result[79].End, DateTime.Parse("9/4/2012 8:00:00 AM"));
            Assert.AreEqual("No availability", result[79].Text);
            Assert.IsFalse(result[79].Available);
            Assert.AreEqual(result[80].Start, DateTime.Parse("9/4/2012 8:00:00 AM"));
            Assert.AreEqual(result[80].End, DateTime.Parse("9/4/2012 8:30:00 AM"));
            Assert.AreEqual("G", result[80].Text);
            Assert.IsFalse(result[80].Available);
            Assert.AreEqual(result[81].Start, DateTime.Parse("9/4/2012 8:30:00 AM"));
            Assert.AreEqual(result[81].End, DateTime.Parse("9/4/2012 9:00:00 AM"));
            Assert.AreEqual("1", result[81].Text);
            Assert.IsTrue(result[81].Available);
            Assert.AreEqual(result[82].Start, DateTime.Parse("9/4/2012 9:00:00 AM"));
            Assert.AreEqual(result[82].End, DateTime.Parse("9/4/2012 9:30:00 AM"));
            Assert.AreEqual("1", result[82].Text);
            Assert.IsTrue(result[82].Available);

            Assert.AreEqual(result[184].Start, DateTime.Parse("9/16/2012 8:30:00 AM"));
            Assert.AreEqual(result[184].End, DateTime.Parse("9/16/2012 9:00:00 AM"));
            Assert.AreEqual("4", result[184].Text);
            Assert.IsTrue(result[184].Available);
            Assert.AreEqual(result[185].Start, DateTime.Parse("9/16/2012 9:00:00 AM"));
            Assert.AreEqual(result[185].End, DateTime.Parse("9/16/2012 9:30:00 AM"));
            Assert.AreEqual("4", result[185].Text);
            Assert.IsTrue(result[185].Available);
            Assert.AreEqual(result[186].Start, DateTime.Parse("9/16/2012 9:30:00 AM"));
            Assert.AreEqual(result[186].End, DateTime.Parse("9/16/2012 10:00:00 AM"));
            Assert.AreEqual("4", result[186].Text);
            Assert.IsTrue(result[186].Available);

            Assert.AreEqual(result[238].Start, DateTime.Parse("9/20/2012 1:00:00 PM"));
            Assert.AreEqual(result[238].End, DateTime.Parse("9/20/2012 1:30:00 PM"));
            Assert.AreEqual("1", result[238].Text);
            Assert.IsTrue(result[238].Available);
            Assert.AreEqual(result[239].Start, DateTime.Parse("9/20/2012 1:30:00 PM"));
            Assert.AreEqual(result[239].End, DateTime.Parse("9/20/2012 2:00:00 PM"));
            Assert.AreEqual("1", result[239].Text);
            Assert.IsTrue(result[239].Available);
            Assert.AreEqual(result[240].Start, DateTime.Parse("9/20/2012 2:00:00 PM"));
            Assert.AreEqual(result[240].End, DateTime.Parse("9/20/2012 2:30:00 PM"));
            Assert.AreEqual("No availability", result[240].Text);
            Assert.IsFalse(result[240].Available);
        }

        [Test]
        [ExpectedException(typeof(MdoException), ExpectedMessage = "Invalid response for building clinic scheduling details")]
        public void testGetClinicSchedulingDetailsNonExistentClinicId()
        {
            HospitalLocation result = _dao.getClinicSchedulingDetails("FAKE");
        }

        [Test]
        public void testGetClinicSchedulingDetails()
        {
            HospitalLocation result = _dao.getClinicSchedulingDetails("195");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Availability.Count, 2232, "Should be 2232 TimeSlot objects");
            Assert.AreEqual("7", result.ClinicDisplayStartTime);
            Assert.AreEqual("4", result.DisplayIncrements);
            Assert.AreEqual("30", result.AppointmentLength);
            Assert.IsTrue(result.AskForCheckIn);
            Assert.IsNull(result.Id);
            Assert.AreEqual("CARDIOLOGY", result.Name);
            Assert.AreEqual("CARDIO", result.Abbr);
            Assert.AreEqual("C", result.Type);
            Assert.AreEqual("C", result.TypeExtension.Key);
            Assert.AreEqual("CLINIC", result.TypeExtension.Value);
            Assert.IsNull(result.Institution.Key);
            Assert.IsNull(result.Institution.Value);
            Assert.IsNull(result.Division.Key);
            Assert.IsNull(result.Division.Value);
            Assert.IsNull(result.Module.Key);
            Assert.IsNull(result.Module.Value);
            Assert.IsNull(result.DispositionAction);
            Assert.AreEqual("", result.VisitLocation);
            Assert.IsNull(result.StopCode.Key);
            Assert.IsNull(result.StopCode.Value);
            Assert.IsNull(result.Department.Key);
            Assert.IsNull(result.Department.Value);
            Assert.AreEqual("M", result.Service.Key);
            Assert.AreEqual("MEDICINE", result.Service.Value);
        }

        #endregion

        #region Appointment Types

        [Test]
        public void testGetAppointmentTypesLiveRun()
        {
            IList<AppointmentType> result = _dao.getAppointmentTypes("A");

            Assert.IsNotNull(result);
            Assert.AreEqual(11, result.Count);
            Assert.AreEqual("2", result[0].ID);
            Assert.AreEqual("CLASS II DENTAL", result[0].Name);
            Assert.IsTrue(result[0].Active);
            Assert.IsNull(result[0].Description);
            Assert.IsNull(result[0].Synonym);
        }

        [Test]
        public void testGetAppointmentTypes()
        {
            IList<AppointmentType> result = _dao.getAppointmentTypes("A");

            Assert.IsNotNull(result);
            Assert.AreEqual(11, result.Count);
            Assert.AreEqual("2", result[0].ID);
            Assert.AreEqual("CLASS II DENTAL", result[0].Name);
            Assert.IsTrue(result[0].Active);
            Assert.IsNull(result[0].Description);
            Assert.IsNull(result[0].Synonym);
            Assert.AreEqual("7", result[1].ID);
            Assert.AreEqual("COLLATERAL OF VET.", result[1].Name);
            Assert.IsTrue(result[1].Active);
            Assert.IsNull(result[1].Description);
            Assert.IsNull(result[1].Synonym);
            Assert.AreEqual("1", result[2].ID);
            Assert.AreEqual("COMPENSATION & PENSION", result[2].Name);
            Assert.IsTrue(result[2].Active);
            Assert.IsNull(result[2].Description);
            Assert.IsNull(result[2].Synonym);
            Assert.AreEqual("10", result[3].ID);
            Assert.AreEqual("COMPUTER GENERATED", result[3].Name);
            Assert.IsTrue(result[3].Active);
            Assert.IsNull(result[3].Description);
            Assert.IsNull(result[3].Synonym);
            Assert.AreEqual("4", result[4].ID);
            Assert.AreEqual("EMPLOYEE", result[4].Name);
            Assert.IsTrue(result[4].Active);
            Assert.IsNull(result[4].Description);
            Assert.IsNull(result[4].Synonym);
            Assert.AreEqual("3", result[5].ID);
            Assert.AreEqual("ORGAN DONORS", result[5].Name);
            Assert.IsTrue(result[5].Active);
            Assert.IsNull(result[5].Description);
            Assert.IsNull(result[5].Synonym);
            Assert.AreEqual("5", result[6].ID);
            Assert.AreEqual("PRIMA FACIA", result[6].Name);
            Assert.IsTrue(result[6].Active);
            Assert.IsNull(result[6].Description);
            Assert.IsNull(result[6].Synonym);
            Assert.AreEqual("9", result[7].ID);
            Assert.AreEqual("REGULAR", result[7].Name);
            Assert.IsTrue(result[7].Active);
            Assert.IsNull(result[7].Description);
            Assert.IsNull(result[7].Synonym);
            Assert.AreEqual("6", result[8].ID);
            Assert.AreEqual("RESEARCH", result[8].Name);
            Assert.IsTrue(result[8].Active);
            Assert.IsNull(result[8].Description);
            Assert.IsNull(result[8].Synonym);
            Assert.AreEqual("11", result[9].ID);
            Assert.AreEqual("SERVICE CONNECTED", result[9].Name);
            Assert.IsTrue(result[9].Active);
            Assert.IsNull(result[9].Description);
            Assert.IsNull(result[9].Synonym);
            Assert.AreEqual("8", result[10].ID);
            Assert.AreEqual("SHARING AGREEMENT", result[10].Name);
            Assert.IsTrue(result[10].Active);
            Assert.IsNull(result[10].Description);
            Assert.IsNull(result[10].Synonym);
        }

        [Test]
        public void testBuildGetAppointmentTypesRequest()
        {
            string request = "[XWB]113021.108SD APPOINTMENT LIST BY NAME50000f0001Af0000f";
            MdoQuery result = _dao.buildGetAppointmentTypesRequest("", "A", "");
            Assert.IsTrue(String.Equals(result.buildMessage(), request));
        }
        
        [Test]
        public void testToAppointmentTypes()
        {
            IList<AppointmentType> result = _dao.toAppointmentTypes("RESULT(0)=11^*^0^\r\nRESULT(1)=\r\nRESULT(1,\"ID\")=2\r\nRESULT(1,\"NAME\")=CLASS II DENTAL\r\nRESULT(2)=\r\nRESULT(2,\"ID\")=7\r\nRESULT(2,\"NAME\")=COLLATERAL OF VET.\r\nRESULT(3)=\r\nRESULT(3,\"ID\")=1\r\nRESULT(3,\"NAME\")=COMPENSATION & PENSION\r\nRESULT(4)=\r\nRESULT(4,\"ID\")=10\r\nRESULT(4,\"NAME\")=COMPUTER GENERATED\r\nRESULT(5)=\r\nRESULT(5,\"ID\")=4\r\nRESULT(5,\"NAME\")=EMPLOYEE\r\nRESULT(6)=\r\nRESULT(6,\"ID\")=3\r\nRESULT(6,\"NAME\")=ORGAN DONORS\r\nRESULT(7)=\r\nRESULT(7,\"ID\")=5\r\nRESULT(7,\"NAME\")=PRIMA FACIA\r\nRESULT(8)=\r\nRESULT(8,\"ID\")=9\r\nRESULT(8,\"NAME\")=REGULAR\r\nRESULT(9)=\r\nRESULT(9,\"ID\")=6\r\nRESULT(9,\"NAME\")=RESEARCH\r\nRESULT(10)=\r\nRESULT(10,\"ID\")=11\r\nRESULT(10,\"NAME\")=SERVICE CONNECTED\r\nRESULT(11)=\r\nRESULT(11,\"ID\")=8\r\nRESULT(11,\"NAME\")=SHARING AGREEMENT\r\n");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(11, result.Count);
            Assert.AreEqual("2", result[0].ID);
            Assert.AreEqual("CLASS II DENTAL", result[0].Name);
            Assert.IsTrue(result[0].Active);
            Assert.IsNull(result[0].Description);
            Assert.IsNull(result[0].Synonym);
            Assert.AreEqual("7", result[1].ID);
            Assert.AreEqual("COLLATERAL OF VET.", result[1].Name);
            Assert.IsTrue(result[1].Active);
            Assert.IsNull(result[1].Description);
            Assert.IsNull(result[1].Synonym);
            Assert.AreEqual("1", result[2].ID);
            Assert.AreEqual("COMPENSATION & PENSION", result[2].Name);
            Assert.IsTrue(result[2].Active);
            Assert.IsNull(result[2].Description);
            Assert.IsNull(result[2].Synonym);
            Assert.AreEqual("10", result[3].ID);
            Assert.AreEqual("COMPUTER GENERATED", result[3].Name);
            Assert.IsTrue(result[3].Active);
            Assert.IsNull(result[3].Description);
            Assert.IsNull(result[3].Synonym);
            Assert.AreEqual("4", result[4].ID);
            Assert.AreEqual("EMPLOYEE", result[4].Name);
            Assert.IsTrue(result[4].Active);
            Assert.IsNull(result[4].Description);
            Assert.IsNull(result[4].Synonym);
            Assert.AreEqual("3", result[5].ID);
            Assert.AreEqual("ORGAN DONORS", result[5].Name);
            Assert.IsTrue(result[5].Active);
            Assert.IsNull(result[5].Description);
            Assert.IsNull(result[5].Synonym);
            Assert.AreEqual("5", result[6].ID);
            Assert.AreEqual("PRIMA FACIA", result[6].Name);
            Assert.IsTrue(result[6].Active);
            Assert.IsNull(result[6].Description);
            Assert.IsNull(result[6].Synonym);
            Assert.AreEqual("9", result[7].ID);
            Assert.AreEqual("REGULAR", result[7].Name);
            Assert.IsTrue(result[7].Active);
            Assert.IsNull(result[7].Description);
            Assert.IsNull(result[7].Synonym);
            Assert.AreEqual("6", result[8].ID);
            Assert.AreEqual("RESEARCH", result[8].Name);
            Assert.IsTrue(result[8].Active);
            Assert.IsNull(result[8].Description);
            Assert.IsNull(result[8].Synonym);
            Assert.AreEqual("11", result[9].ID);
            Assert.AreEqual("SERVICE CONNECTED", result[9].Name);
            Assert.IsTrue(result[9].Active);
            Assert.IsNull(result[9].Description);
            Assert.IsNull(result[9].Synonym);
            Assert.AreEqual("8", result[10].ID);
            Assert.AreEqual("SHARING AGREEMENT", result[10].Name);
            Assert.IsTrue(result[10].Active);
            Assert.IsNull(result[10].Description);
            Assert.IsNull(result[10].Synonym);
        }

        #endregion

        #region Check Valid Appt

        [Test]
        public void testToCheckAppointmentEmptyString()
        {
            Assert.IsTrue(_dao.toAppointmentCheck(""), "Empty result appears to be a valid check");
        }

        [Test]
        public void testToCheckAppointmentBoolOneString()
        {
            Assert.IsTrue(_dao.toAppointmentCheck("1"), "Result of '1' appears to be a valid check");
        }

        [Test]
        public void testToCheckAppointmentBoolZeroString()
        {
            Assert.IsFalse(_dao.toAppointmentCheck("0"), "Result of '0' appears to be an invalid check");
        }

        [Test]
        [ExpectedException(typeof(MdoException))]
        public void testToAppointmentCheckException()
        {
            _dao.toAppointmentCheck("RESULT(0)=APTCLUV^\"There is no availability for this date/time.\"\r\n");
        }

        #endregion

        #region Pending Appts

        [Test]
        public void testToPendingAppts()
        {
            IList<Appointment> result = _dao.toPendingAppointments("RESULT(3120830.08,\"APPOINTMENT TYPE\")=SERVICE CONNECTED\r\nRESULT(3120830.08,\"CLINIC\")=CARDIOLOGY\r\nRESULT(3120830.08,\"COLLATERAL VISIT\")=\r\nRESULT(3120830.08,\"CONSULT LINK\")=\r\nRESULT(3120830.08,\"EKG DATE/TIME\")=\r\nRESULT(3120830.08,\"LAB DATE/TIME\")=\r\nRESULT(3120830.08,\"LENGTH OF APP'T\")=\r\nRESULT(3120830.08,\"X-RAY DATE/TIME\")=\r\nRESULT(3120830.103,\"APPOINTMENT TYPE\")=REGULAR\r\nRESULT(3120830.103,\"CLINIC\")=CARDIOLOGY\r\nRESULT(3120830.103,\"COLLATERAL VISIT\")=\r\nRESULT(3120830.103,\"CONSULT LINK\")=\r\nRESULT(3120830.103,\"EKG DATE/TIME\")=\r\nRESULT(3120830.103,\"LAB DATE/TIME\")=\r\nRESULT(3120830.103,\"LENGTH OF APP'T\")=\r\nRESULT(3120830.103,\"X-RAY DATE/TIME\")=\r\n");

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("", result[0].Length);
            Assert.IsNull(result[0].AppointmentType.ID);
            Assert.AreEqual("SERVICE CONNECTED", result[0].AppointmentType.Name);
            Assert.IsTrue(result[0].AppointmentType.Active);
            Assert.IsNull(result[0].AppointmentType.Description);
            Assert.IsNull(result[0].AppointmentType.Synonym);
            Assert.IsNull(result[0].Id);
            Assert.AreEqual("3120830.08", result[0].Timestamp);
            Assert.IsNull(result[0].Clinic.Id);
            Assert.AreEqual("CARDIOLOGY", result[0].Clinic.Name);
            Assert.IsNull(result[0].Clinic.AppointmentTimestamp);
            Assert.AreEqual("", result[0].LabDateTime);
            Assert.AreEqual("", result[0].XrayDateTime);
            Assert.AreEqual("", result[0].EkgDateTime);
            Assert.AreEqual("", result[0].Purpose);
            Assert.AreEqual("SERVICE CONNECTED", result[0].Type);
            Assert.IsNull(result[0].CurrentStatus);
            Assert.AreEqual("", result[1].Length);
            Assert.IsNull(result[1].AppointmentType.ID);
            Assert.AreEqual("REGULAR", result[1].AppointmentType.Name);
            Assert.IsTrue(result[1].AppointmentType.Active);
            Assert.IsNull(result[1].AppointmentType.Description);
            Assert.IsNull(result[1].AppointmentType.Synonym);
            Assert.IsNull(result[1].Id);
            Assert.AreEqual("3120830.103", result[1].Timestamp);
            Assert.IsNull(result[1].Clinic.Id);
            Assert.AreEqual("CARDIOLOGY", result[1].Clinic.Name);
            Assert.AreEqual("", result[1].LabDateTime);
            Assert.AreEqual("", result[1].XrayDateTime);
            Assert.AreEqual("", result[1].EkgDateTime);
            Assert.AreEqual("", result[1].Purpose);
            Assert.AreEqual("REGULAR", result[1].Type);
            Assert.IsNull(result[1].CurrentStatus);
        
        }

        #endregion

        #region Appointment Type Details

        [Test]
        public void testToAppointmentTypeDetails()
        {
            AppointmentType result = _dao.toAppointmentTypeDetails("RESULT(\"DEFAULT ELIGIBILITY\")=4^COLLATERAL OF VET.\r\nRESULT(\"DESCRIPTION\")=REC(409.1,\"7,\",\"DESCRIPTION\")^REC(409.1,\"7,\",\"DESCRIPTION\")\r\nRESULT(\"DUAL ELIGIBILITY ALLOWED\")=1^YES\r\nRESULT(\"IGNORE MEANS TEST BILLING\")=1^IGNORE\r\nRESULT(\"INACTIVE\")=\r\nRESULT(\"NAME\")=COLLATERAL OF VET.^COLLATERAL OF VET.\r\nRESULT(\"NUMBER\")=7^7\r\nRESULT(\"SYNONYM\")=COV^COV\r\n");

            Assert.IsNotNull(result);
            Assert.AreEqual("7", result.ID);
            Assert.AreEqual("COLLATERAL OF VET.", result.Name);
            Assert.IsTrue(result.Active);
            Assert.AreEqual("REC(409.1,\"7,\",\"DESCRIPTION\")", result.Description);
            Assert.AreEqual("COV", result.Synonym);

        }

        #endregion

    }
}