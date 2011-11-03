using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using MediaPortal.GUI.Library;
using System.Net;
using System.IO;

namespace MyAnimePlugin3.DataHelpers
{
	/// <summary>
	/// We do the sending of data to the web cache async so that the user is not slowed down unncessarily
	/// </summary>
	public class XMLServiceQueue
	{
		 #region Private Members

        private const string QUEUE_STOP = "StopQueue";
		private BlockingList<XMLSendRequest> xmlToSend = new BlockingList<XMLSendRequest>();
        private BackgroundWorker workerXML = new BackgroundWorker();

        #endregion

        public int QueueCount
        {
			get { return xmlToSend.Count; }
        }

		public XMLServiceQueue()
        {
			workerXML.WorkerReportsProgress = true;
			workerXML.WorkerSupportsCancellation = true;
			workerXML.DoWork += new DoWorkEventHandler(workerXML_DoWork);
        }

		public void Init()
		{
			this.workerXML.RunWorkerAsync();
		}

		public void SendXML(XMLSendRequest req)
		{
			xmlToSend.Add(req);
		}

		private void workerXML_DoWork(object sender, DoWorkEventArgs args)
		{


			foreach (XMLSendRequest req in xmlToSend)
			{
				try
				{

					SendData(req.Uri, req.Xml);
					xmlToSend.Remove(req);
					//OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));

				}
				catch (Exception ex)
				{
					xmlToSend.Remove(req);
					//OnQueueUpdateEvent(new QueueUpdateEventArgs(this.QueueCount));
					BaseConfig.MyAnimeLog.Write(ex.ToString());
				}
			}

		}

		private static void SendData(string uri, string xml)
		{

			WebRequest req = null;
			WebResponse rsp = null;
			try
			{
				DateTime start = DateTime.Now;
				

				req = WebRequest.Create(uri);
				req.Method = "POST";        // Post method
				req.ContentType = "text/xml";     // content type

				// Wrap the request stream with a text-based writer
				StreamWriter writer = new StreamWriter(req.GetRequestStream());
				// Write the XML text into the stream
				writer.WriteLine(xml);
				writer.Close();
				// Send the data to the webserver
				rsp = req.GetResponse();

				TimeSpan ts = DateTime.Now - start;
				//BaseConfig.MyAnimeLog.Write("Sent Community Request in {0} ms: {1} --- {2}", ts.TotalMilliseconds, uri, xml);

			}
            catch (WebException webEx)
            {
                BaseConfig.MyAnimeLog.Write("Error(1) in XMLServiceQueue.SendData: {0}", webEx);
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error(2) in XMLServiceQueue.SendData: {0}", ex);
            }
            finally
			{
				if (req != null) req.GetRequestStream().Close();
				if (rsp != null) rsp.GetResponseStream().Close();
			}
		}

	}

	public class XMLSendRequest
	{
		private string uri = "";
		public string Uri
		{
			get { return uri; }
			set { uri = value; }
		}

		private string xml = "";
		public string Xml
		{
			get { return xml; }
			set { xml = value; }
		}

		public XMLSendRequest()
		{
		}

		public XMLSendRequest(string uri, string xml)
		{
			this.uri = uri;
			this.xml = xml;
		}

	}
}
