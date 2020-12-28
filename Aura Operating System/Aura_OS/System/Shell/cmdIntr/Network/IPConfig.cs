﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Network IPCONFIG
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using Aura_OS.HAL.Drivers.Network;
using Aura_OS.System.Network;
using Aura_OS.System.Network.IPV4.UDP.DHCP;
using Aura_OS.System.Network.IPV4;
using System;
using System.Collections.Generic;
using System.Linq;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandIPConfig : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandIPConfig(string[] commandvalues) : base(commandvalues)
        {
            Description = "to set a static IP or get an IP from the DHCP server";
        }

        /// <summary>
        /// CommandIPConfig without args
        /// </summary>
        public override ReturnInfo Execute()
        {
            if (NetworkStack.ConfigEmpty())
            {
                Console.WriteLine("No network configuration detected! Use ipconfig /help");
            }
            foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
            {
                switch (device.CardType)
                {
                    case HAL.Drivers.Network.CardType.Ethernet:
                        Console.Write("Ethernet Card : " + device.NameID + " - " + device.Name);
                        break;
                    case HAL.Drivers.Network.CardType.Wireless:
                        Console.Write("Wireless Card : " + device.NameID + " - " + device.Name);
                        break;
                }
                if (NetworkConfig.CurrentConfig.Key == device)
                {
                    Console.WriteLine(" (current)");
                }
                else
                {
                    Console.WriteLine();
                }
               
                Utils.Settings settings = new Utils.Settings(@"0:\System\" + device.Name + ".conf");
                Console.WriteLine("MAC Address          : " + device.MACAddress.ToString());
                Console.WriteLine("IP Address           : " + NetworkConfig.Get(device).IPAddress.ToString());
                Console.WriteLine("Subnet mask          : " + NetworkConfig.Get(device).SubnetMask.ToString());
                Console.WriteLine("Default Gateway      : " + NetworkConfig.Get(device).DefaultGateway.ToString());
                Console.WriteLine("Preferred DNS server : " + NetworkConfig.Get(device).DefaultDNSServer.ToString());
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandIPConfig
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments[0] == "/release")
            {
                DHCPClient.SendReleasePacket();
            }
            else if (arguments[0] == "/ask")
            {
                DHCPClient.SendDiscoverPacket();
            }
            else if (arguments[0] == "/listnic")
            {
                foreach (var device in NetworkDevice.Devices)
                {
                    switch (device.CardType)
                    {
                        case CardType.Ethernet:
                            Console.WriteLine("Ethernet Card - " + device.NameID + " - " + device.Name + " (" + device.MACAddress + ")");
                            break;
                        case CardType.Wireless:
                            Console.WriteLine("Wireless Card - " + device.NameID + " - " + device.Name + " (" + device.MACAddress + ")");
                            break;
                    }
                }
            }
            else if (arguments[0] == "/set")
            {
                if ((arguments.Count == 3) || (arguments.Count == 4)) // ipconfig /set eth0 192.168.1.2/24 {gw}
                {
                    string[] adrnetwork = arguments[2].Split('/');
                    Address ip = Address.Parse(adrnetwork[0]);
                    NetworkDevice nic = NetworkDevice.GetDeviceByName(arguments[1]);
                    Address gw = null;
                    if (arguments.Count == 4)
                    {
                        gw = Address.Parse(arguments[3]);
                    }

                    int cidr;
                    try
                    {
                        cidr = int.Parse(adrnetwork[1]);
                    }
                    catch (Exception ex)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
                    }
                    Address subnet = Address.CIDRToAddress(cidr);

                    if (nic == null)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Couldn't find network device: " + arguments[1]);
                    }

                    if (ip != null && subnet != null && gw != null)
                    {
                        NetworkInit.Enable(nic, ip, subnet, gw, ip);
                        Console.WriteLine("Config OK!");
                    }
                    else if (ip != null && subnet != null)
                    {
                        NetworkInit.Enable(nic, ip, subnet, ip, ip);
                        Console.WriteLine("Config OK!");
                    }
                    else
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Can't parse IP addresses (make sure they are well formated).");
                    }
                }                
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR, "Usage : ipconfig /set {device} {IPv4/CIDR} {Gateway|null}");
                }
            }
            else
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Wrong usage, please type: ipconfig /help");
            }
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("- ipconfig /listnic    List network devices");
            Console.WriteLine("- ipconfig /set        Manually set an IP Address");
            Console.WriteLine("     Usage:");
            Console.WriteLine("     - ipconfig /set {device} {IPv4} {Subnet} {Gateway}");
            Console.WriteLine("- ipconfig /ask        Find the DHCP server and ask a new IP address");
            Console.WriteLine("- ipconfig /release    Tell the DHCP server to make the IP address available");
        }
    }
}
