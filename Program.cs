using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            _data = new MyIni();

            _groupList = new List<LandingPadGroup>();
            _mainPanels = new List<DisplayPanel>();

            Setup();

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }



        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
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

        //private void ShowDataOnScreen(List<IMyTextPanel> textPanels, List<LandingPadGroup> groups)
        //{
        //    StringBuilder builder = new StringBuilder();

        //    builder.AppendLine("Landing Lights");

        //    builder.AppendLine("--------------");
        //    builder.AppendLine($"Groups={groups.Count}");

        //    foreach (LandingPadGroup group in groups)
        //    {
        //        builder.AppendLine($"Group={group.GroupName}");
        //    }

        //    foreach (IMyTextPanel textPanel in textPanels)
        //    {
        //        textPanel.WriteText(builder.ToString(), false);
        //    }
        //}
        private void Setup()
        {
            _groupList.Clear();

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            // Find all blocks with a "[LandingLights]" section in custom data.
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
