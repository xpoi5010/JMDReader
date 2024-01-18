using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.IO
{
    public static class RaycityObjectManager
    {
        private static Dictionary<uint, RaycityObjectInfo> registeredClasses = new Dictionary<uint, RaycityObjectInfo>();
        
        public static void Initization()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach(TypeInfo typeInfo in assembly.DefinedTypes)
            {
                Type? curType = typeInfo.GetType();
                while(curType != null)
                {
                    if(curType == typeof(RaycityObject))
                    {
                        RegisterClass(typeInfo.GetType());
                        break;
                    }
                    curType = curType.BaseType;
                }
            }
        }

        public static void RegisterClass<TRegisterClass>() where TRegisterClass : RaycityObject, new()
        {
            Type type = typeof(TRegisterClass);
            RegisterClass(type);
        }

        public static void RegisterClass(Type type)
        {
            Type? baseType = type.BaseType;
            while(baseType != null && baseType != typeof(RaycityObject))
                baseType = baseType.BaseType;
            if (baseType is null)
                throw new Exception("");
            ConstructorInfo? constructorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, new Type[0]);
            if (constructorInfo == null)
                throw new Exception("");
            RaycityObjectInfo raycityObjectInfo = new(type, constructorInfo);
            RaycityObject newObj = raycityObjectInfo.CreateObject();
            uint classStamp = newObj.ClassStamp;
            registeredClasses.Add(classStamp, raycityObjectInfo);
        }

        public static void RegisterAssemblyClasses(Assembly assembly)
        {
            assembly.GetTypes();
            IEnumerable<TypeInfo> foundTypes = assembly.DefinedTypes.Where(x => x.GetCustomAttributes<RaycityObjectImplementAttribute>().Count() > 0);
            foreach (TypeInfo typeInfo in foundTypes)
                RegisterClass(typeInfo.UnderlyingSystemType);
        }

        public static bool ContainsClass(uint classStamp)
        {
            return registeredClasses.ContainsKey(classStamp);
        }

        public static T CreateObject<T>(uint ClassStamp) where T : RaycityObject, new()
        {
            if (!registeredClasses.ContainsKey(ClassStamp))
                throw new Exception();
            RaycityObjectInfo raycityObjectInfo = registeredClasses[ClassStamp];
            if (!raycityObjectInfo.CanbeConvertTo(typeof(T)))
                throw new InvalidCastException("");
            return (T)raycityObjectInfo.CreateObject();
        }

        public static RaycityObject CreateObject(uint ClassStamp)
        {
            if (!registeredClasses.ContainsKey(ClassStamp))
                throw new Exception();
            RaycityObjectInfo raycityObjectInfo = registeredClasses[ClassStamp];
            return raycityObjectInfo.CreateObject();
        }
    }

    internal record class RaycityObjectInfo(Type BaseType, ConstructorInfo ConstructorInfo)
    {
        public RaycityObject CreateObject()
        {
            return (RaycityObject)ConstructorInfo.Invoke(new object[0]);
        }

        public bool CanbeConvertTo(Type targetType)
        {
            Type? superType = targetType;
            while(superType != null)
            {
                if(superType == targetType)
                    return true;
                superType = superType.BaseType;
            }
            return false;
        }
    }
}
