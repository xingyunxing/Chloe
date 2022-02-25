using Chloe.Data;
using Chloe.Mapper;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace Chloe.Reflection.Emit
{
    public static class ClassGenerator
    {
        static readonly Dictionary<Assembly, ModuleBuilder> _moduleBuilders = new Dictionary<Assembly, ModuleBuilder>();
        static int _sequenceNumber = 0;

        public static Type CreateMRMType(MemberInfo propertyOrField)
        {
            Type entityType = propertyOrField.DeclaringType;

            Assembly assembly = entityType.GetAssembly();

            ModuleBuilder moduleBuilder;
            if (!_moduleBuilders.TryGetValue(assembly, out moduleBuilder))
            {
                lock (assembly)
                {
                    if (!_moduleBuilders.TryGetValue(assembly, out moduleBuilder))
                    {
                        var assemblyName = new AssemblyName(String.Format(CultureInfo.InvariantCulture, "ChloeMRMs-{0}", assembly.FullName));
                        assemblyName.Version = new Version(1, 0, 0, 0);

                        AssemblyBuilder assemblyBuilder;
#if netcore
                        assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#elif netfx
                        assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
                        moduleBuilder = assemblyBuilder.DefineDynamicModule("ChloeMRMModule");

                        _moduleBuilders.Add(assembly, moduleBuilder);
                    }
                }
            }

            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            TypeBuilder tb = moduleBuilder.DefineType(string.Format("Chloe.Mapper.MRMs.{0}_{1}_{2}", entityType.Name, propertyOrField.Name, Guid.NewGuid().ToString("N").Substring(0, 5) + System.Threading.Interlocked.Increment(ref _sequenceNumber).ToString()), typeAttributes, null, new Type[] { typeof(IMRM) });

            tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName);

            MethodBuilder methodBuilder = tb.DefineMethod("Map", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void), new Type[] { typeof(object), typeof(System.Data.IDataReader), typeof(int) });

            ILGenerator il = methodBuilder.GetILGenerator();

            int parameStartIndex = 1;

            il.Emit(OpCodes.Ldarg_S, parameStartIndex);//将第一个参数 object 对象加载到栈顶
            il.Emit(OpCodes.Castclass, propertyOrField.DeclaringType);//将 object 对象转换为强类型对象 此时栈顶为强类型的对象

            var readerMethod = DataReaderConstant.GetReaderMethod(ReflectionExtension.GetMemberType(propertyOrField));

            //ordinal
            il.Emit(OpCodes.Ldarg_S, parameStartIndex + 1);    //加载参数DataReader
            il.Emit(OpCodes.Ldarg_S, parameStartIndex + 2);    //加载 read ordinal
            il.EmitCall(OpCodes.Call, readerMethod, null);     //调用对应的 readerMethod 得到 value  reader.Getxx(ordinal);  此时栈顶为 value

            EmitHelper.SetValueIL(il, propertyOrField); // object.XX = value; 此时栈顶为空

            il.Emit(OpCodes.Ret);   // 即可 return

            Type t = tb.CreateType();

            return t;
        }

        public static Type CreateDynamicType(List<Type> properties)
        {
            Assembly assembly = typeof(ClassGenerator).GetAssembly();

            ModuleBuilder moduleBuilder;
            if (!_moduleBuilders.TryGetValue(assembly, out moduleBuilder))
            {
                lock (assembly)
                {
                    if (!_moduleBuilders.TryGetValue(assembly, out moduleBuilder))
                    {
                        var assemblyName = new AssemblyName(String.Format(CultureInfo.InvariantCulture, "ChloeMRMs-{0}", assembly.FullName));
                        assemblyName.Version = new Version(1, 0, 0, 0);

                        AssemblyBuilder assemblyBuilder;
                        assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#if netcore
                        assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#elif netfx
                        assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
                        moduleBuilder = assemblyBuilder.DefineDynamicModule("ChloeMRMModule");

                        _moduleBuilders.Add(assembly, moduleBuilder);
                    }
                }
            }

            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
            TypeBuilder tb = moduleBuilder.DefineType($"Chloe.Sharding.PlainType_{System.Threading.Interlocked.Increment(ref _sequenceNumber).ToString()}", typeAttributes, null, new Type[] { });

            tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName);

            for (int i = 0; i < properties.Count; i++)
            {
                var propertyType = properties[i];

                string propertyName = $"P_{i}";
                DefineProperty(tb, propertyType, propertyName);
            }

            Type t = tb.CreateType();

            return t;
        }

        static void DefineProperty(TypeBuilder typeBuilder, Type propertyType, string name)
        {
            string propertyName = name;
            var custNamePropBldr = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            FieldBuilder customerNameBldr = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            MethodBuilder custNameGetPropMthdBldr = typeBuilder.DefineMethod($"get_{propertyName}", getSetAttr, propertyType, Type.EmptyTypes);

            ILGenerator custNameGetIL = custNameGetPropMthdBldr.GetILGenerator();

            custNameGetIL.Emit(OpCodes.Ldarg_0);
            custNameGetIL.Emit(OpCodes.Ldfld, customerNameBldr);
            custNameGetIL.Emit(OpCodes.Ret);

            MethodBuilder custNameSetPropMthdBldr = typeBuilder.DefineMethod($"set_{propertyName}", getSetAttr, null, new Type[] { propertyType });

            ILGenerator custNameSetIL = custNameSetPropMthdBldr.GetILGenerator();

            custNameSetIL.Emit(OpCodes.Ldarg_0);
            custNameSetIL.Emit(OpCodes.Ldarg_1);
            custNameSetIL.Emit(OpCodes.Stfld, customerNameBldr);
            custNameSetIL.Emit(OpCodes.Ret);

            custNamePropBldr.SetGetMethod(custNameGetPropMthdBldr);
            custNamePropBldr.SetSetMethod(custNameSetPropMthdBldr);
        }
    }
}
