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

namespace Lakerfield.Bson.Serialization.IdGenerators
{
    /// <summary>
    /// Represents an Id generator for ObjectIds represented internally as strings.
    /// </summary>
    public class StringObjectIdGenerator : IIdGenerator
    {
        // private static fields
        private static StringObjectIdGenerator __instance = new StringObjectIdGenerator();

        // constructors
        /// <summary>
        /// Initializes a new instance of the StringObjectIdGenerator class.
        /// </summary>
        public StringObjectIdGenerator()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of StringObjectIdGenerator.
        /// </summary>
        public static StringObjectIdGenerator Instance
        {
            get { return __instance; }
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
            return ObjectId.GenerateNewId().ToString();
        }

        /// <summary>
        /// Tests whether an Id is empty.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>True if the Id is empty.</returns>
        public bool IsEmpty(object id)
        {
            return string.IsNullOrEmpty((string)id);
        }
    }
}
