﻿using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using System.Collections;
using System.Windows;

namespace ConsoleApplication1
{
    class Program
    {

        private static string botToken = ""; // I don't publish my token :P
        private static string botUrl = "https://api.telegram.org/bot" + botToken + "/";
        private const string botUsername = "@noinsultbot";
        private const long testgroupid = -1001084458410;
        private const long ludwigsId = 295152997;
        private static readonly long[] adminIds = { ludwigsId, 283886123 , 267376056 };
//                                                    Ludwig   Alexander    Florian
        private static readonly string projectPath = "C:\\Users\\Ludwig\\Documents\\Visual Studio 2015\\Projects\\ConsoleApplication1\\ConsoleApplication1\\";

        private static bool running = true;
        private static long lastUpdate = 0;
        private static long newUpdate = 0;

        private static ArrayList schimpfworte = new ArrayList();
        private static string insult;

        private static Update[] updates;

        static void Main(string[] args)
        {
            running = true;
            readSchimpfwortFile();

            Console.WriteLine("Bot started");
            Console.WriteLine("");

            sendMessage("Bot was started by Ludwig!", testgroupid, null, "Markdown");

            while (running)
            {
                Stream resStream = getUpdates();
                updates = Decode<Update[]>(resStream);
                handleUpdates(updates: updates);
            }

            return;
        }

        private static void handleMessage(string txt, Message msg, Update u)
        {
            string lowertxt = txt.ToLower();

            switch (lowertxt)
            {
                case "/feedback":
                case "/feedback" + botUsername:
                    sendMessage("This is a bot by Ludwig. If you want to provide feedback, join the [test group](https://telegram.me/noinsultbottestinggroup)", msg.Chat.Id, msg, "Markdown", true);
                    break;

                case "/chatid":
                case "/chatid" + botUsername:
                    sendMessage("The Chat ID of the current chat is:" + msg.Chat.Id, msg.Chat.Id);
                    break;


/*                    case "!stopbot":
                        if (msg.From.Id == ludwigsId)
                        {
                            lastUpdate = u.Id;
                            getUpdates();
                            sendMessage("Bot was stopped by global admin!", msg.Chat.Id, msg, null);
                            running = false;
                        }
                        else ludwigOnly(msg);
                        break;
*/

                case "/blacklist":
                case "/blacklist" + botUsername:
                    if (Array.IndexOf(adminIds, msg.From.Id) > -1)
                    {
                        string blacklistMessage = "";
                        blacklistMessage += "*Current Blacklist:*\n\n";
                        for (int i = 0; i < schimpfworte.Count; i++)
                        {
                            blacklistMessage += schimpfworte[i];
                            blacklistMessage += "\n";
                        }

                        sendMessage(blacklistMessage, msg.Chat.Id, null, "Markdown");
                    }
                    else globalAdminsOnly(msg);

                    break;


                case "/betatesters":
                case "/betatesters" + botUsername:
                    sendMessage("Thanks to the Beta Testers!\n\n - [Alexander](https://telegram.me/Xx_3cool5you_xX)\n - [Florian](https://telegram.me/Olfi01)", msg.Chat.Id, null, "Markdown", true);
                    break;


                case "!updateinsults":
                    if (msg.From.Id == ludwigsId)
                    {
                        writeSchimpfwortFile();
                        sendMessage("Insult file has been updated!", msg.Chat.Id, msg);
                        Console.WriteLine("");
                        Console.WriteLine("The Insult File has been updated!");
                        Console.WriteLine("");
                    }
                    else ludwigOnly(msg);
                    break;

                case "/echo":
                case "/echo" + botUsername:
                    string echotext;
                    if (msg.ReplyToMessage != null) echotext = msg.ReplyToMessage.Text;
                    else echotext = "Please reply to a message to echo it.";
                    sendMessage(echotext, msg.Chat.Id, null, "Markdown", true);
                    break;


                case "/kickme":
                case "/kickme" + botUsername:
                    kickUser(msg);
                    sendMessage("Kicked User `" + msg.From.FirstName + "`!", msg.Chat.Id, msg, "Markdown");
                    break;

                case "/kick":
                case "/kick" + botUsername:
                    if (msg.ReplyToMessage != null)
                    {
                        kickUser(msg.ReplyToMessage);
                        sendMessage(msg.From.FirstName + " kicked " + msg.ReplyToMessage.From.FirstName + " " + msg.ReplyToMessage.From.LastName + "!", msg.Chat.Id);
                    }
                    else sendMessage("Please reply to the user you want to kick!", msg.Chat.Id);
                    break;

                case "/callalex":
                    foreach (long l in adminIds) if (l == msg.From.Id)
                    {
                        sendMessage("Hey Alexander! Du wurdest gebeten, die Gruppe @noinsultbottestinggroup wieder zu joinen!", 283886123);
                        sendMessage("Alexander was called to rejoin the testing group in private", msg.Chat.Id);
                    }
                    break;

            }

            if ((lowertxt.StartsWith("/addinsult ") && txt.Length > 11) || (lowertxt.StartsWith("/addinsult" + botUsername) && txt.Length > (botUsername.Length + 11)))
            {
                if (Array.IndexOf(adminIds, msg.From.Id) > -1)
                {
                    if (txt.Contains("\n"))
                    {
                        sendMessage("Error: The insult may not contain multiple lines!", msg.Chat.Id, msg, null);
                    }
                    else
                    {

                        string[] insultarray = txt.Split(' ');
                        insult = "";
                        for (int i = 1; i < insultarray.Length; i++)
                        {
                            insult += insultarray[i];
                            if (i == insultarray.Length - 1) break;
                            insult += " ";
                        }
                        insult = insult.ToLower();
                        schimpfworte.Add(insult.Trim());
                        sendMessage("Following insult was added:\n\n*" + insult + "*", msg.Chat.Id, msg, "Markdown");
                        Console.WriteLine("Following insult was added: " + insult);
                    }
                }
                else globalAdminsOnly(msg);
            }

            if (lowertxt.StartsWith("!leavegroup") && txt.Length > 11)
            {
                string[] splittedtxt = txt.Split(' ');
                string append = "leaveChat?chat_id=";
                append += splittedtxt[1];
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(botUrl + append);
                    request.GetResponse(); //errorsource 1
                }
                catch
                {
                    sendMessage("Error occured at errorsource 1!", msg.Chat.Id);
                }

            }

