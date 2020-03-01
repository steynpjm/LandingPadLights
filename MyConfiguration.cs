using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    // Parses the "LandingLights" configuration.
    // Keys and values:
    //  Group: Name of Group (if Name if missing, it is taken as the main group).
    //  PanelType: Only used for LCD Panels.
    //              Detail: Full Detail for group.
    //              Indicator: Only indicator detail for group.
    //  Sequence: Only used for the lights. Indicates the sequence of this light.
    //              Sequence starts at 1 ( The connector is "0").
    //              Set Sequence from the inside going out like in a concentric circle.
    public class MyConfiguration
    {
        public string GroupName { get; private set; }
        public string PanelType { get; private set; }
        public int Sequence { get; private set; }

        private MyIni _data;

        public MyConfiguration(MyIni data)
        {
            this._data = data;

            Parse();
        }

        private void Parse()
        {
            GroupName = _data.Get(MyConstants.SectionName, MyConstants.GroupNameKey).ToString(MyConstants.DefaultGroupName).ToUpper();
            PanelType = _data.Get(MyConstants.SectionName, MyConstants.PanelTypeKey).ToString(MyConstants.DefaultPanelType).ToUpper();
            Sequence = _data.Get(MyConstants.SectionName, MyConstants.SequenceNumberKey).ToInt32();
        }

    }
}