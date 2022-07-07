using System;
using System.Collections.Generic;
using System.IO;

namespace BP_Fmt
{
    [Serializable]
    public class Request
    {
        public static Request ReadFromBinaryStream(BinaryReader br)
        {
            var req = new Request();
            req.Id = Guid.Parse(br.ReadString());
            req.Uri = br.ReadString();
            req.Method = br.ReadString();
            req.Payload = br.ReadBytes(br.ReadInt32());
            req.Headers = new Dictionary<string, string>();
            var headerCount = br.ReadInt32();
            for (var i = 0; i < headerCount; i++)
            {
                req.Headers.Add(br.ReadString(), br.ReadString());
            }
            return req;
        }
        
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public string Method { get; set; }
        public byte[] Payload { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public void WriteToBinaryStream(BinaryWriter bw)
        {
            bw.Write(Id.ToString());
            bw.Write(Uri);
            bw.Write(Method);
            bw.Write(Payload);
            bw.Write(Headers.Count);
            foreach (var header in Headers)
            {
                bw.Write(header.Key);
                bw.Write(header.Value);
            }
        }

    }
    [Serializable]
    public class Response
    {
        public static Response ReadFromBinaryStream(BinaryReader br)
        {
            var res = new Response();
            res.Id = Guid.Parse(br.ReadString());
            res.StatusCode = br.ReadInt32();
            res.Payload = br.ReadBytes(br.ReadInt32());
            res.Headers = new Dictionary<string, string>();
            var headerCount = br.ReadInt32();
            for (var i = 0; i < headerCount; i++)
            {
                res.Headers.Add(br.ReadString(), br.ReadString());
            }
            return res;
        }
        
        public Guid Id { get; set; }
        public int StatusCode { get; set; }
        public byte[] Payload { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public void WriteToBinaryStream(BinaryWriter bw)
        {
            bw.Write(Id.ToString());
            bw.Write(StatusCode);
            bw.Write(Payload);
            bw.Write(Headers.Count);
            foreach (var header in Headers)
            {
                bw.Write(header.Key);
                bw.Write(header.Value);
            }
        }
    }
}
