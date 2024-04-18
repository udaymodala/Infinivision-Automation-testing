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
    class fft_test : InfiniiVisionTest
    {
        public int mMaxChan = 4;
        int mTimeout = 20000;

        [SetUp]
        public void Setup()
        {
            mScope.Write("*RST");
            WaitForOpc(ref mScope, mTimeout);
            Waveform wfm = new Waveform(Shape.Sine, 10000000, 1.00);
            FgensSetWaveform(wfm);

        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DefaultFFT()
        {
            string retRes = null;
            string expectedRes = ":FFT:DISP 0;SOUR1 CHAN1;RANG +160E+00;OFFS -60.0000E+00;SPAN +100.0000E+03;CENT +50.000000E+03;WIND HANN;VTYP DEC;AVER:COUN 8";

            retRes = mScope.ReadString(":fft?");
            Chk.Val(retRes, expectedRes, "Default FFT setup");
        }



        /// <summary>
        /// :fft:display
        /// :fft:source1
        /// :fft:scale
        /// :fft:range
        /// :fft:offset
        /// :fft:average:count
        /// :fft:window
        /// :fft:detection:type
        /// :fft:detection:points
        /// :fft:frequency:start
        /// :fft:frequency:stop
        /// :fft:span
        /// :fft:center
        /// 
        /// Expecte failure in this test - needs change
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void FFT_scpi()
        {
            // Min & max limits
            const double MAX_SCALE = 1E+12;
            const double MIN_SCALE = 1E-15;
            //const double MAX_OFFSET = 4E+12;
            //const double MIN_OFFSET = -4E+12;

            const double MIN_SPAN = 1;
            const double MAX_SPAN = 50E+9;
            const double MIN_CENTER_FREQ = -25E+9;
            const double MAX_CENTER_FREQ = 25E+9;
            const double MIN_START_STOP_FREQ = -50E+9;
            const double MAX_START_STOP_FREQ = 50E+9;

            const int MIN_POINTS = 640;
            const int MAX_POINTS = 64000;

            const int MIN_AVG = 2;
            const int MAX_AVG = 65536;

            const double freqTol = 25e3; // 25kHz tolerance

            string[] vType = { "VRMS", "DEC" };
            string[] windows = { "RECT", "HANN", "FLAT", "BHAR", "BART" };
            string[] detectionType = { "SAMP", "PPOS", "PNEG", "NORM", "AVER", "OFF" };
            string[] dmode = { "AVER", "MAXH", "MINH", "NORM" };

            double span, scale, offset, cf, startFreq, stopFreq;

            Utils.CmdSend(ref mScope, ":fft:display", "1", "1", "FFT Display state");

            for (int chan = 1; chan <= 4; chan++)
            {
                Utils.CmdSend(ref mScope, ":fft:source1", "CHAN" + chan, "CHAN" + chan, "FFT source");
            }


            for (double i = 0; i < 5; i++)
            {
                scale = MIN_SCALE + ((MAX_SCALE - MIN_SCALE) / 5 * i);
                Utils.CmdSend(ref mScope, ":fft:scale", scale, scale, "FFT Scale (db)");
                Chk.Val(mScope.ReadNumberAsDouble(":fft:range?"), scale * 8, "Corresponding FFT full scale range");
                offset = scale * -8 + ((scale * 8 + scale * 8) / 5 * i);
                Utils.CmdSend(ref mScope, ":fft:offset", offset, offset, "FFT Offset (db)");
            }

            startFreq = mScope.ReadNumberAsDouble(":fft:frequency:start?");
            Pass.Condition(Is.LessOrEqual(startFreq, 9.9999E36), "is not a garbage value");
            stopFreq = mScope.ReadNumberAsDouble(":fft:frequency:stop?");
            Pass.Condition(Is.LessOrEqual(stopFreq, 9.9999E36), "is not a garbage value");
            cf = mScope.ReadNumberAsDouble(":fft:center?");
            Pass.Condition(Is.Equal(cf, (stopFreq + startFreq) / 2, freqTol), "Verifying : CF corresponding to set Start/Stop Frequencies");
            span = mScope.ReadNumberAsDouble(":fft:span?");
            Pass.Condition(Is.Equal(span, stopFreq - startFreq, freqTol), "Verifying : Span corresponding to set Start/Stop Frequencies");


            int avgCount = MIN_AVG;
            do
            {
                Utils.CmdSend(ref mScope, ":fft:average:count", avgCount, avgCount, "FFT average count");
                avgCount = avgCount * avgCount;
            } while (avgCount <= MAX_AVG && avgCount != 0);


            foreach (string window in windows)
            {
                Utils.CmdSend(ref mScope, ":fft:window", window, window, "FFT Window");
            }

            foreach (string type in detectionType)
            {
                Utils.CmdSend(ref mScope, ":fft:detection:type", type, type, "FFT detection type");
            }

            for (int points = MIN_POINTS; points <= MAX_POINTS; points *= 10)
            {
                Utils.CmdSend(ref mScope, ":fft:detection:points", points, points, "FFT detection points");
            }

            foreach (string type in vType)
            {
                Utils.CmdSend(ref mScope, ":FFT:VTYPe", type, type, "Set and check fft vertical units");
            }

            //:FFT:GATE
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            mScope.Send(":FFT:DISPlay 1");
            mScope.Send(":TIMebase:MODE WINDow");
            Wait.MilliSeconds(500);
            string[] gating = { "NONE", "ZOOM" };
            for (int i = 0; i < gating.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":FFT:GATE", gating[i], gating[i], "check for gating command");
            }

            mScope.Send(":TIMebase:MODE MAIN");
            Wait.MilliSeconds(500);

            //:FFT:READout
            string[] values = {"SRAT", "SRATe", "BSIZ", "BSIZe", "RBW", "RBWidth","NONE" };
            for (int i = 0; i < values.Length; i++)
            {
                Utils.CmdSend_startsWith(ref mScope, ":FFT:READout", values[i], values[i], "Check for fft read scpi command");
            }

            //:FFT:BSIZe
            mScope.Send(":FFT:READout BSIZ");
            ScpiError err;
            mScope.Send("*CLS");
            double binSize = mScope.ReadNumberAsDouble(":FFT:BSIZe?");
            Pass.Condition(Is.LessOrEqual(binSize, 9.9999E36), "is not a garbage value");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in fft binsize scpi command");

            //:FFT:DMODe -  will be added in Hawk
            /*
            string[] value = { "NORM", "NORMal", "AVER", "AVERage", "MAXHold", "MAXH", "MINHold", "MINH" };
            foreach (string val in value)
            {
                Utils.CmdSend_startsWith(ref mScope, " :FFT:DMODe", val, val, String.Format("check for the fft dmode scpi command -{0}", val));
            }*/

            //:FFT:CLEar
            string[] dis_values = { "AVER", "MAXH", "MINH" };
            foreach (string val in dis_values)
            {
                //string s = ":FFT:DMODe" + " " + val;
                //mScope.Send(s);
                mScope.Send("*CLS");
                mScope.Send(":FFT:CLEar");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in fft clear scpi command");
            }


            //:FFT:RBWidth?
            mScope.Send(":FFT:READout RBW");
            mScope.Send("*CLS");
            double width = mScope.ReadNumberAsDouble(":FFT:RBWidth?");
            Pass.Condition(Is.LessOrEqual(width, 9.9999E36), "is not a garbage value");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in fft rbwidth scpi command");


            //:FFT:SRATe?
            mScope.Send(":FFT:READout SRAT");
            mScope.Send("*CLS");
            double sample_rate = mScope.ReadNumberAsDouble(":FFT:SRATe?");
            Pass.Condition(Is.LessOrEqual(sample_rate, 9.9999E36), "is not a garbage value");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in fft srate scpi command");

            /*
            for (double i = 0; i < 5; i++)
            {
                mScope.Send("*rst");
                mScope.ReadNumberAsInt32("*opc?");

                startFreq = MIN_START_STOP_FREQ + ((MAX_START_STOP_FREQ - MIN_START_STOP_FREQ) / 5 * i);
                Utils.CmdSend(ref mScope, ":fft:frequency:start", startFreq, startFreq, freqTol, "FFT Start frequency (Hz)");
                //stopFreq = MIN_START_STOP_FREQ + ((MAX_START_STOP_FREQ - MIN_START_STOP_FREQ) / 5 * i);
                stopFreq = startFreq + 50e9;
                if (stopFreq > 50e9)
                {
                    stopFreq = 50e9;
                }
                Utils.CmdSend(ref mScope, ":fft:frequency:stop", stopFreq, stopFreq, freqTol, "FFT Stop frequency (Hz)");

                span = mScope.ReadNumberAsDouble(":fft:span?");
                Pass.Condition(Is.Equal(span, stopFreq - startFreq, freqTol), "Verifying : Span corresponding to set Start/Stop Frequencies");

                cf = mScope.ReadNumberAsDouble(":fft:center?");
                Pass.Condition(Is.Equal(cf, (stopFreq + startFreq) / 2, freqTol), "Verifying : CF corresponding to set Start/Stop Frequencies");

                mScope.Send("*rst");
                mScope.ReadNumberAsInt32("*opc?");

                span = MIN_SPAN + ((MAX_SPAN - MIN_SPAN) / 5 * i);
                Utils.CmdSend(ref mScope, ":fft:span", span, span, freqTol, "FFT span");

                cf = MIN_CENTER_FREQ + ((MAX_CENTER_FREQ - MIN_CENTER_FREQ) / 5 * i);
                Utils.CmdSend(ref mScope, ":fft:center", cf, cf, freqTol, "FFT Center frequency (Hz)");

                stopFreq = mScope.ReadNumberAsDouble(":fft:frequency:stop?");
                Pass.Condition(Is.Equal(stopFreq, (span / 2) + cf, freqTol), "Verifying : Stop Frequency corresponding to set Span and CF");

                startFreq = mScope.ReadNumberAsDouble(":fft:frequency:start?");
                Pass.Condition(Is.Equal(stopFreq, cf - (span / 2), freqTol), "Verifying : Start Frequency corresponding to set Span and CF");
            
            }
            */

            //:FFT:REFerence
            double[] refValues = { 1e-14, 0, 1e11, 3.234e9, 53.34e6 };
            for (int i = 0; i < refValues.Length; i++)
            {
                
                Utils.CmdSend(ref mScope, ":FFT:REFerence", refValues[i], refValues[i], String.Format("Check for the reference scpi -{0}", refValues[i]));
            }

            // Riya -add this
            //:FFT:PHASe:REFerence
            string[] vlaues = { "DISP", "TRIG", "DISPlay", "TRIGger" };
            foreach (string value in vlaues)
            {
                Utils.CmdSend(ref mScope, ":FFT:PHASe:REFerence", value, value, "Check for the fft phase reference scpi -" + value);
            }
            
        }

        //Riya -add this test
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Label_scpi()
        {
            //:FUNCtion<m>:LABel
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            string[] labels = { "A", "AMP", "AMPLITUDE" };
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                Wait.Seconds(1);
                foreach (string label in labels)
                {
                    string val = "\"" + label + "\"";
                    mScope.Send(":FUNCtion" + i + ":LABel " + val);
                    string result = mScope.ReadString(":FUNCtion" + i + ":LABel?");
                    Chk.Val(result, val, "Check for the function label scpi");
                }
                mScope.Send(dis + "0");
            }
        }
        
        [Test]
        public void FunctionFrequency_scpi()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            double[] values = { 1, 3.2e3, 5.3e5, 2.3e7, 1e9 };
            //:FUNCtion<m>:FREQuency:BANDpass:CENTer
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str=":FUNCtion"+i+":OPERation BAND";
                string s = ":FUNCtion" + i + ":FREQuency:BANDpass:CENTer";
                mScope.Send(str);
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "check for frequency bandpass center function");
                    Wait.Seconds(1);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:FREQuency:BANDpass:WIDTh
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation BAND";
                string s = ":FUNCtion" + i + ":FREQuency:BANDpass:WIDTh";
                mScope.Send(str);
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "Check for frequency bandpass width function");
                    Wait.Seconds(1);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:FREQuency:HIGHpass

            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation HIGH";
                string s = ":FUNCtion" + i + ":FREQuency:HIGHpass";
                mScope.Send(str);
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "Check for frequency high pass function");
                    Wait.Seconds(1);
                }
                mScope.Send(dis + "0");
            }


            //:FUNCtion<m>:FREQuency:LOWPass
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation LOW";
                string s = ":FUNCtion" + i + ":FREQuency:LOWPass";
                mScope.Send(str);
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "Check for frequency low pass function");
                    Wait.Seconds(1);
                }
                mScope.Send(dis + "0");

            }
        }

        
        [Test]
        public void Function_Average_scpi()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            int val;
            //:FUNCtion<m>:AVERage:COUNt
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation AVER";
                string s = ":FUNCtion" + i + ":AVERage:COUNt";
                mScope.Send(str);
                for (int j = 2; j <= 10; j++)
                {
                    val = Utils.GenrateRandomInRange_Int(2, 65536);
                    Utils.CmdSend(ref mScope, s, val, val, "check for average count ");
                }
                mScope.Send(dis + "0");
            }
        }


        [Test]
        public void function_Integrate_scpi()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            //:FUNCtion<m>:INTegrate:IICondition
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation INT";
                string s = ":FUNCtion" + i + ":INTegrate:IICondition";
                mScope.Send(str);
                int[] values = { 0, 1 };
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "check for initial condition setting ");
                }
                mScope.Send(dis + "0");
            }


            //:FUNCtion<m>:INTegrate:IOFFset
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation INT";
                string s = ":FUNCtion" + i + ":INTegrate:IOFFset";
                mScope.Send(str);
                for (int j = 0; j < 5; j++)
                {
                    double x = Utils.GenrateRandomInRange_Double(-10, 10);
                    Utils.CmdSend(ref mScope, s, x, x, "check for offset value");
                }
                mScope.Send(dis + "0");
            }

        }


        [Test]
        public void function_linear_scpi()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            double[] values = { -1e9, 1e9, 12.3e6, 3.2e3, 4.8, 8.4e-3, 92.3e-6 };
            //:FUNCtion<m>:LINear:GAIN
            for (int i = 1; i <= mMaxChan; i++)
            {
               
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation LIN";
                string s = ":FUNCtion" + i + ":LINear:GAIN";
                mScope.Send(str);
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "check for linear gain");
                }
                mScope.Send(dis + "0");
            }


            //:FUNCtion<m>:LINear:OFFSet
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string str = ":FUNCtion" + i + ":OPERation LIN";
                string s = ":FUNCtion" + i + ":LINear:OFFSet";
                mScope.Send(str);
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "check for linear offset");
                }
                mScope.Send(dis + "0");
            }
        }



        [Test]
        public void function_range_scpi()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            double[] values = { 1e-15, 2.3e-12, 3.2e-9, 4.5e-6, 6.7e3, 8.9e6, 5.2e9, 1e12 };
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string s = ":FUNCtion" + i + ":RANGe";
                for (int j = 0; j < values.Length; j++)
                {
                    
                    Utils.CmdSend(ref mScope, s, 8*values[j], 8*values[j], "check for range");
                }
                mScope.Send(dis + "0");
            }
        }




        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void FunctionFFT_scpi()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            //:FUNCtion<m>[:FFT]:DETection:POINts

            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string oper = ":FUNCtion" + i + "OPER FFT";
                mScope.Send(oper);
                string s = ":FUNCtion" + i + ":FFT:DETection:POINts";
                for (int j = 0; j < 10; j++)
                {
                    int x = Utils.GenrateRandomInRange_Int(640, 64000);
                    Utils.CmdSend(ref mScope, s, x, x, "check for fft detection points scpi command");
                }
                mScope.Send(dis + "0");
            }


            //:FUNCtion<m>[:FFT]:DETection:TYPE

            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string oper = ":FUNCtion" + i + "OPER FFT";
                mScope.Send(oper);
                string s = ":FUNCtion" + i + ":FFT:DETection:TYPE";
                string[] values = { "OFF", "SAMPle", "PPOSitive", "PNEGative", "NORMal", "AVERage" };
                foreach (string value in values)
                {
                    Utils.CmdSend_startsWith(ref mScope, s, value, value, "Check for the fft detection type");
                }
                mScope.Send(dis + "0");
            }

            /*
            const double MIN_START_STOP_FREQ = -50E+9;
            const double MAX_START_STOP_FREQ = 50E+9;
            double startFreq = 0;
            double stopFreq = 0;
            const double freqTol = 25e3;


            //:FUNCtion<m>[:FFT]:FREQuency:STARt
            for (int i = 1; i <= mMaxChan; i++)
            {
                string s = ":FUNCtion" + i + ":FFT:FREQuency:STARt";
                for (int j = 0; j < 5; j++)
                {
                    startFreq = MIN_START_STOP_FREQ + ((MAX_START_STOP_FREQ - MIN_START_STOP_FREQ) / 5 * i);
                    Utils.CmdSend(ref mScope, s, startFreq, startFreq,freqTol, "check for fft frequency start scpi command ");
                }

            }


            //:FUNCtion<m>[:FFT]:FREQuency:STOP
            for (int i = 1; i <= mMaxChan; i++)
            {
                string s = ":FUNCtion" + i + ":FFT:FREQuency:STOP";
                for (int j = 0; j < 5; j++)
                {
                    stopFreq = MIN_START_STOP_FREQ + ((MAX_START_STOP_FREQ - MIN_START_STOP_FREQ) / 5 * i);
                    stopFreq = startFreq + 50e9;
                    if (stopFreq > 50e9)
                    {
                        stopFreq = 50e9;
                    }
                    Utils.CmdSend(ref mScope, s, stopFreq, stopFreq,freqTol, "check for fft frequency stop scpi command ");
                }
            }
            */


            //:FUNCtion<m>[:FFT]:GATE
            mScope.Send(":TIMebase:MODE WINDow");
            Wait.MilliSeconds(1000);
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string s = ":FUNCtion" + i + ":FFT:GATE";
                string oper = ":FUNCtion" + i + ":OPERation FFT";
                mScope.Send(oper);
                string[] values = { "NONE", "ZOOM" };
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, s, values[j], values[j], "check for fft gate scpi command ");
                }
                mScope.Send(dis + "0");
            }
            mScope.Send(":TIMebase:MODE MAIN");


            //:FUNCtion<m>[:FFT]:PHASe:REFerence
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string s = ":FUNCtion" + i + ":FFT:PHASe:REFerence";
                string[] values = { "TRIGger", "DISPlay", "TRIG", "DISP" };
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend_startsWith(ref mScope, s, values[j], values[j], "check for fft phase referance scpi command ");
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:READout<n>
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string s = ":FUNCtion" + i + ":FFT:READout";
                for (int j = 1; j <= 2; j++)
                {
                    string[] values = { "SRATe", "BSIZe", "RBWidth", "SRAT", "BSIZ", "RBW" };
                    foreach (string value in values)
                    {
                        Utils.CmdSend_startsWith(ref mScope, s + j, value, value, "check for fft readout scpi command");
                    }

                }
                mScope.Send(dis + "0");
            }


            //:FUNCtion<m>[:FFT]:SPAN
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                ScpiError err;
                string s = ":FUNCtion" + i + ":FFT:SPAN";
                for (int j = 0; j < 6; j++)
                {
                    double x = Utils.GenrateRandomInRange_Double(1, 50e9);
                    mScope.Send(s + " " + x);
                    err = mScope.ReadError();
                    Chk.Val(err.ErrorCode, 0, "Check for the error in fft span scpi command");
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:CENTer
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                double start = mScope.ReadNumberAsDouble(":FUNCtion" + i + ":FREQuency:STARt?");
                double stop = mScope.ReadNumberAsDouble(":FUNCtion" + i + ":FREQuency:STOP?");
                double center = mScope.ReadNumberAsDouble(":FUNCtion" + i + ":FFT:CENTer?");
                Chk.Val((stop + start) / 2, center, "Check for the function fft center scpi");
                for (int j = 0; j < 4; j++)
                {
                    double x = Utils.GenrateRandomInRange_Double(-24e9, 24e9);
                    Utils.CmdSend(ref mScope, ":FUNCtion" + i + ":FFT:CENTer", Math.Round(x), Math.Round(x), "Check for the func fft center scpi -" + x);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion[:FFT]:WINDow
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string[] values = { "RECT", "HANN", "FLAT", "BHAR", "BART", "RECTangular", "HANNing", "FLATtop", "BHARris", "BARTlett" };
                string cmd = ":FUNCtion" + i + ":FFT:WINDow";
                foreach (string value in values)
                {
                    Utils.CmdSend_startsWith(ref mScope, cmd, value, value, "check for the func fft window scpi -" + value);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:BSIZe?
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                mScope.Send(":function" + i + ":OPER FFT");
                mScope.Send(":FUNCtion" + i + ":FFT:READout BSIZ");
                ScpiError err;
                mScope.Send("*CLS");
                double binSize = mScope.ReadNumberAsDouble(":FUNCtion" + i + ":FFT:BSIZe?");
                Pass.Condition(Is.LessOrEqual(binSize, 9.9999E36), "is not a garbage value");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in function binsize scpi command");
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:RBWidth?
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                mScope.Send(":function" + i + ":OPER FFT");
                mScope.Send(":FUNCtion" + i + ":FFT:READout RBW");
                ScpiError err;
                mScope.Send("*CLS");
                double binSize = mScope.ReadNumberAsDouble(":FUNCtion" + i + ":FFT:RBWidth?");
                Pass.Condition(Is.LessOrEqual(binSize, 9.9999E36), "is not a garbage value");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in function rbwidth scpi command");
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:SRATe?
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                mScope.Send(":function" + i + ":OPER FFT");
                mScope.Send(":FUNCtion" + i + ":FFT:READout SRAT");
                ScpiError err;
                mScope.Send("*CLS");
                double binSize = mScope.ReadNumberAsDouble(":FUNCtion" + i + ":FFT:SRATe?");
                Pass.Condition(Is.LessOrEqual(binSize, 9.9999E36), "is not a garbage value");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in function rbwidth scpi command");
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:VTYPe -magnitude
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                mScope.Send(":function" + i + ":OPER FFT");
                string cmd = ":FUNCtion" + i + ":FFT:VTYPe";
                string[] values = { "DEC", "VRMS", "DECibel" };
                foreach (string val in values)
                {
                    Utils.CmdSend_startsWith(ref mScope, cmd, val, val, "check for the vtype -" + val);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>[:FFT]:VTYPe -phase;
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                mScope.Send(":function" + i + ":OPER FFTP");
                string cmd = ":FUNCtion" + i + ":FFT:VTYPe";
                string[] values = { "DEGR", "RAD", "DEGRees", "RADians" };
                foreach (string val in values)
                {
                    Utils.CmdSend_startsWith(ref mScope, cmd, val, val, "check for the vtype phase -" + val);
                }
                mScope.Send(dis + "0");
            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void FunctionCommands()
        {
            mScope.Send(":AUToscale:CHANnels DISP");
            mScope.Send(":AUToscale");
            Wait.Seconds(2);
            //:FUNCtion<m>:DISPlay
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display";
                Utils.CmdSend(ref mScope, dis, 1, 1, "Check for the function display scpi");
                Utils.CmdSend(ref mScope, dis, 0, 0, "Check for the function display scpi");
                Utils.CmdSend(ref mScope, dis, 1, 1, "Check for the function display scpi");
            }

            //:FUNCtion<m>:LABel
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                string[] values = { "abc", "x", "ajehr;iaeknvfia;euraknva;ioyra;hn;agdfaiuer;ahsd;fayeowh" };
                string cmd = ":FUNCtion" + i + ":LABel";
                foreach (string value in values)
                {
                    if (value.Length > 32)
                    {
                        mScope.Send(cmd + " \"{0}\"", value);
                        string val = mScope.ReadString(cmd + "?");
                        string res= "\""+value.Substring(0, 32)+"\"" ;
                        Chk.Val(res, val, "check for the label scpi -" + value);
                    }
                    else
                    {
                        mScope.Send(cmd + " \"{0}\"", value);
                        string val = mScope.ReadString(cmd + "?");
                        string res = "\"" + value + "\"";
                        Chk.Val(res, val, "check for the label scpi -" + value);
                    }
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:OPERation
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                string cmd = ":FUNCtion" + i + ":OPERation";
                string[] values = {"ADD","SUBT","MULT","DIV","INT","DIFF","FFT","FFTP","SQRT","MAGN","ABS","SQU","LN","LOG","EXP","TEN","LOWP","HIGH"
                                      ,"BAND","AVER","SMO","ENV","LIN","MAX","MIN","PEAK","MAXH","MINH","TREN"/*,"BTIM","BST","SERC"*/};
                foreach (string value in values)
                {
                    Utils.CmdSend(ref mScope, cmd, value, value, "Check for the operation scpi -" + value);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:OFFSet
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string cmd = ":FUNCtion" + i + ":OFFSet";
                double[] values = { -3.5e12, -5.4e9, -8.3e6, 3.2e3, 6.4e9 ,3.5e12};
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, cmd, values[j], values[j], "Check for the offset");
                }
                mScope.Send(dis + "0");
            }


            //:FUNCtion<m>:SCALe
            for (int i = 1; i <= mMaxChan; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNC" + i + ":AUT");
                string cmd = ":FUNCtion" + i + ":SCALe";
                double[] values = { 1e-15, 1e12, 2.32e9, 4.52e6, 24.3, 3.2e-3, 7.8e-6 };
                for (int j = 0; j < values.Length; j++)
                {
                    Utils.CmdSend(ref mScope, cmd, values[j], values[j], "Check for the yscale");
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:CLEar
            for (int i = 1; i <= 4; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                string[] values = { "AVER", "MINH", "MAXH" };
                foreach (string value in values)
                {
                    mScope.Send(":FUNCtion" + i + ":OPERation " + value);
                    ScpiError err;
                    mScope.Send("*CLS");
                    mScope.Send(":FFT:CLEar");
                    err = mScope.ReadError();
                    Chk.Val(err.ErrorCode, 0, "Check for the error in function clear scpi command");
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>?
            for (int i = 1; i <= 4; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                ScpiError err;
                mScope.Send("*CLS");
                mScope.Send(":FUNCtion" + i + "?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in function query");
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:SOURce1
            for (int i = 1; i <= 4; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                string cmd = ":FUNCtion" + i + ":SOURce1";
                for (int j = 1; j <= 4; j++)
                {
                    string s = "CHAN" + j;
                    Utils.CmdSend(ref mScope, cmd, s, s, "Check for the source scpi" + s);
                }
                for (int j = 1; j < i; j++)
                {
                    string s = "FUNC" + j;
                    Utils.CmdSend(ref mScope, cmd, s, s, "Check for the source scpi" + s);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:SMOoth:POINts
            for (int i = 1; i <= 4; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                string cmd = ":FUNCtion" + i + ":SMOoth:POINts";
                for (int j = 0; j < 4; j++)
                {
                    int num = Utils.GenrateRandomInRange_Int(1, 16383);
                    int n = 2 * num + 1;
                    Utils.CmdSend(ref mScope, cmd, n, n, "Check for the function smooth points -" + n);
                }
                mScope.Send(dis + "0");
            }

            //:FUNCtion<m>:SOURce2
            for (int i = 1; i <= 4; i++)
            {
                string dis = ":FUNCtion" + i + ":Display ";
                mScope.Send(dis + "1");
                mScope.Send(":FUNCtion" + i + ":OPER ADD");
                string cmd = ":FUNCtion" + i + ":SOURce2";
                for (int j = 1; j <= 4; j++)
                {
                    string s = "CHAN" + j;
                    Utils.CmdSend(ref mScope, cmd, s, s, "Check for the source scpi" + s);
                }
                for (int j = 1; j < i; j++)
                {
                    string s = "FUNC" + j;
                    Utils.CmdSend(ref mScope, cmd, s, s, "Check for the source scpi" + s);
                }
                mScope.Send(dis + "0");
            }
        }
    }
}