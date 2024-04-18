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
    class HawkDefects : InfiniiVisionTest
    {
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void INV6038()
        {
            double frequency = 2e6; // max freq can 20e6.
            double vpp = 4;
            double tol = 0.016;
            Waveform wfm = new Waveform(Shape.Sine, frequency, vpp);
            FgensSetWaveform(wfm);
            Wait.MilliSeconds(500);

            mScope.Send("*RST");
            WaitForOpc(ref mScope, 20000);
            mScope.Send(":AUToscale:CHANnels DISP");
            for (int i = 1; i <= 4; i++)
            {
                mScope.Send(":CHANnel" + i + ":DISPlay 1");
                mScope.Send(":STOP");
                mScope.Send(":DISPlay:CLEar");
                mScope.Send(":SINGle");
                mScope.Send(":AUToscale");
                Wait.MilliSeconds(3000);

                double scale = mScope.ReadNumberAsDouble(":CHANnel" + i + ":SCALe?");
                Pass.Condition(Is.Between(scale, vpp / 6, vpp / 2), "Autoscale working fine after single aquasition -vert");  //check for the vertical scaling

                double hrange = mScope.ReadNumberAsDouble(":TIMebase:MAIN:RANGe?");
                double req = 2 / frequency;
                Chk.Val(hrange, req, "Autoscale working fine after single aquasition -hor");  //check for the horizontal scaling.

                mScope.Send(":MEASure:FREQuency CHAN" + i);
                double freq = mScope.ReadNumberAsDouble(":MEASure:FREQuency?");
                Chk.Val(freq, frequency, frequency * tol, "Autoscale working fine after single aquasition -freq");  //check for frequency measurement after autoscale;

                mScope.Send(":CHANnel" + i + ":DISPlay 0");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void INV6091()
        {
            double frequency = 20e6;
            double vpp = 1;
            double tol = 0.016;  //tolerance is 1.6%
            Waveform wfm = new Waveform(Shape.Sine, frequency, vpp);
            FgensSetWaveform(wfm);
            Wait.MilliSeconds(500);

            mScope.Send("*RST");
            WaitForOpc(ref mScope, 20000);
            mScope.Send(":AUToscale:CHANnels DISP");
            for (int i = 1; i <= 4; i++)
            {
                mScope.Send(":CHANnel" + i + ":DISPlay 1");
                mScope.Send(":ACQuire:MODE SEGM");
                Wait.MilliSeconds(1000);
                mScope.Send(":AUToscale");
                Wait.MilliSeconds(3000);
                string mode = mScope.ReadString(":ACQuire:MODE?");
                Chk.Val(mode, "RTIM", "check for the segmented turn off.");
                double scale = mScope.ReadNumberAsDouble(":CHANnel" + i + ":SCALe?");
                Pass.Condition(Is.Between(scale, vpp / 6, vpp / 2), "Autoscale working fine after channel scaling -vert");  //check for the vertical scaling

                double hrange = mScope.ReadNumberAsDouble(":TIMebase:MAIN:RANGe?");
                double req = 2 / frequency;
                Chk.Val(hrange, req, "Autoscale working fine after channel scaling -hor");  //check for the horizontal scaling.

                mScope.Send(":MEASure:FREQuency CHAN" + i);
                double freq = mScope.ReadNumberAsDouble(":MEASure:FREQuency?");
                Chk.Val(freq, frequency, frequency * tol, "Autoscale working fine after channel scaling -freq");  //check for frequency measurement after autoscale;

                mScope.Send(":CHANnel" + i + ":DISPlay 0");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void INV7030()
        {
            double frequency = 20e6;
            double vpp = 0.8;
            double tol = 0.016;  //tolerance is 1.6%
            Waveform wfm = new Waveform(Shape.Sine, frequency, vpp);
            FgensSetWaveform(wfm);
            Wait.MilliSeconds(500);

            mScope.Send("*RST");
            WaitForOpc(ref mScope, 20000);
            mScope.Send(":AUToscale:CHANnels DISP");
            for (int i = 1; i <= 4; i++)
            {
                mScope.Send(":CHANnel" + i + ":DISPlay 1");
                double num = Utils.GenrateRandomInRangeDouble(1, 200);
                num = Math.Round(num, 0);
                mScope.Send(":CHANnel" + i + ":SCALe " + num + "mV");
                mScope.Send(":AUToscale");
                Wait.MilliSeconds(3000);
                double scale = mScope.ReadNumberAsDouble(":CHANnel" + i + ":SCALe?");
                Pass.Condition(Is.Between(scale, vpp / 6, vpp / 2), "Autoscale working fine after channel scaling -vert");  //check for the vertical scaling
                
                double hrange = mScope.ReadNumberAsDouble(":TIMebase:MAIN:RANGe?");
                double req = 2 / frequency;
                Chk.Val(hrange, req, "Autoscale working fine after channel scaling -hor");  //check for the horizontal scaling.

                mScope.Send(":MEASure:FREQuency CHAN" + i);
                double freq = mScope.ReadNumberAsDouble(":MEASure:FREQuency?");
                Chk.Val(freq, frequency, frequency * tol, "Autoscale working fine after channel scaling -freq");  //check for frequency measurement after autoscale;

                mScope.Send(":CHANnel" + i + ":DISPlay 0");
            }
        }

    }
}
