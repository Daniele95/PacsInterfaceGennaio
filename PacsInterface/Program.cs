﻿using Dicom;
using Dicom.Network;
using GUI;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static PacsInterface.QueryParams;

namespace PacsInterface
{
    class Program
    {
        MainWindow mainWindow;
        CurrentConfiguration configuration;

        public class CurrentConfiguration
        {
            public string ip { get; private set; }
            public int port { get; private set; }
            public string AET { get; private set; }
            public bool saveData { get; private set; }
            public string thisNodeAET { get; private set; }
            public int thisNodePort { get; private set; }

            public CurrentConfiguration()
            {
                update();
            }

            public void update()
            {
                ip = File.ReadAllLines("ServerConfig.txt")[0];
                port = int.Parse(File.ReadAllLines("ServerConfig.txt")[1]);
                AET = File.ReadAllLines("ServerConfig.txt")[2];
                saveData = bool.Parse(File.ReadAllLines("ServerConfig.txt")[3]);
                thisNodeAET = File.ReadAllLines("ServerConfig.txt")[4];
                thisNodePort = int.Parse(File.ReadAllLines("ServerConfig.txt")[5]);
            }

        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            configuration.update();

            var eventHandler = new EventHandler((sender, ev) =>
            {
                // ./databaseFolder referenced in Listener
                Directory.Delete("./databaseFolder", true);
            });
            if (!configuration.saveData) AppDomain.CurrentDomain.ProcessExit += eventHandler;
            else AppDomain.CurrentDomain.ProcessExit -= eventHandler;

        }


        public Program(MainWindow mainWindow)
        {
            configuration = new CurrentConfiguration();

            Process[] listeners = Process.GetProcessesByName("Listener");
            if (listeners.Length == 0)
            {
                //MessageBox.Show("No listener available");
                //Environment.Exit(0);
                Process.Start("Listener");
            }
            else
            {
                foreach (var listener in listeners) listener.Kill();
                Process.Start("Listener");
            }

            this.mainWindow = mainWindow;

            // setup GUI
            SetupGUI.setupStudyTable(mainWindow);
            SetupGUI.setupSeriesTable(mainWindow.downloadPage);

            // handle events
            mainWindow.queryPage.Search.Click += searchButton_ClickEvent;
            mainWindow.queryPage.localSearch.Click += localSearchButton_ClickEvent;
            mainWindow.downloadPage.series_ClickEvent += series_ClickEvent;
            mainWindow.downloadPage.thumb_ClickEvent += thumb_ClickEvent;

            // watch changes in configuration
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName("./");
            watcher.Filter = Path.GetFileName("ServerConfig.txt");
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;


            mainWindow.frame.Navigate(mainWindow.queryPage);
        }



        // LOCAL SEARCH----------------------------------------------------------------------
        List<StudyQueryOut> studyResponses;
        void localSearchButton_ClickEvent(object a, EventArgs b)
        {
            mainWindow.queryPage.study_ClickEvent -= study_ClickEvent;
            mainWindow.queryPage.study_ClickEvent += localStudy_ClickEvent;
            //

            StudyQueryIn studyQuery = new StudyQueryIn(mainWindow.queryPage);

            using (var db = new LiteDatabase("./databaseFolder/database.db"))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<StudyQueryOut>("studies");

                var results = col.Find(x =>
                   x.StudyInstanceUID.Equals(studyQuery.StudyInstanceUID) ||
                   x.PatientID.Equals(studyQuery.PatientID) ||
                   x.PatientName.Contains(studyQuery.PatientName) ||
                   x.StudyDate.Equals(studyQuery.StudyDate) ||
                   x.ModalitiesInStudy.Equals(studyQuery.ModalitiesInStudy)
                );

                mainWindow.queryPage.listView.Items.Clear();

                foreach (var result in results)
                    mainWindow.queryPage.listView.Items.Add(result);
            }
        }
        void localStudy_ClickEvent(ListViewItem sender)
        {
            MessageBox.Show("clicked");
        }

        // EXTERN SEARCH:--------------------------------------------------------------------

        // search STUDIES-----------------------------------------------
        void searchButton_ClickEvent(object a, EventArgs b)
        {
            mainWindow.queryPage.study_ClickEvent -= localStudy_ClickEvent;
            mainWindow.queryPage.study_ClickEvent += study_ClickEvent;
            //

            StudyQueryIn studyQuery = new StudyQueryIn(mainWindow.queryPage);

            // find studies
            DicomCFindRequest cfind = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);

