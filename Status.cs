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
    class Status : InfiniiVisionTest
    {
        [SetUp]
        public void SetUp()
        {
            mScope.Write("*CLS;*RST");
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void Status_scpi()
        {
            //:STATus:PRESet
            mScope.Send("*CLS");
            mScope.Send(":STATus:PRESet");
            ScpiError err;
            err = mScope.ReadError();
            Chk.Val(err.ErrorCode, 0, "check for the error in status preset scpi");
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperationArm_scpi()
        {
            //:STATus:OPERation:ARM:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:ARM:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation arm condition scpi");

            //:STATus:OPERation:ARM:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:ENABle", num, num, "check for the status operation arm enable scpi -" + num);
            }

            //:STATus:OPERation:ARM:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:NTRansition", num, num, "check for the status operation arm ntransition scpi -" + num);
            }

            //:STATus:OPERation:ARM:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:PTRansition", num, num, "check for the status operation arm ptransition scpi -" + num);
            }

            //:STATus:OPERation:ARM[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:ARM:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "check for the status operation arm event");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:ARM:BIT<b>:ENABle
                string cmd = ":STATus:OPERation:ARM:BIT" + i + ":ENABle";
                Utils.CmdSend(ref mScope, cmd, 1, 1, "Check for the status arm bit enable -" + i);
                Utils.CmdSend(ref mScope, cmd, 0, 0, "Check for the status arm bit enable -" + i);
                Utils.CmdSend(ref mScope, cmd, 1, 1, "Check for the status arm bit enable -" + i);

                //:STATus:OPERation:ARM:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:ARM:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status arm condition scpi");

                //:STATus:OPERation:ARM:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:BIT" + i + ":NTRansition", 1, 1, "Check for the status arm bit ntransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:BIT" + i + ":NTRansition", 0, 0, "Check for the status arm bit ntransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:BIT" + i + ":NTRansition", 1, 1, "Check for the status arm bit ntransition -" + i);

                //:STATus:OPERation:ARM:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:BIT" + i + ":PTRansition", 1, 1, "check for hte status arm bit ptransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:BIT" + i + ":PTRansition", 0, 0, "check for hte status arm bit ptransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ARM:BIT" + i + ":PTRansition", 1, 1, "check for hte status arm bit ptransition -" + i);

                //:STATus:OPERation:ARM:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:ARM:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status arm bit event");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperation_scpi(){
            //:STATus:OPERation:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation condition");

            //:STATus:OPERation:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:ENABle", num, num, "Check for the status operation enable scpi -" + num);
            }

            //:STATus:OPERation:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:NTRansition", num, num, "check for the status operation ntransistion scpi -" + num);
            }

            //:STATus:OPERation:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:PTRansition", num, num, "check for the status operation ptransition scpi -" + num);
            }

            //:STATus:OPERation[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation event");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status operation bit scpi -" + i);

                //:STATus:OPERation:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":ENABle", 1, 1, "check for the status operation bit enable -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":ENABle", 0, 0, "check for the status operation bit enable -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":ENABle", 1, 1, "check for the status operation bit enable -" + i);

                //:STATus:OPERation:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":NTRansition", 1, 1, "check for the status operation bit Ntransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":NTRansition", 0, 0, "check for the status operation bit Ntransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":NTRansition", 1, 1, "check for the status operation bit Ntransition -" + i);

                //:STATus:OPERation:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation bit ptransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":PTRansition", 0, 0, "Check for the status operation bit ptransition -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation bit ptransition -" + i);

                //:STATus:OPERation:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation bit event scpi -" + i);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperationHardware_scpi()
        {
            //:STATus:OPERation:HARDware:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:HARDware:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation hardware condition scpi");

            //:STATus:OPERation:HARDware:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:ENABle", num, num, "Check for the status operation hardware enable scpi -" + num);
            }

            //:STATus:OPERation:HARDware:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:NTRansition", num, num, "Check for the status operation hardware ntraansition scpi -" + num);
            }

            //:STATus:OPERation:HARDware:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:PTRansition", num, num, "check for the status operation hardware ptransition scpi -" + num);
            }

            //:STATus:OPERation:HARDware[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:HARDware:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation hardware event scpi");

            //:STATus:OPERation:HARDware:BIT<b>:CONDition?
            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:HARDware:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:HARDware:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation hardware bit condition scpi -" + i);

                //:STATus:OPERation:HARDware:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":ENABle", 1, 1, "check for the status operation hardware bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":ENABle", 0, 0, "check for the status operation hardware bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":ENABle", 1, 1, "check for the status operation hardware bit enable scpi -" + i);

                //:STATus:OPERation:HARDware:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation hardware bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":NTRansition", 0, 0, "Check for the status operation hardware bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation hardware bit ntransition scpi -" + i);

                //:STATus:OPERation:HARDware:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":PTRansition", 1, 1, "check for the status operation hardware bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":PTRansition", 0, 0, "check for the status operation hardware bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:HARDware:BIT" + i + ":PTRansition", 1, 1, "check for the status operation hardware bit ptransition scpi -" + i);

                //:STATus:OPERation:HARDware:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:HARDware:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation hardware bit event scpi -" + i);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperationLocal_scpi()
        {
            //:STATus:OPERation:LOCal:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:LOCal:CONDition?");
            Pass.Condition(Is.Between(value,0,32676) , "Check for the status operation local condition scpi");

            //:STATus:OPERation:LOCal:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32676);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:ENABle", num, num, "check for the status operation local enable spci " + num);
            }

            //:STATus:OPERation:LOCal:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:NTRansition", num, num, "check for the status operation local ntransition scpi -" + num);
            }

            //:STATus:OPERation:LOCal:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:PTRansition", num, num, "check for hte status operation local ptransition scpi -" + num);
            }

            //:STATus:OPERation:LOCal[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:LOCal:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation local event scpi");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:LOCal:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:LOCal:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation local bit condition scpi");

                //:STATus:OPERation:LOCal:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":ENABle", 1, 1, "check for the status operation local bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":ENABle", 0, 0, "check for the status operation local bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":ENABle", 1, 1, "check for the status operation local bit enable scpi -" + i);

                //:STATus:OPERation:LOCal:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":NTRansition", 1, 1, "check for the status operation local bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":NTRansition", 0, 0, "check for the status operation local bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":NTRansition", 1, 1, "check for the status operation local bit ntransition scpi -" + i);

                //:STATus:OPERation:LOCal:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":PTRansition", 1, 1, "check for the status operation local bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":PTRansition", 0, 0, "check for the status operation local bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:LOCal:BIT" + i + ":PTRansition", 1, 1, "check for the status operation local bit ptransition scpi -" + i);

                //:STATus:OPERation:LOCal:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:LOCal:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status operation local bit event scpi -" + i);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperationMtest_scpi()
        {
            //:STATus:OPERation:MTESt:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:MTESt:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32676), "Check for the status operation mtest condition scpi");

            //:STATus:OPERation:MTESt:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32676);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:ENABle", num, num, "Check for the status operation mtest enable scpi -" + num);
            }

            //:STATus:OPERation:MTESt:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32676);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:NTRansition", num, num, "Check for the status operation mtest ntransition scpi -" + num);
            }

            //:STATus:OPERation:MTESt:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:PTRansition", num, num, "Check foer the status operation mtest ptransition scpi -" + num);
            }

            //:STATus:OPERation:MTESt[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:MTESt:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation mtest event scpi");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:MTESt:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:MTESt:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation mtest bit condition scpi");

                //:STATus:OPERation:MTESt:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":ENABle", 1, 1, "Check for hte status operation mtest bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":ENABle", 0, 0, "Check for hte status operation mtest bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":ENABle", 1, 1, "Check for hte status operation mtest bit enable scpi -" + i);

                //:STATus:OPERation:MTESt:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation mtest bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":NTRansition", 0, 0, "Check for the status operation mtest bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation mtest bit ntransition scpi -" + i);

                //:STATus:OPERation:MTESt:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":PTRansition", 1, 1, "check for the status operation mtest bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":PTRansition", 0, 0, "check for the status operation mtest bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:MTESt:BIT" + i + ":PTRansition", 1, 1, "check for the status operation mtest bit ptransition scpi -" + i);

                //:STATus:OPERation:MTESt:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:MTESt:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status operation mtest bit event scpi");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperationOverload_scpi()
        {
            //:STATus:OPERation:OVERload:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation overload condition scpi");

            //:STATus:OPERation:OVERload:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:ENABle", num, num, "Check for the status operation overload enable scpi -" + num);
            }

            //:STATus:OPERation:OVERload:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:NTRansition", num, num, "Check for hte status operation overload ntransition scpi -" + num);
            }

            //:STATus:OPERation:OVERload:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PTRansition", num, num, "check for the status operation overload ptransition scpi -" + num);
            }

            //:STATus:OPERation:OVERload[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status operation overload event scpi ");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:OVERload:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status operation overload bit condition scpi -" + i);

                //:STATus:OPERation:OVERload:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":ENABle", 1, 1, "check for the status operation overload bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":ENABle", 0, 0, "check for the status operation overload bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":ENABle", 1, 1, "check for the status operation overload bit enable scpi -" + i);

                //:STATus:OPERation:OVERload:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":NTRansition", 1, 1, "check for the status operation overload bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":NTRansition", 0, 0, "check for the status operation overload bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":NTRansition", 1, 1, "check for the status operation overload bit ntransition scpi -" + i);

                //:STATus:OPERation:OVERload:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation overload bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":PTRansition", 0, 0, "Check for the status operation overload bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation overload bit ptransition scpi -" + i);

                //:STATus:OPERation:OVERload:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation overload bit event scpi");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusOperationPfault_scpi()
        {
            //:STATus:OPERation:OVERload:PFAult:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:PFAult:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32676), "check for the status operation overload pfault condition scpi");

            //:STATus:OPERation:OVERload:PFAult:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32676);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:ENABle", num, num, "check for the status operation overload pfault enable scpi -" + num);
            }

            //:STATus:OPERation:OVERload:PFAult:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32676);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:NTRansition", num, num, "check for the status operation overload pfault ntransition scpi -" + num);
            }

            //:STATus:OPERation:OVERload:PFAult:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32676);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:PTRansition", num, num, "check for the status operation overload pfault ptransition scpi -" + num);
            }

            //:STATus:OPERation:OVERload:PFAult[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:PFAult:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32676), "Check for the status operation overload pfault event scpi");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:OVERload:PFAult:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:PFAult:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status operation overload pfault bit condition scpi");

                //:STATus:OPERation:OVERload:PFAult:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":ENABle", 1, 1, "check for the status operation overload pfault bit enable scpi- " + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":ENABle", 0, 0, "check for the status operation overload pfault bit enable scpi- " + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":ENABle", 1, 1, "check for the status operation overload pfault bit enable scpi- " + i);

                //:STATus:OPERation:OVERload:PFAult:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation overload pfault bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":NTRansition", 0, 0, "Check for the status operation overload pfault bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation overload pfault bit ntransition scpi -" + i);

                //:STATus:OPERation:OVERload:PFAult:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation overload pfault bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":PTRansition", 0, 0, "Check for the status operation overload pfault bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:OVERload:PFAult:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation overload pfault bit ptransition scpi -" + i);

                //:STATus:OPERation:OVERload:PFAult:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:OVERload:PFAult:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status operation overload pfault bit event scpi");
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusPower_scpi()
        {
            //:STATus:OPERation:POWer:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:OPERation:POWer:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32676), "Check for the status operation power condition scpi");

            //:STATus:OPERation:POWer:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:ENABle", num, num, "Check for the status operation power enable scpi -" + num);
            }

            //:STATus:OPERation:POWer:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:NTRansition", num, num, "Check for the status operation power nTransition scpi -" + num);
            }

            //:STATus:OPERation:POWer:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:PTRansition", num, num, "Check for the status operation power pransition scpi -" + num);
            }

            //:STATus:OPERation:POWer[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:OPERation:POWer:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for hte status operation power event scpi");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:OPERation:POWer:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:OPERation:POWer:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for hte staus operation power bit condition scpi -" + i);

                //:STATus:OPERation:POWer:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":ENABle", 1, 1, "check for the status operatin power bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":ENABle", 0, 0, "check for the status operatin power bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":ENABle", 1, 1, "check for the status operatin power bit enable scpi -" + i);

                //:STATus:OPERation:POWer:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation power bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":NTRansition", 0, 0, "Check for the status operation power bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":NTRansition", 1, 1, "Check for the status operation power bit ntransition scpi -" + i);

                //:STATus:OPERation:POWer:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation power bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":PTRansition", 0, 0, "Check for the status operation power bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:OPERation:POWer:BIT" + i + ":PTRansition", 1, 1, "Check for the status operation power bit ptransition scpi -" + i);

                //:STATus:OPERation:POWer:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:OPERation:POWer:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status operation power bit event scpi -" + i);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusQuestionnble_scpi()
        {
            //:STATus:QUEStionable:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:QUEStionable:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status questionable condition scpi");

            //:STATus:QUEStionable:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:ENABle", num, num, "Check for the status question enable scpi -" + num);
            }

            //:STATus:QUEStionable:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:NTRansition", num, num, "check for the status questionable ntransition scpi -" + num);
            }

            //:STATus:QUEStionable:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:PTRansition", num, num, "check for the status questionable ptransition scpi -" + num);
            }

            //:STATus:QUEStionable[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:QUEStionable:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status questionable event scpi");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:QUEStionable:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:QUEStionable:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status questionable bit condition scpi -" + i);

                //:STATus:QUEStionable:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":ENABle", 1, 1, "Check for the status questionable bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":ENABle", 0, 0, "Check for the status questionable bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":ENABle", 1, 1, "Check for the status questionable bit enable scpi -" + i);

                //:STATus:QUEStionable:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":NTRansition", 1, 1, "Check for the status questionable bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":NTRansition", 0, 0, "Check for the status questionable bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":NTRansition", 1, 1, "Check for the status questionable bit ntransition scpi -" + i);

                //:STATus:QUEStionable:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":PTRansition", 1, 1, "check for the status questionable bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":PTRansition", 1, 1, "check for the status questionable bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:QUEStionable:BIT" + i + ":PTRansition", 1, 1, "check for the status questionable bit ptransition scpi -" + i);

                //:STATus:QUEStionable:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:QUEStionable:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status questionable bit event scpi-" + i);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusTrigger_scpi()
        {
            //:STATus:TRIGger:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:TRIGger:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "check for the status trigger condition scpi");

            //:STATus:TRIGger:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:ENABle", num, num, "Check for hte status trigger enable scpi-" + num);
            }

            //:STATus:TRIGger:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:NTRansition", num, num, "Check for the status trigger ntransition scpi -" + num);
            }

            //:STATus:TRIGger:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:PTRansition", num, num, "check for hte status trigger ptransition scpi -" + num);
            }

            //:STATus:TRIGger[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:TRIGger:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for hte status trigger event scpi");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:TRIGger:BIT{0:15}:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:TRIGger:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status trigger bit scpi -" + i);

                //:STATus:TRIGger:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":ENABle", 1, 1, "check for the trigger bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":ENABle", 0, 0, "check for the trigger bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":ENABle", 1, 1, "check for the trigger bit enable scpi -" + i);

                //:STATus:TRIGger:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":NTRansition", 1, 1, "Check for the trigger bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":NTRansition", 0, 0, "Check for the trigger bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":NTRansition", 1, 1, "Check for the trigger bit ntransition scpi -" + i);

                //:STATus:TRIGger:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":PTRansition", 1, 1, "check for the trigger bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":PTRansition", 0, 0, "check for the trigger bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:TRIGger:BIT" + i + ":PTRansition", 1, 1, "check for the trigger bit ptransition scpi -" + i);

                //:STATus:TRIGger:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:TRIGger:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the trigger bit event scpi -" + i);
            }
        }

        [Test]
        [RunRule(RuleFieldName.Parameter, "TestEvent", RuleOperator.In, "Developer,NightlyBuild")]
        public void StatusUser_scpi()
        {
            //:STATus:USER:CONDition?
            int value = mScope.ReadNumberAsInt32(":STATus:USER:CONDition?");
            Pass.Condition(Is.Between(value, 0, 32767), "Check for the status user condition scpi");

            //:STATus:USER:ENABle
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:USER:ENABle", num, num, "check for the status user enable scpi -" + num);
            }

            //:STATus:USER:NTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:USER:NTRansition", num, num, "check for the status user ntransition scpi -" + num);
            }

            //:STATus:USER:PTRansition
            for (int i = 0; i < 4; i++)
            {
                int num = Utils.GenrateRandomInRange_Int(0, 32767);
                Utils.CmdSend(ref mScope, ":STATus:USER:PTRansition", num, num, "Check for the status user ptransition scpi -" + num);
            }

            //:STATus:USER[:EVENt]?
            value = mScope.ReadNumberAsInt32(":STATus:USER:EVENt?");
            Pass.Condition(Is.Between(value, 0, 32767), "check for the status user event scpi ");

            for (int i = 0; i < 15; i++)
            {
                //:STATus:USER:BIT<b>:CONDition?
                int bit = mScope.ReadNumberAsInt32(":STATus:USER:BIT" + i + ":CONDition?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "Check for the status user bit condition scpi" + i);

                //:STATus:USER:BIT<b>:ENABle
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":ENABle", 1, 1, "check for the status user bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":ENABle", 0, 0, "check for the status user bit enable scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":ENABle", 1, 1, "check for the status user bit enable scpi -" + i);

                //:STATus:USER:BIT<b>:NTRansition
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":NTRansition", 1, 1, "Check for the status user bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":NTRansition", 0, 0, "Check for the status user bit ntransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":NTRansition", 1, 1, "Check for the status user bit ntransition scpi -" + i);

                //:STATus:USER:BIT<b>:PTRansition
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":PTRansition", 1, 1, "check for the status user bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":PTRansition", 0, 0, "check for the status user bit ptransition scpi -" + i);
                Utils.CmdSend(ref mScope, ":STATus:USER:BIT" + i + ":PTRansition", 1, 1, "check for the status user bit ptransition scpi -" + i);

                //:STATus:USER:BIT<b>[:EVENt]?
                bit = mScope.ReadNumberAsInt32(":STATus:USER:BIT" + i + ":EVENt?");
                Chk.Val(Utils.IsZeroOrOne(bit), true, "check for the status user bit enable scpi -"+i);
            }
        }
    }
}
