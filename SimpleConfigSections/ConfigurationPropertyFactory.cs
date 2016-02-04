﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Reflection;
using Castle.Core.Internal;

using System.Linq;

namespace SimpleConfigSections
{
    internal abstract partial class ConfigurationPropertyFactory
	{
        public ConfigurationPropertyFactory()
        {
		}

        public ConfigurationProperty Interface(PropertyInfo pi)
        {
            return NewConfigurationProperty(pi, 
                typeof(ConfigurationElementForInterface<>).MakeGenericType(pi.PropertyType));
          
        }

        public ConfigurationProperty Simple(PropertyInfo pi)
        {
            return NewConfigurationProperty(pi, pi.PropertyType);
        }

        public ConfigurationProperty Class(PropertyInfo pi)
        {
            return Interface(pi);
        }

#if false
		public ConfigurationProperty Section(Type type)
		{
			if (!typeof(ConfigurationSection).IsAssignableFrom(type))
				throw new ArgumentOutOfRangeException("type is not a ConfigurationSection");

			var name = NamingConvention.Current.SectionNameByClassType(type);
			var options = GetOptions(type);
			var validator = GetValidator(type);
			Console.Error.WriteLine("  +++++>>>> Validator: {0} -- {1}",  type, validator);
			var configurationProperty = new ConfigurationProperty(name, type, null, null, validator, options);
			return configurationProperty;
		}
#endif

        protected ConfigurationProperty NewConfigurationProperty(PropertyInfo pi, Type elementType)
        {
			var name = NamingConvention.Current.AttributeName(pi);
            var options = GetOptions(pi);
            var defaultValue = GetDefaultValue(pi);
            var validator = GetValidator(pi);            
            var configurationProperty = new ConfigurationProperty(name, elementType, defaultValue, TypeDescriptor.GetConverter(elementType), validator, options);            
            return configurationProperty;
        }

        private static ConfigurationPropertyOptions GetOptions(MemberInfo mi)
        {
            var options = ConfigurationPropertyOptions.None;
            var requiredAttribute = mi.GetAttribute<RequiredAttribute>();
            if (requiredAttribute != null)
            {
                options = ConfigurationPropertyOptions.IsRequired;
            }
			return options;
        }

        private static object GetDefaultValue(PropertyInfo member)
        {
            object defaultValue = null;
            var defaultAttribute = member.GetAttribute<DefaultAttribute>();
            if (defaultAttribute != null)
            {
                defaultValue = defaultAttribute.Default();
            }
            
            if (defaultValue == null)
            {
                var attribute = member.GetAttribute<DefaultValueAttribute>();
                if (attribute != null)
                {
                    defaultValue = attribute.Value;
                }
            }
            return defaultValue;
        }

        private static ConfigurationValidatorBase GetValidator(MemberInfo mi)
        {
            ConfigurationValidatorBase validator = new DefaultValidator();
            var validators = mi.GetAttributes<ValidationAttribute>();
            if (validators.IsNullOrEmpty() == false)
            {
                validator = new CompositeConfigurationValidator(validators, mi.Name);
            }
            return validator;
        }

		public static IConfigurationPropertyFactory Create()
		{
			return ReflectionHelpers.RunningOnMono ? new MonoFactory() as IConfigurationPropertyFactory : new DotNetFactory();
		}
    }
}
