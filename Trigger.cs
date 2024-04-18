using System;
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
    class Trigger : InfiniiVisionTest
    {
        [SetUp]
        public void setup()
        {
            mScope.Write("*CLS");
            mScope.Write("*RST");
            WaitForOpc(ref mScope);
            Waveform wfm = new Waveform(Shape.Sine, 10000000, 1.00);
            FgensSetWaveform(wfm);

            //Enable all channels 
            mScope.Write(":CHAN1:DISP 1");
            mScope.Write(":CHAN2:DISP 1");
            mScope.Write(":CHAN3:DISP 1");
            mScope.Write(":CHAN4:DISP 1");
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Trig()
        {
            mScope.Send("*CLS");
            ScpiError err;
            mScope.Send(":TRIGger?");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in trigger scpi");
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void TriggerPattern()
        {
            mScope.Send(":TRIGger:MODE PATTern");
            //:TRIGger:PATTern:FORMat
            //:TRIGger:PATTern
            string[] formatValues = { "ASCii", "HEX", "ASC" };
            foreach (string value in formatValues)
            {
                Utils.CmdSend_startsWith(ref mScope, ":TRIGger:PATTern:FORMat", value, value, "Check for the trigger format -" + value);
                string[] nums1 = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "X" };
                string[] nums2 = { "0", "1", "X" };
                if (value.Equals("HEX"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        string str = "\"0x";
                        for (int j = 0; j < 5; j++)
                        {
                            int n = Utils.GenrateRandomInRange_Int(0, 16);
                            str = str + nums1[n];
                        }
                        str = str + "\"";
                        str = str + ",NONE,POS";
                        Utils.CmdSend(ref mScope, ":TRIGger:PATTern", str, str, "Check for the trigger pattern scpi" + str);
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        string str = "\"";
                        for (int j = 0; j < 20; j++)
                        {
                            int n = Utils.GenrateRandomInRange_Int(0, 2);
                            str = str + nums2[n];
                        }
                        str = str + "\"";
                        str = str + ",NONE,POS";
                        Utils.CmdSend(ref mScope, ":TRIGger:PATTern", str, str, "check for the trigger pattern scpi -" + str);
                    }
                }
            }


            //note:error in this ---Riya


            //:TRIGger:PATTern:QUALifier
            //:TRIGger:PATTern:GREaterthan
            //:TRIGger:PATTern:LESSthan
            //:TRIGger:PATTern:RANGe
            string[] qualValues = { "ENTered", "GREaterthan", "LESSthan", "INRange", "OUTRange", "TIMeout", "ENT", "GRE", "LESS", "INR", "OUTR", "TIM" };
            foreach (string value in qualValues)
            {
                Utils.CmdSend_startsWith(ref mScope, ":TRIGger:PATTern:QUALifier", value, value, "check for the trigger qualifier -" + value);
                if (value.StartsWith("GRE"))
                {
                    double[] greValues = { 10e-9, 4.3e-6, /*2.8e-3,*/ 9.3e-1, 8.3 };
                    for (int i = 0; i < greValues.Length; i++)
                    {
                        Utils.CmdSend(ref mScope, ":TRIGger:PATTern:GREaterthan", greValues[i], greValues[i], "Check for the trigger greaterthan scpi -" + greValues[i]);
                    }
                }
                else if (value.StartsWith("LESS"))
                {
                    double[] lessValues = { 15e-9, 3.4e-6, 2.8e-4, /*7.6e-2,*/ 2.4 };
                    for (int i = 0; i < lessValues.Length; i++)
                    {
                        Utils.CmdSend(ref mScope, ":TRIGger:PATTern:LESSthan", lessValues[i], lessValues[i], "Check for the trigger lessthan scpi -" + lessValues[i]);
                    }
                }
                    
                else if (value.StartsWith("INR") || value.StartsWith("OUTR"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        double small = Utils.GenrateRandomInRangeDouble(10e-9, 9.9);
                        small = Math.Round(small, 2);
                        double big = Utils.GenrateRandomInRangeDouble(small, 10);
                        big = Math.Round(big, 2);
                        mScope.Send(":TRIGger:PATTern:RANGe " + big + "," + small);
                        string full = mScope.ReadString(":TRIGger:PATTern:RANGe?");
                        string[] f = full.Split(',');
                        double first = Convert.ToDouble(f[0]);
                        double second = Convert.ToDouble(f[1]);
                        Chk.Val(first, big, "Check for the pulsewidth range scpi");
                        Chk.Val(second, small, "Check for the pulsewidth range scpi");
                    }
                }
            }
        }

        //uncomment this when runt scpi's are available -riya
        /*
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
       public void TriggerRunt()
       {
           mScope.Send(":TRIGger:MODE RUNT");
          
           string[] polarityValues = { "NEG", "EITH", "POS", "NEGative", "POSitive", "EITHer" };
           string[] qualValues = { "GRE", "LESS", "NONE", "GREaterthan", "LESSthan" };
           double[] time = { 2e-9, 4e-6, 8.2e-3, 4.8e-1, 7.8 };

           //:TRIGger:RUNT:SOURce
           //:TRIGger:RUNT:POLarity
           //:TRIGger:RUNT:QUALifier
           //:TRIGger:RUNT:TIME
           string[] sourceValues = { "CHAN1", "CHAN2", "CHAN3", "CHAN4" };
           foreach (string val in sourceValues)
           {
               Utils.CmdSend(ref mScope,":TRIGger:RUNT:SOURce",val,val,"Check for the trigger source -"+val);
               foreach (string value in polarityValues)
               {
                   Utils.CmdSend_startsWith(ref mScope, ":TRIGger:RUNT:POLarity", value, value, "check for the trigger polority -" + value);
               }
               foreach (string value in qualValues)
               {
                   Utils.CmdSend_startsWith(ref mScope, ":TRIGger:RUNT:QUALifier", value, value, "Check for the trigger qualifier -" + value);
                   if (value.StartsWith("GRE"))
                   {
                       for (int i = 0; i < time.Length; i++)
                       {
                           Utils.CmdSend(ref mScope, ":TRIGger:RUNT:QUALifier", time[i], time[i], "Check for the trigger time -" + time[i]);
                       }
                   }
                   else if (value.StartsWith("LESS"))
                   {
                       for (int i = 0; i < time.Length; i++)
                       {
                           Utils.CmdSend(ref mScope, ":TRIGger:RUNT:QUALifier", time[i], time[i], "Check for the trigger time -" + time[i]);
                       }
                   }
               }
           }
       }
        */
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void TriggerEBurst()
        {
            mScope.Send(":TRIGger:MODE EBURst");
            string[] slopeValues = { "POS", "NEG", "POSitive", "NEGative" };
            //:TRIGger:EBURst:SOURce
            //:TRIGger:EBURst:COUNt
            //:TRIGger:EBURst:SLOPe
            //:TRIGger:EBURst:IDLE
            string[] sourceValues = { "CHAN", "DIG" };
            foreach (string val in sourceValues)
            {
                if (val == "CHAN")
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        mScope.Send(":chan" + i + ":display 1");
                        string source = val + i;
                        Utils.CmdSend(ref mScope, ":TRIGger:EBURst:SOURce", source, source, "Check for the source scpi -" + source);
                        for (int j = 0; j < 5; j++)
                        {
                            int x = Utils.GenrateRandomInRange_Int(1, 65535);
                            Utils.CmdSend(ref mScope, ":TRIGger:EBURst:COUNt", x, x, "Check for the trigger eburst count scpi -" + x);
                        }
                        foreach (string value in slopeValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:EBURst:SLOPe", value, value, "Check for the eburst slope scpi -" + value);
                        }
                        double[] idleTime = { 10e-9, 4.5e-6, 8.2e-3, 9.3e-1, 3.4, 10 };
                        for (int j = 0; j < idleTime.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":TRIGger:EBURst:IDLE", idleTime[j], idleTime[j], "check for the idle time scpi-" + idleTime[j]);
                        }
                        mScope.Send(":chan" + i + ":display 0");
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        mScope.Send(":DIGital" + i + ":DISPlay 1");
                        string source = val + i;
                        Utils.CmdSend(ref mScope, ":TRIGger:EBURst:SOURce", source, source, "Check for the source scpi -" + source);
                        for (int j = 0; j < 5; j++)
                        {
                            int x = Utils.GenrateRandomInRange_Int(1, 65535);
                            Utils.CmdSend(ref mScope, ":TRIGger:EBURst:COUNt", x, x, "Check for the trigger eburst count scpi -" + x);
                        }
                        foreach (string value in slopeValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:EBURst:SLOPe", value, value, "Check for the eburst slope scpi -" + value);
                        }
                        double[] idleTime = { 10e-9, 4.5e-6, 8.2e-3, 9.3e-1, 3.4, 10 };
                        for (int j = 0; j < idleTime.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":TRIGger:EBURst:IDLE", idleTime[j], idleTime[j], "check for the idle time scpi-" + idleTime[j]);
                        }
                        mScope.Send(":DIGital" + i + ":DISPlay 0");
                    }
                }
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void TriggerEdgeThenEdge()
        {
            mScope.Send(":TRIGger:MODE DELay");
            string[] SlopeValues = { "POS", "NEG", "POSitive", "NEGative" };
            //:TRIGger:DELay:ARM:SOURce 
            //:TRIGger:DELay:TRIGger:SOURce
            //:TRIGger:DELay:ARM:SLOPe
            //:TRIGger:DELay:TRIGger:SLOPe
            //:TRIGger:DELay:TRIGger:COUNt
            //:TRIGger:DELay:TDELay:TIME
            string[] sourceValues = { "CHAN", "DIG" };
            foreach (string val in sourceValues)
            {
                if (val == "CHAN")
                {
                    for (int i = 1; i < 4; i++)
                    {
                        mScope.Send(":chan" + i + ":display 1");
                        string source = val + i;
                        Utils.CmdSend(ref mScope, ":TRIGger:DELay:ARM:SOURce", source, source, "Check for the arm source scpi -" + source);
                        Utils.CmdSend(ref mScope, ":TRIGger:DELay:TRIGger:SOURce", source, source, "Check for the the trigger source -" + source);
                        foreach (string value in SlopeValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:DELay:ARM:SLOPe", value, value, "Check for the arm slope scpi -" + value);
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:DELay:TRIGger:SLOPe", value, value, "Check for the trigger slope scpi -" + value);
                        }

                        for (int j = 0; j < 5; j++)
                        {
                            int x = Utils.GenrateRandomInRange_Int(1, 65535);
                            Utils.CmdSend(ref mScope, ":TRIGger:DELay:TRIGger:COUNt", x, x, "Check for the count scpi -" + x);
                        }
                        double[] time = { 4e-9, 5.3e-6, 7.8e-3, 8.2e-2, 9.1e-1, 5.3, 10 };
                        for (int j = 0; j < time.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":TRIGger:DELay:TDELay:TIME", time[j], time[j], "check for the edge time scpi-" + time[j]);
                        }
                        mScope.Send(":chan" + i + ":display 0");
                    }
                }
                else
                {
                    for (int i = 0; i < 15; i++)
                    {
                        mScope.Send(":DIGital" + i + ":DISPlay 1");
                        string source = val + i;
                        Utils.CmdSend(ref mScope, ":TRIGger:DELay:ARM:SOURce", source, source, "Check for the arm source scpi -" + source);
                        Utils.CmdSend(ref mScope, ":TRIGger:DELay:TRIGger:SOURce", source, source, "Check for the the trigger source -" + source);
                        foreach (string value in SlopeValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:DELay:ARM:SLOPe", value, value, "Check for the arm slope scpi -" + value);
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:DELay:TRIGger:SLOPe", value, value, "Check for the trigger slope scpi -" + value);
                        }
                        for (int j = 0; j < 5; j++)
                        {
                            int x = Utils.GenrateRandomInRange_Int(1, 65535);
                            Utils.CmdSend(ref mScope, ":TRIGger:DELay:TRIGger:COUNt", x, x, "Check for the count scpi -" + x);
                        }
                        double[] time = { 4e-9, 5.3e-6, 7.8e-3, 8.2e-2, 9.1e-1, 5.3, 10 };
                        for (int j = 0; j < time.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":TRIGger:DELay:TDELay:TIME", time[j], time[j], "check for the edge time scpi-" + time[j]);
                        }
                        mScope.Send(":DIGital" + i + ":DISPlay 0");
                    }
                }
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void TriggerPulseWidth()
        {
            mScope.Send(":TRIGger:MODE GLITch");
            string[] polValues = { "POS", "NEG", "POSitive", "NEGative" };
            string[] QualValues = { "GRE", "LESS", "RANG", "GREaterthan", "LESSthan", "RANGe" };


            //:TRIGger:GLITch:SOURce
            //:TRIGger:GLITch:POLarity
            //:TRIGger:GLITch:QUALifier
            //:TRIGger:GLITch:GREaterthan
            //:TRIGger:GLITch:LESSthan
            //:TRIGger:GLITch:RANGe
            //:TRIGger:GLITch:LEVel
            string[] sourceValues = { "CHAN", "DIG" };
            foreach (string val in sourceValues)
            {
                if (val == "CHAN")
                {
                    for (int i = 1; i < 4; i++)
                    {
                        mScope.Send(":chan" + i + ":display 1");
                        string source = val + i;
                        Utils.CmdSend(ref mScope, ":TRIGger:GLITch:SOURce", source, source, "Check for the pulse width source scpi -" + source);
                        foreach (string value in polValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:GLITch:POLarity", value, value, "Check for the pulse width polarity -" + value);
                        }
                        foreach (string value in QualValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:GLITch:QUALifier", value, value, "Check for the pulse width qualifier -" + value);
                            if (value.StartsWith("GRE"))
                            {
                                double[] greValues = { 2e-9, 4.3e-6, 8.4e-3, 0.32, 5.6 };
                                for (int j = 0; j < greValues.Length; j++)
                                {
                                    Utils.CmdSend(ref mScope, ":TRIGger:GLITch:GREaterthan", greValues[j], greValues[j], "Check for the pulse width greaterthan scpi -" + greValues[j]);
                                }
                            }
                            else if (value.StartsWith("LESS"))
                            {
                                double[] lessValues = { 2e-9, 4.3e-6, 8.4e-3, 0.32, 5.6 };
                                for (int j = 0; j < lessValues.Length; j++)
                                {
                                    Utils.CmdSend(ref mScope, ":TRIGger:GLITch:LESSthan", lessValues[j], lessValues[j], "Check for the pulse width lessthan scpi -" + lessValues[j]);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    double small = Utils.GenrateRandomInRangeDouble(10e-9, 9.9);
                                    small = Math.Round(small, 2);
                                    double big = Utils.GenrateRandomInRangeDouble(small, 10);
                                    big = Math.Round(big, 2);
                                    mScope.Send(":TRIGger:GLITch:RANGe " + big + "," + small);
                                    string full = mScope.ReadString(":TRIGger:GLITch:RANGe?");
                                    string[] f = full.Split(',');
                                    double first = Convert.ToDouble(f[0]);
                                    double second = Convert.ToDouble(f[1]);
                                    Chk.Val(first, big, "Check for the pulsewidth range scpi");
                                    Chk.Val(second, small, "Check for the pulsewidth range scpi");
                                }
                            }
                        }
                        double[] level = { 0.1, -0.1, 0.2, -0.2, 0.3, -0.3, 0.4, -0.4 };
                        for (int j = 0; j < level.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":TRIGger:GLITch:LEVel", level[j], level[j], "Check for the pulse width level scpi -" + level[j]);
                        }
                        mScope.Send(":chan" + i + ":display 0");
                    }
                }
                else
                {
                    for (int i = 0; i < 15; i++)
                    {
                        mScope.Send(":DIGital" + i + ":DISPlay 1");
                        string source = val + i;
                        Utils.CmdSend(ref mScope, ":TRIGger:GLITch:SOURce", source, source, "Check for the pulse width source scpi -" + source);
                        foreach (string value in polValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:GLITch:POLarity", value, value, "Check for the pulse width polarity -" + value);
                        }
                        foreach (string value in QualValues)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":TRIGger:GLITch:QUALifier", value, value, "Check for the pulse width qualifier -" + value);
                            if (value.StartsWith("GRE"))
                            {
                                double[] greValues = { 5e-9, 4.3e-6, 8.4e-3, 0.32, 5.6 };
                                for (int j = 0; j < greValues.Length; j++)
                                {
                                    Utils.CmdSend(ref mScope, ":TRIGger:GLITch:GREaterthan", greValues[j], greValues[j], "Check for the pulse width greaterthan scpi -" + greValues[j]);
                                }
                            }
                            else if (value.StartsWith("LESS"))
                            {
                                double[] lessValues = { 5e-9, 4.3e-6, 8.4e-3, 0.32, 5.6 };
                                for (int j = 0; j < lessValues.Length; j++)
                                {
                                    Utils.CmdSend(ref mScope, ":TRIGger:GLITch:LESSthan", lessValues[j], lessValues[j], "Check for the pulse width lessthan scpi -" + lessValues[j]);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    double small = Utils.GenrateRandomInRangeDouble(10e-9, 9.9);
                                    small = Math.Round(small, 2);
                                    double big = Utils.GenrateRandomInRangeDouble(small, 10);
                                    big = Math.Round(big, 2);
                                    mScope.Send(":TRIGger:GLITch:RANGe " + big + "," + small);
                                    string full = mScope.ReadString(":TRIGger:GLITch:RANGe?");
                                    string[] f = full.Split(',');
                                    double first = Convert.ToDouble(f[0]);
                                    double second = Convert.ToDouble(f[1]);
                                    Chk.Val(first, big, "Check for the pulsewidth range scpi");
                                    Chk.Val(second, small, "Check for the pulsewidth range scpi");
                                }
                            }
                        }
                        double[] level = { 0.1, -0.1, 0.2, -0.2, 0.3, -0.3, 0.4, -0.4 };
                        for (int j = 0; j < level.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":TRIGger:GLITch:LEVel", level[j], level[j], "Check for the pulse width level scpi -" + level[j]);
                        }
                        mScope.Send(":DIGital" + i + ":DISPlay 0");
                    }
                }
            }

            //:TRIGger:FORCe
            ScpiError err;
            mScope.Send("*CLS");
            mScope.Send(":TRIGger:FORCe");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in trigger force scpi");
        }
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void holdOff()
        {
            //:TRIGger:HOLDoff:RANDom
            Utils.CmdSend(ref mScope, ":TRIGger:HOLDoff:RANDom", 1, 1, "Check for the holdoff random");
            Utils.CmdSend(ref mScope, ":TRIGger:HOLDoff:RANDom", 0, 0, "Check for the holdoff random");
            Utils.CmdSend(ref mScope, ":TRIGger:HOLDoff:RANDom", 1, 1, "Check for the holdoff random");

            //:TRIGger:HOLDoff:MINimum
            //:TRIGger:HOLDoff:MAXimum
            double[] minValues = { 40e-9, 8.3e-6, 9.2e-3, 4.9e-1, 3.2, 9.8 };
            double[] maxValues = { 45e-9, 10e-8, 8.3e-6, 5.4e-3, 0.23, 3.12, 10 };
            for (int i = 0; i < minValues.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":TRIGger:HOLDoff:MINimum", minValues[i], minValues[i], "Check for the trigger holdoff minimum -" + minValues[i]);
            }
            for (int i = 0; i < maxValues.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":TRIGger:HOLDoff:MAXimum", maxValues[i], maxValues[i], "Check for the trigger holdoff maximum -" + maxValues[i]);
            }

            //:TRIGger:JFRee
            Utils.CmdSend(ref mScope, ":TRIGger:JFRee", 1, 1, "Check for the jitter free scpi");
            Utils.CmdSend(ref mScope, ":TRIGger:JFRee", 0, 0, "Check for the jitter free scpi");
            Utils.CmdSend(ref mScope, ":TRIGger:JFRee", 1, 1, "Check for the jitter free scpi");

            //:TRIGger:LEVel:ASETup
            ScpiError err;
            mScope.Send("*CLS");
            mScope.Send(":TRIGger:LEVel:ASETup");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in trigger level setup");
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void TriggerZone()
        {
            ScpiError err;
            //:TRIGger:ZONE<n>:STATe
            //:TRIGger:ZONE<n>:MODE
            //:TRIGger:ZONE<n>:VALidity?
            string[] modes = { "INT", "NOT" };
            for (int i = 1; i <= 2; i++)
            {
                string state = ":TRIGger:ZONE" + i + ":STATe";
                Utils.CmdSend(ref mScope, state, 1, 1, "check for the trigger zone state scpi");
                Utils.CmdSend(ref mScope, state, 0, 0, "check for the trigger zone state scpi");
                Utils.CmdSend(ref mScope, state, 1, 1, "check for the trigger zone state scpi");
                string command = ":TRIGger:ZONE" + i + ":MODE";
                for (int j = 0; j < modes.Length; j++)
                {
                    Utils.CmdSend(ref mScope, command, modes[j], modes[j], "Check for the trigger zone mode scpi");
                }
                string valid = ":TRIGger:ZONE" + i + ":VALidity?";
                mScope.Send("*CLS");
                mScope.Send(valid);
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in zone validity");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void TriggerOr()
        {
            //:TRIGger:OR
            mScope.Send(":TRIGger:MODE OR");
            string[] values = { "R", "F", "E", "X" };
            for (int i = 0; i < 5; i++)
            {
                string str = "\"";
                for (int j = 0; j < 20; j++)
                {
                    str = str + values[Utils.GenrateRandomInRange_Int(0, 3)];
                }
                str = str + "\"";
                Utils.CmdSend(ref mScope, ":TRIGger:OR", str, str, "Check for the trigger or sci -" + str);
            }
        }
        
    }
}
