using MPack;
using System;
using System.Net.WebSockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace SeeClearlyLighthouse
{
    /// <summary>
    /// Provides an implementation of the lighthouse API communication protocol
    /// </summary>
    /// <param name="Username">Your accounts username</param>
    /// <param name="Token">Your current token</param>
    /// <param name="ServerUri">Optionally, the server uri</param>
    public class LighthouseClient(
            string Username,
            string Token,
            string ServerUri = "wss://lighthouse.uni-kiel.de/websocket") : IDisposable
    {
        ClientWebSocket webSocket = new();
        MDict? msgpackDict = null;

        /// <summary>
        /// Connect to the server
        /// </summary>
        /// <returns>Self</returns>
        public async Task<LighthouseClient> Connect(CancellationToken cancellationToken = default)
        {
            await webSocket.ConnectAsync(new Uri(ServerUri), cancellationToken);

            msgpackDict = new MDict
            {
                {"REID", MToken.From(0)},
                {"AUTH", MToken.From(new MDict {
                        {"USER", Username},
                        {"TOKEN", Token},
                    })
                },
                {"VERB", "PUT"},
                {"PATH", MToken.From(new[] {
                        "user",
                        Username,
                        "model",
                    })
                },
                {"META", MToken.From(new MDict { }) },
                {"PAYL", ""},
            };

            return this;
        }

        /// <summary>
        /// Send an image to the server
        /// </summary>
        /// <param name="image">The image tensor with dimensions [width, height, color channel] of size [14, 28, 3]</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> SendImage(byte[,,] image, CancellationToken cancellationToken = default)
        {
            if (msgpackDict == null)
            {
                throw new ArgumentException("Not connected yet!");
            }
            if (image.GetLength(0) != 14 || image.GetLength(1) != 28 || image.GetLength(2) != 3)
            {
                throw new ArgumentException("The image tensor dimensions need to be [14, 28, 3]!");
            }

            msgpackDict["PAYL"] = image.Cast<byte>().ToArray();

            await webSocket.SendAsync(new ReadOnlyMemory<byte>(msgpackDict.EncodeToBytes()), WebSocketMessageType.Binary, true, cancellationToken);

            var answerBuffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(answerBuffer, cancellationToken);
            return Encoding.UTF8.GetString(answerBuffer, 0, result.Count);
        }

        public void Dispose()
        {
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default).Wait();
            webSocket.Dispose();
        }
    }
}