            cfind.Dataset.Add(DicomTag.PatientName, studyQuery.PatientName);
            cfind.Dataset.Add(DicomTag.StudyDate, studyQuery.StudyDate);
            cfind.Dataset.Add(DicomTag.ModalitiesInStudy, studyQuery.ModalitiesInStudy);
            cfind.Dataset.Add(DicomTag.StudyInstanceUID, "");
            cfind.Dataset.Add(DicomTag.PatientID, studyQuery.PatientID);
            cfind.Dataset.Add(DicomTag.PatientBirthDate, "");
            cfind.Dataset.Add(DicomTag.StudyDescription, "");

            int numResponses = 0;

            cfind.OnResponseReceived = (request, response) =>
            {
                if (response.HasDataset)
                {
                    numResponses++;
                    studyResponses.Add(new StudyQueryOut(response));
                }
                if (!response.HasDataset) Console.WriteLine("got " + numResponses.ToString() + " studies" + Environment.NewLine);
            };

            var client = new DicomClient();
            client.AddRequest(cfind);

            mainWindow.queryPage.listView.Items.Clear();
            studyResponses = new List<StudyQueryOut>();

            try
            {
                Console.WriteLine("Querying server " + configuration.ip + ":" + configuration.port
                    + " for STUDIES with:" + Environment.NewLine + studyQuery.ToString());
                client.Send(configuration.ip, configuration.port, false, configuration.thisNodeAET, configuration.AET);
            }
            catch (Exception ec)
            {
                Console.WriteLine(ec.Message + Environment.NewLine + ec.StackTrace);
            }
            // arrange results in table
            foreach (StudyQueryOut studyResponse in studyResponses)
                mainWindow.queryPage.listView.Items.Add(studyResponse);
        }

        // search SERIES------------------------------------------------
        List<SeriesQueryOut> seriesResponses;
        void study_ClickEvent(ListViewItem sender)
        {
            // find series ...............................
            var studyQueryOut = sender.Content as StudyQueryOut;
            DicomCFindRequest cfind = new DicomCFindRequest(DicomQueryRetrieveLevel.Series);
            PropertyInfo[] properties = typeof(SeriesQueryOut).GetProperties();
            foreach (var property in properties)
            {
                string tag = typeof(DicomTag).GetField(property.Name).GetValue(null).ToString();
                DicomTag theTag = (DicomTag.Parse(tag));
                if (theTag == DicomTag.StudyInstanceUID)
                    cfind.Dataset.Add(DicomTag.StudyInstanceUID, studyQueryOut.StudyInstanceUID);
                else
                    cfind.Dataset.Add(theTag, "");
            }
            cfind.OnResponseReceived = (request, response) =>
            {
                if (response.HasDataset) seriesResponses.Add(new SeriesQueryOut(response));
                else Console.WriteLine("got " + seriesResponses.Count + " series");
            };

            var client = new DicomClient();
            client.AddRequest(cfind);

            mainWindow.frame.Navigate(mainWindow.downloadPage);
            mainWindow.downloadPage.dgUsers.Items.Clear();
            seriesResponses = new List<SeriesQueryOut>();

            try
            {
                Console.WriteLine(Environment.NewLine + "#############################################################" + Environment.NewLine + Environment.NewLine
                    + "Querying server " + configuration.ip + ":" + configuration.port +
                    " for SERIES in study no. " + studyQueryOut.StudyInstanceUID);
                client.Send(configuration.ip, configuration.port, false, configuration.thisNodeAET, configuration.AET);
            }
            catch (Exception ec)
            {
                Console.WriteLine("Impossible to connect to server");
            }
            // arrange results in table
            foreach (SeriesQueryOut seriesResponse in seriesResponses)
            {

                Console.WriteLine("Added series to table" + Environment.NewLine +
                    "-------------------------------------------------------------------" + Environment.NewLine);
                SetupGUI.addElementToSeriesTable(mainWindow.downloadPage, seriesResponse);
            }
        }

        // search IMAGES------------------------------------------------
        List<string> imageIDs;
        string getImagesInSeries(SeriesQueryOut seriesResponse)
        {
            // find images ...............................
            DicomCFindRequest cfind = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);
            cfind.Dataset.Add(DicomTag.StudyInstanceUID, seriesResponse.StudyInstanceUID);
            cfind.Dataset.Add(DicomTag.SeriesInstanceUID, seriesResponse.SeriesInstanceUID);
            cfind.Dataset.Add(DicomTag.SOPInstanceUID, "");
            cfind.Dataset.Add(DicomTag.InstanceNumber, "");
            int numImages = 0;
            cfind.OnResponseReceived = (request, response) =>
            {
                if (response.HasDataset)
                {
                    imageIDs.Add(response.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID));
                    numImages++;
                }
                if (!response.HasDataset) Console.WriteLine("got " + numImages + " images" + Environment.NewLine);
            };

            var client = new DicomClient();

            client.AddRequest(cfind);
            /* var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                 DicomStorageCategory.Image,
                 DicomTransferSyntax.ExplicitVRLittleEndian,
                 DicomTransferSyntax.ImplicitVRLittleEndian,
                 DicomTransferSyntax.ImplicitVRBigEndian);
             client.AdditionalPresentationContexts.AddRange(pcs); */

            imageIDs = new List<string>();

            try
            {
                Console.WriteLine(Environment.NewLine + "Querying server " + configuration.ip + ":" + configuration.port +
                    " for IMAGES in series no. " + seriesResponse.SeriesInstanceUID);
                client.Send(configuration.ip, configuration.port, false, configuration.thisNodeAET, configuration.AET);
            }
            catch (Exception ec)
            {
                Console.WriteLine("Impossible to connect to server");
            }

            string SOPInstanceUID = "";
            if (imageIDs.Count > 0)
                SOPInstanceUID = imageIDs[(int)(imageIDs.Count / 2.0f)];

            return SOPInstanceUID;
        }

        // download sample image----------------------------------------
        public void thumb_ClickEvent(FrameworkElement sender)
        {

            var dyn = ((FrameworkElement)sender).DataContext as dynamic;
            var list = mainWindow.downloadPage.dgUsers.Items;
            var index = list.IndexOf(dyn);
            var seriesResponse = seriesResponses[index];

            try
            {
                BitmapImage img = new BitmapImage();
                string SOPInstanceUID = getImagesInSeries(seriesResponse);
                if (SOPInstanceUID != "")
                {
                    Console.WriteLine("Downloading from server " + configuration.ip + ":" + configuration.port +
                        " sample image no. " + SOPInstanceUID + Environment.NewLine);
                    img = downloadSampleImage(seriesResponse, SOPInstanceUID);

                    SetupGUI.addImage(mainWindow.downloadPage, sender, img);
                    Console.WriteLine("Done.");
                }
            }
            catch (Exception ec)
            {
                Console.WriteLine(ec.StackTrace);
            }
            
        }
        BitmapImage downloadSampleImage(SeriesQueryOut seriesResponse, string SOPInstanceUID)
        {
            var cmove = new DicomCMoveRequest(configuration.thisNodeAET, seriesResponse.StudyInstanceUID, seriesResponse.SeriesInstanceUID, SOPInstanceUID);

            var client = new DicomClient();
            client.AddRequest(cmove);

            File.Create("singleImage.txt").Close();

            DirectoryInfo di = new DirectoryInfo("./images/");
            foreach (FileInfo file in di.GetFiles()) file.Delete();
            client.Send(configuration.ip, configuration.port, false, configuration.thisNodeAET, configuration.AET);

            var image = new BitmapImage();
            if (File.Exists("./images/file.jpg"))
            {
                var uriSource = new Uri(Path.GetFullPath("./images/file.jpg"));
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = uriSource;
                image.EndInit();
            }
            else MessageBox.Show("Listener didn't receive anything or didn't work");

            File.Delete("singleImage.txt");
            return image;
        }

        // download series----------------------------------------------
        void series_ClickEvent(ListViewItem sender)
        {
            var obj = (IDictionary<string, object>)(sender.Content);
            string studyInstanceUID = obj["StudyInstanceUID"].ToString();
            string seriesInstanceUID = obj["SeriesInstanceUID"].ToString();
            // download
            File.Delete("singleImage.txt");
            var cmove = new DicomCMoveRequest(configuration.thisNodeAET, studyInstanceUID, seriesInstanceUID);

            var client = new DicomClient();
            client.AddRequest(cmove);

            Console.WriteLine("Downloading series from server " + configuration.ip + ":" + configuration.port);
            client.Send(configuration.ip, configuration.port, false, configuration.thisNodeAET, configuration.AET);
            Console.WriteLine("Done.");
        }
    }
}