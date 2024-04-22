using MPack;
using see_clearly_lighthouse;
using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Hello, World!");

Uri uri = new("wss://lighthouse.uni-kiel.de/websocket");

byte[,,] image = new byte[14, 28, 3];
for (int i = 0; i < 14; i++)
    image[i, 4, 0] = 128;

var msgpackDict = new MDict
{
    {"REID", MToken.From(0)},
    {"AUTH", MToken.From(new MDict {
            {"USER", Secret.Username},
            {"TOKEN", Secret.Token},
        })
    },
    {"VERB", "PUT"},
    {"PATH", MToken.From(new[] {
            "user",
            Secret.Username,
            "model",
        })
    },
    {"META", MToken.From(new MDict { }) },
    {"PAYL", image.Cast<byte>().ToArray()},
};

using ClientWebSocket ws = new();
await ws.ConnectAsync(uri, default);

await ws.SendAsync(new ReadOnlyMemory<byte>(msgpackDict.EncodeToBytes()), WebSocketMessageType.Binary, true, default);

var bytes = new byte[1024];
var result = await ws.ReceiveAsync(bytes, default);
string res = Encoding.UTF8.GetString(bytes, 0, result.Count);

await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);

Console.WriteLine(res);