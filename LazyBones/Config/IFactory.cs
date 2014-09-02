namespace LazyBones.Config
{
    interface IFactory
    {
        void ScanAssembly(System.Reflection.Assembly assembly, string prefix);
        void RegisterType(System.Type type, string prefix);
        void RegisterTypeByName(string typeName);
        void Clear();
    }
}
