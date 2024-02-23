namespace Animatic
{
    [System.Serializable]
    public class AnimaticMotion
    {
        public string Name;
        public string GUID;
        public bool Loop;

        public virtual float GetLength() {  return 0; }
    }
}
