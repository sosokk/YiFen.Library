using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace YiFen.DataHelper
{
    public class EntityBuilder<T>
    {
        private delegate T Load(IDataReader Reader);
        private Load handler;

        private EntityBuilder() { }

        public T Build(IDataReader Reader)
        {
            return handler(Reader);
        }
        public static EntityBuilder<T> CreateBuilder(IDataRecord Reader, PropertyInfo[] AllowedProperties)
        {
            EntityBuilder<T> result = new EntityBuilder<T>();
            DynamicMethod method = new DynamicMethod("CreateBuilder", typeof(T), new Type[] { typeof(IDataReader) }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();
            generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc_0);
            List<KeyValue> Fileds = result.ArrayToKeyValue(AllowedProperties);

            for (int i = 0; i < Reader.FieldCount; i++)
            {
                KeyValue keyValue = Fileds.FirstOrDefault(x => x.Key == Reader.GetName(i));
                if (keyValue != null)
                {
                    PropertyInfo propertyInfo = keyValue.Value;
                    if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                    {
                        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            ILForNullable(Reader, i, propertyInfo, generator);
                        }
                        else if (propertyInfo.PropertyType.BaseType == typeof(Entity) || propertyInfo.PropertyType.BaseType.BaseType == typeof(Entity))
                        {
                            ILForEntity(Reader, i, propertyInfo, generator);
                        }
                        else
                        {
                            ILForDefault(Reader, i, propertyInfo, generator);
                        }
                    }
                }
            }

            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);
            result.handler = (Load)method.CreateDelegate(typeof(Load));
            return result;
        }

        static void ILForDefault(IDataRecord Reader, int i, PropertyInfo propertyInfo, ILGenerator generator)
        {
            Label label = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, Reader.GetType().GetMethod("IsDBNull"));
            generator.Emit(OpCodes.Brtrue, label);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, Reader.GetType().GetMethod("get_Item", new Type[] { typeof(int) }));
            generator.Emit(OpCodes.Unbox_Any, Reader.GetFieldType(i));
            generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
            generator.MarkLabel(label);

        }
        static void ILForNullable(IDataRecord Reader, int i, PropertyInfo propertyInfo, ILGenerator generator)
        {
            Type a1 = propertyInfo.PropertyType.GetGenericArguments()[0];

            ConstructorInfo ctor2 = typeof(Nullable<>).MakeGenericType(a1).GetConstructor(
                                                new Type[] { a1 });
            // Preparing locals
            LocalBuilder c = generator.DeclareLocal(typeof(object));
            // Preparing labels
            Label label24 = generator.DefineLabel();
            Label label = generator.DefineLabel();
            // Writing body
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, Reader.GetType().GetMethod("IsDBNull"));
            generator.Emit(OpCodes.Brtrue, label);

            generator.Emit(OpCodes.Ldloc, 0);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, Reader.GetType().GetMethod("get_Item", new Type[] { typeof(int) }));
            generator.Emit(OpCodes.Unbox_Any, Reader.GetFieldType(i));
            generator.Emit(OpCodes.Newobj, ctor2);
            generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
            generator.Emit(OpCodes.Nop);
            generator.MarkLabel(label);
        }
        static void ILForEntity(IDataRecord Reader, int ReaderIndex, PropertyInfo Property, ILGenerator ILGen)
        {
            Type a1 = Property.PropertyType;
            LocalBuilder c = ILGen.DeclareLocal(a1);
            ConstructorInfo ctor2 = a1.GetConstructor(Type.EmptyTypes);
            Label label = ILGen.DefineLabel();
            // Writing body
            ILGen.Emit(OpCodes.Ldarg_0);
            ILGen.Emit(OpCodes.Ldc_I4, ReaderIndex);
            ILGen.Emit(OpCodes.Callvirt, Reader.GetType().GetMethod("IsDBNull"));
            ILGen.Emit(OpCodes.Brtrue, label);

            // ILGen.Emit(OpCodes.Stloc_0);
            ILGen.Emit(OpCodes.Ldloc, 0);
            //ILGen.Emit(OpCodes.Ldarg_0);
            ILGen.Emit(OpCodes.Newobj, ctor2);
            ILGen.Emit(OpCodes.Callvirt, Property.GetSetMethod());
            ILGen.Emit(OpCodes.Ldloc_0);
            ILGen.Emit(OpCodes.Callvirt, Property.GetGetMethod());
            ILGen.Emit(OpCodes.Ldarg_0);
            ILGen.Emit(OpCodes.Ldc_I4, ReaderIndex);
            ILGen.Emit(OpCodes.Callvirt, Reader.GetType().GetMethod("get_Item", new Type[] { typeof(int) }));
            ILGen.Emit(OpCodes.Unbox_Any, Reader.GetFieldType(ReaderIndex));
            ILGen.Emit(OpCodes.Callvirt, a1.GetProperty("ID").GetSetMethod());
            ILGen.Emit(OpCodes.Nop);
            ILGen.MarkLabel(label);
        }

        List<KeyValue> ArrayToKeyValue(PropertyInfo[] Properties)
        {
            List<KeyValue> Fileds = new List<KeyValue>();
            foreach (PropertyInfo Property in Properties)
            {
                KeyValue Filed = new KeyValue();
                NameInDatabaseAttribute NameInDatabase = Attribute.GetCustomAttribute(Property, typeof(NameInDatabaseAttribute), true) as NameInDatabaseAttribute;
                if (NameInDatabase == null)
                {
                    Filed.Key = Property.Name;
                }
                else
                {
                    Filed.Key = NameInDatabase.Name;
                }
                Filed.Value = Property;
                Fileds.Add(Filed);
            }
            return Fileds;
        }

        class KeyValue
        {
            public string Key { get; set; }
            public PropertyInfo Value { get; set; }
        }
    }
}
