﻿/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Lakerfield.Bson.Serialization
{
    /// <summary>
    /// Default, global implementation of an <see cref="IBsonSerializerRegistry"/>.
    /// </summary>
    public sealed class BsonSerializerRegistry : IBsonSerializerRegistry
    {
        // private fields
        private readonly ConcurrentDictionary<Type, IBsonSerializer> _cache;
        private readonly ConcurrentStack<IBsonSerializationProvider> _serializationProviders;
        private readonly Func<Type, IBsonSerializer> _createSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonSerializerRegistry"/> class.
        /// </summary>
        public BsonSerializerRegistry()
        {
            _cache = new ConcurrentDictionary<Type, IBsonSerializer>();
            _serializationProviders = new ConcurrentStack<IBsonSerializationProvider>();
            _createSerializer = CreateSerializer;
        }

        // public methods
        /// <summary>
        /// Gets the serializer for the specified <paramref name="type" />.
        /// If none is already registered, the serialization providers will be used to create a serializer and it will be automatically registered.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The serializer.
        /// </returns>
        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters)
            {
                var message = string.Format("Generic type {0} has unassigned type parameters.", BsonUtils.GetFriendlyTypeName(type));
                throw new ArgumentException(message, "type");
            }

            return _cache.GetOrAdd(type, _createSerializer);
        }

        /// <summary>
        /// Gets the serializer for the specified <typeparamref name="T" />.
        /// If none is already registered, the serialization providers will be used to create a serializer and it will be automatically registered.
        /// </summary>
        /// <typeparam name="T">The value type of the serializer.</typeparam>
        /// <returns>
        /// The serializer.
        /// </returns>
        public IBsonSerializer<T> GetSerializer<T>()
        {
            return (IBsonSerializer<T>)GetSerializer(typeof(T));
        }

        /// <summary>
        /// Registers the serializer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializer">The serializer.</param>
        public void RegisterSerializer(Type type, IBsonSerializer serializer)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            EnsureRegisteringASerializerForThisTypeIsAllowed(type);

            if (!_cache.TryAdd(type, serializer))
            {
                var message = string.Format("There is already a serializer registered for type {0}.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Registers the serialization provider. This behaves like a stack, so the
        /// last provider registered is the first provider consulted.
        /// </summary>
        /// <param name="serializationProvider">The serialization provider.</param>
        public void RegisterSerializationProvider(IBsonSerializationProvider serializationProvider)
        {
            if (serializationProvider == null)
            {
                throw new ArgumentNullException("serializationProvider");
            }

            _serializationProviders.Push(serializationProvider);
        }

        /// <summary>
        /// Tries to register the serializer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>True if the serializer was registered on this call, false if the same serializer was already registered on a previous call, throws an exception if a different serializer was already registered.</returns>
        public bool TryRegisterSerializer(Type type, IBsonSerializer serializer)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            EnsureRegisteringASerializerForThisTypeIsAllowed(type);

            if (_cache.TryAdd(type, serializer))
            {
                return true;
            }
            else
            {
                var existingSerializer = _cache[type];
                if (!existingSerializer.Equals(serializer))
                {
                    var message = $"There is already a different serializer registered for type {BsonUtils.GetFriendlyTypeName(type)}.";
                    throw new BsonSerializationException(message);
                }
                return false;
            }
        }

        // private methods
        private IBsonSerializer CreateSerializer(Type type)
        {
            foreach (var serializationProvider in _serializationProviders)
            {
                IBsonSerializer serializer;

                var registryAwareSerializationProvider = serializationProvider as IRegistryAwareBsonSerializationProvider;
                if (registryAwareSerializationProvider != null)
                {
                    serializer = registryAwareSerializationProvider.GetSerializer(type, this);
                }
                else
                {
                    serializer = serializationProvider.GetSerializer(type);
                }

                if (serializer != null)
                {
                    return serializer;
                }
            }

            var message = string.Format("No serializer found for type {0}.", type.FullName);
            throw new BsonSerializationException(message);
        }

        private void EnsureRegisteringASerializerForThisTypeIsAllowed(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeof(BsonValue).GetTypeInfo().IsAssignableFrom(type))
            {
                var message = string.Format("A serializer cannot be registered for type {0} because it is a subclass of BsonValue.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }
            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters)
            {
                var message = string.Format("Generic type {0} has unassigned type parameters.", BsonUtils.GetFriendlyTypeName(type));
                throw new ArgumentException(message, "type");
            }
        }
    }
}
