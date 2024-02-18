namespace Animatic
{
    [System.Serializable]
    public class SimulateMotion
    {
        public string Name;
        public AnimaticClip Clip;

        public bool Update(AnimaticAsset asset, string name)
        {
            if (Name != name)
                return false;
            
            return true;
        }
    }
}
