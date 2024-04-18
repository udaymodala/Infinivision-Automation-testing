using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keysight.Fusion.Runtime;
using Keysight.Fusion.Logging;
using Keysight.Fusion.Visa;
using System.Threading;
using System.IO;

namespace Fusion_Tests.P2_Tests
{
    [TestFixture]
    class Display : InfiniiVisionTest
    {
        public int mMaxChan = 4;
        public ScpiError err;

        [SetUp]
        public void Setup()
        {
            mMaxChan = 4;
            mScope.Write("*RST");
        }

        #region Annotation test - taken from legacy tests

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Annotation()
        {
            // SCPIs verified in this test
            string strDisp = ":DISPlay:ANNotation";
            string scpiDisp = ":DISPlay:ANNotation";
            string scpiText = ":TEXT";
            string scpiColor = ":COLor";
            string scpiBg = ":BACKground";
            string scpiXPos = ":XPosition";
            string scpiYPos = ":YPosition";
            string scpiMode = ":MODE";
            string scpiBlight = ":BACKlight";
            string scpiGrid = ":GRID";

            string charList = " ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                              "abcdefghijklmnopqrstuvwxyz" +
                              "0123456789" +
                              "`~!@#$%^&*()-=_+[]\\;',./{}|:<>?";

            string[] colorList = { "CH1", "CH2", "CH3", "CH4", "DIG", "MATH", 
                                 "REF", "MARK", "WHIT", "RED" };

            string[] bgList = { "OPAQ", "INV", "TRAN" };

            string[] mode = { "GRID", "SOUR" };

            double Xpos, Ypos;
            string state = null;


            Random rand = new Random();

            int N_ANNOTATION_CHARS = 254;

            int N_CHARS_TO_SELECT = charList.Length;
            int N_COLORS = colorList.Length;
            int N_BG_MODES = bgList.Length;



            for (int j = 1; j <= 20; j = j + 5 )
            {
                // Annotation dispay controls 
                mScope.Send(scpiDisp + j + " 1");
                Wait.MilliSeconds(1000);
                state = mScope.ReadString(scpiDisp + j + "?");
                Wait.MilliSeconds(1000);
                Pass.Condition(Is.Equal(state, "1"), "Annotaion" + j + " display - ON");
                //Utils.CmdSend(ref mScope, scpiDisp + j , 1, 1, "Annotaion" + j + " display - ON");

                
                string text = "";

                // --- Check that overflowed text is handled correctly ---
                for (int ch = 0; ch < N_ANNOTATION_CHARS * 2; ++ch)
                {
                    text += charList[rand.Next(0, N_CHARS_TO_SELECT)];
                }

                mScope.Write("*CLS");
                mScope.Write(strDisp + j + scpiText + " \"" + text + "\"");
                List<ScpiError> errList = mScope.ReadErrors();

                bool gIsInv568Fixed = true;

                if (!gIsInv568Fixed)
                {
                    Chk.Val(errList[0].ErrorString,
                            "-151, \"Invalid string data\"",
                            "Verify the error returned when setting to a very long annotation");
                }
                else
                {
                    Chk.Val(errList[0].ErrorString,
                            "-223, \"Too much data\"",
                            "Verify the error returned when setting to a very long annotation");
                }

                // --- Check that the commands work ---
                for (int i = 0; i < N_ANNOTATION_CHARS; ++i)
                {
                    int nChars = i;
                    text = "";

                    for (int ch = 0; ch < nChars; ++ch)
                    {
                        if (ch < nChars - 1 && rand.Next(0, 20) == 7)
                        {
                            // Randomly add a line break.
                            text += "\\n";
                            ch += 2;
                        }
                        else
                        {
                            text += charList[rand.Next(0, N_CHARS_TO_SELECT)];
                        }
                    }

                    mScope.Write(strDisp + j + scpiText + " \"" + text + "\"");

                    string exp = "\"" + text + "\"";
                    exp = exp.Replace("\\n", "\r");

                    Chk.Val(exp, mScope.ReadString(strDisp + j + scpiText + "?"),
                            "Queried annotation not the same as the one set");

                    if (rand.Next(0, 50) == 7)
                    {
                        // Test Color
                        foreach (string color in colorList)
                        {
                            mScope.Write(strDisp + j + scpiColor + " " + color);
                            ChkStr(color, mScope.ReadString(strDisp + j + scpiColor + "?"), "");
                            Thread.Sleep(50);
                        }

                        // Test Bg Mode
                        foreach (string bg in bgList)
                        {
                            mScope.Write(strDisp + j + scpiBg + " " + bg);
                            ChkStr(bg, mScope.ReadString(strDisp + j + scpiBg + "?"), "");
                            Thread.Sleep(50);
                        }

                        // Test position 
                        for (int k = 0; k <= 10; k++)
                        {
                            Xpos = Utils.GenrateRandomInRange_Double(0.0, 1.0);
                            Utils.CmdSend(ref mScope, strDisp + j + scpiXPos, Xpos, Xpos, "Annotaion" + j + " X position - " + Xpos);

                            Ypos = Utils.GenrateRandomInRange_Double(0.0, 1.0);
                            Utils.CmdSend(ref mScope, strDisp + j + scpiYPos, Ypos, Ypos, "Annotaion" + j + " Y position - " + Ypos);
                        }

                        // TesT backlight 
                        mScope.Write(":DISPlay" + scpiBlight + " ON");
                        mScope.Write(":DISPlay" + scpiBlight + " OFF");
                        mScope.Write(":DISPlay" + scpiBlight + " ON");

                        //Test mode 
                        foreach (string md in mode)
                        {
                            Utils.CmdSend(ref mScope, strDisp + j + scpiMode, md, md, "Annotaion" + j + " mode - " + md);
                            /// remove GRID from below if statemnet
                            if (md == "GRID")
                            {
                                for (int grid = 1; grid <= 4; grid++)
                                {
                                    mScope.Write(":chan" + grid + ":disp ON");
                                    Utils.CmdSend(ref mScope, strDisp + j + scpiGrid, grid, grid, "Annotation" + j + "grid value " + grid);
                                }
                            }
                        }
                    }
                    Chk.Errors(ref mScope);
                }

                Utils.CmdSend(ref mScope, scpiDisp + j, 0, 0, "Annotaion" + j + " display - OFF");
            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DisplayData()
        {
            //
            Dictionary<string, string> formats = new Dictionary<string, string>()
                                                {
                                                    {"BMP", ".bmp"},
                                                    
                                                    {"PNG", ".png"}
                                                };
            //{"BMP8bit", ".bmp"},
            List<string> palettes = new List<string>() { "COLor", "GRAYscale" };
            List<string> inksavers = new List<string>() { "ON", "OFF" };

            string path = Str.PATH_TEST_OUTPUT_DISP_DATA;
            if (Directory.Exists(path))
            {
                Message.GenerateF("Clear: " + path);
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }
            }
            else
            {
                Message.GenerateF("Create: " + path);
                Directory.CreateDirectory(path);
            }

            foreach (var format in formats)
            {
                foreach (var palette in palettes)
                {
                    foreach (var ink in inksavers)
                    {
                        string filePath = path + "\\";
                        filePath += format.Key + "_";
                        filePath += palette + "_";
                        filePath += ink == "ON" ? "GratInvert" : "GratNormal";
                        filePath += format.Value;

                        Message.GenerateF("Save: " + filePath);

                        //mScope.Write(":HARD:INKS " + ink);
                        mScope.Timeout = 60000 * 3;
                        byte[] img = mScope.ReadIeeeBlock<byte>(":DISP:DATA? " + format.Key); // + ", " + palette);


                        using (Stream sw = new System.IO.FileStream(filePath, FileMode.Create))
                        using (BinaryWriter bw = new BinaryWriter(sw))
                        {
                            foreach (var b in img)
                            {
                                bw.Write(b);
                            }
                        }

                        Chk.Errors(ref mScope);
                    }
                }
            }
        }

        private void ChkStr(string expected, string ret, string failureMsg)
        {
            if (ret != expected)
            {
                if (failureMsg != "")
                {
                    failureMsg += " \n";
                }

                Failure.GenerateF(failureMsg +
                                  "   Expected: " + expected + " \n" +
                                  "   Returned: " + ret + " \n");
            }
        }

#endregion 


        /// <summary>
        /// :display:persistence
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DisplayPersistence_scpi()
        {
            string[] persistence_string = { "INFinite", "MINimum", "INF", "MIN" };
            double[] persistence_double = { 100e-3, 500e-3, 1.0, 10.0, 30.0, 60.0 };

            Pass.Condition(Is.Equal(mScope.ReadString(":display:persistence?"), "MIN"), "Default persistence is not Minimum");

            foreach (string pers in persistence_string)
            {
                Utils.CmdSend_startsWith(ref mScope, ":display:persistence", pers, pers, String.Format("Display persistence not set to {0}", pers));
            }

            for (int i = 0; i < persistence_double.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":display:persistence", persistence_double[i], persistence_double[i],
                    String.Format("Display persistence not set to {0}", persistence_double[i]));
            }

            mScope.Send("*cls");
            mScope.Send(":DISPlay:PERSistence:CLEar");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, +0, "No errors should be reported on clearing persistence");

        }



