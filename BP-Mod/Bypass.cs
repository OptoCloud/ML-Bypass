using MelonLoader;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ML_Bypass
{
    public class Bypass : MelonMod
    {
        public static Bypass Instance { get; private set; }

        Process _bypassProcess;
        BinaryReader _bypassReader;
        BinaryWriter _bypassWriter;

        ConcurrentDictionary<Guid, Action<BP_Fmt.Response>> _responseCallbacks = new ConcurrentDictionary<Guid, Action<BP_Fmt.Response>>();

        void ListenerThread()
        {
            while (true)
            {
                var response = BP_Fmt.Response.ReadFromBinaryStream(_bypassReader);
                if (_responseCallbacks.TryRemove(response.Id, out Action<BP_Fmt.Response> callback))
                {
                    callback(response);
                }
            }
        }
        
        public Bypass()
        {
            if (Instance != null)
            {
                MelonLogger.Error("[Bypass] Instance already exists!");
                return;
            }
            Instance = this;

            // Create executable file from resource "BP-Proc.exe"
            File.WriteAllBytes(Path.Combine("UserData", "BP-Proc.exe"), Properties.Resources.BP_Proc);

            // Launch executable file
            _bypassProcess = Process.Start(Path.Combine("UserData", "BP-Proc.exe"));

            // Get STDIN and STDOUT
            _bypassReader = new BinaryReader(_bypassProcess.StandardInput.BaseStream);
            _bypassWriter = new BinaryWriter(_bypassProcess.StandardOutput.BaseStream);

            // Start listening for messages in seperate thread
            new Thread(ListenerThread);
        }

        public void Request(BP_Fmt.Request request, Action<BP_Fmt.Response> responseCallback)
        {
            _responseCallbacks.TryAdd(request.Id, responseCallback);
            request.WriteToBinaryStream(_bypassWriter);
        }
    }
}
