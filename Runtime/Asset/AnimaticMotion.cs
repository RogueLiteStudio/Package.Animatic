namespace Animatic
{
    [System.Serializable]
    public class AnimaticMotion
    {
        public string GUID;
        public string Name;
        public bool Loop;

        public virtual float GetLength() {  return 0; }
    }
}