        /// <summary>
        /// :DISPlay:CLOCk:IGRid
        /// :DISPlay:CLOCk:IGRid:XPOSition
        /// :DISPlay:CLOCk:IGRid:YPOSition
        /// :DISPlay:CLOCk[:STATe]
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DisplayClock_scpi()
        {
            string state = null;
            double pos;
            state = mScope.ReadString(":display:clock:state?");
            Chk.Val(state, "1", "Default clock display state");

            Utils.CmdSend(ref mScope, ":display:clock:state", "0", "0", "Verify : Clock display state");
            Utils.CmdSend(ref mScope, ":display:clock:state", "1", "1", "Verify : Clock display state");

            Utils.CmdSend(ref mScope, ":display:clock:igrid", "1", "1", "Verify : In grid clock display state");
            for (int i = 0; i < 5; i++)
            {
                pos = Utils.GenrateRandomInRange_Double(0, 1);

                Utils.CmdSend(ref mScope, ":display:clock:igrid:xposition", pos, pos, "Verify : In grid clock x-position");
                Utils.CmdSend(ref mScope, ":display:clock:igrid:yposition", pos, pos, "Verify : In grid clock y-position");
            }
            Utils.CmdSend(ref mScope, ":display:clock:igrid", "0", "0", "Verify : In grid clock display state");

        }



