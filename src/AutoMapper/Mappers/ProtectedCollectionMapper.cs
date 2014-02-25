using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    /// <summary>
    /// Maps to a collection that is not directly accessible and must be mutated via Add/Remove methods on the declaring type
    /// IEnumerable{T} Things -> Method: AddThing(T item)
    /// Property: IEnumerable{T} Children -> Method: AddChildren(T item)
    /// </summary>
    public class ProtectedCollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            Type genericType = typeof(EnumerableMapper<>);

            var elementType = TypeHelper.GetElementType(context.DestinationType);

            var enumerableMapper = genericType.MakeGenericType(elementType);

            var objectMapper = (IObjectMapper)Activator.CreateInstance(enumerableMapper);

            var nullDestinationValueSoTheReadOnlyCollectionMapperWorks = context.CreateMemberContext(context.TypeMap, context.SourceValue, null, context.SourceType, context.PropertyMap);

            return objectMapper.Map(nullDestinationValueSoTheReadOnlyCollectionMapperWorks, mapper);
        }

        public bool IsMatch(ResolutionContext context)
        {
            if(!context.SourceType.IsEnumerableType() || !context.DestinationType.IsEnumerableType())
                return false;

            MethodInfo addMethod = GetAddMethod(context);
            return addMethod != null;
        }

        private static MethodInfo GetAddMethod(ResolutionContext context)
        {
            if(context.PropertyMap == null || context.PropertyMap.DestinationProperty == null || context.PropertyMap.DestinationProperty.MemberInfo.DeclaringType == null)
                return null;

            // todo: cache this based on the destination type
            string addMethodName = GetAddMethodName(context);
            Type declaringType = context.PropertyMap.DestinationProperty.MemberInfo.DeclaringType;
            return declaringType.GetMethod(addMethodName);
        }

        private static string GetAddMethodName(ResolutionContext context)
        {
            // todo: use naming strategy in mapper configuration
            string memberName = context.MemberName;
            string suffix = memberName.EndsWith("s") ? memberName.Substring(0, memberName.Length - 1) : memberName;

            return string.Format("Add{0}", suffix);
        }

        #region NestedType: EnumerableMapper
        private class EnumerableMapper<TElement> : EnumerableMapperBase<IList<TElement>>
        {
            private Action<TElement> _addElement;
 
            public override bool IsMatch(ResolutionContext context)
            {
                throw new NotImplementedException();
            }

            protected override void SetElementValue(IList<TElement> elements, object mappedValue, int index)
            {
                _addElement((TElement)mappedValue);
            }

            protected override IList<TElement> GetEnumerableFor(object destination)
            {
                return null;
            }

            protected override IList<TElement> CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                throw new NotImplementedException();
            }

            protected override object CreateDestinationObject(ResolutionContext context, Type destinationElementType, int count, IMappingEngineRunner mapper)
            {
                throw new NotImplementedException();
            }

            protected override object GetOrCreateDestinationObject(ResolutionContext context, IMappingEngineRunner mapper, Type destElementType, int sourceLength)
            {
                // todo: use a compiled expression for improved performance
                MethodInfo addMethod = GetAddMethod(context);
                object parent = context.Parent.Parent.DestinationValue;
                _addElement = element => addMethod.Invoke(parent, new object[] {element});

                return null;
            }
        }
        #endregion
    }
}