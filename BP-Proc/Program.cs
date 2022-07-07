using BP_Fmt;
using System.Collections.Concurrent;
using System.Text;

var inputReader = new BinaryReader(Console.OpenStandardInput());
var binaryWriter = new BinaryWriter(Console.OpenStandardOutput());

// Launch 2 threads, one for queueing incoming requests, and one for handling them
var requestQueue = new ConcurrentQueue<Request>();
var requestHandler = new Thread(() =>
{
    HttpClient httpClient = new HttpClient();
    while (true)
    {
        if (requestQueue.TryDequeue(out Request? request))
        {
            if (request == null)
                continue;

            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), request.Uri);
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }

            httpClient.SendAsync(httpRequest).ContinueWith(async (task) =>
            {
                if (task.Exception != null)
                {
                    var response = new Response
                    {
                        Id = request.Id,
                        StatusCode = 600,
                        Payload = Encoding.UTF8.GetBytes(task.Exception.ToString())
                    };
                    response.WriteToBinaryStream(binaryWriter);
                }
                else
                {
                    var result = task.Result;

                    var response = new Response
                    {
                        Id = request.Id,
                        StatusCode = (int)result.StatusCode,
                        Payload = await result.Content.ReadAsByteArrayAsync()
                    };
                    foreach (var header in task.Result.Headers)
                    {
                        response.Headers.Add(header.Key, header.Value.First());
                    }
                    response.WriteToBinaryStream(binaryWriter);
                }
            });
        }
        else
        {
            Thread.Sleep(10);
        }
    }
});

while (true)
{
    var request = Request.ReadFromBinaryStream(inputReader);
    requestQueue.Enqueue(request);
}