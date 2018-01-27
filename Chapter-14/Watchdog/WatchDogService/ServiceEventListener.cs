// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WatchDogService
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// ServiceEventListener is a class which listens to the eventsources registered and redirects the traces to a file
    /// Note that this class serves as a template to EventListener class and redirects the logs to /tmp/{appname_serviceName_yyyyMMddHHmmssffff}.
    /// You can extend the functionality by writing your code to implement rolling logs for the logs written through this class.
    /// You can also write your custom listener class and handle the registered evestsources accordingly. 
    /// </summary>
    internal class ServiceEventListener : EventListener
    {
        private const string filepath = "/tmp/";
        private string fileName =  "SidecardDemo" + "_" +  "WatchDogService" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".log";

        /// <summary>
        /// We override this method to get a callback on every event we subscribed to with EnableEvents
        /// </summary>
        /// <param name="eventData">The event arguments that describe the event.</param>
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            string message = "";

            if (eventData.Message != null)
            {
                message = string.Format(CultureInfo.InvariantCulture, eventData.Message, eventData.Payload.ToArray());
            }

            using (StreamWriter writer = new StreamWriter( new FileStream(filepath + fileName, FileMode.Append)))
            {
                // report all event information
                writer.WriteLine(Write(eventData.Task.ToString(),
                eventData.EventName,
                eventData.EventId.ToString(),
                eventData.Level, message));
            }
        }

        private static string Write(string taskName, string eventName, string id, EventLevel level, string message)
        {
            StringBuilder output = new StringBuilder();

            DateTime now = DateTime.UtcNow;
            output.Append(now.ToString("yyyy/MM/dd-HH:mm:ss.fff", CultureInfo.InvariantCulture));
            output.Append(',');
            output.Append(level);
            output.Append(',');
            output.Append(taskName);

            if (!string.IsNullOrEmpty(eventName))
            {
                output.Append('.');
                output.Append(eventName);
            }

            if (!string.IsNullOrEmpty(id))
            {
                output.Append('@');
                output.Append(id);
            }

            if (!string.IsNullOrEmpty(message))
            {
                output.Append(',');
                output.Append(message);
            }

            return output.ToString();
        }
    }
}