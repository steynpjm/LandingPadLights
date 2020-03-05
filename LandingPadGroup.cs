using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{
    public enum ConnectorStatus
    {
        NoConnector,
        NotConnected,
        AwaitingConnection,
        Connected
    }

    public class LandingPadGroup
    {

        public string GroupName { get; private set; }

        public List<LandingLight> Lights { get; set; }
        public List<DisplayPanel> Panels { get; private set; }
        public IMyShipConnector Connector { get; set; }

        public ConnectorStatus ConnectorStatus { get; private set; }

        public int MaxSequence { get; set; }
        public float TotalTime { get; set; }
        public float OffsetPerLight { get; set; }

        public Color CurrentColor { get; set; }

        public string ConnectorStatusAsText
        {
            get
            {
                string result = string.Empty;

                switch (ConnectorStatus)
                {
                    case ConnectorStatus.NoConnector:
                        result = "X";
                        break;
                    case ConnectorStatus.NotConnected:
                        result = "-";
                        break;
                    case ConnectorStatus.AwaitingConnection:
                        result = "=";
                        break;
                    case ConnectorStatus.Connected:
                        result = "O";
                        break;
                    default:
                        break;
                }

                return result;
            }
        }

        public string ConnectedShipName
        {
            get
            {
                string result = "None";
                if (Connector.OtherConnector != null) result = Connector.OtherConnector.CubeGrid.CustomName;
                return result;
            }
        }


        public void ConfigureBlocks()
        {
            if (Connector != null)
            {
                CurrentColor = NotConnected;
                if (Connector.Status == MyShipConnectorStatus.Connected)
                {
                    CurrentColor = Connected;
                }
            }

            if (Lights == null || Lights.Count == 0) return;

            MaxSequence = Lights.Max(x => x.Sequence);

            if (MaxSequence > 0)
            {
                TotalTime = (MaxSequence + 3) * _timePerLight;   // The total time for this group.

                OffsetPerLight = (1.0f / (MaxSequence + 3)) * 100.0f;

                foreach (LandingLight light in Lights)
                {
                    light.Light.BlinkIntervalSeconds = TotalTime;
                    light.Light.BlinkOffset = (MaxSequence - light.Sequence) * OffsetPerLight;
                    light.Light.Intensity = 10.0f;
                    light.Light.Radius = 1.0f;
                    light.Light.Falloff = 0.0f;
                }

                SetBlockColors();
            }
        }

        public void CheckBlocks()
        {
            ConnectorStatus currentConnectorStatus = ConnectorStatus;

            if (Connector == null)
            {
                ConnectorStatus = ConnectorStatus.NoConnector;
                CurrentColor = NotConnected;
            }
            else
            {
                switch (Connector.Status)
                {
                    case MyShipConnectorStatus.Connected:
                        ConnectorStatus = ConnectorStatus.Connected;
                        CurrentColor = Connected;
                        break;
                    case MyShipConnectorStatus.Connectable:
                        ConnectorStatus = ConnectorStatus.AwaitingConnection;
                        CurrentColor = Connectable;
                        break;
                    case MyShipConnectorStatus.Unconnected:
                        ConnectorStatus = ConnectorStatus.NotConnected;
                        CurrentColor = NotConnected;
                        break;
                }
            }

            if (ConnectorStatus != currentConnectorStatus)
            {
                foreach (DisplayPanel panel in Panels)
                {
                    panel.UpdatePanel();
                }
            }

            SetBlockColors();
        }

        private void SetBlockColors()
        {
            foreach (LandingLight light in Lights)
            {
                light.Light.Color = CurrentColor;
            }
        }

        public LandingPadGroup(string groupName)
        {
            GroupName = groupName;
            Lights = new List<LandingLight>();
            Panels = new List<DisplayPanel>();

            OffsetPerLight = 0.0f;

            CurrentColor = NotConnected;
            ConnectorStatus = ConnectorStatus.NoConnector;
        }

        private Color NotConnected = Color.Green;
        private Color Connected = Color.Red;
        private Color Connectable = Color.Orange;

        private const float _timePerLight = 0.25f;
    }
}
