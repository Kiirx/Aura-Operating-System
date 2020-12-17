﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Ping command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Sys = Cosmos.System;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Network;
using Aura_OS.System;
using System.Text;
using System.Collections.Generic;
using Aura_OS.System.Network.IPV4.UDP.DNS;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandDns : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandDns(string[] commandvalues) : base(commandvalues)
        {
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (NetworkStack.ConfigEmpty())
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /set.");
            }
            if (arguments.Count == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            DNSClient.SendAskPacket(arguments[0]);

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}