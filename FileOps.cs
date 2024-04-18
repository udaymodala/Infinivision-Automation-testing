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
    class FileOps : InfiniiVisionTest
    {
        int mMaxChan;
        public ScpiError err;
        Utils sf = new Utils();

        int mTimeout = 20000;


        [SetUp]
        public void Setup()
        {
            mMaxChan = IsScopeFourChan ? 4 : 2;
            mScope.Write("*CLS;*RST");
        }


        /*
        [Test(Name = "File Save/Recall")]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void fileSaveRecall()
        {

            // file name
            // set it as for save operations
            // set it as for recall operations

            // possible extensions for files - setup, waveforms, mask, protocols, results (measure/mark)
            // possible extensions for recall - setup, wavform, mask, protocol 

        }*/



        [Test(Name = "Setup Save/Recall")]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void setupSaveRecall()
        {
            string fileName = "SaveSetupTest";

            // Perform settings to save
            // Enbale all channels, set vertical scale to 1V 
            // and apply frequency measurement
            // Enable FFT for channel 2
            // Stop acquisition
            for (int chan = 1; chan <= mMaxChan; chan++)
            {
                mScope.Send(":channel{0}:display 1", chan);
                mScope.Send(":channel{0}:scale 1.0", chan);
                mScope.Send(":measure:frequency CHAN{0}", chan);
            }
            mScope.Send(":display:graticule:layout OVERLay");
            mScope.Send(":fft:source chan2");
            mScope.Send(":fft:display on");
            WaitForOpc(ref mScope);
            mScope.Send(":stop");

            // save setup 
            mScope.Send(":save:setup:start \"{0}\"", fileName);
            Wait.MilliSeconds(500);

            // do default setup 
            mScope.Send(":SYSTEM:PRESET");
            Wait.MilliSeconds(10000);

            // Recall setup
            mScope.Send(":RECall:SETup \"{0}\"", fileName);
            Wait.MilliSeconds(10000);

            // Check settings done for the setup
            mScope.Send(":measure:statistics on");
            string measStr = mScope.ReadString(":measure:results?");
            for (int chan = 1; chan <= mMaxChan; chan++)
            {
                Chk.Val(mScope.ReadString(":channel{0}:display?", chan), "1", "Channel is ON aftre reacll");
                Chk.Val(mScope.ReadNumberAsDouble(":channel{0}:scale?", chan), 1.0, "Channel scale is 1V/div after recall");
                Pass.Condition(measStr.Contains("Frequency(" + chan + ")"), "Measurements are reapplied after recall");
            }

            Chk.Val(mScope.ReadString(":fft:display?"), "1", "FFT is enabled after recall");
            Chk.Val(mScope.ReadString(":fft:source1?"), "CHAN2", "FFT source is channel 2");
            Chk.Val(mScope.ReadString(":rstate?"), "STOP", "Acquistion stopped after recall ");
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SaveWaveform_scpi()
        {
            string readPwd;
            string readFileName;
            mScope.Timeout = 180000;
            // check default directory and waveform format
            readPwd = (mScope.ReadString(":save:pwd?")).Trim('"');
            //Pass.Condition(Is.Equal(readPwd, "/firmware/user/tlouser/User Files/"), "Default working diractory is not correct");
            Pass.Condition(Is.Equal(readPwd, "/User Files/setups/"), "Default working diractory - User Files");
            Pass.Condition(Is.Equal(mScope.ReadString(":save:waveform:format?"), "NONE"), "Default waveform format - NONE");

            // check :save:pwd
            string setPwd = "/User Files/arb/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            readPwd = mScope.ReadString(":save:pwd?");
            readPwd = readPwd.Trim('"');
            Pass.Condition(Is.Equal(readPwd, setPwd), String.Format("Working directory - {0}", setPwd));

            // check :save:filename
            string setFilename = "FILE123";
            mScope.Send(":save:filename \"{0}\"", setFilename);
            readFileName = mScope.ReadString(":save:filename?");
            readFileName = readFileName.Trim('"');
            Pass.Condition(Is.Equal(readFileName, setFilename), String.Format(":save:filename - {0}", setFilename));

            // check :save:waveform:format
            string[] waveformFormat = { "ASCiixy", "CSV", "BINary", "ASC", "BIN" };
            foreach (string format in waveformFormat)
            {
                Utils.CmdSend_startsWith(ref mScope, ":save:waveform:format", format, format, String.Format("Waveform format - {0}", format));
            }

            // check :save:waveform:start
            mScope.ReadErrors();                                        // empty error queue
            mScope.Send(":save:waveform:start \'{0}\'", setFilename);   // save the file
            err = mScope.ReadError();                                   // read if there are any errors on save operation
            Pass.Condition(Is.Equal(err.ErrorCode, 0));                 // should not be any error
        }


        /// <summary>
        /// :SAVE[:SETup[:STARt]]
        /// :RECall:SETup[:STARt] 
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void SaveRecallSetup_scpi()
        {
            string fileName = "DemoSetup";
            // Keeping default location
            // save setup and check no errors
            mScope.ReadErrors();
            mScope.Send(":save:setup:start \"{0}\"", fileName);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "No error should be reported on saving setup file");

            // do default setup
            mScope.Send(":system:preset");
            WaitForOpc(ref mScope, mTimeout);

            // recall setup and check no errors
            mScope.ReadErrors();
            mScope.Send(":recall:setup:start \'{0}\'", fileName);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "No error should be reported on recalling setup file");

            // save setup with .bmp extension and check -257 error
            mScope.ReadErrors();
            mScope.Send(":save:setup:start \"{0}.bmp\"", fileName);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, -257, "File name error (-257) should be reported on saving " +
                                        "setup file with an extension other than .scp"); // -257,"File name error"
        }





        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void save_image_scpi()
        {
            ///:SAVE:IMAGe:FORMat
            string[] formats = { "BMP", "BMP24bit", "PNG" };
            foreach (string format in formats)
            {
                Utils.CmdSend_startsWith(ref mScope, ":SAVE:IMAGe:FORMat", format, format, "Check for the save image format scpi command");
            }


            ///:SAVE:IMAGe:INKSaver

            foreach (string format in formats)
            {
                mScope.Send(":SAVE:IMAGe:FORMat " + format);
                Utils.CmdSend(ref mScope, ":SAVE:IMAGe:INKSaver", 0, 0, "Check for the save image inksaver scpi command");
                Utils.CmdSend(ref mScope, ":SAVE:IMAGe:INKSaver", 1, 1, "Check for the save image inksaver scpi command");
                Utils.CmdSend(ref mScope, ":SAVE:IMAGe:INKSaver", 0, 0, "Check for the save image inksaver scpi command");
                
            }

            ///:SAVE:IMAGe:FACTors
            Utils.CmdSend(ref mScope, ":SAVE:IMAGe:FACTors", 1, 1, "Check for the save image factor scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:IMAGe:FACTors", 0, 0, "Check for the save image factor scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:IMAGe:FACTors", 1, 1, "Check for the save image factor scpi command");


            ///:SAVE:IMAGe[:STARt]
            string fileName = "abc";
            mScope.Send(":SAVE:FILename  \'{0}\'",fileName);
            string setPwd = "/USB1/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":SAVE:IMAGe \'{0}\'",fileName);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in save image scpi command");

            
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void save_result_scpi()
        {
            
            
            ///:SAVE:RESults:FORMat:HISTogram
            mScope.Send(":CHANnel1:DISPlay 1");
            mScope.Send(":HISTogram:DISPlay 1");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:HISTogram", 0, 0, "Check for the save result format histogram scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:HISTogram", 1, 1, "Check for the save result format histogram scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:HISTogram", 0, 0, "Check for the save result format histogram scpi command");



            ///:SAVE:RESults:FORMat:MASK

            mScope.Send(":MTESt:ENABle 1");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:MASK", 0, 0, "Check for the save results format mask scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:MASK", 1, 1, "Check for the save results format mask scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:MASK", 0, 0, "Check for the save results format mask scpi command");



            ///:SAVE:RESults:FORMat:MEASurement
           
            mScope.Send(":MEASure:FREQuency chan1");
            Utils.CmdSend(ref mScope,":SAVE:RESults:FORMat:MEASurement",0,0,"Check for the save result format measurement scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:MEASurement", 1, 1, "Check for the save result format measurement scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:MEASurement", 0, 0, "Check for the save result format measurement scpi command");

            ///:SAVE:RESults:FORMat:CURSor
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:CURSor", 0, 0, "Check for the save result format cursor scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:CURSor", 1, 1, "Check for the save result format cursor scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:CURSor", 0, 0, "Check for the save result format cursor scpi command");



            ///:SAVE:RESults:FORMat:SEGMented
            mScope.Send(":HISTogram:DISPlay 0");
            mScope.Send(":MTESt:ENABle 0");
            mScope.Send(":ACQuire:MODE segm");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:SEGMented", 0, 0, "Check for the save results format segmented scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:SEGMented", 1, 1, "Check for the save results format segmented scpi command");
            Utils.CmdSend(ref mScope, ":SAVE:RESults:FORMat:SEGMented", 0, 0, "Check for the save results format segmented scpi command");
            

            ///:SAVE:RESults:[STARt]

            string name = "filename12";
            mScope.Send(":SAVE:FILename  \'{0}\'",name);
            string setPwd = "/USB1/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":SAVE:RESults \'{0}\'",name);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in save image scpi command");

        }



        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void save_waveform_scpi()
        {
            mScope.Send(":SYSTem:PRESet");
            string[] formats = { "CSV","BIN", "ASC" };
            foreach (string format in formats)
            {
                mScope.Send(":SAVE:WAVeform:FORMat " + format);
                //:SAVE:WAVeform:LENGth:MAX
                Utils.CmdSend(ref mScope, ":SAVE:WAVeform:LENGth:MAX", 1, 1, "Check for the save waveform length max scpi command");
                Utils.CmdSend(ref mScope, ":SAVE:WAVeform:LENGth:MAX", 0, 0, "Check for the save waveform length max scpi command");
                Utils.CmdSend(ref mScope, ":SAVE:WAVeform:LENGth:MAX", 1, 1, "Check for the save waveform length max scpi command");

                //:SAVE:WAVeform:LENGth
                mScope.Send(":SAVE:WAVeform:LENGth:MAX 0");
                for (int i = 0; i < 5; i++)
                {

                    int x = Utils.GenrateRandomInRange_Int(100, 800000);
                    Utils.CmdSend(ref mScope, ":SAVE:WAVeform:LENGth", x, x, "Check for the save waveform length scpi command");
                }

                //:SAVE:WAVeform:SEGMented
                mScope.Send(":ACQuire:MODE segm");
                mScope.Send(":RUN");
                Wait.MilliSeconds(10000);
                string[] segments = { "CURRent", "ALL", "CURR" };
                foreach (string segment in segments)
                {
                    Utils.CmdSend_startsWith(ref mScope, ":SAVE:WAVeform:SEGMented", segment, segment, "Check for the save waveform segmented scpi command - " + segment);
                }

                string setFilename = "FILE123";
                mScope.Send(":save:filename \"{0}\"", setFilename);
                string setPwd = "/USB1/";
                mScope.Send(":save:pwd \'{0}\'", setPwd);
                mScope.Send("*CLS");                                      // empty error queue
                mScope.Send(":save:waveform:start \'{0}\'", setFilename);  // save the file
                err = mScope.ReadError();                                   // read if there are any errors on save operation
                Pass.Condition(Is.Equal(err.ErrorCode, 0));


                //:SAVE?
                mScope.Send("*CLS");
                mScope.Send(":SAVE?");
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the error in save? scpi command");



            }
            
           
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void save_scpi_commands()
        {
            //:SAVE:MASK[:STARt]
            mScope.Send(":MTESt:AMASk:CREate");
            Wait.Seconds(0.5);
            string setFilename = "FILE123";
            string setPwd = "/USB1/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":SAVE:MASK \"{0}\" ",setFilename);
            err=mScope.ReadError();
            Chk.Val(err.ErrorCode,0,"Check for the save mask scpi command");

            //:SAVE:WMEMory:SOURce
            //:SAVE:WMEMory[:STARt]
            string[] sources={"FUNC","CHAN","WMEM","FUNCtion","CHANnel","WMEMory"};
            foreach(string source in sources){
                if (source == "FUNCtion" || source == "FUNC")
                {
                    for(int i=1;i<=4;i++){
                        string str=":"+source+i+":DISPlay "+1;
                        mScope.Send(str);
                        string val=source+i;
                        if (source == "FUNC")
                        Utils.CmdSend(ref mScope,":SAVE:WMEMory:SOURce",val,val,"Check for the save wmem source scpi command -"+val);
                        else
                        {
                            string s = mScope.ReadString(":SAVE:WMEMory:SOURce " + val);
                            Chk.Val(s, "FUNC" + i,"Check for the save wmem source scpi command -"+val);

                        }
                        
                        mScope.Send("*CLS");
                        mScope.Send(":SAVE:WMEMory");
                        mScope.Send(":SAVE:WMEMory:start");
                        err=mScope.ReadError();
                        Chk.Val(err.ErrorCode,0,"Check for the save wmemory scpi command");

                        str = ":" + source + i + ":DISPlay " + 0;
                        mScope.Send(str);
                    }
                }
                else if(source == "CHAN" || source == "CHANnel")
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        string str = ":" + source + i + ":DISPlay " + 1;
                        mScope.Send(str);
                        string val = source + i;
                        if (source == "CHAN")
                            Utils.CmdSend(ref mScope, ":SAVE:WMEMory:SOURce", val, val, "Check for the save wmem source scpi command -" + val);
                        else
                        {
                            string s = mScope.ReadString(":SAVE:WMEMory:SOURce " + val);
                            Chk.Val(s, "CHAN" + i, "Check for the save wmem source scpi command -" + val);

                        }

                        mScope.Send("*CLS");
                        mScope.Send(":SAVE:WMEMory");
                        mScope.Send(":SAVE:WMEMory:start");
                        err = mScope.ReadError();
                        Chk.Val(err.ErrorCode, 0, "Check for the save wmemory scpi command");

                        str = ":" + source + i + ":DISPlay " + 0;
                        mScope.Send(str);
                    }
                }
                else
                {
                    for(int i=1;i<=2;i++){
                        string str=":"+source+i+":DISPlay "+1;
                        mScope.Send(str);
                        string val=source+i;
                        mScope.Send(":chan1:display 1");
                        mScope.Send(":" + source + i + ":save chan1");   //saves the waveform in chan1 in ref wave memeory i
                        if (source == "WMEM")
                        {
                            Utils.CmdSend_startsWith(ref mScope, ":SAVE:WMEMory:SOURce", val, val, "Check for the save wmem source scpi command -" + val);
                        }
                        else
                        {
                            string s = mScope.ReadString(":SAVE:WMEMory:SOURce " + val);
                            Chk.Val(s, "WMEM" + i, "Check for the save wmem source scpi command -" + val);
                        }
                        mScope.Send("*CLS");
                        mScope.Send(":SAVE:WMEMory");
                        mScope.Send(":SAVE:WMEMory:start");
                        err = mScope.ReadError();
                        Chk.Val(err.ErrorCode, 0, "Check for the save wmemory scpi command");
                        str=":"+source+i+":DISPlay "+0;
                        mScope.Send(str);
                        

                    }
                }

                
            }
        }






        [Test]
       [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void recall_scpi_command()
        {
            //:RECall?
            mScope.Send(":RECall?");
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the recall query scpi command ");

            //:RECall:PWD
            string recallPwd = "/USB1/";
            mScope.Send(":RECall:PWD \"{0}\"", recallPwd);
            string res = mScope.ReadString(":RECall:PWD?");
            Chk.Val(res, recallPwd, "Check for the recall pwd scpi command");

     
            //Riya -add this
            //:RECall:MASK:ENABle
            //:RECall:MASK[:STARt]
            mScope.Send(":MTESt:AMASk:CREate");
            Wait.Seconds(0.5);
            string setFilename = "a";
            string setPwd = "/USB1/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send(":SAVE:MASK \"{0}\"", setFilename);
            Utils.CmdSend(ref mScope, ":RECall:MASK:ENABle", 1, 1, "check for the recall mask enable scpi");
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":RECall:MASK \"{0}\"", setFilename);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the recall mask scpi command");
            Utils.CmdSend(ref mScope, ":RECall:MASK:ENABle", 0, 0, "check for the recall mask enable scpi");
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":RECall:MASK \"{0}\"", setFilename);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, -221, "Check for the recall mask scpi command");
            

            //Riya - add this
            //:RECall:DBC[:STARt]
            setFilename = "CAN_Scope_Demo_DBC";
            setPwd = "/USB1/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":RECall:DBC:STARt \"{0}\"", setFilename);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the recall mask scpi command");

            //Riya - add this
            //:RECall:LDF[:STARt]
            setFilename = "LIN_Symbolic";
            setPwd = "/USB1/";
            mScope.Send(":save:pwd \'{0}\'", setPwd);
            mScope.Send("*CLS");
            mScope.Send(":RECall:LDF:STARt \"{0}\"", setFilename);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the recall mask scpi command");


            //:RECall:WMEMory<r>[:STARt]
            for(int i=1;i<=2;i++){
                string str = ":WMEM"+i + ":DISPlay " + 1;
                mScope.Send(str);
                mScope.Send(":chan1:display 1");
                mScope.Send(":WMEM"+ i + ":save chan1");
                mScope.Send(":SAVE:WMEMory:SOURce WMEM"+i);
                mScope.Send(":SAVE:WMEMory");
                mScope.Send("*CLS");
                mScope.Send(" :RECall:WMEMory"+i);
                err = mScope.ReadError();
                Chk.Val(err.ErrorCode, 0, "Check for the recall wmemory scpi command");
            }
             
        }


        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void arbi_scpi_command()
        {
            //:SAVE:ARBitrary[:STARt]
            //:RECall:ARBitrary[:STARt]
            mScope.Send(":WGEN1:OUTPut 1");
            mScope.Send(":WGEN1:FUNCtion ARBitrary");
            mScope.Send(":WGEN1:VOLTage 1");
            mScope.Send(":WGEN1:FREQuency 10");
            string name = "xyz";
            mScope.Send("*CLS");
            mScope.Send(":SAVE:ARBitrary \"{0}\"",name);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in save arbitrary scpi command");
            mScope.Send("*CLS");
            mScope.Send(":RECall:ARBitrary \"{0}\"", name);
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "Check for the error in recall arbitrary scpi command");
            double freq = mScope.ReadNumberAsDouble(":WGEN1:FREQuency?");
            double vol = mScope.ReadNumberAsDouble(":WGEN1:VOLTage?");
            Chk.Val(freq, 10.0, "Check for the recall arbitrary scpi command");
            Chk.Val(vol, 1.0, "check for the reccall arbitrary scpi command");

        }


    }
}
