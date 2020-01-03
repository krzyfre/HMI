using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace HMI
{
    public static class OPC
    {
        static UaTcpSessionChannel channel;
        static string path;

        public static void SetPath(string _path)
        {
            path = _path;
        }

        public static async Task<DataValue[]> ReadVar(int node, string[] VarNames)
        {
            try
            {
                ReadValueId[] valuesToRead = new ReadValueId[VarNames.Length];
                for (int i = 0; i < VarNames.Length; i++)
                {
                    valuesToRead[i] = new ReadValueId
                    {
                        // you can parse the nodeId from a string.
                        NodeId = NodeId.Parse("ns=" + node.ToString() + ";s=" + VarNames[i]),
                        // variable class nodes have a Value attribute.
                        AttributeId = AttributeIds.Value
                    };
                }

                var readRequest = new ReadRequest
                {
                    NodesToRead = valuesToRead
                };
                // send the ReadRequest to the server.
                var readResult = await channel.ReadAsync(readRequest);
                return readResult.Results;
            }
            catch (Exception ex)
            {
                //await channel.AbortAsync();
                return null;
            }
        }

        public static async Task<object> WriteVar(int node, KeyValuePair<string, object>[] ValuesToSet)
        {
            try
            {
                WriteValue[] valuesToWrite = new WriteValue[ValuesToSet.Length];
                for (int i = 0; i < ValuesToSet.Length; i++)
                {
                    valuesToWrite[i] = new WriteValue
                    {
                        // you can parse the nodeId from a string.
                        NodeId = NodeId.Parse("ns=" + node.ToString() + ";s=" + ValuesToSet[i].Key),
                        // variable class nodes have a Value attribute.
                        AttributeId = AttributeIds.Value,
                        Value = new DataValue(ValuesToSet[i].Value)
                    };
                }


                var writeRequest = new WriteRequest
                {
                    NodesToWrite = valuesToWrite
                };
                // send the ReadRequest to the server.
                var writeResult = await channel.WriteAsync(writeRequest);
                return writeResult;
            }
            catch (Exception ex)
            {
                //await channel.AbortAsync();
                return null;
            }
            
        }
        public static async Task<bool> StartOPC()
        {
            var clientDescription = new ApplicationDescription
            {
                ApplicationName = "Workstation.UaClient.FeatureTests",
                ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:Workstation.UaClient.FeatureTests",
                ApplicationType = ApplicationType.Client
            };

            try
            {
                if (channel != null) channel.CloseAsync();
            }
            catch { }
            

            // create a 'UaTcpSessionChannel', a client-side channel that opens a 'session' with the server.
            channel = new UaTcpSessionChannel(
                clientDescription,
                null, // no x509 certificates
                new AnonymousIdentity(), // no user identity

                //IP serwera
                path,// the public endpoint of a server at opcua.rocks.
                SecurityPolicyUris.None); // no encryption
            try
            {
                // try opening a session and reading a few nodes.
                await channel.OpenAsync();
                //HMI_SuctionCup1
                return true;
            }
            catch (Exception ex)
            {
                await channel.AbortAsync();
                Console.WriteLine(ex.Message);
                return true;
            }
        }
    }
    
}
