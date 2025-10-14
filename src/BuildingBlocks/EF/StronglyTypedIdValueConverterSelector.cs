using System.Reflection;
using BuildingBlocks.Types;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.EF;

public class StronglyTypedIdValueConverterSelector<T>(ValueConverterSelectorDependencies dependencies)
    : ValueConverterSelector(dependencies)
{
    public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type providerClrType = null)
    {
        var baseConverters = base.Select(modelClrType, providerClrType);
        foreach (var converter in baseConverters)
        {
            yield return converter;
        }

        var underlyingModelType = UnwrapNullableType(modelClrType);
        var underlyingProviderType = UnwrapNullableType(providerClrType);

        if (underlyingProviderType is null || underlyingProviderType == typeof(T))
        {
            if (IsStronglyTypedId(underlyingModelType, out var valueType) && valueType == typeof(T))
            {
                var converterType = typeof(StronglyTypedIdConverter<,>).MakeGenericType(underlyingModelType, typeof(T));

                yield return new ValueConverterInfo(
                    modelClrType,
                    typeof(T),
                    info => (ValueConverter)Activator.CreateInstance(converterType, info.MappingHints)
                );
            }
        }
    }

    private static bool IsStronglyTypedId(Type type, out Type valueType)
    {
        if (type is null)
        {
            valueType = null;
            return false;
        }

        // Check for the marker interface
        var idInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>));

        if (idInterface != null)
        {
            valueType = idInterface.GetGenericArguments()[0];
            return true;
        }

        valueType = null;
        return false;
    }

    private static Type UnwrapNullableType(Type type) =>
        type is null ? null : (Nullable.GetUnderlyingType(type) ?? type);
}

public class StronglyTypedIdConverter<TStronglyTypedId, TValue>(ConverterMappingHints mappingHints = null)
    : ValueConverter<TStronglyTypedId, TValue>(
        id => ((IStronglyTypedId<TValue>)id).Value,
        value => CreateFromValue(value),
        mappingHints
    )
    where TStronglyTypedId : notnull
{
    private static readonly Func<TValue, TStronglyTypedId> CreateFromValue = CreateConverter();

    private static Func<TValue, TStronglyTypedId> CreateConverter()
    {
        var type = typeof(TStronglyTypedId);

        // Handle records with the primary constructor
        var ctor = type.GetConstructor(new[] { typeof(TValue) });
        if (ctor != null)
        {
            return value => (TStronglyTypedId)ctor.Invoke(new object[] { value! });
        }

        // Handle records/mutable types with parameterless constructor (private or public) and settable Value property
        var parameterlessCtor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null
        );

        if (parameterlessCtor != null)
        {
            return value =>
            {
                var instance = (TStronglyTypedId)parameterlessCtor.Invoke(null);

                // Use the interface to set the value if possible
                if (instance is IStronglyTypedId<TValue> stronglyTypedId)
                {
                    // This assumes the Value property is settable through reflection
                    // even if it's not publicly settable
                    var valueProperty = type.GetProperty("Value");
                    valueProperty?.SetValue(instance, value);
                }

                return instance;
            };
        }

        throw new InvalidOperationException(
            $"Type {type} must either have a constructor that takes a {typeof(TValue)} "
                + $"or a parameterless constructor with a settable Value property"
        );
    }
}
