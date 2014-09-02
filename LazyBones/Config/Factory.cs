using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LazyBones.Extensions;

namespace LazyBones.Config
{
    public class Factory<TItemBase, TItemAttribute> : IFactory, IItemFactory<TItemBase>, IDisposable
        where TItemBase : class
        where TItemAttribute : ConfigItemAttribute
    {
        private struct FactoryItem<T>
        {
            internal Type Type;
            internal ConstructorInfo ctor;
            internal object obj;
        }
        readonly Dictionary<string, FactoryItem<TItemBase>> itemCreators
            = new Dictionary<string, FactoryItem<TItemBase>>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 扫描程序集中类型TItemBase的子类（包括自身）
        /// </summary>
        /// <param name="assembly">需要扫描的程序集</param>
        /// <param name="prefix">程序集前缀</param>
        public void ScanAssembly(Assembly assembly, string prefix)
        {
            foreach (var type in assembly.GetTypeSafe().Where(t => typeof(TItemBase).IsAssignableFrom(t)))
            {
                RegisterType(type, prefix);
            }
        }

        public void RegisterType(Type type, string prefix)
        {
            if (type == null)
                return;
            var attr = type.GetSingleAttribute<TItemAttribute>();
            if (attr == null)
                return;

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new ConfigException("类型<{0}>缺少参数为空的构造函数，注册失败！", type);

            RegisterCreator(ctor, prefix + attr.Name, type);
        }
        void RegisterCreator(ConstructorInfo ctor, string name, Type type)
        {
            if (type.IsDefined(typeof(StaticAttribute), false))  //对于标识了SingletonAttribute特性的类，只生成一个单独的实例
            {
                var item = (TItemBase)ctor.Invoke(null);
                itemCreators.Add(name, new FactoryItem<TItemBase>
                {
                    Type = type,
                    obj = item
                });
            }
            else
            {
                itemCreators.Add(name, new FactoryItem<TItemBase>
                {
                    Type = type,
                    ctor = ctor
                });
            }
        }
        public void RegisterTypeByName(string typeName)
        {
            RegisterType(Type.GetType(typeName, false), string.Empty);
        }

        public void Clear()
        {
            itemCreators.Clear();
        }

        public TItemBase GetInstance(string itemName)
        {
            FactoryItem<TItemBase> item;
            if (itemCreators.TryGetValue(itemName, out item))
            {
                return (TItemBase)(item.obj ?? item.ctor.Invoke(null));
            }
            throw new ConfigException("不存在类型{0}", itemName);
        }

        public Type GetType(string itemName)
        {
            if (itemCreators.ContainsKey(itemName))
            {
                return itemCreators[itemName].Type;
            }
            throw new ConfigException("不存在类型{0}", itemName);
        }

        public Type[] RegisteredTypes { get { return itemCreators.Values.Select(v => v.Type).ToArray(); } }

        public void Dispose()
        {
            itemCreators.Clear();
        }
    }
}
