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
    class WaveGenTest : InfiniiVisionTest
    {
        int mTimeout = 20000;

        const int roundDigits = 3;
        const int secScopeWgenChan = 2;

        // wgen limits -> High-Z
        const double mWgenMaxAmp = 10.0;
        const double mWgenMinAmp = 100e-3;//20e-3;
        const double mWgenMaxOffs = 2.0;
        const double mWgenMinOffs = -2.00;
        const double mWgenMaxFreq = 100e+6;
        const double mWgenMinFreq = 10e3;

        double[] frequency = { 10e3, 10e5, 10e6 };

        [SetUp]
        public void Setup()
        {
            mScope.Write("*RST");
            WaitForOpc(ref mScope, mTimeout);
            Waveform wfm = new Waveform(Shape.Sine, 10000000, 1.00);
            FgensSetWaveform(wfm);
        }

        /// <summary>
        /// This test verifies default wavegen settings after 
        /// scope reset
        /// wgen reset
        /// wgen reset wgen<n> scpi format
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DefaultWaveGen()
        {
            // Verify wavegen default settings after wavegen reset
            mScope.Write(":WGEN:RST");
            WaitForOpc(ref mScope, mTimeout);
            checkDefaultSetting_wgen();

            // Verify wavegen default settings after wavegen reset with WGEN<N> scpi
            mScope.Write(":WGEN1:RST");
            WaitForOpc(ref mScope, mTimeout);
            checkDefaultSetting_wgen();
        }

        /// <summary>
        /// INV-5896
        /// </summary>
        /*
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Wavegen()
        {
            int nTestLoops = 3;
            int nCyclesOnScreen = 5;
            double freq = 0.0;
            double ampl = 0.0;
            double offset = 0.0;
            int dCycle = 20;
            string load = "ONEM";
            // wgen functions
            string[] wgenWaveforms = { "SIN", "SQU" };

            mScope.Write(":WGEN:OUTPUT ON");
            foreach (string waveform in wgenWaveforms)
            {
                for (int i = 0; i < nTestLoops; i++)
                {
                    GetTestParam(ref freq, ref ampl, ref offset, ref dCycle, i, nTestLoops);
                    ConfigureWgen(freq, ampl, offset, waveform, load);

                    mScopeA.Send(":blank");
                    Utils.ScopeAutoAdjust(mScopeA, freq, ampl, offset, nCyclesOnScreen, secScopeWgenChan);

                    string paramStr = "=======================\n" +
                                      "WaveGen " + secScopeWgenChan + "\n" +
                                      "=======================\n" +
                                      "Waveform - " + waveform + " :\n" +
                                      "Loop " + (i + 1) + "/" + nTestLoops + "\n\n" +
                                      "   Freq = " + freq + " Hz\n" +
                                      "   Ampl = " + ampl + " Vpp\n" +
                                      "   Offs = " + offset + " V\n" +
                                      "   Duty = " + dCycle + " %";

                    Utils.ShowScreenMsg(ref mScope, paramStr);

                    if (waveform == "SIN")
                    {
                        VerifySineSquareWaveform(secScopeWgenChan, ampl, offset, freq);
                        //VerifyFFTSin(secScopeWgenChan, freq, ampl);
                    }
                    else if (waveform == "SQU")
                    {
                        // add dutycycle as last parameter when functionality is available
                        VerifySineSquareWaveform(secScopeWgenChan, ampl, offset, freq, dCycle);
                    }
                }
            }

        }*/

        
        /// <summary>
        /// Check default wave gen settings
        /// </summary>
        public void checkDefaultSetting_wgen()
        {
            string expectedRes = ":WGEN:FUNC SIN;OUTP 0;FREQ +1.00000E+03;VOLT +500.0E-03;VOLT:OFFS +0.0E+00;:WGEN:OUTP:LOAD ONEM";

            string retRes = mScope.ReadString(":wgen?");
            Chk.Val(retRes, expectedRes, "Default WGen controls");
        }


        /// <summary>
        /// This tets covers the following wgen SCPIs:
        /// :wgen:output
        /// :wgen:function
        /// :wgen:frequency
        /// :wgen:voltage
        /// :wgen:voltage:offset
        /// :wgen:output:load
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void WaveGen_scpi()
        {
            const double MAX_FREQ_SQU = 50.0E+6;        // 50MHz
            const double MAX_FREQ_SIN = 100.0E+6;       // 100MHz
            const double MIN_FREQ = 100.0E-3;           // 100mHz
            const double MAX_AMP_ONEM = 10.0;           // 10Vpp
            const double MIN_AMP_ONEM = 20.0E-3;        // 20mVpp
            const double MAX_AMP_50ohms = 5.0;          // 10Vpp
            const double MIN_AMP_50ohms = 20.0E-3;      // 20mVpp

            //string[] outputLoad = { "FIFTy", "ONEMeg", "FIFT", "ONEM" };
            string[] outputLoad = { "ONEMeg", "ONEM" };
            string[] waveforms = { "SINusoid", "SQUare", "DC", "SIN", "SQU" };

            double maxAmp = 0.0, minAmp = 0.0;
            double maxFreq = 0.0;
            double randomVal;

            // Enable wgen if disabled
            int getState = mScope.ReadNumberAsInt32(":wgen:output?");
            if (getState == 0)
            {
                mScope.Write(":wgen:output 1");
            }

            // Verify settings set and get values with bot
            foreach (string load in outputLoad)
            {

                Utils.CmdSend_startsWith(ref mScope, ":wgen:output:load", load, load, String.Format("Output load - to {0}", load));

                if (load.StartsWith("ONEM"))
                {
                    maxAmp = MAX_AMP_ONEM;
                    minAmp = MIN_AMP_ONEM;
                }
                else if (load.StartsWith("FIFT"))
                {
                    maxAmp = MAX_AMP_50ohms;
                    minAmp = MIN_AMP_50ohms;
                }

                // Verify waveforms set and get values
                foreach (string func in waveforms)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":wgen:function", func, func, String.Format("Waveform - {0}", func));

                    // Verify Frequency, amplitude, offset for Max, Min and a random value in available range
                    if (func.StartsWith("SIN"))
                    {
                        maxFreq = MAX_FREQ_SIN;
                    }
                    if (func.StartsWith("SQU"))
                    {
                        maxFreq = MAX_FREQ_SQU;
                    }
                    if (func == "SIN" || func == "SQU")
                    {
                        randomVal = 28.556E+6;
                        Utils.CmdSend(ref mScope, ":wgen:frequency", randomVal, randomVal, String.Format("Wgen frequency - {0}", randomVal));
                        // enbale the below code line when INV-3783 is fixed
                        //CmdSend(ref mScope, ":wgen:frequency", minFreq, minFreq, String.Format("Wgen frequency - min value {0}", minFreq));
                        Utils.CmdSend(ref mScope, ":wgen:frequency", maxFreq, maxFreq, String.Format("Wgen frequency - max value {0}", maxFreq));
                        randomVal = 3.55;
                        Utils.CmdSend(ref mScope, ":wgen:voltage", randomVal, randomVal, String.Format("Wgen amplitude - {0}", randomVal));
                        if (mScope.ReadNumberAsDouble(":wgen:voltage?") > 50E-3 && mScope.ReadNumberAsDouble(":wgen:voltage:offset?") > 800E-3)
                        {
                            minAmp = 50e-3;
                        }
                        Utils.CmdSend(ref mScope, ":wgen:voltage", maxAmp, maxAmp, String.Format("Wgen amplitude - max value {0}", maxAmp));
                        Utils.CmdSend(ref mScope, ":wgen:voltage", minAmp, minAmp, String.Format("Wgen amplitude - min value {0}", minAmp));
                    }
                    randomVal = 2.56;
                    mScope.Write(":WGEN:DCMODE WID");
                    Utils.CmdSend(ref mScope, ":wgen:voltage:offset", randomVal, randomVal, String.Format("Wgen offset - {0}", randomVal));
                }
            }
        }



        #region Cnfiguration functions

        /// <summary>
        /// Generating freq, amp, offset, duty cycle parameters
        /// based on Min-Max ranges andnumber of combinations we need.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="ampl"></param>
        /// <param name="offset"></param>
        /// <param name="dCycle"></param>
        /// <param name="nLoop"></param> 
        /// <param name="nTestLoops"></param>
        private void GetTestParam(ref double freq, ref double ampl, ref double offset, ref int dCycle, int nLoop, int nTestLoops)
        {
            //freq = Utils.GenrateRandomInRange_Double(mWgenMinFreq, mWgenMaxFreq);  // frequency scpi currently fails on high resoluion input values
            //ampl = Utils.GenrateRandomInRange_Double(mWgenMinAmp, mWgenMaxAmp);
            //offset = Utils.GenrateRandomInRange_Double(mWgenMinOffs, mWgenMaxOffs);
            dCycle = Utils.GenrateRandomInRange_Int(20, 80);
            freq = mWgenMinFreq + ((mWgenMaxFreq - mWgenMinFreq) / nTestLoops * nLoop);
            ampl = mWgenMinAmp + ((mWgenMaxAmp - mWgenMinAmp) / nTestLoops * nLoop);
            offset = mWgenMinOffs + ((mWgenMaxOffs - mWgenMinOffs) / nTestLoops * nLoop);
            
        }


        /// <summary>
        /// Set Wave Gen parametrs
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="amp"></param>
        /// <param name="offset"></param>
        /// <param name="waveform"></param>
        /// <param name="load"></param>
        public void ConfigureWgen(double freq, double amp, double offset, string waveform, string load)
        {
            // Perform and check the wgen settings for easy debugging
            Utils.CmdSend(ref mScope, ":wgen:function", waveform, waveform, "Wgen function not set correctly");
            Utils.CmdSend(ref mScope, ":wgen:output:load", load, load, "Wgen outpul load not set correctly");
            if (waveform == "SIN" || waveform == "SQU")
            {
                Utils.CmdSend(ref mScope, ":wgen:frequency", freq, freq, "Wgen frequency not set correctly", true, 1);
                Utils.CmdSend(ref mScope, ":wgen:voltage", amp, amp, "Wgen amplitude not set correctly");
                Utils.CmdSend(ref mScope, ":wgen:voltage:offset", offset, offset, "Wgen offset not set correctly");
            }
            if (waveform == "SQU")
            {
                // set duty cycle for wgen here when the functionality is available
            }
            if (waveform == "DC")
            {
                Utils.CmdSend(ref mScope, ":wgen:voltage:offset", offset, offset, "Wgen offset not set correctly");
            }
        }

        #endregion


        #region Verification functions

        /// <summary>
        /// Verifies the amplitude, frequency, offset,
        /// duty cycle of a sin waveform.
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="expectedAmp"></param>
        /// <param name="expectedOffset"></param>
        /// <param name="expectedFrequency"></param>
        /// <param name="expectedDCycle"></param>
        public void VerifySineSquareWaveform(int chan, double expectedAmp, double expectedOffset, double expectedFrequency, double expectedDCycle = 50)
        {
            mScopeA.Send(":measure:vamplitude chan{0}", chan);
            mScopeA.Send(":measure:vaverage chan{0}", chan);
            mScopeA.Send(":measure:frequency chan{0}", chan);
            mScopeA.Send(":measure:dutycycle chan{0}", chan);

            WaitForOpc(ref mScopeA, mTimeout);

            double readAmpl = Meas.Amplitude(ref mScopeA, chan);
            double readOffs = Meas.Average(ref mScopeA, chan);
            double readFreq = Meas.Frequency(ref mScopeA, chan);
            double readDutyCycle = Meas.Duty(ref mScopeA, chan);

            Chk.Val(readAmpl, expectedAmp, GetAmpTol(expectedAmp), "Output Amplitude");
            Chk.Val(readOffs, expectedOffset, GetOffsetTol(expectedOffset), "Output Offset");
            Chk.Val(readFreq, expectedFrequency, 0.05 * expectedFrequency, "Output Frequency");
            Chk.Val(readDutyCycle, expectedDCycle, 2.0, "Output Duty Cycle");
        }


        /// <summary>
        /// Verifies the FFT of Sinusoidal input waveform.
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="freq"></param>
        /// <param name="amp"></param>
        public void VerifyFFTSin(int chan, double freq, double amp)
        {
            // Check frequency domain:
            mScopeA.Write(":FUNC:DISP 1");
            mScopeA.Write(":FUNC:OPER FFT");
            mScopeA.Write(":FUNC:SOURCE1 CHAN{0}", chan);
            mScopeA.Write(":FUNC:SPAN {0}", freq * 100);
            mScopeA.Write(":FUNC:CENTER {0}", freq * 1.5);
            mScopeA.Write(":FUNC:SCALE 50");
            mScopeA.Write(":FUNC:OFFSET 0");

            double fftMaxDb = Meas.MaxVolt(ref mScopeA, "MATH");
            double fftMaxHz = Meas.MaxTime(ref mScopeA, "MATH");

            Chk.Val(fftMaxHz, freq, 0.05 * freq, "Fundamental frequency");

            mScopeA.Write(":TIM:MODE WINDOW");
            double timeScale = mScopeA.ReadNumberAsDouble(":tim:scale?");

            mScopeA.Write(":TIM:WINDOW:SCALE {0}", timeScale / 2.1);
            mScopeA.Write(":TIM:WINDOW:POSITION {0}", timeScale / 2.0 * 5.0);
            double fftBaseDb = mScopeA.ReadNumberAsDouble(":MEAS:VMAX? MATH");
            double fftMargin = (amp <= 10e-3) ? 46.0 : 40.0;

            Chk.Cond(Is.LessThan(fftBaseDb, fftMaxDb - fftMargin), "2nd harmonic onwards must be 40db below the max");

            mScopeA.Write(":TIM:MODE NORMAL");
            mScopeA.Write(":FUNC:DISP 0");
        }

        #endregion

        #region "Get Tolerance" Functions

        private double GetAmpTol(double amp)
        {
            double accuracyPercent = 0.10;
            if (amp < 50e-3)
            {
                return 2 * 50e-3;
            }
            else
            {
                return Math.Abs(amp * accuracyPercent);
            }
        }


        private double GetOffsetTol(double offset)
        {
            double accuracyPercent = 0.10;
            if (-20e-3 < offset && offset < 20e-3)
            {
                return 2 * 20e-3;
            }
            else
            {
                return Math.Abs(offset * accuracyPercent);
            }
        }

        #endregion


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void WaveGenModulation_Scpi()
        {
            //:WGEN<w>:FUNCtion
            string[] sources = { "SIN", "SQU", "RAMP", "PULS", "DC", "NOIS", "SINC", "EXPR", "EXPF", "CARD", "GAUS", "ARB" };
            foreach (string source in sources)
            {
                Utils.CmdSend(ref mScope, ":WGEN1:FUNCtion", source, source, "Check for the wave gen function -" + source);
                if (source.Equals("SIN") || source.Equals("RAMP") || source.Equals("SINC") || source.Equals("EXPR") || source.Equals("EXPF") || source.Equals("CARD"))
                {
                    //:WGEN<w>:MODulation:STATe
                    Utils.CmdSend(ref mScope, ":WGEN1:MODulation:STATe", 1, 1, "Check for the modulation state");
                    Utils.CmdSend(ref mScope, ":WGEN1:MODulation:STATe", 0, 0, "Check for the modulation state");
                    Utils.CmdSend(ref mScope, ":WGEN1:MODulation:STATe", 1, 1, "Check for the modulation state");

                    //:WGEN<w>:MODulation:TYPE
                    string[] types = { "AM", "FM", "FSK" };
                    foreach (string type in types)
                    {
                        Utils.CmdSend(ref mScope, ":WGEN1:MODulation:TYPE", type, type, "Check for the modulation type scpi");
                        if (type.Equals("AM"))
                        {
                            //:WGEN<w>:MODulation:AM:DEPTh
                            for (int i = 0; i < 4; i++)
                            {
                                int num = Utils.GenrateRandomInRange_Int(0, 100);
                                Utils.CmdSend(ref mScope, ":WGEN1:MODulation:AM:DEPTh", num, num, "Check for the modulation am depth scpi");
                            }

                            //:WGEN<w>:MODulation:AM:FREQuency
                            double[] freq = { 1, 100, 450, 900, 1.2e3, 20e3 };
                            for (int i = 0; i < freq.Length; i++)
                            {
                                Utils.CmdSend(ref mScope, ":WGEN1:MODulation:AM:FREQuency", freq[i], freq[i], "Check for the modulation am frequency scpi -" + freq[i]);
                            }
                        }
                        else if (type.Equals("FM"))
                        {
                            //:WGEN<w>:MODulation:FM:FREQuency
                            double[] freq = { 1, 100, 450, 900, 1.2e3, 20e3 };
                            for (int i = 0; i < freq.Length; i++)
                            {
                                Utils.CmdSend(ref mScope, ":WGEN1:MODulation:FM:FREQuency", freq[i], freq[i], "Check for the modulation fm frequency scpi -" + freq[i]);
                            }

                            //:WGEN<w>:MODulation:FM:DEViation
                            double[] deviation = { 1, 10, 15, 24, 45, 65 };
                            for (int i = 0; i < deviation.Length; i++)
                            {
                                Utils.CmdSend(ref mScope, ":WGEN1:MODulation:FM:DEViation", deviation[i], deviation[i], "Check for the modulation fm deviation -" + deviation[i]);
                            }
                        }
                        else
                        {
                            //:WGEN<w>:MODulation:FSKey:FREQuency
                            double[] freq = { 2e3, 4.342e3, 53.34e3, 999.43e3, 10e6 };
                            for (int i = 0; i < freq.Length; i++)
                            {
                                double r = mScope.ReadNumberAsDouble(":WGEN1:MODulation:FSKey:RATE?");
                                if (freq[i] >= 2 * r)
                                {
                                    Utils.CmdSend(ref mScope, ":WGEN1:MODulation:FSKey:FREQuency", freq[i], freq[i], "Check for the modulation fskey frequency -" + freq[i]);
                                }
                            }

                            //:WGEN<w>:MODulation:FSKey:RATE
                            double[] rate = { 1, 100, 450, 900, 1.2e3, 20e3 };
                            for (int i = 0; i < rate.Length; i++)
                            {
                                Utils.CmdSend(ref mScope, ":WGEN1:MODulation:FSKey:RATE", rate[i], rate[i], "Check for the modulation fskey rate -" + rate[i]);
                            }
                        }
                    }
                    /*
                    //:WGEN<w>:MODulation:FUNCtion
                    string[] functions = { "SIN", "SQU", "RAMP", "SINusoid", "SQUare" };
                    foreach (string func in functions)
                    {
                        Utils.CmdSend_startsWith(ref mScope, ":WGEN1:MODulation:FUNCtion", func, func, "Check for the modulation function -" + func);
                        if (func.Equals("RAMP"))
                        {
                            //:WGEN<w>:MODulation:FUNCtion:RAMP:SYMMetry
                            for (int i = 0; i < 4; i++)
                            {
                                int percent = Utils.GenrateRandomInRange_Int(0, 100);
                                Utils.CmdSend(ref mScope, ":WGEN1:MODulation:FUNCtion:RAMP:SYMMetry", percent, percent, "Check for the modulation function ramp symmetry ");
                            }
                        }
                    }
                    */
                }

                //:WGEN<w>:MODulation:NOISe
                if (!source.Equals("NOIS"))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int num = Utils.GenrateRandomInRange_Int(0, 100);
                        Utils.CmdSend(ref mScope, ":WGEN1:MODulation:NOISe", num, num, "Check for the wgen modulation noise - " + num);
                    }
                }
            }
        }
    }
}
