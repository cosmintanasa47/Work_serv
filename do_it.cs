using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace Work_serv
{
    public class do_it : BackgroundService
    {
        private readonly ILogger<do_it> logger;
        public do_it(ILogger<do_it> _logger)
        {
            this.logger = _logger;
        }

        public string filePath;

        public void Maintenance()
        {
            string folderName = "Days";
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
            Environment.SetEnvironmentVariable("DAYS_PATH", folderPath, EnvironmentVariableTarget.Process);
            logger.LogInformation("days: "+folderPath);
          //  Console.WriteLine(folderPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = DateTime.Now.ToString("ddMMyyyy") + ".txt";
            filePath = Path.Combine(folderPath, fileName);

            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                DateTime fileDate = File.GetCreationTime(file);
                TimeSpan difference = DateTime.Now - fileDate;

                int dayLimit = 30;

                if (difference.TotalDays > dayLimit)
                {
                    File.Delete(file);
                }
            }
        }

        public void Get_Timer()
        {
           var time = new System.Timers.Timer(5 * 1000);
           time.Elapsed += (sender, e) => Time_pass();
           time.Start();
            void Time_pass()
            {
                 using (Aes new_aes = Aes.Create())
                 {
                 byte[] en = null;
                 new_aes.KeySize = 256;
                 string text = null;
              //   logger.LogInformation("WRITE:");
                    try
                    {
                        foreach (ProcessActivity _process in processActivities)
                        {
                          //  logger.LogInformation("1");
                            text = text + _process.MainWindowTitle.ToString() + "\n"
                                        // + _process.Type + "\n"
                                        + _process.StartTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n"
                                        + _process.ActiveDuration.ToString(@"hh\:mm\:ss") + "\n";
                            if (string.IsNullOrEmpty(text)) logger.LogError("text e gol");
                          //  logger.LogInformation("2");
                        }
                        try { File.WriteAllText(filePath, text); }
                        catch(Exception ex) { logger.LogError($"ERROR: {ex}"); }
                       // logger.LogInformation("3");
                    }
                    catch(Exception ex)
                    {
                        logger.LogError($"ERROR: {ex}");
                    }
                    // en = Encrypt(text, new_aes.Key, new_aes.IV);
                    // File.WriteAllText(filePath, en.ToString());
                    //en = Encrypt(text, new_aes.Key, new_aes.IV);
                    // Console.Write($"{Encoding.UTF8.GetString(en)}");
                    // string dec = Decrypt(en,new_aes.Key,new_aes.IV);
                    // Console.Write($"{dec}");
                }
            }
        }

        public class StreamString
        {
            private Stream ioStream;
            private UnicodeEncoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                int len = 0;

                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                return streamEncoding.GetString(inBuffer);
            }

            public int WriteString(string outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }

        public class ProcessActivity
        {
            public string MainWindowTitle { get; set; }
          // public string Type { get; set; }
            public DateTime StartTime { get; set; }
            public TimeSpan ActiveDuration { get; set; }
        }

        List<ProcessActivity> processActivities = new List<ProcessActivity>();

        List<string> titles = new List<string>();
        string target, pass,email; 
        TimeSpan start, stop;

        public async Task Pipe_S()
        {
            /* try
             {
                 logger.LogInformation("Intra in PIPE");
                 using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("mypipe", PipeDirection.In))
                 {
                     await pipeServer.WaitForConnectionAsync();
                     logger.LogInformation("CONECTAT");
                     // using (var reader = new StreamReader(pipeServer))
                     // {
                     // using (StreamWriter writer = new StreamWriter(pipeServer))
                     //  {
                     if (pipeServer.IsConnected)
                     {
                         logger.LogInformation("CONECTAT 2");
                         string all = null;
                         StreamString _s = new StreamString(pipeServer);
                         while (test == false)
                         {
                             all = _s.ReadString(); // await
                             logger.LogInformation("INTRA TRUE SI CITESTE");
                             // string message = null;
                             using (StringReader sr = new StringReader(all))
                                 if (sr.ReadLine() != null)
                                 {
                                     logger.LogInformation("all = " + all);
                                     //      message = sr.ReadLine();
                                     string[] line = all.Split("\n");
                                     logger.LogInformation("split string");
                                     if (line[0] == "Title_IN")
                                     {
                                         logger.LogInformation("Title in");
                                         titles.Clear();
                                         logger.LogInformation("titles.Clear");
                                         int i = 1;
                                         bool t = false;
                                         while (line[i] != "Title_OUT")
                                         {
                                             if (!titles.Contains(line[i]))
                                                 titles.Add(line[i]); logger.LogInformation($"Ttiles ADD: {line[i]}");
                                             i++;
                                             if (line[i] == "Title_OUT") { t = true; break; }
                                         }
                                         if (t == true)
                                         {

                                             i++;
                                             target = line[i];
                                             i++;
                                             pass = line[i];
                                             i++;
                                             email = line[i];
                                             i++;
                                             start = TimeSpan.Parse(line[i]);
                                             i++;
                                             stop = TimeSpan.Parse(line[i]); logger.LogInformation("A LUAT DATELE");
                                             logger.LogInformation($"Data ADD: {target},{pass},{email},{start},{stop}");
                                             logger.LogInformation("IA TITLURI SI DATE");
                                             Supervise();
                                             // pipeServer.Disconnect();
                                         }
                                     }
                                     else if (line[0] == "Start_P_List")
                                     {
                                         int i = 1;
                                         while (line[i] != "Stop_P_List")
                                         {
                                             kill.Add(line[i]); logger.LogInformation($"Kill ADD: {line[i]}");
                                             i++;
                                         }
                                         logger.LogInformation("Ia kill processes");
                                     }
                                 }
                         }
                         if (test == true)
                         {
                             // await writer.WriteLineAsync("Supervising");
                             // await writer.FlushAsync();
                             logger.LogInformation("test = true");
                             if (all == "Stop_supervise")
                             {
                                 Send_Email_with_File();
                                 test = false;
                                 logger.LogInformation("merge stop");

                             }
                             pipeServer.Disconnect();
                         }
                     }
                     // }
                     // }
                     pipeServer.Close();
                 }
             }
             catch (Exception e) { logger.LogInformation($"Error pipe function: {e.Message}"); }*/

            await Task.Run(() => P());
        }
    

        public async void P()
        {
            try
            {
                logger.LogInformation("Intra in PIPE");
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("mypipe", PipeDirection.In))
                {
                    await pipeServer.WaitForConnectionAsync();
                    logger.LogInformation("CONECTAT");
                    // using (var reader = new StreamReader(pipeServer))
                    // {
                    // using (StreamWriter writer = new StreamWriter(pipeServer))
                    //  {
                    if (pipeServer.IsConnected)
                    {
                        logger.LogInformation("CONECTAT 2");
                        string all = null;
                        StreamString _s = new StreamString(pipeServer);
                        while (test == false)
                        {
                            all = _s.ReadString(); // await
                            logger.LogInformation("INTRA TRUE SI CITESTE");
                            // string message = null;
                            using (StringReader sr = new StringReader(all))
                                if (sr.ReadLine() != null)
                                {
                                    logger.LogInformation("all = " + all);
                                    //      message = sr.ReadLine();
                                    string[] line = all.Split("\n");
                                    logger.LogInformation("split string");
                                    if (line[0] == "Title_IN")
                                    {
                                        logger.LogInformation("Title in");
                                        titles.Clear();
                                        logger.LogInformation("titles.Clear");
                                        int i = 1;
                                        bool t = false;
                                        while (line[i] != "Title_OUT")
                                        {
                                            if (!titles.Contains(line[i]))
                                                titles.Add(line[i]); logger.LogInformation($"Ttiles ADD: {line[i]}");
                                            i++;
                                            if (line[i] == "Title_OUT") { t = true; break; }
                                        }
                                        if (t == true)
                                        {

                                            i++;
                                            target = line[i];
                                            i++;
                                            pass = line[i];
                                            i++;
                                            email = line[i];
                                            i++;
                                            start = TimeSpan.Parse(line[i]);
                                            i++;
                                            stop = TimeSpan.Parse(line[i]); logger.LogInformation("A LUAT DATELE");
                                            logger.LogInformation($"Data ADD: {target},{pass},{email},{start},{stop}");
                                            logger.LogInformation("IA TITLURI SI DATE");
                                            test = true;
                                            pipeServer.Disconnect();
                                            await Task.Run(Supervise);
                                        }
                                    }
                                    else if (line[0] == "Start_P_List")
                                    {
                                        int i = 1;
                                        while (line[i] != "Stop_P_List")
                                        {
                                            kill.Add(line[i]); logger.LogInformation($"Kill ADD: {line[i]}");
                                            i++;
                                        }
                                        logger.LogInformation("Ia kill processes");
                                    }
                                }
                        }
                        if (test == true)
                        {
                            // await writer.WriteLineAsync("Supervising");
                            // await writer.FlushAsync();
                            try
                            {
                                logger.LogInformation("test = true");
                                if (all == "Stop_supervise")
                                {
                                    Send_Email_with_File();
                                    test = false;
                                    logger.LogInformation("merge stop");

                                }
                            }
                            catch(Exception e) { logger.LogError($"ERROR AT STOP SUPERVISE: {e.Message}"); }
                           // pipeServer.Disconnect();
                        }
                    }
                    // }
                    // }
                    pipeServer.Close();
                }
            }
            catch (Exception e) { logger.LogInformation($"Error pipe function: {e.Message}"); }
        }

        List<string> kill = new List<string>();
        List<ProcessActivity> Bad = new List<ProcessActivity>();

        public bool test = false;

        public void Supervise()
        {
            logger.LogInformation("Supervise");
            var until_start = new System.Timers.Timer(start-DateTime.Now.TimeOfDay);
            until_start.Elapsed += (sender, e) => Time_pass();
            until_start.Start();
            logger.LogInformation("INTRA in supervise");
            void Time_pass()
            {
                var until_stop = new System.Timers.Timer(stop - start);
                logger.LogInformation($"interval examen = {stop-start}");
                until_stop.Elapsed += (sender, e) => Time_pass1();
                until_stop.Start();
                logger.LogInformation("FINAL COUNTDOWN");
                void Time_pass1()
                {
                    logger.LogInformation("Inainte de if test = true");
                    if (test == true)
                    {
                        logger.LogInformation("inainte send email");
                        Send_Email_with_File();
                        logger.LogInformation("s-a dus timpul 1");
                        test = false;
                        logger.LogInformation("s-a dus timpul 2");
                    }
                    until_stop.Stop();
                }
                until_start.Stop();
                test = true;
            }
        }

        public void MonitorProcessActivity()
        {
            logger.LogInformation("MONITOR");
            Process[] processes = null;
            try {processes = Process.GetProcesses(); } catch(Exception ex) { logger.LogError("NU IA PROCESELE"); }
            foreach (Process process in processes.Where(x => x.MainWindowTitle.Length > 0))
            {
               // logger.LogInformation($"Intra in foreach in monitor -> {process.MainWindowTitle}");
                try
                {
                    if ((!process.MainWindowTitle.Equals("Settings")) && (!process.MainWindowTitle.Equals("Microsoft Text Input Application")))
                    {
                        // logger.LogInformation($"trece de if ala mare cu : {process.MainWindowTitle}");
                        if (!kill.Contains(process.ProcessName))
                        {
                            logger.LogInformation("NON KILL");
                            //  logger.LogInformation("Trece de kill");
                            DateTime startTime = process.StartTime;
                            TimeSpan activeDuration;
                            if (!process.HasExited)
                            {
                                activeDuration = DateTime.Now - startTime;
                            }
                            else
                            {
                                DateTime exitTime = process.ExitTime;
                                activeDuration = exitTime - startTime;
                            }
                            ProcessActivity activity = new ProcessActivity
                            {
                                MainWindowTitle = process.MainWindowTitle,
                                // Type = result.PredictedLabel,
                                StartTime = startTime,
                                ActiveDuration = activeDuration,
                            };
                            bool found = false;
                            if (test == false)
                            {
                                foreach (var _item in processActivities.Where(x => x.MainWindowTitle == activity.MainWindowTitle))
                                    found = true;
                                if (found == false)
                                {
                                    //  var sampleData = new ML_Model.ModelInput()
                                    //  {
                                    //     Col1 = @process.MainWindowTitle,
                                    //  };
                                    // var result = ML_Model.Predict(sampleData);
                                    // activity.Type = result.PredictedLabel;
                                    //logger.LogInformation("ADDED:");
                                    processActivities.Add(activity);
                                    //logger.LogInformation(activity.MainWindowTitle);
                                }
                                else
                                {
                                    TimeSpan Time = TimeSpan.FromMilliseconds(interval);
                                    foreach (var item in processActivities.Where(x => x.MainWindowTitle == activity.MainWindowTitle))
                                    {
                                        item.ActiveDuration = item.ActiveDuration + Time;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var _item in Bad.Where(x => x.MainWindowTitle == activity.MainWindowTitle))
                                    found = true;
                                if ((found == false) && (!titles.Contains(activity.MainWindowTitle)))
                                {
                                    //  var sampleData = new ML_Model.ModelInput()
                                    //  {
                                    //     Col1 = @process.MainWindowTitle,
                                    //  };
                                    // var result = ML_Model.Predict(sampleData);
                                    // activity.Type = result.PredictedLabel;
                                    Bad.Add(activity);
                                    logger.LogInformation("L-a pus la bad");
                                }
                                else
                                {
                                    TimeSpan Time = TimeSpan.FromMilliseconds(interval);
                                    foreach (var item in Bad.Where(x => x.MainWindowTitle == activity.MainWindowTitle))
                                    {
                                        item.ActiveDuration = item.ActiveDuration + Time;
                                    }
                                }

                                foreach (var _item in processActivities.Where(x => x.MainWindowTitle == activity.MainWindowTitle))
                                    found = true;
                                if (found == false)
                                {
                                    //  var sampleData = new ML_Model.ModelInput()
                                    //  {
                                    //     Col1 = @process.MainWindowTitle,
                                    //  };
                                    // var result = ML_Model.Predict(sampleData);
                                    // activity.Type = result.PredictedLabel;
                                    processActivities.Add(activity);
                                }
                                else
                                {
                                    TimeSpan Time = TimeSpan.FromMilliseconds(interval);
                                    foreach (var item in processActivities.Where(x => x.MainWindowTitle == activity.MainWindowTitle))
                                    {
                                        item.ActiveDuration = item.ActiveDuration + Time;
                                    }
                                }
                            }
                        }
                        else { process.Kill(); logger.LogInformation("IL OMOARA"); }
                    }
                }
                catch(Exception ex)
                {
                    logger.LogError($"Exception:{ex}");
                }
            }
        }

        static int interval = 500;
        int pipe = 0;
        int maintenance = 0;

      /*  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (maintenance == 0) { Maintenance(); maintenance++; }
            if (pipe == 0) { await Pipe_S(); pipe++; }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    MonitorProcessActivity();
                  //  await  Pipe_S();
                }
                catch(Exception ex) { logger.LogError(ex,ex.Message); }
                await Task.Delay(interval,stoppingToken);
            }
        }*/
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (maintenance == 0) { Maintenance(); maintenance++; }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    MonitorProcessActivity();
                    await  Pipe_S();
                }
                catch (Exception ex) { logger.LogError(ex, ex.Message); }
                logger.LogInformation("Other also run");
                await Task.Delay(interval, stoppingToken);
            }
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Get_Timer();
            Environment.SetEnvironmentVariable("FOLDER_PATH", AppDomain.CurrentDomain.BaseDirectory, EnvironmentVariableTarget.Process);
            logger.LogInformation("main folder: "+ AppDomain.CurrentDomain.BaseDirectory);
            logger.LogInformation("Started");
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopped");
            return base.StopAsync(cancellationToken);
        }

        public byte[] Encrypt(string simple_text, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(simple_text);
                        encrypted = ms.ToArray();
                    }
                }
            }
            return encrypted;
        }
        
        public string Decrypt(byte[] encrypted_text, byte[] Key , byte[] IV)
        {
            string simple_text = null;
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream(encrypted_text))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                            simple_text = reader.ReadToEnd();
                    }
                }
            }
            return simple_text;
        }

        public void Send_Email_with_File()
        {
            try
            {
                string folderName = "Supervise";
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    logger.LogInformation("folder facut");
                }
                //  string start_hour = start.ToString(@"hh\:mm");
                //    for (int i = 0; i < start_hour.Length; i++) if (start_hour[i] == ':') start_hour[i] = '.';
                string _st = start.ToString(@"hh\.mm");
                string fileName = target + " - ora" + _st + ".txt";
                string filePath = Path.Combine(folderPath, fileName);

                string text = null;
                foreach (ProcessActivity _process in Bad)
                {
                    text = text + _process.MainWindowTitle.ToString() + "\n"
                                // + _process.Type + "\n"
                                + _process.StartTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n"
                                + _process.ActiveDuration.ToString(@"hh\:mm\:ss") + "\n";
                    logger.LogInformation("Ia din Bad pt mail");
                }
                File.WriteAllText(filePath, text);
                logger.LogInformation("face fisierul text");
                MailMessage mail = new MailMessage("supervisoremail0@gmail.com", email, target + " - " + start, "All the unpermitted processes are in the text file.");
                mail.Attachments.Add(new Attachment(filePath));
                SmtpClient support_email = new SmtpClient("smtp.gmail.com");
                support_email.Port = 587;
                support_email.UseDefaultCredentials = false;
                support_email.Credentials = new System.Net.NetworkCredential("supervisoremail0", "fdkoztcrfnqjepui");
                support_email.EnableSsl = true;
                support_email.DeliveryMethod = SmtpDeliveryMethod.Network;
                support_email.Send(mail);
                logger.LogInformation("Mail TRIMIS");
            }
            catch(Exception e) { logger.LogError($"SEND MAIL FAILED: {e.Message}"); }
        }
    }
}

