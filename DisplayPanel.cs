using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{

    public interface IPanelTextBuilder
    {
        string BuildText();
    }

    public enum PanelType
    {
        Main,
        Detail,
        Indicator
    }

    public class DisplayPanel
    {
        
        public IMyTextPanel TextPanel { get; private set; }
        public PanelType PanelType { get; private set; }
        public LandingPadGroup Group { get; private set; }

        public DisplayPanel(LandingPadGroup group, IMyTextPanel textPanel, PanelType panelType)
        {
            Group = group;
            TextPanel = textPanel;
            PanelType = panelType;

            // LCD Panels can be a detail panel or an indicator panel.
            switch (panelType)
            {
                case PanelType.Detail:                
                    _textBuilder = new DetailPanelTextBuilder(group);
                    break;
                case PanelType.Indicator:
                    _textBuilder = new IndicatorPanelTextBuilder(group);
                    break;
                case PanelType.Main:
                    break;

                default:
                    break;
            }
        }

        public DisplayPanel(List<LandingPadGroup> groups, IMyTextPanel textPanel)
        {
            Group = null;
            TextPanel = textPanel;
            PanelType = PanelType.Main;
            _textBuilder = new MainPanelTextBuilder(groups);
        }


        public void UpdatePanel()
        {
            TextPanel.WriteText(_textBuilder.BuildText());
        }

        private IPanelTextBuilder _textBuilder;
    }

    public class MainPanelTextBuilder : IPanelTextBuilder
    {

        public MainPanelTextBuilder(List<LandingPadGroup> groups)
        {
            _groups = groups;
        }

        public string BuildText()
        {
            return BuildInformationForGroups(_groups);
        }

        private string BuildInformationForGroups(List<LandingPadGroup> groups)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Landing Pads");

            builder.AppendLine("--------------");
            builder.AppendLine($"Groups={groups.Count}");

            foreach (LandingPadGroup group in groups)
            {
                builder.AppendLine($"Group={group.GroupName}");
            }

            return builder.ToString();
        }

        private List<LandingPadGroup> _groups;
    }

    public class DetailPanelTextBuilder : IPanelTextBuilder
    {
        public string BuildText()
        {
            return BuildInformationForGroup(_group);
        }

        private string BuildInformationForGroup(LandingPadGroup group)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(_title);
            builder.AppendLine("-------------------------");
            builder.AppendLine($"Connector: {group.ConnectorStatusAsText}");
            builder.AppendLine($"Ship     : {group.ConnectedShipName}");
            builder.AppendLine($"Lights   : {group.Lights.Count}");
            builder.AppendLine($"Max Seq  : {group.MaxSequence}");
            builder.AppendLine($"Time     : {group.TotalTime}");
            builder.AppendLine($"Offset   : {group.OffsetPerLight}");
            return builder.ToString();
        }



        public DetailPanelTextBuilder(LandingPadGroup group)
        {
            _group = group;
            _lineLength = MyConstants.LCDPanelLineLength;
            _title = SetupTitleLine(group.GroupName, _lineLength);
        }

        private string SetupTitleLine(string groupName, int lineLength)
        {
            int startPos = (lineLength - groupName.Length) / 2;
            return $"Landing Pad {new string(' ', startPos)}{groupName}";
        }

        private LandingPadGroup _group;
        private int _lineLength;
        private string _title;
    }

    public class IndicatorPanelTextBuilder : IPanelTextBuilder
    {
        public string BuildText()
        {
            return BuildInformationForGroup(_group);
        }

        private string BuildInformationForGroup(LandingPadGroup group)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{_topLine}");
            builder.AppendLine($"{_title}");
            builder.AppendLine($"{_bottomLine}");
            builder.AppendLine();
            builder.AppendLine($"{SetupShipLine(group.ConnectedShipName, _lineLength)}");


            return builder.ToString();

        }

        public IndicatorPanelTextBuilder(LandingPadGroup group)
        {
            _group = group;
            _lineLength = MyConstants.LCDPanelLineLength;
            _title = SetupTitleLine(group.GroupName, _lineLength);
            _topLine = SetupTopLine(_lineLength);
            _bottomLine = SetupBottomLine(_lineLength);
            _currentShipName = string.Empty;
        }

        private string SetupTopLine(int lineLength)
        {
            return $"╔{new string('═', lineLength - 2)}╗";
        }

        private string SetupBottomLine(int lineLength)
        {
            return $"╚{new string('═', lineLength - 2)}╝";
        }

        private string SetupTitleLine(string groupName, int lineLength)
        {
            int startPos = (lineLength - groupName.Length) / 2;
            string templine = $"║{new string(' ', startPos)}{groupName}";
            templine += new string(' ', lineLength - templine.Length - 1) + "║";
            return templine;
        }

        private string SetupShipLine(string shipname, int lineLength)
        {
            if(shipname != _currentShipName)
            {
                _currentShipName = shipname;
                int startPos = (lineLength - shipname.Length) / 2;
                _shipLine = $"{new string(' ', startPos)}{shipname}";
            }

            return _shipLine;
        }



        private LandingPadGroup _group;
        private int _lineLength;
        private string _title;
        private string _topLine;
        private string _bottomLine;
        private string _currentShipName;
        private string _shipLine;

    }

}
