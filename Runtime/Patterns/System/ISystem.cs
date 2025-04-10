namespace App.Prototyping.MissileCommand
{
    public interface ISystem<in TComponent>
    {
        public float TickInterval { get; }
        public float ComponentCount { get; }
        void Register(TComponent component);
        void UnRegister(TComponent component);
    }
}