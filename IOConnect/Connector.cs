using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

namespace IOConnect
{
    public class Connector
    {
        private static MqttClient client;
        private static string prefix = string.Empty;

        public Connector(string hostname, string prefix = "")
        {
            client = new MqttClient(hostname);
            if (prefix.Length > 0)
            {
                prefix = prefix.Trim(new char[] { '/' });
                prefix += @"/";
            }
            Connect();
        }

        private void Connect()
        {
            try
            {
                client.Connect("ioconnect_" + Guid.NewGuid());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }

        private void Publish(string topic, string data)
        {
            if (!client.IsConnected)
            {
                Connect();
            }
            if (client.IsConnected)
            {
                try
                {
                    client.Publish(topic, Encoding.UTF8.GetBytes(data), 0, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void DispatchMessage(MouseMoveEvent mevent)
        {
            Publish(prefix + "probe/mouse/move", JsonConvert.SerializeObject(mevent));
        }

        public void DispatchMessage(MouseClickEvent mevent)
        {
            Publish(prefix + "probe/mouse/click", JsonConvert.SerializeObject(mevent));
        }

    }
}
