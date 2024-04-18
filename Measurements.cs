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
    class Measurements : InfiniiVisionTest
    {
        int mMaxChan;
        int mTimeout = 20000;
        
        [SetUp]
        public void Setup()
        {
            mMaxChan = IsScopeFourChan ? 4 : 2;
            // Reset all devices
            mScope.Write("*CLS;*RST");
            mScopeA.Write("*CLS;*RST");
            mFgenA.Write("*CLS;*RST");
            mFgenB.Write("*CLS;*RST");
            mFgenC.Write("*CLS;*RST");
            mFgenD.Write("*CLS;*RST");

            WaitForOpc(ref mScope, mTimeout);
            WaitForOpc(ref mFgenA);
            WaitForOpc(ref mFgenB);
            WaitForOpc(ref mFgenC);
            WaitForOpc(ref mFgenD);
            WaitForOpc(ref mScopeA);

            // Output load set to High-Z on Fgens
            mFgenA.WriteLine(":OUTP:LOAD INF");
            mFgenB.WriteLine(":OUTP:LOAD INF");
            mFgenC.WriteLine(":OUTP:LOAD INF");
            mFgenD.WriteLine(":OUTP:LOAD INF");

            // Generate a default waveform on all fgens
            Waveform wfm = new Waveform(Shape.Sine, 10e6, 1.00);
            FgensSetWaveform(wfm);
        }
        
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DefaultMeasurementsSettings()
        {
            // check default settings
            // modify the code for 2 chan and 4 chan scope or the for loop used below will fail for 2 chan scope**********
            Chk.Val(mScope.ReadNumberAsInt32(":measure:show?"), 0, "Measurement marker tracking should be disabled by default");
            Chk.Val(mScope.ReadString(":measure:source?"), "CHAN1,CHAN2", "Measurement sources should be channel1 & channel2 by default");

            // Check default measurement threshold settings
            for (int chan = 1; chan <= mMaxChan; chan++)
            {
                Utils.CheckPercAbsMeasThresholds(ref mScope, chan, "THR", "PERC", 90.0, 50.0, 10.0);
            }
        }


        /// <summary>
        /// :measure:show
        /// :measure:source
        /// :measure:statistics
        /// :measure:clear
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Measurements_scpi()
        {
            // Check Set and Get values for Measurement SCPIs
            // :measure:show
            mScope.Send(":measure:statistics on");
            mScope.Send(":measure:frequency");
            Wait.Seconds(2);
            Failure.Condition(Is.NotEqual(mScope.ReadString(":measure:show?"), "1"), "Marker tracking not enabled for applied measurement");
            Utils.CmdSend(ref mScope, ":measure:show", "0", "0", "Marker tracking not disabled");
            string result = mScope.ReadString(":measure:results?");
            Pass.Condition(result.StartsWith("Frequency(1)"), "Read measurement is not correct");


            // :measure:source
            for (int source = 1; source <= mMaxChan; source++)
            {
                for (int source2 = 1; source2 <= mMaxChan; source2++)
                {
                    Utils.CmdSend(ref mScope, ":measure:source", "CHAN" + Convert.ToString(source) + "," + "CHAN" + Convert.ToString(source2),
                        "CHAN" + Convert.ToString(source) + "," + "CHAN" + Convert.ToString(source2), "Sources not set correctly");
                }
            }

            // Apply measurements and check if it was successfully applied
            mScope.Write(":measure:source CHAN1,CHAN2");
            mScope.Write(":measure:vmax");
            mScope.Write(":measure:vmin");
            mScope.Write(":measure:ris");
            mScope.Write(":measure:fall");
            mScope.Write(":measure:over");
            mScope.Write(":measure:acr");
            //mScope.Write(":measure:coun");
            mScope.Write(":measure:tvol");
            result = mScope.ReadString(":measure:results?");
            string[] measurements = { "Maximum(1)", "Minimum(1)", "Rise Time(1)", "Fall Time(1)", "Overshoot(1)" };
            foreach (string str in measurements)
            {
                Pass.Condition(result.Contains(str), str + " was applied successfully");
            }

            // :measure:statistics
            string[] stats = { "CURRent", "MINimum", "MAXimum", "MEAN", "STDDev", "COUNt" };
            foreach (string stat in stats)
            {
                Utils.CmdSend_startsWith(ref mScope, "measure:statistics", stat, stat, "Measurement statistics not enabled");
            }

            // :measure:clear
            mScope.Write(":measure:clear");
            Failure.Condition(Is.NotEqual(mScope.ReadString(":measure:show?"), "0"), "Measurements not cleared");

        }


        /// <summary>
        /// :measure:define
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void MeasureDefine_scpi()
        {
            // :measure:define threshold, perc
            double[] upper_perc = { 95.0, 70.0, 35.0, 90.0 };
            double[] middle_perc = { 55.0, 43.0, 20.0, 70.0 };
            double[] lower_perc = { 25.0, 15.0, 5.0, 60.0 };
            for (int i = 0; i <= 3; i++)
            {
                for (int chan = 1; chan <= mMaxChan; chan++)
                {
                    mScope.Send(":measure:define thresholds, percent, {0}, {1}, {2}, CHAN{3}", upper_perc[i], middle_perc[i], lower_perc[i], chan);
                    Utils.CheckPercAbsMeasThresholds(ref mScope, chan, "THR", "PERC", upper_perc[i], middle_perc[i], lower_perc[i]);
                }
            }

            // :measure:define threshold, absolute
            double[] upper_abs = { 15.0, 12.0, 10.0, 50e-3, -5.0, -2.0, -1.0 };
            double[] middle_abs = { 10.0, 7.0, 5.0, 0.0, -10.0, -7.0, -5.0 };
            double[] lower_abs = { 5.0, 2.0, 1.0, -50e-3, -15.0, -12.0, -10.0 };
            for (int i = 0; i <= upper_abs.Length - 1; i++)
            {
                for (int chan = 1; chan <= mMaxChan; chan++)
                {
                    mScope.Send(":measure:define thresholds, absolute, {0}, {1}, {2}, CHAN{3}", upper_abs[i], middle_abs[i], lower_abs[i], chan);
                    Utils.CheckPercAbsMeasThresholds(ref mScope, chan, "THR", "ABS", upper_abs[i], middle_abs[i], lower_abs[i]);
                }
            }

            // :measure:define delay
            string[] edgeSpecSlope = { "+", "-" };
            int occurence1, occurence2;
            string readValue, setValue;
            for (int chan = 1; chan <= mMaxChan; chan++)
            {
                foreach (string edge1Slope in edgeSpecSlope)
                {
                    foreach (string edge2Slope in edgeSpecSlope)
                    {
                        occurence1 = Utils.rnd.Next(1, 10);
                        occurence2 = Utils.rnd.Next(1, 10);
                        setValue = edge1Slope + occurence1 + "," + edge2Slope + occurence2;
                        mScope.Send(":measure:define delay, {0}, CHAN{1}", setValue, chan);
                        readValue = mScope.ReadString(":measure:define? delay");

                        Pass.Condition(Is.Equal(readValue, setValue), String.Format(":measure:define delay edges not set to {0}", setValue));
                    }
                }
            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void VAmplitude()
        {
            double[] mVpp = { 100e-3, 500e-3, 1.0, 5.0, 8.0, 10.0 };
            Shape[] shapes = { Shape.Square, Shape.Sine };
            double vMax, vMin, vpp, vamp;

            for (int chan = 1; chan <= 4; chan++)
            {
                for (int j = 0; j < shapes.Length; j++)
                {
                    for (int i = 0; i < mVpp.Length; i++)
                    {
                        Message.Generate(String.Format("Channel -> {0} \n Waveform Shape -> {1} \n Amplitude -> {2}", chan, shapes[j], mVpp[i]));
                        Waveform wfm = new Waveform(shapes[j], 1e6, mVpp[i]);
                        FgensSetWaveform(wfm);

                        mScope.Send(":blank");
                        Utils.ScopeAutoAdjust(mScope, 1e6, mVpp[i], 0.0, 10, chan);

                        mScope.Send(":measure:vpp chan{0}", chan);
                        mScope.Send(":measure:vamplitude chan{0}", chan);
                        mScope.Send(":measure:vmax chan{0}", chan);
                        mScope.Send(":measure:vmin chan{0}", chan);

                        WaitForOpc(ref mScope, mTimeout);
                        vpp = Meas.PeakToPeak(ref mScope, chan);
                        vamp = Meas.Amplitude(ref mScope, chan);
                        vMax = Meas.MaxVolt(ref mScope, chan);
                        vMin = Meas.MinVolt(ref mScope, chan);

                        Chk.Val(vamp, mVpp[i], GetAmpTol(mVpp[i]), "Read amplitude");
                        Chk.Val(vpp, mVpp[i], GetAmpTol(mVpp[i]), "Read Vpp");
                        //Chk.Cond(Is.GreaterThan(vpp, vamp), "Vamp is larger than Vpp");
                        Chk.Val(vpp, vMax - vMin, 50e-3, "Read Vpp = Vmax-Vmin");

                        mScope.Send(":measure:clear");
                    }
                }
            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void RiseTime()
        {
            double freq = 1e5;
            double amp = 500e-3;
            double offset = 0;
            int nCyclesOnScreen = 10;
            double period = 1 / freq;

            double retRise = 0.0;
            double expRise = 0.0;
            double tol = 0.016; //1.6% tolerance

            Shape[] shapes = { Shape.Sine };

            for (int chan = 1; chan <= 4; chan++)
            {
                for (int j = 0; j < shapes.Length; j++)
                {
                    Message.Generate(String.Format("Channel -> {0} \n Waveform Shape -> {1} \n Amplitude -> {2} Volts", chan, shapes[j], 1.0));
                    Waveform wfm = new Waveform(shapes[j], freq, amp);
                    FgensSetWaveform(wfm);

                    mScope.Send(":blank");
                    Utils.ScopeAutoAdjust(mScope, freq, amp, offset, nCyclesOnScreen, chan);
                    
                    expRise = period * 0.3;
                    retRise = Meas.RiseTime(ref mScope, chan);
                    Chk.Val(retRise, expRise, expRise * tol, "Rise time measurement on channel " + chan);
                    retRise = Meas.FallTime(ref mScope, chan);
                    Chk.Val(retRise, expRise, expRise * tol, "Fall time measurement on channel " + chan);

                    mScope.Send(":measure:clear");
                }
            }
        }


        private double GetAmpTol(double amp)
        {
            double accuracyPercent = 0.1;
            if (amp < 1.0)
            {
                return 50e-3;
            }
            else
            {
                return Math.Abs(amp * accuracyPercent);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void MeasureHistogramHor()
        {
            double freq = 10000;
            double amp = 1;
            Waveform wfm = new Waveform(Shape.Sine, freq, amp);
            FgensSetWaveform(wfm);
            //:MEASure:HISTogram:MEDian
            //:MEASure:HISTogram:PPEak
            //:MEASure:HISTogram:MAXimum
            //:MEASure:HISTogram:MINimum
            //:MEASure:HISTogram:BWIDth
            //:MEASure:HISTogram:MEAN
            //:MEASure:HISTogram:MODE
            //:MEASure:HISTogram:PEAK
            //:MEASure:HISTogram:HITS
            //:MEASure:HISTogram:M1S
            //:MEASure:HISTogram:M2S
            //:MEASure:HISTogram:M3S
            //:MEASure:HISTogram:SDEViation
            for (int i = 1; i <= 4; i++)
            {
                mScope.Send(":CHAN" + i + ":DISP 1");
                int cycles = 5;
                Utils.ScopeAutoAdjust(mScope, freq, amp, 0, cycles, i);
                

                mScope.Send(":HISTogram:DISPlay 1");
                mScope.Send(":HISTogram:TYPE HOR");
                mScope.Send(":HISTogram:WINDow:SOURce CHAN" + i);
                mScope.Send(":HISTogram:WINDow:TLIMit 600e-3");
                mScope.Send(":HISTogram:WINDow:BLIMit -600e-3");

                double timescale = mScope.ReadNumberAsDouble("TIM:SCALE?");

                double median = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MEDian?");
                Chk.Val(median, 0, 0.08 * 8 * timescale, "Verify Histogram Median value");

                double ptop = mScope.ReadNumberAsDouble(":MEASure:HISTogram:PPEak?");
                Chk.Val(ptop, 8* timescale , 0.08 * 8 * timescale, "verify histogram p-p value");

                double min = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MINimum?");
                Chk.Val(min, -4 * timescale, 0.08 * 4 * timescale, "verify histogram min value");

                double max = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MAXimum?");
                Chk.Val(max, 4 * timescale, 0.08 * 4 * timescale, "verify histogram max value");

                double bwidth = mScope.ReadNumberAsDouble(":MEASure:HISTogram:BWIDth?");
                Pass.Condition(Is.LessOrEqual(bwidth, 9.9999E36), "is not a garbage value bwidth");

                double mean = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MEAN?");
                Pass.Condition(Is.LessOrEqual(mean, 9.9999E36), "is not a garbage value mean");

                double mode = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MODE?");
                Pass.Condition(Is.LessOrEqual(mode, 9.9999E36), "is not a garbage value mode");

                double phits = mScope.ReadNumberAsDouble(":MEASure:HISTogram:PEAK?");
                Pass.Condition(Is.LessOrEqual(phits, 9.9999E36), "is not a garbage value phits");

                double hits = mScope.ReadNumberAsDouble(":MEASure:HISTogram:HITS?");
                Pass.Condition(Is.LessOrEqual(hits, 9.9999E36), "is not a garbage value hits");

                double m1 = mScope.ReadNumberAsDouble(":MEASure:HISTogram:M1S?");
                Pass.Condition(Is.Between(m1,0, 100), "is in between 0 and 100");

                double m2 = mScope.ReadNumberAsDouble(":MEASure:HISTogram:M2S?");
                Pass.Condition(Is.Between(m2, 0, 100), "is in between 0 and 100");

                double m3 = mScope.ReadNumberAsDouble(":MEASure:HISTogram:M3S?");
                Pass.Condition(Is.Between(m3, 0, 100), "is in between 0 and 100");

                double deviation = mScope.ReadNumberAsDouble(":MEASure:HISTogram:SDEViation?");
                Pass.Condition(Is.LessOrEqual(deviation, 9.9999E36), "is not a garbage value deviation");

                mScope.Send(":HISTogram:DISPlay 0");
                mScope.Send(":CHAN" + i + ":DISP 0");

            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void MeasureHistogramVert()
        {
            double freq = 10000000;
            double amp = 1;   //it is peak to peak value.
            Waveform wfm = new Waveform(Shape.Sine, freq, amp);
            FgensSetWaveform(wfm);
            //:MEASure:HISTogram:MEDian
            //:MEASure:HISTogram:PPEak
            //:MEASure:HISTogram:MAXimum
            //:MEASure:HISTogram:MINimum
            //:MEASure:HISTogram:BWIDth
            //:MEASure:HISTogram:MEAN
            //:MEASure:HISTogram:MODE
            //:MEASure:HISTogram:PEAK
            //:MEASure:HISTogram:HITS
            //:MEASure:HISTogram:M1S
            //:MEASure:HISTogram:M2S
            //:MEASure:HISTogram:M3S
            //:MEASure:HISTogram:SDEViation
            for (int i = 1; i <= 4; i++)
            {
                mScope.Send(":CHAN" + i + ":DISP 1");
                int cycles = 5;
                Utils.ScopeAutoAdjust(mScope, freq, amp, 0, cycles, i);

                mScope.Send(":HISTogram:DISPlay 1");
                mScope.Send(":HISTogram:TYPE VERT");
                mScope.Send(":HISTogram:WINDow:SOURce CHAN" + i);
                mScope.Send(":HISTogram:WINDow:TLIMit 600e-3");
                mScope.Send(":HISTogram:WINDow:BLIMit -600e-3");

                double median = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MEDian?");
                Chk.Val(median, 0, 0.08 * amp, "verify the median value");

                double ptop = mScope.ReadNumberAsDouble(":MEASure:HISTogram:PPEak?");
                Chk.Val(ptop, amp, 0.08 * amp, "verify histogram p-p value");

                double min = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MINimum?");
                Chk.Val(min, -0.5 * amp, 0.08 * amp, "verify histogram min value");

                double max = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MAXimum?");
                Chk.Val(max, 0.5 * amp, 0.08 * amp, "verify histogram max value");

                double bwidth = mScope.ReadNumberAsDouble(":MEASure:HISTogram:BWIDth?");
                Pass.Condition(Is.LessOrEqual(bwidth, 9.9999E36), "is not a garbage value bwidth");

                double mean = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MEAN?");
                Pass.Condition(Is.LessOrEqual(mean, 9.9999E36), "is not a garbage value mean");

                double mode = mScope.ReadNumberAsDouble(":MEASure:HISTogram:MODE?");
                Pass.Condition(Is.LessOrEqual(mode, 9.9999E36), "is not a garbage value mode");

                double phits = mScope.ReadNumberAsDouble(":MEASure:HISTogram:PEAK?");
                Pass.Condition(Is.LessOrEqual(phits, 9.9999E36), "is not a garbage value phits");

                double hits = mScope.ReadNumberAsDouble(":MEASure:HISTogram:HITS?");
                Pass.Condition(Is.LessOrEqual(hits, 9.9999E36), "is not a garbage value hits");

                double m1 = mScope.ReadNumberAsDouble(":MEASure:HISTogram:M1S?");
                Pass.Condition(Is.Between(m1,0, 100), "is in between 0 and 100");

                double m2 = mScope.ReadNumberAsDouble(":MEASure:HISTogram:M2S?");
                Pass.Condition(Is.Between(m2, 0, 100), "is in between 0 and 100");

                double m3 = mScope.ReadNumberAsDouble(":MEASure:HISTogram:M3S?");
                Pass.Condition(Is.Between(m3, 0, 100), "is in between 0 and 100");

                double deviation = mScope.ReadNumberAsDouble(":MEASure:HISTogram:SDEViation?");
                Pass.Condition(Is.LessOrEqual(deviation, 9.9999E36), "is not a garbage value deviation");

                mScope.Send(":HISTogram:DISPlay 0");
                mScope.Send(":CHAN" + i + ":DISP 0");

            }
        }

        // add this test
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void MeasureStatistics_scpi()
        {
            //:MEASure:STATistics:DISPlay
            Utils.CmdSend(ref mScope, ":MEASure:STATistics:DISPlay", 1, 1, "check for the measure statistics display scpi");
            Utils.CmdSend(ref mScope, ":MEASure:STATistics:DISPlay", 0, 0, "check for the measure statistics display scpi");
            Utils.CmdSend(ref mScope, ":MEASure:STATistics:DISPlay", 1, 1, "check for the measure statistics display scpi");

            //:MEASure:STATistics
            string[] statistics = { "ON", "CURR", "MIN", "MAX", "MEAN", "STDD", "COUN" };
            foreach (string s in statistics)
            {
                Utils.CmdSend(ref mScope, ":MEASure:STATistics", s, s, "check for the measure statistics scpi ");
            }

            //:MEASure:STATistics:MCOunt
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(2, 2000);
                Utils.CmdSend(ref mScope, ":MEASure:STATistics:MCOunt", num, num, "check for the measure statistics mcount scpi");
            }

            //:MEASure:STATistics:RESet
            ScpiError err;
            mScope.Send("*CLS");
            mScope.Send(":MEASure:STATistics:RESet");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "check for the error in statistics reset scpi");
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void MeasureThreshould_scpi()
        {
            string[] sources = { "CHAN1", "CHAN2", "CHAN3", "CHAN4", "FUNC1", "FUNC2", "FUNC3", "FUNC4", "WMEM1", "WMEM2" };
            string[] methods = { "ABS", "PER", "ABSolute", "PERcent" };
            foreach (string source in sources)
            {
                mScope.Send(":" + source + ":DISP 1");
                if (source.StartsWith("WMEM"))
                {
                    mScope.Send(":" + source + ":SAVE CHAN1");
                }
                foreach (string method in methods)
                {
                    string value = method + "," + source;
                    //:MEASure:THResholds:METHod
                    mScope.Send(":MEASure:THResholds:METHod " + value);
                    string ret = mScope.ReadString(":MEASure:THResholds:METHod? " + source);
                    Chk.Val(ret.StartsWith("ABS") || ret.StartsWith("PER"), true, "check for the threshold method scpi");
                    //:MEASure:THResholds:PERCent
                    if (method.StartsWith("PER"))
                    {
                        double high = Utils.GenrateRandomInRangeDouble(2, 100);
                        high = Math.Round(high, 0);
                        double middle = Utils.GenrateRandomInRangeDouble(1, high - 1);
                        middle = Math.Round(middle, 0);
                        double low = Utils.GenrateRandomInRangeDouble(0, middle - 1);
                        low = Math.Round(low, 0);
                        string sValue = high + ", " + middle + ", " + low + ", " + source;
                        string eValue = high + ", " + middle + ", " + low;
                        mScope.Send(":MEASure:THResholds:PERCent " + sValue);
                        string rValue = mScope.ReadString(":MEASure:THResholds:PERCent? " + source);
                        Chk.Val(rValue, eValue, "Check for the threshold percent scpi");
                    }
                    else
                    {
                        //:MEASure:THResholds:ABSolute
                        double high = Utils.GenrateRandomInRangeDouble(-14.98, 15);
                        high = Math.Round(high, 2);
                        double middle = Utils.GenrateRandomInRangeDouble(-14.99, high - 0.01);
                        middle = Math.Round(middle, 2);
                        double low = Utils.GenrateRandomInRangeDouble(-15, middle - 0.01);
                        low = Math.Round(low, 2);
                        string sValue = high + ", " + middle + ", " + low + ", " + source;
                        string eValue = high + ", " + middle + ", " + low;
                        mScope.Send(":MEASure:THResholds:ABSolute " + sValue);
                        string rValue = mScope.ReadString(":MEASure:THResholds:ABSolute? " + source);
                        Chk.Val(rValue, eValue, "Check for the threshould absolute scpi");
                    }
                }
                if (!source.Equals("CHAN1"))
                {
                    mScope.Send(":" + source + ":DISP 0");
                }

                //:MEASure:THResholds:STANdard
                mScope.Send(":MEASure:THResholds:STANdard " + source);
                string stanValue = "90, 50, 10";
                string retValue = mScope.ReadString(":MEASure:THResholds:PERCent? " + source);
                Chk.Val(retValue, stanValue, "check for the standard scpi cmd");
            }
        }
    }
}