            foreach (string s in schimpfworte)
            {
                if(lowertxt.Contains(s))
                {
                    if (UserInGroup(msg))
                    {
                        kickUser(msg);
                        sendMessage("User `" + msg.From.FirstName + "` was automatically kicked because of this message!", msg.Chat.Id, msg, "Markdown");
                        lastUpdate = u.Id;
                        getUpdates();
                    }
                    else sendMessage("Tried to kick `" + msg.From.FirstName + "` because of this message, but `" + msg.From.FirstName + "` doesn't seem to be a member of this group (anymore)!", msg.Chat.Id, msg, "Markdown");
                }
            }
        }

        static bool UserInGroup(Message msg)
        {
            bool isInGroup = false;

            string append = "getChatMember?chat_id=" + msg.Chat.Id + "&user_id=" + msg.From.Id;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(botUrl + append);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            ChatMember cm = Decode<ChatMember>(resStream);

            if (cm.Status.ToString() == "Creator" || cm.Status.ToString() == "Administrator" || cm.Status.ToString() == "Member") isInGroup = true;

            return isInGroup;
        }



        static void kickUser (Message msg)
        {
            bool publicsupergroup = false;
            if (msg.Chat.Username != null) publicsupergroup = true;

            bool supergroup = false;
            if (msg.Chat.Type.ToString() == "Supergroup") supergroup = true;

            string append = "kickChatMember?chat_id=";

            if (publicsupergroup) append += "@" + msg.Chat.Username;
            else append += msg.Chat.Id;

            append += "&user_id=" + msg.From.Id;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(botUrl + append);
            request.GetResponse();

            if (supergroup)
            {
                append = "unbanChatMember?chat_id=";
                if (publicsupergroup) append += "@" + msg.Chat.Username;
                else append += msg.Chat.Id;

                append += "&user_id=" + msg.From.Id;

                request = (HttpWebRequest)WebRequest.Create(botUrl + append);
                request.GetResponse();

            }
        }

