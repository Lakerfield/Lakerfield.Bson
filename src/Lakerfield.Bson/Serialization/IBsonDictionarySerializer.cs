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

using Lakerfield.Bson.Serialization.Options;

namespace Lakerfield.Bson.Serialization
{
    /// <summary>
    /// Represents a dictionary serializer that can be used in LINQ queries.
    /// </summary>
    public interface IBsonDictionarySerializer
    {
        /// <summary>
        /// Gets the dictionary representation.
        /// </summary>
        /// <value>
        /// The dictionary representation.
        /// </value>
        DictionaryRepresentation DictionaryRepresentation { get; }

        /// <summary>
        /// Gets the key serializer.
        /// </summary>
        /// <value>
        /// The key serializer.
        /// </value>
        IBsonSerializer KeySerializer { get; }

        /// <summary>
        /// Gets the value serializer.
        /// </summary>
        /// <value>
        /// The value serializer.
        /// </value>
        IBsonSerializer ValueSerializer { get; }
    }
}
