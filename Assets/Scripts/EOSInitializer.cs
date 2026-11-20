using UnityEngine;
using EpicTransport;

public class EOSBootstrap : MonoBehaviour
{
    private void Start()
    {
        var options = new TransportInitializeOptions
        {
            ProductName = "Wool Did It",
            ProductId = "a90391692fd54c1bb29d56ce1b9365a6",
            ClientId = "xyza7891NyONG6oCJJVbDsixx3f7SIuB",
            ClientSecret = "aA0JRkZlzn34Igxsf3sKfpXDwQgGTOC6WhdN/NGIGho",
            SandboxId = "a8ad4740c8a241948efd12c680fc5659",
            DeploymentId = "119f5aa95a584e4485afc36897ca6663",

            EncryptionKey = "86e11096c906e6f610a30cfc7228a403f2c419c8cc10d18d53a2afe10ea6aaa5",

            DisplayName = "Player"
        };

        EOSManager.Initialize(options);
    }
}