        static void globalAdminsOnly(Message msg)
        {
            sendMessage("Can't you read? GLOBAL ADMINS ONLY! You are not a global admin, *" + msg.From.FirstName + "*!", msg.Chat.Id, msg, "Markdown");
        }

        static void ludwigOnly(Message msg)
        {
            sendMessage("This command is only for Ludwig, the creator of this bot!", msg.Chat.Id, msg, null);
        }


        private static T Decode<T>(Stream str)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(str))
            using (var jtr = new JsonTextReader(sr))
            {
                var response = serializer.Deserialize<ApiResponse<T>>(jtr);
                return response.ResultObject;
            }
        }

        private static void handleUpdates(Update[] updates)
        {
            foreach (Update u in updates)
            {
                lastUpdate = u.Id;
                if (u.Message != null)  //if a message exists
                {
                    if (u.Message.Text != null)  //if the message is a text message
                    {
                        string txt = u.Message.Text;
                        handleMessage(txt: txt, msg: u.Message, u: u);
                    }
                }
            }
        }

        private static Stream getUpdates()
        {
            newUpdate = lastUpdate + 1;
            string append = "getUpdates";
            if (newUpdate != 1)
            {
                append += "?offset=" + newUpdate.ToString();
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(botUrl + append);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                return resStream;
            }
            catch (Exception e)
            {
                sendMessage("Error occured in the getUpdates function! Exception:\n\n" + e, testgroupid);
                return null;
            }
            
        }


        private static bool sendMessage(string txt, long chatid, Message replyto = null, string parsemode = null, bool disablepagepreview = false)
        {
            string append = "sendMessage?text=" + txt + "&chat_id=" + chatid.ToString();
            if (replyto != null)
            {
                append += "&reply_to_message_id=" + replyto.MessageId;
            }
            if (parsemode != null)
            {
                append += "&parse_mode=" + parsemode;
            }
            if (disablepagepreview == true) append += "&disable_web_page_preview=true";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(botUrl + append);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();            //errorsource 4
                Stream resStream = response.GetResponseStream();
                return DecodeRaw<Message>(resStream).Ok;
            }
            catch
            {
                Console.WriteLine("Error occured at errorsource 4!");
                return false;
            }
        }

        private static ApiResponse<T> DecodeRaw<T>(Stream str)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(str))
            using (var jtr = new JsonTextReader(sr))
            {
                return serializer.Deserialize<ApiResponse<T>>(jtr);
            }
        }



        private static void writeSchimpfwortFile()
        {
            if (!System.IO.File.Exists(projectPath + "Schimpfworte.txt"))
            {
                System.IO.File.Create(projectPath + "Schimpfworte.txt");
            }
            System.IO.File.WriteAllText(projectPath + "Schimpfworte.txt", "", System.Text.Encoding.UTF8);
            foreach (string s in schimpfworte)
            {
                System.IO.File.AppendAllText(projectPath + "Schimpfworte.txt", s + "\n");
            }
        }
        private static void readSchimpfwortFile()
        {
            try
            {
                if (!System.IO.File.Exists(projectPath + "Schimpfworte.txt"))
                {
                    System.IO.File.Create(projectPath + "Schimpfworte.txt");
                }
                using (StreamReader sr = new StreamReader(projectPath + "Schimpfworte.txt", System.Text.Encoding.UTF8))
                {
                    string s;
                    Console.WriteLine("Insults are being loaded...");
                    do
                    {
                        s = sr.ReadLine();
                        if (s != null)
                        {
                            schimpfworte.Add(s);
                            Console.WriteLine(s);
                        }
                    } while (s != null);
                    Console.WriteLine("");
                    Console.WriteLine("Insults loaded.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}