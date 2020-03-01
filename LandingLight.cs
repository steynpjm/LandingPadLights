using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class LandingLight
    {
        public IMyLightingBlock Light { get; private set; }
        public int Sequence { get; private set; }

        public LandingLight(IMyLightingBlock light, int sequence)
        {
            Light = light;
            Sequence = sequence;
        }
    }
}
