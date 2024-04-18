using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keysight.Fusion.Runtime;
using Keysight.Fusion.Logging;
using Keysight.Fusion.Visa;

namespace Fusion_Tests.P2_Tests
{
    [TestFixture]
    class Lister : InfiniiVisionTest
    {
        [SetUp]
        public void setup()
        {
            mScope.Write("*CLS;*RST");
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void List_scpi()
        {
            mScope.Send(":SBUS1:DISPlay 1");
            mScope.Send(":SBUS2:DISPlay 1");


            //note -check this lister date - riya
            /*
            //:LISTer:DATA
            mScope.Send("*CLS");
            ScpiError err;
            mScope.Send(":LISTer:DATA?");
            Wait.Seconds(4);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in Lister Data scpi");
            */
            //:LISTer:REFerence
            string[] RefValues = { "TRIG", "PREV", "TRIGger", "PREVious" };
            foreach (string value in RefValues)
            {
                Utils.CmdSend_startsWith(ref mScope, ":LISTer:REFerence", value, value, "Check for the list reference values scpi -" + value);
            }

            //:LISTer:DISPlay
            string[] DispValues = { "OFF", "SBUS1", "SBUS2", "ALL" };
            foreach (string value in DispValues)
            {
                Utils.CmdSend(ref mScope, ":LISTer:DISPlay", value, value, "Check for the list display scpi -" + value);
            }
        }
    }
}
