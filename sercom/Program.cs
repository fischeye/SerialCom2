using System;
using System.IO.Ports;
using System.Threading;


namespace sercom
{
    class Program
    {

        static void Main(string[] args)
        {
            // Set up some Starting Paramters
            string portName = "COM4";
            bool isListen = false;
            string valueRGB = "0,0,0";
            // Check the Command Line Arguments
            foreach (string argument in args)
            {
                if (argument.ToLower().StartsWith("com")) { portName = argument; }
                if (argument.ToLower().StartsWith("rgb=")) { valueRGB = argument.Split('=')[1]; }
                if (argument.ToLower().StartsWith("listen")) { isListen = true; }
            }
            // Start the Serial Port Communication
            Console.WriteLine("Create COM Port on " + portName);
            SerialPort ComPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            ComPort.Open();

            // ------------------- LISTENING MODE
            if (isListen)
            {
                Console.WriteLine("Listening ...");
                string data;
                bool ready = false;
                while (!ready)
                {
                    data = ComPort.ReadExisting();
                    if (data.Trim() != "")
                    {
                        Console.WriteLine("DATA: " + data);
                        if (data.Trim() == "s")
                        {
                            ComPort.WriteLine("s");
                            Console.WriteLine("Wait for RGB Values");
                            data = ComPort.ReadLine(); // R,G,B
                            int datalen = data.Length;

                            // ----------------------------------------------------------------------------
                            // Translate Char Array into Numbers in c++ Style;
                            // Has to be done on Arduino
                            int number = 0;
                            int curValue = 3;
                            int startDecimal = 1;
                            int[] RGBvalues = new int[3];

                            for (int i = datalen - 1; i > -1; i--)
                            {
                                char oneChar = data[i];
                                if (oneChar == ',')
                                {
                                    // Add the current Number to the Array and Start with a New one
                                    RGBvalues[curValue - 1] = number;
                                    curValue--;
                                    number = 0;
                                    startDecimal = 1;
                                } else
                                {
                                    // Add Character to the current Number
                                    number += ((int)oneChar - 48) * startDecimal;
                                    startDecimal = startDecimal * 10;
                                }
                            }
                            // Add the last Number to the Array
                            RGBvalues[0] = number;
                            // RGBvalues Integer Array with a Size of 3 and the RGB Values as Integer
                            // ----------------------------------------------------------------------------

                            Console.Write("Receiving:");
                            Console.Write(" R=" + RGBvalues[0].ToString());
                            Console.Write(" G=" + RGBvalues[1].ToString());
                            Console.Write(" B=" + RGBvalues[2].ToString());
                            Console.WriteLine();

                            ready = true;
                        }
                        if (data.Trim() == "x") { ready = true; }
                    }
                }
                Console.WriteLine("Stop Listening");
            }

            // ------------------- SENDING MODE
            if (!isListen)
            {
                bool ready = false;
                string data;
                Console.WriteLine("Sending Start Command and wait for Answer");
                // Loop until we are ready to send RGB Data
                while (!ready)
                {
                    // Write the Start Command to the Port
                    ComPort.WriteLine("s");
                    // Wait for the Receipient
                    Thread.Sleep(500);
                    // Check if we reveived and Answer
                    data = ComPort.ReadExisting();
                    // If the Answer is correct, we are ready to send Data
                    if (data.Trim() == "s") { ready = true; }
                }
                Console.WriteLine("Sending RGB Values: " + valueRGB);
                // Send the RGB Values to the Port
                // Values are in the format (<Red>,<Green>,<Blue>)
                // The Receipient will transform this in to single Integers
                ComPort.WriteLine(valueRGB);
                // Sending a Stop Command (not needed with Arduino)
                ComPort.WriteLine("x");
            }

            ComPort.Close();
        }
    }
}
