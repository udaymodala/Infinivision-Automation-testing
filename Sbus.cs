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
    class Sbus : InfiniiVisionTest
    {
        [SetUp]
        public void setup()
        {
            mScope.Write("*CLS;*RST");
            WaitForOpc(ref mScope, 20000);
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SBUS_scpi()
        {
            ScpiError err;
            //:SBUS<n>:DISPlay
            //:SBUS<n>?
            for (int i = 1; i </*=*/ 2; i++)
            {
                string cmd = ":SBUS" + i + ":DISPlay";
                Utils.CmdSend(ref mScope, cmd, 1, 1, "check for the sbus display scpi");
                Utils.CmdSend(ref mScope, cmd, 0, 0, "check for the sbus display scpi");
                Utils.CmdSend(ref mScope, cmd, 1, 1, "check for the sbus display scpi");
                mScope.Send("*CLS");
                mScope.Send(":SBUS" + i + "?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in sbus scpi ");
            }

            
        }

        //--uncomment = when protocol-2 is ready
        // -- remove comments after modification --need description of CAN:type scpi 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusCAN_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE CAN");
                //:SBUS<n>:CAN:COUNt:ERRor
                mScope.Send("*CLS");
                ScpiError err;
                int count=mScope.ReadNumberAsInt32(":SBUS"+i+":CAN:COUNt:ERRor?");
                err=mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in the count scpi");
                

                //:SBUS<n>:CAN:COUNt:OVERload
                mScope.Send("*CLS");
                int overload = mScope.ReadNumberAsInt32(":SBUS" + i + ":CAN:COUNt:OVERload?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in overload scpi");

                //:SBUS<n>:CAN:COUNt:RESet
                mScope.Send("*CLS");
                mScope.Send(":SBUS" + i + ":CAN:COUNt:RESet");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in count reset scpi");

                //:SBUS<n>:CAN:COUNt:SPEC
                mScope.Send("*CLS");
                int spec = mScope.ReadNumberAsInt32(":SBUS" + i + ":CAN:COUNt:SPEC?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in count spec scpi");

                //:SBUS<n>:CAN:COUNt:TOTal
                mScope.Send("*CLS");
                int t_count = mScope.ReadNumberAsInt32(":SBUS" + i + ":CAN:COUNt:TOTal?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in count total scpi");

                //:SBUS<n>:CAN:COUNt:UTILization
                mScope.Send("*CLS");
                double util = mScope.ReadNumberAsDouble(":SBUS" + i + ":CAN:COUNt:UTILization?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in utilization scpi");
                /*
                //:SBUS<n>:CAN:DISPlay
                string[] disp_values = { "SYMB", "HEX", "SYMBolic", "HEXadecimal" };
                foreach (string dis in disp_values)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":CAN:DISPlay", dis, dis, "Check for the can display scpi-" + dis);
                }
                */
                //:SBUS<n>:CAN:TYPE
                string[] types = { "STAN", "FD", "XFAS", "XSIC", "STANdard", "XFASt" };
                foreach (string type in types)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":CAN:TYPE", type, type, "check for the sbus can type scpi -"+type);
                    //:SBUS<n>:CAN:SAMPlepoint
                    for (int j = 0; j < 4; j++)
                    {
                        double num = Utils.GenrateRandomInRangeDouble(15, 45);
                        num = Math.Round(num, 0);
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SAMPlepoint", 2 * num, 2 * num, "Check for the can fdspoint -" + num);
                    }

                    //:SBUS<n>:CAN:SIGNal:BAUDrate
                    for (int j = 0; j < 4; j++)
                    {
                        int num = Utils.GenrateRandomInRange_Int(100, 40000);
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SIGNal:BAUDrate", num * 100, num * 100, "Check for the can samplepoint-" + num);
                    }

                    if (type.StartsWith("XFAS") || type.Equals("XSIC"))
                    {
                        //:SBUS<n>:CAN:SIGNal:XLBaudrate
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(100, 100000);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SIGNal:XLBaudrate", num * 100, num * 100, "Check for the can signal xlbaudrate -" + num);
                        }

                        /*
                        //:SBUS<n>:CAN:XLSPoint
                        for (int j = 0; j < 4; j++)
                        {
                            double num = Utils.GenrateRandomInRangeDouble(15, 45);
                            num = Math.Round(num, 0);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:XLSPoint", 2 * num, 2 * num, "Check for the can xlspoint -" + num);
                        }
                         * */

                    }
                    if (type.Equals("FD") || type.Equals("XSIC"))
                    {
                        /*
                        //:SBUS<n>:CAN:FDSPoint  
                        for (int j = 0; j < 4; j++)
                        {
                            double num = Utils.GenrateRandomInRangeDouble(15, 45);
                            num = Math.Round(num, 0);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:FDSPoint", 2 * num, 2 * num, "Check for the can fdspoint -" + num);
                        }
                         * */
                        //:SBUS<n>:CAN:SIGNal:FDBaudrate
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(100, 100000);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SIGNal:FDBaudrate", num * 100, num * 100, "Check for the can signal fdbaudrate -" + num);
                        }
                    }
                }
               
                //:SBUS<n>:CAN:SIGNal:DEFinition
                string[] signal_values = { "CANH", "CANL", "RX", "TX", "DIFL", "DIFH" };
                foreach (string value in signal_values)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SIGNal:DEFinition", value, value, "Check for the signal definition -" + value);
                }

                //:SBUS<n>:CAN:SOURce
                string[] sources = { "CHAN", "DIG" };
                foreach (string source in sources)
                {
                    if (source.Equals("CHAN"))
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            string sou = source + j;
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SOURce", sou, sou, "Check for the source scpi -" + sou);
                        }
                    }
                    else
                    {
                        for (int j = 0; j <= 15; j++)
                        {
                            string sou = source + j;
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:SOURce", sou, sou, "Check for the source scpi -" + sou);
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusIIC_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE IIC");
                //:SBUS<n>:IIC:ASIZe
                string[] size_values = { "BIT7", "BIT8" };
                foreach (string value in size_values)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:ASIZe", value, value, "Check for the size -" + value);
                }

                //:SBUS<n>:IIC[:SOURce]:CLOCk
                //:SBUS<n>:IIC[:SOURce]:DATA
                string[] sources = { "CHAN1", "CHAN2", "CHAN3", "CHAN4", "DIG0", "DIG1", "DIG2", "DIG3", "DIG4", "DIG5", "DIG6", "DIG7", "DIG8", "DIG9", "DIG10", "DIG11", "DIG12", "DIG13", "DIG14", "DIG15" };
                foreach (string val1 in sources)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:SOURce:CLOCk", val1, val1, "Check for the clock source -" + val1);
                    foreach (string val2 in sources)
                    {
                        if (!val2.Equals(val1))
                        {
                            Utils.CmdSend(ref mScope, ":SBUS"+i+":IIC:SOURce:DATA", val2, val2, "Check for the data source -" + val2);
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusIICTrigger_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE IIC");
                mScope.Send(":TRIGger:MODE SBUS" + i);
                //:SBUS<n>:IIC:TRIGger[:TYPE]
                string[] types = { "STAR", "STOP", "REST", "ADDR", "ANAC", "DNAC", "NACK", "REPR", "READ7", "WRIT7", "R7D2", "W7D2", "WRIT10" };
                foreach (string type in types)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:TRIGger:TYPE", type, type, "Check for the iic trigger type-" + type);
                    //:SBUS<n>:IIC:TRIGger:PATTern:ADDRess
                    if (type.Equals("ADDR") || type.Equals("ANAC") || type.Equals("READ7") || type.Equals("WRIT7") || type.Equals("R7D2") || type.Equals("W7D2"))
                    {
                        int add = Utils.GenrateRandomInRange_Int(0, 127);
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:TRIGger:PATTern:ADDRess", add, add, "Check for the address scpi-" + add);
                    }
                    else if (type.Equals("WRIT10"))
                    {
                        int add = Utils.GenrateRandomInRange_Int(0, 1023);
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:TRIGger:PATTern:ADDRess", add, add, "Check for the address scpi-" + add);
                    }

                    //:SBUS<n>:IIC:TRIGger:PATTern:DATA
                    //:SBUS<n>:IIC:TRIGger:PATTern:DATa2
                    if (type.Equals("READ7") || type.Equals("WRIT7") || type.Equals("R7D2") || type.Equals("W7D2"))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int data1 = Utils.GenrateRandomInRange_Int(0, 255);
                            if (type.Equals("R7D2") || type.Equals("W7D2"))
                            {
                                Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:TRIGger:PATTern:DATa2", data1, data1, "Check for the data scpi -" + data1);
                            }
                            int data2 = Utils.GenrateRandomInRange_Int(0, 255);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":IIC:TRIGger:PATTern:DATA", data2, data2, "Check for the data scpi -" + data2);
                        }
                    }

                    //:SBUS<n>:IIC:TRIGger:QUALifier
                    if (type.Equals("REPR"))
                    {
                        string[] Qualifiers = { "EQU", "NOT", "LESS", "GRE", "EQUal", "NOTequal", "LESSthan", "GREaterthan" };
                        foreach (string qual in Qualifiers)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SBUS"+i+":IIC:TRIGger:QUALifier", qual, qual, "Check for qualifier -" + qual);
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        // -- Need to check this :SBUS<n>:CAN:TRIGger:PATTern:DATA:DLC scpi --riya
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusCANTrigger_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE CAN");
                mScope.Send(":TRIGger:MODE SBUS" + i);
                //:SBUS<n>:CAN:TRIGger
                string[] modes = { "SOF", "EOF", "IDD", "DATA", "FDD", "IDR", "IDE", "ERR", "ACK", "FORM", "STUF", "CRC", "SPEC", "ALL", "BRSB", "CRCD", "EBA", "EBP", "OVER", /*"MESS", "MSIG", "FDMS" */};
                string[] pattern_mode = { "EXT", "STAN", "EXTended", "STANdard" };
                string[] values = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
                string[] bValues = { "0", "1", "X" };
                foreach (string mode in modes)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger", mode, mode, "Check for the can trigger -" + mode);

                    //:SBUS<n>:CAN:TRIGger:IDFilter
                    if (mode.Equals("EOF") || mode.Equals("ERR") || mode.Equals("ACK") || mode.Equals("FORM") || mode.Equals("STUF") || mode.Equals("CRC") || mode.Equals("SPEC") || mode.Equals("ALL") || mode.Equals("BRSB") || mode.Equals("CRCD") || mode.Equals("EBA") || mode.Equals("EBP"))
                    {
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger:IDFilter", 1, 1, "Check for the trigger id filter");
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger:IDFilter", 0, 0, "Check for the trigger id filter");
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger:IDFilter", 1, 1, "Check for the trigger id filter");
                    }

                    //:SBUS<n>:CAN:TRIGger:PATTern:ID:MODE         
                    //:SBUS<n>:CAN:TRIGger:PATTern:ID
                    if (mode.Equals("EOF") || mode.Equals("ERR") || mode.Equals("ACK") || mode.Equals("FORM") || mode.Equals("STUF") || mode.Equals("CRC") || mode.Equals("SPEC") || mode.Equals("ALL") || mode.Equals("BRSB") || mode.Equals("CRCD") || mode.Equals("EBA") || mode.Equals("EBP") || mode.Equals("IDE") || mode.Equals("IDD") || mode.Equals("FDD") || mode.Equals("IDR"))
                    {
                        foreach (string pattern in pattern_mode)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":CAN:TRIGger:PATTern:ID:MODE", pattern, pattern, "Check for the pattern mode -" + pattern);

                            if (pattern.StartsWith("STAN"))
                            {
                                string val = "\"" + "000000000000000000";
                                for (int k = 0; k < 11; k++)
                                {
                                    val = val + bValues[Utils.GenrateRandomInRange_Int(0, 2)];
                                }
                                val = val + "\"";
                                mScope.Send(":SBUS" + i + ":CAN:TRIGger:PATTern:ID " + val);
                                string res = mScope.ReadString(":SBUS" + i + ":CAN:TRIGger:PATTern:ID?");
                                Chk.Val(res, val, "Check for the trigger pattern id");
                            }
                            else
                            {
                                string val = "\"";
                                for (int k = 0; k < 29; k++)
                                {
                                    val = val + bValues[Utils.GenrateRandomInRange_Int(0, 2)];
                                }
                                val = val + "\"";
                                mScope.Send(":SBUS" + i + ":CAN:TRIGger:PATTern:ID " + val);
                                string res = mScope.ReadString(":SBUS" + i + ":CAN:TRIGger:PATTern:ID?");
                                Chk.Val(res, val, "Check for the trigger pattern id");
                            }
                        }
                    }

                    //:SBUS<n>:CAN:TRIGger:PATTern:DATA:LENGth
                    //:SBUS<n>:CAN:TRIGger:PATTern:DATA
                    if (mode.Equals("DATA") || mode.Equals("FDD"))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int len = Utils.GenrateRandomInRange_Int(1, 8);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger:PATTern:DATA:LENGth", len, len, "Check for the length -" + len);
                            string data = "\"";
                            for (int k = 0; k < 8 * len; k++)
                            {
                                data = data + bValues[Utils.GenrateRandomInRange_Int(0, 2)];
                            }
                            data = data + "\"";
                            mScope.Send(":SBUS" + i + ":CAN:TRIGger:PATTern:DATA " + data);
                            string res = mScope.ReadString(":SBUS" + i + ":CAN:TRIGger:PATTern:DATA?");
                            Chk.Val(res, data, "Check for the data -" + data);

                            //:SBUS<n>:CAN:TRIGger:PATTern:DATA:DLC
                            //:SBUS<n>:CAN:TRIGger:PATTern:DATA:STARt
                            if (mode.Equals("FDD"))
                            {
                                
                                int num = Utils.GenrateRandomInRange_Int(len+1, 64);
                                /*
                                Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger:PATTern:DATA:DLC", num, num, "Check for the pattern data dlc -" + num);
                                 * */
                                int num1 = Utils.GenrateRandomInRange_Int(0, num-len);
                                Utils.CmdSend(ref mScope, ":SBUS" + i + ":CAN:TRIGger:PATTern:DATA:STARt", num1, num1, "Check for the pattern data start -" + num1);
                            }
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusSPI_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE SPI");
                //:SBUS<n>:SPI:BITorder
                string[] orders = { "LSBFirst", "MSBFirst", "LSBF", "MSBF" };
                foreach (string order in orders)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":SPI:BITorder", order, order, "Check for the spi bitorder scpi-" + order);
                }

                //:SBUS<n>:SPI:CLOCk:SLOPe
                string[] slopes = { "POS", "NEG", "POSitive", "NEGative" };
                foreach (string slope in slopes)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":SPI:CLOCk:SLOPe", slope, slope, "Check for the clock slope -" + slope);
                }

                //:SBUS<n>:SPI:FRAMing
                //:SBUS<n>:SPI:CLOCk:TIMeout
                string[] frames = { "CHIP", "NCH", "TIM","CHIPselect","NCHipselect","TIMeout" };
                double[] time = { 1e-7, 2.3e-6, 5.3e-3, 6.2e-1, 0.2, 1.3, 10 };
                foreach (string frame in frames)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":SPI:FRAMing", frame, frame, "Check for the spi frame -" + frame);
                    if (frame.StartsWith("TIM"))
                    {
                        for (int j = 0; j < time.Length; j++)
                        {
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:CLOCk:TIMeout", time[j], time[j], "Check for the time scpi -" + time[j]);
                        }
                    }
                }

                //:SBUS<n>:SPI:DELay
                for (int j = 0; j < 4; j++)
                {
                    int num = Utils.GenrateRandomInRange_Int(2, 63);
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:DELay", num, num, "Check for the spi delay -" + num);
                }

                //:SBUS<n>:SPI:WIDTh
                for (int j = 0; j < 4; j++)
                {
                    int num = Utils.GenrateRandomInRange_Int(4, 16);
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:WIDTh", num, num, "Check for the spi width scpi -" + num);
                }

                //:SBUS<n>:SPI:TRUNcate
                for (int j = 0; j < 4; j++)
                {
                    int num = Utils.GenrateRandomInRange_Int(4, 63);
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:TRUNcate", num, num, "check for the spi truncate scpi -" + num);
                }
            }
        }

        //--uncomment = when protocol-2 is ready   
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusSPISource_scpi()
        {
            string[] sources = { "CHAN1", "CHAN2"/*, "CHAN3", "CHAN4"*/, "DIG0", "DIG1", "DIG2"/*, "DIG3", "DIG4", "DIG5", "DIG6", "DIG7", "DIG8", "DIG9", "DIG10", "DIG11", "DIG12", "DIG13", "DIG14", "DIG15"*/ };
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE SPI");
                //:SBUS<n>:SPI:SOURce:CLOCk
                //:SBUS<n>:SPI:SOURce:MOSI
                //:SBUS<n>:SPI:SOURce:MISO
                //:SBUS<n>:SPI:SOURce:FRAMe
                foreach (string val1 in sources)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:SOURce:CLOCk", val1, val1, "Check for the source clock -" + val1);
                    foreach (string val2 in sources)
                    {
                        if (!val2.Equals(val1))
                        {
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:SOURce:MOSI", val2, val2, "Check for the source mosi -" + val2);
                            foreach (string val3 in sources)
                            {
                                if (!(val3.Equals(val1) || val3.Equals(val2)))
                                {
                                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:SOURce:MISO", val3, val3, "Check for the source miso -" + val3);
                                    foreach (string val4 in sources)
                                    {
                                        if (!(val4.Equals(val1) || val4.Equals(val2) || val4.Equals(val3)))
                                        {
                                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:SOURce:FRAMe", val4, val4, "Check for the source frame -" + val4);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusSPITrigger_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE SPI");
                mScope.Send(":TRIGger:MODE SBUS" + i);
                //:SBUS<n>:SPI:TRIGger:TYPE
                string[] types = { "MOSI", "MISO" };
                foreach (string type in types)
                {
                    string[] values={"0","1","X"};
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:TRIGger:TYPE", type, type, "Check for the spi trigger type -" + type);
                    if (type.Equals("MOSI"))
                    {
                        //:SBUS<n>:SPI:TRIGger:PATTern:MOSI:DATA
                        //:SBUS<n>:SPI:TRIGger:PATTern:MOSI:WIDTh
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(4, 64);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:TRIGger:PATTern:MOSI:WIDTh", num, num, "Check for the trigger pattern mosi width -" + num);
                            string data = "\"";
                            for (int k = 0; k < num; k++)
                            {
                                data = data + values[Utils.GenrateRandomInRange_Int(0, 2)];
                            }
                            data = data + "\"";
                            mScope.Send(":SBUS"+i+":SPI:TRIGger:PATTern:MOSI:DATA " + data);
                            string res = mScope.ReadString(":SBUS" + i + ":SPI:TRIGger:PATTern:MOSI:DATA?");
                            Chk.Val(res, data, "Check for the mosi data -" + data);
                        }
                    }
                    else
                    {
                        //:SBUS<n>:SPI:TRIGger:PATTern:MISO:DATA
                        //:SBUS<n>:SPI:TRIGger:PATTern:MISO:WIDTh
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(4, 64);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":SPI:TRIGger:PATTern:MISO:WIDTh", num, num, "Check for the trigger pattern miso width -" + num);
                            string data = "\"";
                            for (int k = 0; k < num; k++)
                            {
                                data = data + values[Utils.GenrateRandomInRange_Int(0, 2)];
                            }
                            data = data + "\"";
                            mScope.Send(":SBUS" + i + ":SPI:TRIGger:PATTern:MISO:DATA " + data);
                            string res = mScope.ReadString(":SBUS"+i+":SPI:TRIGger:PATTern:MISO:DATA?");
                            Chk.Val(res, data, "Check for the mosi data -" + data);
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusUART_Scpi()
        {
            string[] sources = { "CHAN1", "CHAN2", "CHAN3", "CHAN4", "DIG0", "DIG1", "DIG2", "DIG3", "DIG4", "DIG5", "DIG6", "DIG7", "DIG8", "DIG9", "DIG10", "DIG11", "DIG12", "DIG13", "DIG14", "DIG15" };
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE UART");
                //:SBUS<n>:UART:BASE
                string[] bases = { "ASC", "BIN", "HEX", "ASCii", "BINary" };
                foreach(string b in bases){
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":UART:BASE", b, b, "check for the uart base scpi -" + b);
                }

                //:SBUS<n>:UART:BITorder
                string[] bits = { "LSBFirst", "MSBFirst", "LSBF", "MSBF" };
                foreach (string bit in bits)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":UART:BITorder", bit, bit, "Check for the uart bitorder -" + bit);
                }

                //:SBUS<n>:UART:COUNt:ERRor?
                ScpiError err;
                mScope.Send("*CLS");
                int err_count = mScope.ReadNumberAsInt32(":SBUS" + i + ":UART:COUNt:ERRor?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in uart count error scpi");

                //:SBUS<n>:UART:COUNt:RESet
                mScope.Send("*CLS");
                mScope.Send(":SBUS" + i + ":UART:COUNt:RESet");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in uart count reset scpi");

                //:SBUS<n>:UART:COUNt:RXFRames
                mScope.Send("*CLS");
                int RxFrames = mScope.ReadNumberAsInt32(":SBUS" + i + ":UART:COUNt:RXFRames?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in uart count rxframes");

                //:SBUS<n>:UART:COUNt:TXFRames
                mScope.Send("*CLS");
                int TxFrames = mScope.ReadNumberAsInt32(":SBUS" + i + ":UART:COUNt:TXFRames?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in uart count txframes");

                //:SBUS<n>:UART:FRAMing
                for (int j = 0; j < 4; j++)
                {
                    int num = Utils.GenrateRandomInRange_Int(0, 255);
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:FRAMing", num, num, "Check for the uart framing -" + num);
                }

                //:SBUS<n>:UART:PARity
                string[] parity = { "EVEN", "ODD", "NONE" };
                foreach (string par in parity)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:PARity", par, par, "Check for the uart parity scpi -" + par);
                }

                //:SBUS<n>:UART:POLarity
                string[] polarity = { "HIGH", "LOW" };
                foreach (string pol in polarity)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:POLarity", pol, pol, "check for the uart polarity -" + pol);
                }

                //:SBUS<n>:UART:SOURce:RX
                //:SBUS<n>:UART:SOURce:TX
                foreach (string val1 in sources)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:SOURce:RX", val1, val1, "Check for the source rx scpi -" + val1);
                    foreach (string val2 in sources)
                    {
                        if (!val2.Equals(val1))
                        {
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:SOURce:TX", val2, val2, "Check for the source tx scpi -" + val2);
                        }
                    }
                }

                //:SBUS<n>:UART:BAUDrate
                for (int j = 0; j < 4; j++)
                {
                    int num = Utils.GenrateRandomInRange_Int(100, 8000000);
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:BAUDrate", num, num, "Check for the uart baudrate -" + num);
                }

                //:SBUS<n>:UART:WIDTh
                for (int j = 5; j <= 9; j++)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:WIDTh", j, j, "Check for the uart width -" + j);
                }
            }
        }


        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusUARTTrigger_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE UART");
                mScope.Send(":TRIGger:MODE SBUS" + i);
                //:SBUS<n>:UART:TRIGger:TYPE
                string[] types = { "RSTA", "RSTO", "RDAT",/* "PAR",*/ "TSTA", "TSTO", "TDAT", "RSTArt", "RSTOp", "RDATa", /*"PARityerror",*/ "TSTArt", "TSTOp", "TDATa"/*, "RD1", "RD0", "RDX", "TD1", "TD0", "TDX" */};
                foreach (string type in types)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":UART:TRIGger:TYPE", type, type, "Check for the uart trigger type -" + type);
                    
                    //:SBUS<n>:UART:TRIGger:QUALifier
                    string[] qualifier = { "EQU", "NOT", "GRE", "LESS", "EQUal", "NOTequal", "GREaterthan", "LESSthan" };
                    if (type.StartsWith("RDAT") || type.StartsWith("TDAT") || type.Equals("RD1") || type.Equals("RD0") || type.Equals("RDX") || type.Equals("TD1") || type.Equals("TD0") || type.Equals("TDX"))
                    {
                        foreach (string qual in qualifier)
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":UART:TRIGger:QUALifier", qual, qual, "Check for the uart trigger qualifier scpi -" + qual);
                        }
                    }

                    //:SBUS<n>:UART:TRIGger:BASE
                    //:SBUS<n>:UART:TRIGger:DATA
                    string[] bases = { "ASC", "HEX", "ASCii" };
                    foreach (string b in bases)
                    {
                        Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":UART:TRIGger:BASE", b, b, "Check for the uart trigger base scpi-" + b);
                        if (b.StartsWith("ASC"))
                        {
                            int num = Utils.GenrateRandomInRange_Int(0, 255);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:TRIGger:DATA", num, num, "Check for the uart trigger data scpi -" + num);

                        }
                    }

                    //:SBUS<n>:UART:TRIGger:BURSt
                    //:SBUS<n>:UART:TRIGger:IDLE
                    double[] idle_values = { 1e-6, 2.3e-3, 0.3, 8.4, 10 };
                    for (int j = 0; j < 4; j++)
                    {
                        int num = Utils.GenrateRandomInRange_Int(1, 4096);
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:TRIGger:BURSt", num, num, "Check for the uart trigger burst -" + num);
                        for (int k = 0; k < idle_values.Length; k++)
                        {
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":UART:TRIGger:IDLE", idle_values[k], idle_values[k], "check for the uart trigger idle scpi -" + idle_values[k]);
                        }
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusLIN_scpi()
        {
            //:SBUS<n>:LIN:SOURce
            string[] sources = { "CHAN1", "CHAN2", "CHAN3", "CHAN4", "DIG0", "DIG1", "DIG2", "DIG3", "DIG4", "DIG5", "DIG6", "DIG7", "DIG8", "DIG9", "DIG10", "DIG11", "DIG12", "DIG13", "DIG14", "DIG15" };
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE UART");
                foreach (string source in sources)
                {
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:SOURce", source, source, "check for the sbus lin source scpi -" + source);

                    //:SBUS<n>:LIN:SIGNal:BAUDrate
                    for (int j = 0; j < 4; j++)
                    {
                        int num = Utils.GenrateRandomInRange_Int(24, 6250);
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:SIGNal:BAUDrate", num * 100, num * 100, "Check for the lin baudrate -" + num * 100);
                    }
                    //:SBUS<n>:LIN:SAMPlepoint
                    double[] sampleValues = { 60, 62.5, 68, 70, 75, 80, 87.5 };
                    for (int j = 0; j < sampleValues.Length; j++)
                    {
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:SAMPlepoint", sampleValues[j], sampleValues[j], "Check for the lin samplepoint scpi -" + sampleValues[j]);
                    }

                    //:SBUS<n>:LIN:PARity
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:PARity", 1, 1, "Check for the lin parity scpi");
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:PARity", 0, 0, "Check for the lin parity scpi");
                    Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:PARity", 1, 1, "Check for the lin parity scpi");

                    //:SBUS<n>:LIN:STANdard
                    string[] stanValues = { "LIN13", "LNLC13", "LIN20" };
                    foreach (string stan in stanValues)
                    {
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:STANdard", stan, stan, "Check for the lin standard scpi -" + stan);
                    }

                    //:SBUS<n>:LIN:SYNCbreak
                    int[] breakValues = { 11, 12, 13 };
                    for (int j = 0; j < breakValues.Length; j++)
                    {
                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:SYNCbreak", breakValues[j], breakValues[j], "check for the LIN syncbreak scpi -" + breakValues[j]);  
                    }
                }
            }
        }

        //--uncomment = when protocol-2 is ready
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SbusLINTrigger_scpi()
        {
            for (int i = 1; i </*=*/ 2; i++)
            {
                mScope.Send(":SBUS" + i + ":DISPlay 1");
                mScope.Send(":SBUS" + i + ":MODE LIN");
                mScope.Send(":TRIGger:MODE SBUS" + i);

                //:SBUS<n>:LIN:TRIGger
                string[] triggerValues = { "SYNC", "ID", "DATA", "PAR", "CSUM", "SYNCbreak", "PARityerror", "CSUMerror"/*, "FRAMe", "FSIGnal", "FRAM", "FSIG"*/ };
                foreach (string trig in triggerValues)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":LIN:TRIGger", trig, trig, "Check for the lin trigger scpi -" + trig);
                    if (trig.StartsWith("ID") || trig.StartsWith("DATA"))
                    {
                        //:SBUS<n>:LIN:TRIGger:ID
                        for (int j = 0; j < 4; j++)
                        {
                            int num = Utils.GenrateRandomInRange_Int(0, 63);
                            Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:TRIGger:ID", num, num, "Check for the lin trigger id scpi -" + num);
                        }

                        //:SBUS<n>:LIN:TRIGger:PATTern:DATA
                        //:SBUS<n>:LIN:TRIGger:PATTern:DATA:LENGth
                        //:SBUS<n>:LIN:TRIGger:PATTern:FORMat
                        string[] values={"0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F","X"};
                        string[] bValues = { "0", "1", "X" };
                        if (trig.Equals("DATA"))
                        {
                            string[] formats = { "BIN", "HEX", "BINary"/*,"DEC"*/ };
                            foreach (string format in formats)
                            {
                                Utils.CmdSend_startsWith(ref mScope, ":SBUS" + i + ":LIN:TRIGger:PATTern:FORMat", format, format, "Check for the trigger pattern format");
                                if (format.Equals("HEX"))
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        string val = "\"" + "0x";
                                        int len = Utils.GenrateRandomInRange_Int(1, 8);
                                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:TRIGger:PATTern:DATA:LENGth", len, len, "Check for the trigger pattern data length scpi -" + len);
                                        for (int k = 0; k < 2 * len; k++)
                                        {
                                            val = val + values[Utils.GenrateRandomInRange_Int(0, 16)];
                                        }
                                        val = val + "\"";
                                        Utils.CmdSend(ref mScope, ":SBUS"+i+":LIN:TRIGger:PATTern:DATA", val, val, "Check for the trigger pattern data -" + val);
                                    }
                                }
                                else if (format.StartsWith("BIN"))
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        string val = "\"";
                                        int len = Utils.GenrateRandomInRange_Int(1, 8);
                                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:TRIGger:PATTern:DATA:LENGth", len, len, "Check for the trigger pattern data length scpi -" + len);
                                        for (int k = 0; k < 8 * len; k++)
                                        {
                                            val = val + bValues[Utils.GenrateRandomInRange_Int(0, 2)];
                                        }
                                        val = val + "\"";
                                        Utils.CmdSend(ref mScope, ":SBUS" + i + ":LIN:TRIGger:PATTern:DATA", val, val, "Check for the trigger pattern data -" + val);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
