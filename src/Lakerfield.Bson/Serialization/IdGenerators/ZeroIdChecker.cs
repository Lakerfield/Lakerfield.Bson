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

namespace Lakerfield.Bson.Serialization.IdGenerators
{
    /// <summary>
    /// Represents an Id generator that only checks that the Id is not all zeros.
    /// </summary>
    /// <typeparam name="T">The type of the Id.</typeparam>
    // TODO: is it worth trying to remove the dependency on IEquatable<T>?
    public class ZeroIdChecker<T> : IIdGenerator where T : struct, IEquatable<T>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the ZeroIdChecker class.
        /// </summary>
        public ZeroIdChecker()
        {
        }

        // public methods
        /// <summary>
        /// Generates an Id for a document.
        /// </summary>
        /// <param name="container">The container of the document (will be a MongoCollection when called from the C# driver). </param>
        /// <param name="document">The document.</param>
        /// <returns>An Id.</returns>
        public object GenerateId(object container, object document)
        {
            throw new InvalidOperationException("Id cannot be default value (all zeros).");
        }

        /// <summary>
        /// Tests whether an Id is empty.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>True if the Id is empty.</returns>
        public bool IsEmpty(object id)
        {
            return id == null || ((T)id).Equals(default(T));
        }
    }
}
