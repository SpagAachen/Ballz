using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Gui;
using GeonBit.UI.Entities;
using System.Net;

namespace Ballz
{
    class JoinByIpMenu: Gui.MenuPanel
    {
        TextInput IpInput = new TextInput(false);
        TextInput PortInput = new TextInput(false);
        Button JoinButton = new Button("Join");

        public JoinByIpMenu(): base("Join by IP")
        {
            AddItem(new Label("Host IP Address:"));
            AddItem(IpInput);
            AddItem(new Label("Host Port:"));

            PortInput.Value = "16116";
            AddItem(PortInput);

            JoinButton.OnClick += (e) =>
            {
                IPAddress host;

                if(!IPAddress.TryParse(IpInput.Value, out host))
                {
                    MessageOverlay.ShowError("Invalid IP Address format. Please enter a valid IP address, such as 192.168.0.13 (IPv4) or 2001:4860:4860::8844 (IPv6)");
                    return;
                }

                int port = 0;
                if(!int.TryParse(PortInput.Value, out port) || port < 0 || port > 65536)
                {
                    MessageOverlay.ShowError("Invalid Port number. Please specify a number between 0 and 65536");
                    return;
                }

                var overlay = MessageOverlay.ShowWaitMessage("Connecting", "Please Wait", onCancel: () => { Ballz.The().Network.Disconnect(); });

                Ballz.The().Network.ConnectToServer(
                    host,
                    port,
                    onSuccess: () => {
                        Ballz.The().Logic.OpenMenu(new LobbyMenu(isHost: false, gameName: "", isPrivate: false));
                        overlay.Hide();
                    },
                    onFail: () => {
                        overlay.Hide();
                        MessageOverlay.ShowError("Could not connect to host.");
                    }
                );
            };
            AddItem(JoinButton);

            AddItem(new BackButton());
        }
    }
}
