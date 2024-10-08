/* Copyright 2010-present MongoDB Inc.
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
using System.Dynamic;
using System.Linq.Expressions;
using Lakerfield.Bson.IO;

namespace Lakerfield.Bson.Serialization.Serializers
{
    /// <summary>
    /// Base serializer for dynamic types.
    /// </summary>
    /// <typeparam name="T">The dynamic type.</typeparam>
    public abstract class DynamicDocumentBaseSerializer<T> : SerializerBase<T> where T : class, IDynamicMetaObjectProvider
    {
        // private static fields
        private static readonly IBsonSerializer<object> __objectSerializer = BsonSerializer.LookupSerializer<object>();

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDocumentBaseSerializer{T}"/> class.
        /// </summary>
        protected DynamicDocumentBaseSerializer()
        { }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            string message;
            switch (bsonType)
            {
                case BsonType.Document:
                    var dynamicContext = context.With(ConfigureDeserializationContext);
                    bsonReader.ReadStartDocument();
                    var document = CreateDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var name = bsonReader.ReadName();
                        var value = __objectSerializer.Deserialize(dynamicContext);
                        SetValueForMember(document, name, value);
                    }
                    bsonReader.ReadEndDocument();
                    return document;

                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                default:
                    message = string.Format("Cannot deserialize a '{0}' from BsonType '{1}'.", BsonUtils.GetFriendlyTypeName(typeof(T)), bsonType);
                    throw new FormatException(message);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
                return;
            }

            var metaObject = value.GetMetaObject(Expression.Constant(value));
            var memberNames = metaObject.GetDynamicMemberNames();
            var dynamicContext = context.With(ConfigureSerializationContext);

            bsonWriter.WriteStartDocument();
            foreach (var memberName in memberNames)
            {
                object memberValue;
                if (TryGetValueForMember(value, memberName, out memberValue))
                {
                    bsonWriter.WriteName(memberName);
                    __objectSerializer.Serialize(dynamicContext, memberValue);
                }
            }
            bsonWriter.WriteEndDocument();
        }

        // protected methods
        /// <summary>
        /// Configures the deserialization context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected abstract void ConfigureDeserializationContext(BsonDeserializationContext.Builder builder);

        /// <summary>
        /// Configures the serialization context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected abstract void ConfigureSerializationContext(BsonSerializationContext.Builder builder);

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <returns>A <typeparamref name="T"/></returns>
        protected abstract T CreateDocument();

        /// <summary>
        /// Sets the value for the member.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="value">The value.</param>
        protected abstract void SetValueForMember(T document, string memberName, object value);

        /// <summary>
        /// Tries to get the value for a member.  Returns true if the member should be serialized.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the member should be serialized; otherwise <c>false</c>.</returns>
        protected abstract bool TryGetValueForMember(T document, string memberName, out object value);
    }
}
