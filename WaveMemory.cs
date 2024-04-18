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
    class WaveMemory : InfiniiVisionTest
    {
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void WaveformMemory()
        {
            /*
            :WMEMory<r>:CLEar
            :WMEMory<r>:COMPare? -not available
            :WMEMory<r>:DISPlay
            :WMEMory<r>:LABel
            :WMEMory<r>:SAVE
            :WMEMory<r>:SKEW
            :WMEMory<r>:YOFFset
            :WMEMory<r>:YRANge
            :WMEMory<r>:YSCale
             * */
            for (int i = 1; i <= 2; i++)
            {
                //:WMEMory<r>:DISPlay
                string dispCmd = ":WMEMory" + i + ":DISPlay ";
                mScope.Send(dispCmd + "1");

                //:WMEMory<r>:YOFFset
                string offCmd = ":WMEMory" + i + ":YOFFset";
                double[] offset = { -4e15, 2.345e15, 12e12, 3.2442e9, 4e15, 0, 3.234e6 };
                for (int j = 0; j < offset.Length; j++)
                {
                    Utils.CmdSend(ref mScope, offCmd, offset[j], offset[j], "Check for the offset scpi command -" + offset[j]);
                }


                //:WMEMory<r>:YSCale
                //:WMEMory<r>:YRANge
                string scaleCmd = ":WMEMory" + i + ":YSCale";
                string rangeCmd = ":WMEMory" + i + ":YRANge";
                double[] scaleValue = { 8e-15, 1e+15, 2.3e12, 5.6e9, 1.4e6, 9.8e3, 3.2e-9, 9 };
                for (int j = 0; j < scaleValue.Length; j++)
                {
                    mScope.Send(scaleCmd + " " + scaleValue[j]);
                    double resultScale = mScope.ReadNumberAsDouble(scaleCmd + "?");
                    Chk.Val(scaleValue[j], resultScale, "Check for the resultScale scpi -" + resultScale);
                    mScope.Send(rangeCmd + " " + scaleValue[j] * 8);
                    double yrange = mScope.ReadNumberAsDouble(rangeCmd + "?");
                    Chk.Val(scaleValue[j] * 8, yrange, "Check for the range scpi -" + yrange);
                }


                //:WMEMory<r>:SKEW
                string skewCmd = ":WMEMory" + i + ":SKEW";
                for (int j = 0; j < 4; j++)
                {
                    double skew = Utils.GenrateRandomInRange_Double(-1, 1);
                    Utils.CmdSend(ref mScope, skewCmd, skew, skew, "Check for the skew scpi -" + skew);
                }

                //:WMEMory<r>:LABel
                string lab = ":WMEMory" + i + ":LABel";
                string[] labels = { "abc", "uieyar;wesb8iauydsfaHdf;a" };
                foreach (string label in labels)
                {
                    mScope.Send(lab + " \"" + label + "\"");
                    string res = mScope.ReadString(lab + "?");
                    Chk.Val("\"" + label + "\"", res, "Check for the label scpi-" + label);
                }
                //:WMEMory<r>:SAVE
                //:WMEMory<r>:CLEar
                ScpiError err;
                string save = ":WMEMory" + i + ":SAVE";
                string clear = ":WMEMory" + i + ":CLEar";
                string[] sources = { "CHAN1", "CHAN2", "CHAN3", "CHAN4", "FUNC1", "FUNC2", "FUNC3", "FUNC4" };
                foreach (string source in sources)
                {
                    mScope.Send(":" + source + ":DISP 1");
                    mScope.Send("*CLS");
                    mScope.Send(save + " " + source);
                    err = mScope.ReadError();
                    Chk.Val(err.ErrorCode, 0, "Check for error in save scpi");
                    mScope.Send("*CLS");
                    mScope.Send(clear);
                    err = mScope.ReadError();
                    Chk.Val(err.ErrorCode, 0, "Check for erro in clear scpi");
                    mScope.Send(":" + source + ":DISP 0");
                }


            }
        }
    }
}
