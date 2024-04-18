using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Keysight.Fusion.Runtime;
using Keysight.Fusion.Logging;
using Keysight.Fusion.Visa;

namespace Fusion_Tests.P2_Tests
{
    [TestFixture]
    public class Histogram : InfiniiVisionTest
    {
        int mTimeout = 20000;
        ScpiError err;
        int mMaxChan;

        const double defaultFreq = 10000000;
        const double defaultAmp = 1.00;

        [SetUp]
        public void Setup()
        {
            mMaxChan = IsScopeFourChan ? 4 : 2;
            mScope.Write("*RST");
            WaitForOpc(ref mScope, mTimeout);
            Waveform wfm = new Waveform(Shape.Sine, defaultFreq, defaultAmp);
            FgensSetWaveform(wfm);
        }


        /// <summary>
        /// :HISTogram:AXIS
        ///:HISTogram:DISPlay
        ///:HISTogram:MEASurement
        ///:HISTogram:MODE
        ///:HISTogram:RESet
        ///:HISTogram:SIZE
        ///:HISTogram:TYPE
        ///:HISTogram:WINDow:BLIMit
        ///:HISTogram:WINDow:LLIMit
        ///:HISTogram:WINDow:RLIMit
        ///:HISTogram:WINDow:SOURce
        ///:HISTogram:WINDow:TLIMit
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Histogram_scpi_test()
        {
            ///:HISTogram:DISPlay
            int[] values = { 0, 1 };
            for (int i = 0; i < values.Length; i++)
            {
                Utils.CmdSend(ref mScope, " :HISTogram:DISPlay", values[i], values[i], "check for the histogram display command");
            }

            // :HISTogram:AXIS
            mScope.Send(":HISTogram:DISPlay 1");
            string[] axis_values = { "VERTical", "VERT", "HORizontal", "HOR" };
            for (int i = 0; i < axis_values.Length; i++)
            {
                Utils.CmdSend_startsWith(ref mScope, ":HISTogram:AXIS", axis_values[i], axis_values[i], "Check for the historgram axis value command");
            }



            //:HISTogram:MODE
            string[] mode_values = { "OFF", "WAV", "WAVeform", "MEASurement", "MEAS" };
            for (int i = 0; i < mode_values.Length; i++)
            {
                if (mode_values[i].StartsWith("MEAS"))
                {
                    mScope.Send(":MEASure:VPP");
                }
                Utils.CmdSend_startsWith(ref mScope, ":HISTogram:MODE", mode_values[i], mode_values[i], "Check for the histogram mode scpi command");
            }



            //:HISTogram:MEASurement
            mScope.Send(":HISTogram:MODE MEAS");
            mScope.Send(":MEASure:QUICK chan1");
            for (int i = 1; i <= 10; i++)
            {
                Utils.CmdSend(ref mScope, ":HISTogram:MEASurement", "MEAS" + i, "MEAS" + i, "Check for the histogram measurement scpi command");
            }


            //:HISTogram:RESet
            mScope.Send("*CLS");
            mScope.Send(":HISTogram:RESet");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the histogram reset scpi command. No error should be reported.");


            //:HISTogram:SIZE
            double[] x = { 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0 };
            mScope.Send(":HISTogram:AXIS VERT");
            for (int i = 0; i < x.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":HISTogram:SIZE", x[i], x[i], "check for the histogram size scpi command");
            }

            mScope.Send(":HISTogram:AXIS HOR");
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] <= 4.0)
                {
                    Utils.CmdSend(ref mScope, ":HISTogram:SIZE", x[i], x[i], "Check for the histogram size scpi command");
                }
            }


            //:HISTogram:TYPE
            string[] type_values = { "VERT", "VERTical", "HOR", "HORizontal", "MEAS", "MEASurement" };
            for (int i = 0; i < type_values.Length; i++)
            {
                if (type_values[i].StartsWith("MEAS"))
                {
                    mScope.Send(":MEASure:VPP");
                }
                Utils.CmdSend_startsWith(ref mScope, ":HISTogram:TYPE", type_values[i], type_values[i], String.Format("Check for the histogram type command- {0}", type_values[i]));
            }


            //:HISTogram:WINDow:BLIMit
            mScope.Send(":HISTogram:MODE WAV");
            for (int i = 0; i < 5; i++)
            {
                double y = Utils.GenrateRandomInRange_Double(-1.15e6, 1.15e6);
                Utils.CmdSend(ref mScope, ":HISTogram:WINDow:BLIMit", y, y, 10, String.Format("Check for the histogram window blimit scpi {0}", x));
            }


            //:HISTogram:WINDow:LLIMit
            for (int i = 0; i < 5; i++)
            {
                double y = Utils.GenrateRandomInRange_Double(-2e-6, 1);
                Utils.CmdSend(ref mScope, ":HISTogram:WINDow:LLIMit", y, y, String.Format("Check for the histogram window llimit scpi {0}", x));
            }


            //:HISTogram:WINDow:RLIMit
            for (int i = 0; i < 5; i++)
            {
                double y = Utils.GenrateRandomInRange_Double(-2e-6, 1);
                Utils.CmdSend(ref mScope, ":HISTogram:WINDow:RLIMit", y, y, String.Format("Check for the histogram window rlimit scpi {0}", x));
            }


            //:HISTogram:WINDow:TLIMit
            for (int i = 0; i < 5; i++)
            {
                double y = Utils.GenrateRandomInRange_Double(-1.15e6, 1.15e6);
                Utils.CmdSend(ref mScope, ":HISTogram:WINDow:TLIMit", y, y, 10, String.Format("Check for the histogram window tlimit scpi {0}", x));
            }
        }
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void HistSource()
        {
            mScope.Send(":HIST:MODE WAV");
            string[] values = { "CHAN1", "CHAN2", "CHAN3", "CHAN4", "FUNC1", "FUNC2", "FUNC3", "FUNC4", "WMEM1", "WMEM2" };
            foreach (string value in values)
            {
                mScope.Send(":" + value + ":DISP 1");
                Utils.CmdSend(ref mScope, ":HISTogram:WINDow:SOURce", value, value, "Check for the histogram source scpi- " + value);
                mScope.Send(":" + value + ":DISP 0");
            }
        }


        
    }
}
