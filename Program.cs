namespace ContractStalker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Input address of contract: ");
            string contractAddress = Console.ReadLine();

            Console.Write("input ether to check: ");
            double ether = double.Parse(Console.ReadLine());

            GetContractTransactions(contractAddress, ether);
        }

        public static async void GetContractTransactions(string contractAddr,double etherToCheck)
        {
            while (true)
            {
                Thread.Sleep(2000);
                //etherbots contract
                string pendingTxUrl = string.Format("https://etherscan.io/txsPending?a={0}",contractAddr);
                string funcName = "createAuction";
                string txsUrl = "https://etherscan.io/tx/";
                try
                {
                    string res = new WebClient().DownloadString(pendingTxUrl);

                    //gets all href with tx hash
                    Match[] matches1 = Regex.Matches(res, @"<a\s+(?:[^>]*?\s+)?href='/tx/\w+")
                           .Cast<Match>()
                           .ToArray();
                    List<string> arr = new List<string>();
                    foreach (var item in matches1)
                    {
                        arr.Add(item.Value.Substring(13));
                    }

                    foreach (var item in arr)
                    {
                        string infoAboutTxs = new WebClient().DownloadString(txsUrl + item);
                        string temp = Regex.Match(infoAboutTxs, string.Format(@"{0}(.*|\n)([^\\<]*)",funcName)).Value;

                        if (!string.IsNullOrEmpty(temp))
                        {
                            string[] tempClean = temp.Split('\n');

                            string tempZero = tempClean[3].Substring(6).TrimStart(new Char[] { '0' });
                            string tempOne = tempClean[4].Substring(6).TrimStart(new Char[] { '0' });

                            string dataZero = tempZero.Substring(0, tempZero.Length - 1);
                            string dataOne = tempOne.Substring(0, tempOne.Length - 1);

                            if(dataOne.Length <= 16)
                            {
                                long itemID = long.Parse(dataZero, System.Globalization.NumberStyles.HexNumber);
                                long givenWei = long.Parse(dataOne, System.Globalization.NumberStyles.HexNumber);

                                if (givenWei <= etherToCheck * Math.Pow(10,18))
                                {
                                    decimal calculatedWei = givenWei / 1000000000000000000m;
                                    Console.WriteLine("https://etherbots.io/app/part/" + itemID + " data from [0]: " + calculatedWei  + " " + txsUrl + item);

                                    Console.WriteLine();
                                    using (StreamWriter writer = File.AppendText("CreateAuctions.log.html"))
                                    {
                                        await writer.WriteLineAsync("<a href=\"https://etherbots.io/app/part/" + itemID +"\"> "+ "https://etherbots.io/app/part/" +itemID +"</a>"
                                            + " ether PRICE for part: " + calculatedWei + "  " 
                                            + " <a href =\""  + txsUrl + item + "\">" + txsUrl + item + "</a>" + "</br>");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
