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
    class Search : InfiniiVisionTest
    {
        [SetUp]
        public void setUp()
        {
            mScope.Write("*RST");
            WaitForOpc(ref mScope, 20000);
            mScope.Send(":CHAN1:DISP 1");
            mScope.Send(":CHAN2:DISP 1");
            mScope.Send(":CHAN3:DISP 1");
            mScope.Send(":CHAN4:DISP 1");
            Waveform wfm = new Waveform(Shape.Sine, 10000000, 1.00);
            FgensSetWaveform(wfm);
        }


        //modified --add this
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Search_scpi()
        {
            //:SEARch:STATe
            Utils.CmdSend(ref mScope, ":SEARch:STATe", 1, 1, "Check for the search state scpi");
            Utils.CmdSend(ref mScope, ":SEARch:STATe", 0, 0, "Check for the search state scpi");
            Utils.CmdSend(ref mScope, ":SEARch:STATe", 1, 1, "Check for the search state scpi");

            //:SEARch:MODE
            string[] modeValues = { "EDGE", "GLIT", "RUNT", "TRAN", "SER1", "SER2", "PEAK" };
            foreach (string value in modeValues)
            {
                Utils.CmdSend(ref mScope, ":SEARch:MODE", value, value, "Check for the search mode scpi");
                // //:SEARch:COUNt
                if (value.Equals("EDGE"))
                {
                    mScope.Send(":MEASure:FREQuency CHAN1");
                    mScope.Send(":AUToscale:CHANnels DISP");
                    mScope.Send(":AUToscale");
                    Wait.Seconds(3);
                    mScope.Send(":SEARch:EDGE:SLOPe NEG");
                    int count = mScope.ReadNumberAsInt32(":SEARch:COUNt?");
                    Chk.Val(count, 2, "check for th serach count scpi");
                    for (int i = 1; i <= count; i++)
                    {
                        Utils.CmdSend(ref mScope, ":SEARch:EVENt", i, i, "check for the search event scpi");
                    }
                }
            }

            

        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchEdge_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SEARch:MODE EDGE");
            //:SEARch:EDGE:SOURce
            //:SEARch:EDGE:SLOPe
            string[] slopeValues = { "NEG", "POS", "EITH", "NEGative", "POSitive", "EITHer" };
            for (int i = 1; i <= 4; i++)
            {
                string val = "CHAN" + i;
                Utils.CmdSend(ref mScope, ":SEARch:EDGE:SOURce", val, val, "Check for the search edge source scpi -"+val);
                foreach (string value in slopeValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:EDGE:SLOPe", value, value, "Check for the edge slope scpi");
                }
            }
        }



        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchGlitch_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            //:SEARch:GLITch:SOURce
            //:SEARch:GLITch:POLarity
            //:SEARch:GLITch:QUALifier
            //:SEARch:GLITch:GREaterthan
            //:SEARch:GLITch:LESSthan
            //:SEARch:GLITch:RANGe
            string[] slopeValues = { "POS", "NEG", "POSitive", "NEGative" };
            string[] qualifierValues = { "GREaterthan", "LESSthan", "RANGe", "GRE", "LESS", "RANG" };
            double[] greValues = { 2e-9, 8.3e-6, 4.2e-3, 0.23, 8.3, 10};
            double[] lessValues = { 2e-9, 4.2e-6, 2.3e-3, 0.88, 4.2, 10 };
            mScope.Send(":SEARch:MODE GLIT");
            for (int i = 1; i <= 4; i++)
            {
                string val = "CHAN" + i;
                Utils.CmdSend(ref mScope, ":SEARch:GLITch:SOURce", val, val, "Check for the search glitch source scpi");
                foreach (string value in slopeValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:GLITch:POLarity", value, value, "Check for the glitch polority scpi- "+value);
                }
                foreach (string value in qualifierValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:GLITch:QUALifier", value, value, "Check for the glitch qual scpi -" + value);
                    if (value.StartsWith("GRE"))
                    {
                        for (int j = 0; j < greValues.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SEARch:GLITch:GREaterthan", greValues[j], greValues[j], "Check for the glitch greaterthan scpi");
                        }
                    }
                    else if(value.StartsWith("LESS"))
                    {
                        for (int j = 0; j < lessValues.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SEARch:GLITch:LESSthan", lessValues[j], lessValues[j], "Check for the glitch lessthan scpi");
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
                            mScope.Send(":SEARch:GLITch:RANGe " + big + "," + small);
                            string full = mScope.ReadString(":SEARch:GLITch:RANGe?");
                            string[] f = full.Split(',');
                            double first = Convert.ToDouble(f[0]);
                            double second = Convert.ToDouble(f[1]);
                            Chk.Val(first, big, "Check for the pulsewidth range scpi");
                            Chk.Val(second, small, "Check for the pulsewidth range scpi");
                        }
                    }
                }
            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchTransition_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SEARch:MODE TRAN");
            //:SEARch:TRANsition:SOURce
            //:SEARch:TRANsition:SLOpe
            //:SEARch:TRANsition:QUALifier
            //:SEARch:TRANsition:TIME
            string[] slopeValues = { "POS", "NEG", "POSitive", "NEGative" };
            string[] qualValues={"GRE", "LESS", "GREaterthan", "LESSthan"};
             double[] greValues = { 1e-9, 8.3e-6, 4.2e-3, 0.23, 8.3, 10 };
             double[] lessValues = { 1e-9, 4.2e-6, 2.3e-3, 0.88, 4.2, 10 };
            for (int i = 1; i <= 4; i++)
            {
                string str = "CHAN" + i;
                Utils.CmdSend(ref mScope, ":SEARch:TRANsition:SOURce", str, str, "Check for the transition source scpi -" + str);
                foreach (string value in slopeValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:TRANsition:SLOpe", value, value, "Check for the transition slope scpi -" + value);
                }
                foreach (string value in qualValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:TRANsition:QUALifier", value, value, "Check for the transition qualifier scpi -" + value);
                    if (value.StartsWith("GRE"))
                    {
                        for (int j = 0; j < greValues.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SEARch:TRANsition:TIME", greValues[j], greValues[j], "Check for the transition time scpi -" + greValues[j]);
                        }
                    }
                    else if (value.StartsWith("LESS"))
                    {
                        for (int j = 0; j < lessValues.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SEARch:TRANsition:TIME", lessValues[j], lessValues[j], "check for the transition time scpi -" + lessValues[j]);
                        }
                    }
                }
            }
        }



        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
         public void SearchPeak_scpi()
         {
             mScope.Send(":SEARch:STATe 1");
             //:SEARch:PEAK:SOURce
             //:SEARch:PEAK:NPEaks
             //:SEARch:PEAK:THReshold
             mScope.Send(":FFT:DISPlay 1");
             mScope.Send(":SEARch:MODE PEAK");
             Utils.CmdSend(ref mScope, ":SEARch:PEAK:SOURce", "FFT", "FFT", "Check for the peak source scpi -fft");
             for (int i = 1; i <= 4; i++)
             {
                 string disp = ":FUNC" + i + ":DISP ";
                 mScope.Send(disp + "1");
                 mScope.Send(":FUNC" + i + ":OPER FFT");
                 Utils.CmdSend(ref mScope, ":SEARch:PEAK:SOURce", "FUNC" + i, "FUNC" + i, "Check for the peak source scpi");
                 for (int j = 0; j < 5; j++)
                 {
                     int num = Utils.GenrateRandomInRange_Int(1, 15);
                     Utils.CmdSend(ref mScope, ":SEARch:PEAK:NPEaks", num, num, "Check for the npeak scpi");
                 }
                 mScope.Send(disp + "0");
             }
         }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchRunt_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
             mScope.Send(":SEARch:MODE RUNT");
            //:SEARch:RUNT:SOURce
            //:SEARch:RUNT:POLarity
            //:SEARch:RUNT:QUALifier
            //:SEARch:RUNT:TIME
             string[] RuntPolarity = { "POSitive", "NEGative", "EITHer", "POS", "NEG", "EITH" };
             string[] QualValues = { "GRE", "LESS", "NONE", "GREaterthan", "LESSthan" };
             double[] greValues = { 2e-9, 8.3e-6, 4.2e-3, 0.23, 8.3, 10 };
             double[] lessValues = { 2e-9, 4.2e-6, 2.3e-3, 0.88, 4.2, 10 };
            for (int i = 1; i <= 4; i++)
            {
                string val = "CHAN" + i;
                Utils.CmdSend(ref mScope, ":SEARch:RUNT:SOURce", val, val, "Check for the runt source");
                foreach (string value in RuntPolarity)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:RUNT:POLarity", value, value, "Check for the runt polarity scpi - "+value);
                }
                foreach (string value in QualValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:RUNT:QUALifier", value, value, "Check for the runt qual scpi -" + value);
                    if (value.StartsWith("GRE"))
                    {
                        for (int j = 0; j < greValues.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SEARch:RUNT:TIME", greValues[j], greValues[j], "Check for the runt time scpi -" + greValues[j]);
                        }
                    }
                    else if (value.StartsWith("LESS"))
                    {
                        for (int j = 0; j < lessValues.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SEARch:RUNT:TIME", lessValues[j], lessValues[j], "Check for the runt time scpi -" + lessValues[j]);
                        }
                    }
                }
            }
        }

        //uncomment = when serial protocol-2 is available 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchSerialCAN_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SBUS1:DISPlay 1");
            mScope.Send(":SBUS2:DISPlay 1");
            string[] can_mode = { "IDE", "IDD", "DATA", "IDR", "ERR", "ACK", "FORM", "STUF", "CRC", "ALL", "OVER", /*"MESS", "MISG",*/ "IDEither", "IDData", "IDRemote", "ERRor", "ACKerror", "FORMerror", "STUFferror", "CRCerror", "ALLerrors", "OVERload"/*, "MESSage", "MSIGnal" */};
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":LISTer:DISPlay SBUS" + i);
                mScope.Send(":SEARch:MODE SER"+i);
                mScope.Send(":SBUS" + i + ":MODE CAN");
                string[] values = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "X" };
                foreach (string mode in can_mode)
                {
                    //:SEARch:SERial:CAN:MODE
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:CAN:MODE", mode, mode, "Check for the serial can mode -" + mode);
                    if (mode.Equals("DATA"))
                    {
                        
                        for (int j = 0; j < 4; j++)
                        {
                            //:SEARch:SERial:CAN:PATTern:DATA:LENGth
                            int num = Utils.GenrateRandomInRange_Int(1, 8);
                            Utils.CmdSend(ref mScope, ":SEARch:SERial:CAN:PATTern:DATA:LENGth", num, num, "Check for the can data length scpi -" + num);
                            string data = "0x";
                            for (int k = 0; k < 2 * num; k++)
                            {
                                data = data + values[Utils.GenrateRandomInRange_Int(0, 15)];
                            }
                            /*
                            //:SEARch:SERial:CAN:PATTern:DATA
                            string d="\"" + data + "\"";
                            mScope.Send(":SEARch:SERial:CAN:PATTern:DATA " + d);
                            string res = mScope.ReadString(":SEARch:SERial:CAN:PATTern:DATA?");
                            Chk.Val(res, d, "Check for the pattern data- " + d);
                             */
                        }
                    }
                    if (mode.Equals("DATA") || mode.Equals("ID") || mode.Equals("IDE") || mode.Equals("IDR"))
                    {
                        //:SEARch:SERial:CAN:PATTern:ID:MODE
                        string[] mode_values = { "STAN", "EXT", "STANdard", "EXTended" };
                        foreach (string val in mode_values)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:CAN:PATTern:ID:MODE", val, val, "Check for the can pattern id mode- " + val);
                            //:SEARch:SERial:CAN:PATTern:ID
                            if (val.StartsWith("STAN"))
                            {
                                string id_value = "0x";
                                for (int j = 0; j < 3; j++)
                                {
                                    if (j == 0)
                                    {
                                        id_value = id_value + values[Utils.GenrateRandomInRange_Int(0, 7)];
                                    }
                                    else
                                    {
                                        id_value = id_value + values[Utils.GenrateRandomInRange_Int(0, 15)];
                                    }
                                }
                                string id = "\"" + id_value + "\"";
                                mScope.Send(":SEARch:SERial:CAN:PATTern:ID " + id);
                                string r = mScope.ReadString(":SEARch:SERial:CAN:PATTern:ID?");
                                Chk.Val(r, id, "Check for the pattern id -" + id);
                            }
                            else
                            {
                                string id_value = "0x";
                                for (int j = 0; j < 8; j++)
                                {
                                    if (j == 0)
                                    {
                                        id_value = id_value + values[Utils.GenrateRandomInRange_Int(0, 1)];
                                    }
                                    else
                                    {
                                        id_value = id_value + values[Utils.GenrateRandomInRange_Int(0, 15)];
                                    }
                                    
                                }
                                string id = "\"" + id_value + "\"";
                                mScope.Send(":SEARch:SERial:CAN:PATTern:ID " + id);
                                string r = mScope.ReadString(":SEARch:SERial:CAN:PATTern:ID?");
                                Chk.Val(r, id, "Check for the pattern id -" + id);
                            }
                        }
                    }
                }
            }

        }

        //uncomment = when serial protocol-2 is available 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void searchSerialIIC_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SBUS1:DISPlay 1");
            mScope.Send(":SBUS2:DISPlay 1");
            for (int j = 1; j </*=*/ 2; j++)
            {
                mScope.Send(":SEARch:MODE SER" + j);
                mScope.Send(":SBUS" + j + ":MODE IIC");
                mScope.Send(":LISTer:DISPlay SBUS" + j);
                //:SEARch:SERial:IIC:MODE
                string[] modes = { "REST", "ADDR", "ANAC", "NACK", "READE", "READ7", "WRIT7", "R7D2", "W7D2" };
                foreach (string mode in modes)
                {
                    Utils.CmdSend(ref mScope, ":SEARch:SERial:IIC:MODE", mode, mode, "Check for the i2c mode -" + mode);
                    if (mode.Equals("ADDR") || mode.Equals("ANAC") || mode.Equals("READ7") || mode.Equals("WRIT7") || mode.Equals("R7D2") || mode.Equals("W7D2"))
                    {
                        //:SEARch:SERial:IIC:PATTern:ADDRess
                        int add = Utils.GenrateRandomInRange_Int(0, 127);
                        Utils.CmdSend(ref mScope, ":SEARch:SERial:IIC:PATTern:ADDRess", add, add, "Check for the address scpi-" + add);
                        
                    }

                    //:SEARch:SERial:IIC:PATTern:DATA
                    //:SEARch:SERial:IIC:PATTern:DATA2
                    if (mode.Equals("READ7") || mode.Equals("WRIT7") || mode.Equals("R7D2") || mode.Equals("W7D2"))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int data1 = Utils.GenrateRandomInRange_Int(0, 255);
                            if (mode.Equals("R7D2") || mode.Equals("W7D2"))
                            {
                                Utils.CmdSend(ref mScope, ":SEARch:SERial:IIC:PATTern:DATA2", data1, data1, "Check for the data scpi -" + data1);
                            }
                            int data2 = Utils.GenrateRandomInRange_Int(0, 255);
                            Utils.CmdSend(ref mScope, ":SEARch:SERial:IIC:PATTern:DATA", data2, data2, "Check for the data scpi -" + data2);
                        }
                    }

                    //:SEARch:SERial:IIC:QUALifier
                    if (mode.Equals("READE"))
                    {
                        string[] Qualifiers = { "EQU", "NOT", "LESS", "GRE", "EQUal", "NOTequal", "LESSthan", "GREaterthan" };
                        foreach (string qual in Qualifiers)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:IIC:QUALifier", qual, qual, "Check for qualifier -" + qual);
                        }
                    }
                    
                }
            }
        }

        //uncomment = when serial protocol-2 is available 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchSerialLIN_Scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SBUS1:DISPlay 1");
            mScope.Send(":SBUS2:DISPlay 1");
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SEARch:MODE SER" + i);
                mScope.Send(":SBUS" + i + ":MODE LIN");
                mScope.Send(":LISTer:DISPlay SBUS" + i);
                //:SEARch:SERial:LIN:MODE
                string[] modes = { "ID", "DATA", "ERRor",/* "FRAMe", "FSIGnal",*/ "ERR"/*, "FRAM", "FSIG"*/ };
                string[] values = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
                foreach (string mode in modes)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:LIN:MODE", mode, mode, "Check for the mode -" + mode);
                    if (!mode.StartsWith("ERR"))
                    {
                        //:SEARch:SERial:LIN:ID
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(0, 63);
                            Utils.CmdSend(ref mScope, ":SEARch:SERial:LIN:ID", num, num, "Check for the serial lin id");
                        }
                        if (mode.Equals("DATA"))
                        {
                            //:SEARch:SERial:LIN:PATTern:FORMat
                            string[] formats = { "DEC", "HEX", "DECimal" };
                            foreach (string format in formats)
                            {
                                Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:LIN:PATTern:FORMat", format, format, "Check for the lin format -" + format);
                                for (int k = 0; k < 4; k++)
                                {
                                    //:SEARch:SERial:LIN:PATTern:DATA:LENGth
                                    int len = Utils.GenrateRandomInRange_Int(1, 8);
                                    Utils.CmdSend(ref mScope, ":SEARch:SERial:LIN:PATTern:DATA:LENGth", len, len, "Check for the length -" + len);
                                    if (format.StartsWith("HEX"))
                                    {
                                        //:SEARch:SERial:LIN:PATTern:DATA
                                        string data = "\"";
                                        data = data + "0x";
                                        for (int m = 0; m < 2 * len; m++)
                                        {
                                            data = data + values[Utils.GenrateRandomInRange_Int(0, 14)];
                                        }
                                        data = data + "\"";
                                        mScope.Send(":SEARch:SERial:LIN:PATTern:DATA " + data);
                                        string res = mScope.ReadString(":SEARch:SERial:LIN:PATTern:DATA?");
                                        Chk.Val(res, data, "Check for the data -" + data);
                                    }
                                } 
                            }
                        }
                    }
                }
            }
        }

        //uncomment = when serial protocol-2 is available 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SearchSerialSPI_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SBUS1:DISPlay 1");
            mScope.Send(":SBUS2:DISPlay 1");
            string[] values = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SEARch:MODE SER" + i);
                mScope.Send(":SBUS" + i + ":MODE SPI");
                mScope.Send(":LISTer:DISPlay SBUS" + i);
                //:SEARch:SERial:SPI:MODE
                string[] modes = { "MISO", "MOSI" };
                foreach (string mode in modes)
                {
                    Utils.CmdSend(ref mScope, ":SEARch:SERial:SPI:MODE", mode, mode, "Check for the mode -" + mode);
                    //:SEARch:SERial:SPI:PATTern:WIDTh
                    for (int j = 0; j < 4; j++)
                    {
                        int width = Utils.GenrateRandomInRange_Int(1, 10);
                        Utils.CmdSend(ref mScope, ":SEARch:SERial:SPI:PATTern:WIDTh", width, width, "Check for the width scpi-" + width);
                        //:SEARch:SERial:SPI:PATTern:DATA
                        string data = "\"";
                        data = data + "0x";
                        for (int k = 0; k < 2 * width; k++)
                        {
                            data = data + values[Utils.GenrateRandomInRange_Int(0, 14)];
                        }
                        data = data + "\"";
                        mScope.Send(":SEARch:SERial:SPI:PATTern:DATA " + data);
                        string res = mScope.ReadString(":SEARch:SERial:SPI:PATTern:DATA?");
                        Chk.Val(res, data, "check for the data -" + data);
                    }
                }
            }
        }

        //uncomment = when serial protocol-2 is available 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void searchSerialUART_scpi()
        {
            mScope.Send(":SEARch:STATe 1");
            mScope.Send(":SBUS1:DISPlay 1");
            mScope.Send(":SBUS2:DISPlay 1");
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SEARch:MODE SER" + i);
                mScope.Send(":SBUS" + i + ":MODE UART");
                mScope.Send(":LISTer:DISPlay SBUS" + i);
                //:SEARch:SERial:UART:MODE
                string[] modes = { "RDAT", "TDAT", "PAR", "AERR", "RDATa", "TDATa", "PARityerror", "AERRor"/*, "RD1", "RD0", "RDX", "TD1", "TD0", "TDX" */};
                foreach (string mode in modes)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:UART:MODE", mode, mode, "Check for the mode -" + mode);
                    if (mode.StartsWith("RDAT") || mode.StartsWith("TDAT"))
                    {
                        //:SEARch:SERial:UART:QUALifier
                        string[] Qualifiers = { "EQU", "NOT", "LESS", "GRE", "EQUal", "NOTequal", "LESSthan", "GREaterthan" };
                        foreach (string qual in Qualifiers)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SEARch:SERial:IIC:QUALifier", qual, qual, "Check for qualifier -" + qual);
                        }
                        //:SEARch:SERial:UART:DATA
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(0, 255);
                            Utils.CmdSend(ref mScope, ":SEARch:SERial:UART:DATA", num, num, "Check for the data -" + num);
                        }
                    }
                }
            }
        }
    }
}
