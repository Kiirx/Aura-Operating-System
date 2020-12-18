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
using Aura_OS.System.Network.IPV4.UDP;

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
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            Console.WriteLine("Usage: dns {dns_server_ip} {url}");
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (NetworkStack.ConfigEmpty())
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /set.");
            }
            if (arguments.Count < 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            var xClient = new DnsClient();

            xClient.Connect(Address.Parse(arguments[0]));

            xClient.SendAsk("google.com");

            string url = xClient.Receive().ToString();

            if (url == null)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Unable to get URL for " + arguments[0]);
            }
            else
            {
                Console.WriteLine(url);
            }

            xClient.Close();

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}