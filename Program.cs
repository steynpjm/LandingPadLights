using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        public Program()
        {
            _data = new MyIni();

            _groupList = new List<LandingPadGroup>();
            _mainPanels = new List<DisplayPanel>();

            Setup();

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _counter++;

            Echo(updateSource.ToString());
            Echo(_counter.ToString());

            // Check if run from Terminal.
            if ((updateSource & UpdateType.Terminal) != 0)
            {
                Setup();
            }

            if ((updateSource & UpdateType.Update100) != 0)
            {
                foreach (LandingPadGroup group in _groupList)
                {
                    group.CheckBlocks();
                }
            }

            // Display information on main LCD panel.
            if (_mainPanels.Any())
            {
                //ShowDataOnScreen(_mainPanels, _groupList);
                foreach (DisplayPanel panel in _mainPanels)
                {
                    panel.UpdatePanel();
                }
            }
        }

        private void Setup()
        {
            _groupList.Clear();

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            // Find all blocks with a "[LandingPad]" section in custom data.
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, block => MyIni.HasSection(block.CustomData, MyConstants.SectionName) && block.IsSameConstructAs(Me));

            // Step through each block and determine groups
            foreach (IMyTerminalBlock block in blocks)
            {
                // parse the custom data for the block
                MyIniParseResult parsedIniResult;
                if (_data.TryParse(block.CustomData, out parsedIniResult))
                {
                    MyConfiguration blockConfig = new MyConfiguration(_data);

                    // Get the Group value.
                    string groupName = blockConfig.GroupName;

                    // If there is no group specified and if this is a text panel, use that as a main panel,
                    // otherwise continue with next block.
                    if (blockConfig.GroupName == MyConstants.DefaultGroupName)
                    {
                        IMyTextPanel lcdPanel = block as IMyTextPanel;
                        if (lcdPanel != null)
                        {
                            DisplayPanel panel = new DisplayPanel(_groupList, lcdPanel);
                            _mainPanels.Add(panel);
                        }

                        continue;
                    };

                    // If group does not exist, add a new group with this name.
                    if (!_groupList.Exists(x => x.GroupName == groupName))
                    {
                        LandingPadGroup landingLightGroup = new LandingPadGroup(groupName);
                        _groupList.Add(landingLightGroup);
                    }

                    // Get the group for this block it should be added to.
                    LandingPadGroup group = _groupList.Find(x => x.GroupName == groupName);

                    AddBlockToGroup(group, block, blockConfig);
                }
            }


            // All block should now be assigned.
            // Configure the individual blocks for each group.
            foreach (LandingPadGroup group in _groupList)
            {
                group.ConfigureBlocks();
            }

        }

        private void AddBlockToGroup(LandingPadGroup group, IMyTerminalBlock block, MyConfiguration blockConfig)
        {
            // Is this an LCD Panel?
            IMyTextPanel lcdPanel = block as IMyTextPanel;
            if (lcdPanel != null)
            {
                PanelType panelType = PanelType.Main;
                switch (blockConfig.PanelType)
                {
                    case MyConstants.PanelTypeDetail:
                        panelType = PanelType.Detail;
                        break;
                    case MyConstants.PanelTypeIndicator:
                        panelType = PanelType.Indicator;
                        break;
                    default:
                        break;
                }

                DisplayPanel panel = new DisplayPanel(group, lcdPanel, panelType);
                group.Panels.Add(panel);
            }

            // Is this a lighting block?
            IMyLightingBlock light = block as IMyLightingBlock;
            if (light != null)
            {
                if (blockConfig.Sequence > 0)
                {
                    LandingLight landingLight = new LandingLight(light, blockConfig.Sequence);
                    group.Lights.Add(landingLight);
                }
            }

            // Is this a connector block?
            IMyShipConnector shipConnector = block as IMyShipConnector;
            if (shipConnector != null && group.Connector == null)
            {
                group.Connector = shipConnector;
            }
        }

        private int _counter = 0;
        private MyIni _data;
        private List<LandingPadGroup> _groupList;
        private List<DisplayPanel> _mainPanels;
    }
}
