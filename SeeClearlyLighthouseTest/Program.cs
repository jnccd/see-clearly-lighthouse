using PrototypeSeeClearlyLighthouse;
using SeeClearlyLighthouse;

var client = await new LighthouseClient(Secret.Username, Secret.Token).Connect();

byte[,,] image = new byte[14, 28, 3];
for (int i = 0; i < 14; i++)
    image[i, 4, 1] = 128;

var res = await client.SendImage(image);
Console.WriteLine(res);