        /// <summary>
        /// :DISPLAY:RESults:CATalog?
        /// :DISPlay:RESults:LAYout LIST/TAB
        /// :DISPlay:RESults:LAYout:LIST[:SELect] 
        /// :DISPlay:RESults:LAYout:TAB:LEFT[:SELect] 
        /// :DISPlay:RESults:LAYout:TAB:RIGHt[:SELect] 
        /// :DISPlay:RESults:SIZe
        /// 
        /// INV-5697
        /// </summary>
        [Test(Name = "Display Results SCPIs Test")]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void DisplayResults_scpi()
        {
            string catalog = null;
            string err = null;

            // :DISPLAY:RESults:CATalog? -----------------------------------------
            // No markers/meas enabled - None displayed 
            catalog = mScope.ReadString(":DISPLAY:RESults:CATalog?");
            Chk.Val(catalog, "NONE", "No markers/meas enabled - None displayed ");

            // Enable a measurement - Meas, Mark
            mScope.Send(":MEASure:Frequency CHAN1");
            catalog = mScope.ReadString(":DISPLAY:RESults:CATalog?");
            Chk.Val(catalog, "MEAS, MARK", "Enable a measurement - Meas, Mark");

            // Disable Markers - Meas
            mScope.Send(":MARKer:MODE MANual");
            mScope.Send("MARKer:MODE OFF");
            catalog = mScope.ReadString(":DISPLAY:RESults:CATalog?");
            Chk.Val(catalog, "MEAS", "Disable meas markes - Meas");

            // Disable meas, enable marker - Mark
            mScope.Send(":MEASure:CLEAR");
            mScope.Send(":MARKer:MODE MANual");
            catalog = mScope.ReadString(":DISPLAY:RESults:CATalog?");
            Chk.Val(catalog, "MARK", "Disable meas Markes - Meas");

            mScope.Send("MARKer:MODE OFF");

            // :DISPlay:RESults:LAYout LIST/TAB -----------------------------------
            mScope.Send(":MEASure:Frequency CHAN1");
            Chk.Val(mScope.ReadString(":DISPlay:RESults:LAYout?"), "TAB", "Default result display is TAB");

            Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout", "LIST", "LIST", "Results display mode - List");
            Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout", "TAB", "TAB", "Results display mode - Tab");

            // :DISPlay:RESults:LAYout:LIST[:SELect] <result>
            string[] results_left = { "MEAS", "MARK" };  // , "MTESt", "DVM", "COUN", "LIST", "HIST", "CGR", "FRAN" };
            foreach (string result in results_left)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout:LIST", result, result, "List layout " + result);
            }
            foreach (string result in results_left)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout:LIST:SELect", result, result, "List layout " + result);
            }

            // :DISPlay:RESults:LAYout:TAB:LEFT[:SELect] 
            foreach (string result in results_left)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout:TAB:LEFT", result, result, "Tab layout (left) " + result);
            }
            foreach (string result in results_left)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout:TAB:LEFT:SELect", result, result, "Tab layout (left) " + result);
            }

            // :DISPlay:RESults:LAYout:TAB:RIGHt[:SELect]
            // enable DVM and counter and check for them
            mScope.Send(":DVM:ENABle 1");
            mScope.Send(":COUNter:ENABle 1");
            string[] results_right = { "DVM", "COUN" };
            foreach (string result in results_right)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout:TAB:RIGHt", result, result, "Tab layout (right) " + result);
            }
            foreach (string result in results_right)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:RESults:LAYout:TAB:RIGHt:SELect", result, result, "Tab layout (right) " + result);
            }

            // :DISPlay:RESults:PSIZe Hidden/Full/Custom
            
            string[] sizes = { "HIDD", "FULL", "CUST", "HIDDen", "CUSTom" };
            double[] custSizes = { 50.0, 536.0 };     // acceptable range (50 to 536)
            double[] custSize_invalid = { -50.0, -536.0, 49.0, 537.0, 600.0, 1024.0 };
            double rndSize;
            string sizeStr = null;
            string retSizeStr = null;
            string[] retSizeList = null;
            double sizeDouble;
            foreach (string size in sizes)
            {
                if (size == "CUST" || size == "CUSTom")
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        rndSize = Utils.GenrateRandomInRange_Double(custSizes[0], custSizes[1]);
                        sizeStr = size + "," + Convert.ToString(rndSize);

                        mScope.Send(":DISPlay:RESults:SIZe " + sizeStr);
                        retSizeStr = mScope.ReadString(":DISPlay:RESults:SIZe?");
                        retSizeList = retSizeStr.Split(',');

                        sizeDouble = Convert.ToDouble(retSizeList[1]);

                        Chk.Val(retSizeList[0], "CUST", "Size type - CUST");

                        if (rndSize > 50.0 && rndSize < 536.0)
                        {
                            Pass.Condition(Is.Equal(sizeDouble, Math.Round(rndSize, 0)), "Custom Size Value - " + rndSize);
                        }
                    }

                    foreach (double size_invalid in custSize_invalid)
                    {
                        mScope.ReadErrors();

                        sizeStr = size + "," + Convert.ToString(size_invalid);
                        mScope.ReadErrors();
                        mScope.Send(":DISPlay:RESults:SIZe " + sizeStr);

                        err = mScope.ReadString(":SYST:ERR?");                         // read if there are any errors
                        Pass.Condition(Is.Equal(err, "-222,\"Data out of range\""));               // -222 Data out of range is expected 

                        retSizeStr = mScope.ReadString(":DISPlay:RESults:SIZe?");
                        retSizeList = retSizeStr.Split(',');

                        sizeDouble = Convert.ToDouble(retSizeList[1]);

                        Chk.Val(retSizeList[0], "CUST", "Size type - CUST");

                        if (size_invalid > 536.0)
                        {
                            Pass.Condition(Is.Equal(sizeDouble, 536.0), "Custom Size Value - " + size_invalid);
                        }
                        if (size_invalid < 50.0)
                        {
                            Pass.Condition(Is.Equal(sizeDouble, 50.0), "Custom Size Value - " + size_invalid);
                        }

                    }
                }
                else
                {
                    Utils.CmdSend_startsWith(ref mScope, ":DISPlay:RESults:SIZe", size, size, "Results pane size " + size);
                }
            }
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Grid_scpi()
        {
            //:DISPlay:GRATicule:ALABels
            int[] grid_settings = { 0, 1, 0 };
            for (int i = 0; i < grid_settings.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:GRATicule:ALABels", grid_settings[i], grid_settings[i], "Check for the Grid label is on or off");
            }

            //:DISPlay:GRATicule:ALABels:IGRid
            mScope.Send(":DISPlay:GRATicule:ALABels 1");
            int[] Igrid = { 1, 0 };
            for (int i = 0; i < Igrid.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:GRATicule:ALABels:IGRid", Igrid[i], Igrid[i], "check for grid axis labels are displayed in seperate scale bars or not.");
            }

            //:DISPlay:GRATicule:COUNt 
            mScope.Send(":DISPlay:GRATicule:LAYout CUST");
            int[] cValues = { 2, 3, 4, 1 };
            for (int i = 0; i < cValues.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:GRATicule:COUNt", cValues[i], cValues[i], "Check for numbers of grids present");
            }


            // :DISPlay:GRATicule:FSCReen
            int[] screen = { 1, 0 };
            for (int i = 0; i < screen.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:GRATicule:FSCReen", screen[i], screen[i], "Check for full screen or not");
            }

            //:DISPlay:GRATicule:INTensity
            int[] intensity = { 0, 5, 10, 50, 100 };
            for (int i = 0; i < intensity.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:GRATicule:INTensity", intensity[i], intensity[i], "Check for grid intensity");
            }
            
            
            
            //:DISPlay:INTensity:WAVeform
            int[] intense = { 0, 1, 2, 3, 5, 10, 20, 50, 100 };
            for (int i = 0; i < intense.Length; i++)
            {
                Utils.CmdSend(ref mScope, ":DISPlay:INTensity:WAVeform", intense[i], intense[i], "check for waveform intensity");
            }


            //:DISPlay:MESSage:CLEar
            mScope.Send("*CLS");
            mScope.Send(":DISPlay:MESSage:CLEar");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the display message clear scpi command");

        }

        //Riya - gain and phas is not working currently 
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void GridSet()
        {
            //:DISPlay:GRATicule:SET
            //:DISPlay:GRATicule:SOURce?
            mScope.Send(":CHAN1:DISP 0");
            mScope.Send(":DISPlay:GRATicule:LAYout CUST");
            mScope.Send(":DISPlay:GRATicule:COUNt 4");
            mScope.Send(":CHAN1:DISP 1");
            mScope.Send(":CHAN2:DISP 1");
            mScope.Send(":FUNC1:DISP 1");
            mScope.Send(":FUNC2:DISP 1");
            mScope.Send(":DISPlay:GRATicule:SET CHAN1,1");
            mScope.Send(":DISPlay:GRATicule:SET CHAN2,1");
            mScope.Send(":DISPlay:GRATicule:SET FUNC1,1");
            mScope.Send(":DISPlay:GRATicule:SET FUNC2,1");
            string result = mScope.ReadString(":DISPlay:GRATicule:SOURce? " + 1);
            Chk.Val(result.Contains("CHAN1"), true, "check for the source scpi");
            Chk.Val(result.Contains("CHAN2"), true, "check for the source scpi");
            Chk.Val(result.Contains("FUNC1"), true, "check for the source scpi");
            Chk.Val(result.Contains("FUNC2"), true, "check for the source scpi");
            mScope.Send(":CHAN1:DISP 0");
            mScope.Send(":CHAN2:DISP 0");
            mScope.Send(":FUNC1:DISP 0");
            mScope.Send(":FUNC2:DISP 0");
            string[] source = { "CHAN", "WMEM", "FUNC", "FFT"/*, "GAIN", "PHAS"*/ };
            int[] grid = { 1, 2, 3, 4 };
            HashSet<string> set1 = new HashSet<string>();
            set1.Add("CHAN");
            set1.Add("FUNC");
            HashSet<string> set2 = new HashSet<string>();
            set2.Add("WMEM");
            for (int i = 0; i < source.Length; i++)
            {
                string s = source[i];
                int x=0;
                int y=0;
                if (set1.Contains(source[i]))
                {
                    x = Utils.GenrateRandomInRange_Int(1, 4);
                    s = s + x;
                    if (source[i] == "CHAN")
                    {
                        mScope.Send(":CHAN" + x + ":DISP 1");
                    }
                    else
                    {
                        mScope.Send(":FUNC" + x + ":DISP 1");
                    }
                }
                else if (set2.Contains(source[i]))
                {
                    y = Utils.GenrateRandomInRange_Int(1, 2);
                    s = s + y;
                    mScope.Send(":WMEM" + y + ":DISP 1");
                }
                else if(source[i]=="FFT")
                {
                    mScope.Send(":FFT:DISP 1");
                }
                /*else
                {
                    mScope.Send(":FRANalysis:ENABle 1");
                    mScope.Send(":FRANalysis:RUN");
                }
                 * */

                for (int j = 1; j < grid.Length; j++)
                {
                    s = s + ",";
                    s = s + j;
                    mScope.Send(":DISPlay:GRATicule:SET " + s);
                    s = s.Substring(0, s.Length - 2);
                    string res = mScope.ReadString(":DISPlay:GRATicule:SOURce? " + j);
                    Chk.Val(res, s, "Check for the grid source scpi");
                }
                if (source[i] == "CHAN")
                {
                    mScope.Send(":CHAN" + x + ":DISP 0");
                }
                else if (source[i] == "FUNC")
                {
                    mScope.Send(":FUNC" + x + ":DISP 0");
                }
                else if (source[i] == "WMEM")
                {
                    mScope.Send(":WMEM" + y + ":DISP 0");
                }
                else if(source[i] =="FFT")
                {
                    mScope.Send(":FFT:DISP 0");
                }
                /* else
                 {
                     mScope.Send(":FRANalysis:ENABle 0");
                 }
                 */

            }
        }
         
        
    }
}